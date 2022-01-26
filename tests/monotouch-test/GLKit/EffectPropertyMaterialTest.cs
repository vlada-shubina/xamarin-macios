// Copyright 2012 Xamarin Inc. All rights reserved

#if HAS_GLKIT

using System;
using System.Drawing;
using Foundation;
using GLKit;
using ObjCRuntime;
using NUnit.Framework;
using Xamarin.Utils;

namespace MonoTouchFixtures.GLKit {
	
	[TestFixture]
	[Preserve (AllMembers = true)]
	public class EffectPropertytMaterialTest {
		
		[Test]
		[Culture ("en")]
		public void Properties ()
		{
			TestRuntime.AssertSystemVersion (ApplePlatform.MacOSX, 10, 8, throwIfOtherPlatform: false);

			var material = new GLKEffectPropertyMaterial ();
			Assert.That (material.AmbientColor.ToString (), Is.EqualTo ("(0.2, 0.2, 0.2, 1)"), "AmbientColor");
			Assert.That (material.DiffuseColor.ToString (), Is.EqualTo ("(0.8, 0.8, 0.8, 1)"), "DiffuseColor");
			Assert.That (material.SpecularColor.ToString (), Is.EqualTo ("(0, 0, 0, 1)"), "SpecularColor");
			Assert.That (material.EmissiveColor.ToString (), Is.EqualTo ("(0, 0, 0, 1)"), "EmissiveColor");

			material = new GLKBaseEffect ().Material;
			Assert.That (material.AmbientColor.ToString (), Is.EqualTo ("(0.2, 0.2, 0.2, 1)"), "AmbientColor");
			Assert.That (material.DiffuseColor.ToString (), Is.EqualTo ("(0.8, 0.8, 0.8, 1)"), "DiffuseColor");
			Assert.That (material.SpecularColor.ToString (), Is.EqualTo ("(0, 0, 0, 1)"), "SpecularColor");
			Assert.That (material.EmissiveColor.ToString (), Is.EqualTo ("(0, 0, 0, 1)"), "EmissiveColor");
		}
	}
}

#endif // HAS_GLKIT
