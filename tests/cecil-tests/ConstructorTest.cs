using System;
using System.Collections.Generic;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;
using NUnit.Framework;

using Xamarin.Tests;
using Xamarin.Utils;

namespace Cecil.Tests {
	public class ConstructorTest {
		static bool IsMatch (MethodDefinition ctor, params (string Namespace, string Name) [] parameterTypes)
		{
			if (!ctor.IsConstructor)
				return false;
			if (ctor.IsStatic)
				return false;
			if (!ctor.HasParameters)
				return false;
			var parameters = ctor.Parameters;
			if (parameters.Count != parameterTypes.Length)
				return false;
			for (var i = 0; i < parameters.Count; i++) {
				if (!parameters [i].ParameterType.Is (parameterTypes [i].Namespace, parameterTypes [i].Name))
					return false;
			}
			return true;
		}

		static MethodDefinition GetConstructor (TypeDefinition type, params (string Namespace, string Name) [] parameterTypes)
		{
			foreach (var ctor in type.Methods) {
				if (IsMatch (ctor, parameterTypes))
					return ctor;
			}
			return null;
		}

		static string GetLocation (MethodDefinition method)
		{
			if (method.DebugInformation.HasSequencePoints) {
				var seq = method.DebugInformation.SequencePoints [0];
				return seq.Document.Url + ":" + seq.StartLine + ": ";
			}
			return string.Empty;
		}

		static bool IsFunctionEnd (IList<Instruction> instructions, int index)
		{
			if (instructions.Count == index + 1 && instructions [index].OpCode == OpCodes.Ret)
				return true;
			if (instructions.Count == index + 2 && instructions [index].OpCode == OpCodes.Newobj && ((MethodReference) instructions [index].Operand).Resolve ().Parameters.Count == 0 && instructions [index + 1].OpCode == OpCodes.Throw)
				return true;
			if (instructions.Count == index + 3 && instructions [index].OpCode == OpCodes.Ldstr && instructions [index + 1].OpCode == OpCodes.Newobj && ((MethodReference) instructions [index + 1].Operand).Resolve ().Parameters.Count == 1 && instructions [index + 2].OpCode == OpCodes.Throw)
				return true;
			return false;
		}

		static bool VerifyInstructions (MethodDefinition method, IList<Instruction> instructions, out string reason)
		{
			reason = null;

			if (instructions [0].OpCode == OpCodes.Ldarg_0 && instructions [1].OpCode == OpCodes.Ldarg_1 && instructions [2].OpCode == OpCodes.Call) {
				var targetMethod = (instructions [2].Operand as MethodReference).Resolve ();
				if (!targetMethod.IsConstructor) {
					reason = $"Calls another method which is not a constructor: {targetMethod.FullName}";
					return false;
				}
				var isChainedCtorCall = targetMethod.DeclaringType == method.DeclaringType || targetMethod.DeclaringType == method.DeclaringType.BaseType;
				if (!isChainedCtorCall) {
					reason = $"Calls unknown (unchained) constructor: {targetMethod.FullName}";
					return false;
				}

				if (IsFunctionEnd (instructions, 3))
					return true;

				if (instructions [3].OpCode == OpCodes.Ldarg_0 && instructions [4].OpCode == OpCodes.Ldc_I4_0 && instructions [5].OpCode == OpCodes.Call) {
					targetMethod = (instructions [5].Operand as MethodReference).Resolve ();
					if (targetMethod.Name != "set_IsDirectBinding") {
						reason = $"Calls unknown method: {targetMethod.FullName}";
						return false;
					}

					if (IsFunctionEnd (instructions, 6))
						return true;
				}

				if (instructions [3].OpCode == OpCodes.Ldarg_0 && instructions [4].OpCode == OpCodes.Call) {
					targetMethod = (instructions [4].Operand as MethodReference).Resolve ();
					if (targetMethod.Name != "MarkDirtyIfDerived") {
						reason = $"Calls unknown method: {targetMethod.FullName}";
						return false;
					}

					if (IsFunctionEnd (instructions, 5))
						return true;
				}
			}

			if (instructions [0].OpCode == OpCodes.Ldarg_0 && instructions [1].OpCode == OpCodes.Ldarg_1 && instructions [2].OpCode == OpCodes.Ldc_I4_0 && instructions [3].OpCode == OpCodes.Call) {
				var targetMethod = (instructions [3].Operand as MethodReference).Resolve ();
				if (!targetMethod.IsConstructor) {
					reason = $"Calls another method which is not a constructor (2): {targetMethod.FullName}";
					return false;
				}
				var isChainedCtorCall = targetMethod.DeclaringType == method.DeclaringType || targetMethod.DeclaringType == method.DeclaringType.BaseType;
				if (!isChainedCtorCall) {
					reason = $"Calls unknown (unchained) constructor (2): {targetMethod.FullName}";
					return false;
				}

				if (IsFunctionEnd (instructions, 4))
					return true;
			}

			if (reason is null)
				reason = $"Sequence of instructions didn't match any known sequence.";

			return false;
		}

