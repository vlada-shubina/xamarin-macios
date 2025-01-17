#if NET

using System;
using System.Collections.Generic;

#nullable enable

namespace ObjCRuntime {
	public readonly struct NativeHandle : IEquatable<NativeHandle> {
		readonly IntPtr handle;

		public IntPtr Handle {
			get { return handle; }
		}

		public static NativeHandle Zero = default (NativeHandle);

		public NativeHandle (IntPtr handle)
		{
			this.handle = handle;
		}

		public static bool operator == (NativeHandle left, IntPtr right)
		{
			return left.handle == right;
		}

		public static bool operator == (NativeHandle left, NativeHandle right)
		{
			return left.handle == right.handle;
		}

		public static bool operator == (IntPtr left, NativeHandle right)
		{
			return left == right.Handle;
		}

		public static bool operator != (NativeHandle left, IntPtr right)
		{
			return left.handle != right;
		}

		public static bool operator != (IntPtr left, NativeHandle right)
		{
			return left != right.Handle;
		}

		public static bool operator != (NativeHandle left, NativeHandle right)
		{
			return left.handle != right.Handle;
		}

		// Should this be made explicit? The JIT seems to optimize conversions away to
		// treat everything as a plain IntPtr, so I'm not sure there's any reason
		// to not keep it implicit.
		public static implicit operator IntPtr (NativeHandle value)
		{
			return value.Handle;
		}

		// Should this be made explicit? The JIT seems to optimize conversions away to
		// treat everything as a plain IntPtr, so I'm not sure there's any reason
		// to not keep it implicit.
		public static implicit operator NativeHandle (IntPtr value)
		{
			return new NativeHandle (value);
		}

		public override bool Equals (object? o)
		{
			if (o is NativeHandle nh)
				return nh.handle == this.handle;
			return false;
		}

		public override int GetHashCode ()
		{
			return handle.GetHashCode ();
		}

		public bool Equals (NativeHandle other)
		{
			return other.handle == handle;
		}

		public override string ToString ()
		{
			return "0x" + handle.ToString ("x");
		}
	}
}
#endif