		static bool VerifyConstructor (MethodDefinition ctor, out string failureReason)
		{
			failureReason = null;
			// There's nothing wrong with a constructor that doesn't exist
			if (ctor is null)
				return true;

			// Verify that the constructor only does valid stuff
			if (!VerifyInstructions (ctor, ctor.Body.Instructions, out failureReason)) {
				Console.WriteLine (ctor.FullName);
				foreach (var instr in ctor.Body.Instructions)
					Console.WriteLine (instr);

				return false;
			}

			return true;
		}

		public static bool ImplementsINativeObject (TypeDefinition type)
		{
			if (type is null)
				return false;

			foreach (var id in type.Interfaces) {
				if (id.InterfaceType.Name == "INativeObject") {
					return true;
				}
			}

			return ImplementsINativeObject (type.BaseType?.Resolve ());
		}

		[Test]
		[TestCase (ApplePlatform.iOS)]
		[TestCase (ApplePlatform.TVOS)]
		[TestCase (ApplePlatform.MacCatalyst)]
		[TestCase (ApplePlatform.MacOSX)]
		public void INativeObjectIntPtrConstructorDoesNotOwnHandle (ApplePlatform platform)
		{
			Configuration.IgnoreIfIgnoredPlatform (platform);

			var failures = new List<string> ();
			foreach (var dll in Configuration.GetBaseLibraryImplementations (platform)) {
				Console.WriteLine (dll);
				using (var ad = AssemblyDefinition.ReadAssembly (dll, new ReaderParameters (ReadingMode.Deferred) { ReadSymbols = true })) {
					foreach (var type in ad.MainModule.Types) {
						// Skip classes we know aren't (properly) reference counted.
						switch (type.Name) {
						case "Selector": // not really refcounted
						case "Class": // not really refcounted
						case "Protocol": // not really refcounted
						case "AURenderEventEnumerator": // this class shouldn't really be an INativeObject in the first place
							continue;
						}

						// Find classes that implement INativeObject, but doesn't subclass NSObject.
						if (!type.IsClass)
							continue;
						if (!type.HasInterfaces)
							continue;

						// Does type implement INativeObject?
						if (!ImplementsINativeObject (type))
							continue;

						// Find the constructors constructors we care about
						var intptrCtor = GetConstructor (type, ("System", "IntPtr"));
						var intptrBoolCtor = GetConstructor (type, ("System", "IntPtr"), ("System", "bool"));
						var nativeHandleCtor = GetConstructor (type, ("ObjCRuntime", "NativeHandle"));
						var nativeHandleBoolCtor = GetConstructor (type, ("ObjCRuntime", "NativeHandle"), ("System", "bool"));

						if (intptrCtor is not null) {
							var msg = $"{type}: (IntPtr) constructor found. It should not exist.";
							Console.WriteLine ($"{GetLocation (intptrCtor)}{msg}");
							failures.Add (msg);
						}

						if (intptrBoolCtor is not null) {
							var msg = $"{type}: (IntPtr, bool) constructor found. It should not exist.";
							Console.WriteLine ($"{GetLocation (intptrBoolCtor)}{msg}");
							failures.Add (msg);
						}

						if (nativeHandleCtor is not null) {
							if (nativeHandleCtor.IsPublic) {
								var msg = $"{type}: public (NativeHandle) constructor found. If it exists it should not be public.";
								Console.WriteLine ($"{GetLocation (nativeHandleCtor)}{msg}");
								failures.Add (msg);
							}
						}

						if (nativeHandleBoolCtor is not null) {
							if (nativeHandleBoolCtor.IsPublic) {
								var msg = $"{type}: public (NativeHandle, bool) constructor found. If it exists it should not be public.";
								Console.WriteLine ($"{GetLocation (nativeHandleBoolCtor)}{msg}");
								failures.Add (msg);
							}
						}

						var skipILVerification = false;
						switch (type.Name) {
						case "NSObject": // NSObject is a base class and needs custom constructor logic
							skipILVerification = true;
							break;
						}

						if (!skipILVerification) {
							if (!VerifyConstructor (nativeHandleCtor, out var failureReason)) {
								var msg = $"{type}: (NativeHandle) ctor failed IL verification: {failureReason}";
								Console.WriteLine ($"{GetLocation (nativeHandleCtor)}{msg}");
								failures.Add (msg);
							}

							if (!VerifyConstructor (nativeHandleBoolCtor, out failureReason)) {
								var msg = $"{type}: (NativeHandle, bool) ctor failed IL verification: {failureReason}";
								Console.WriteLine ($"{GetLocation (nativeHandleBoolCtor)}{msg}");
								failures.Add (msg);
							}
						}
					}
				}
			}
			Assert.That (failures, Is.Empty, "No failures");
		}
	}
}