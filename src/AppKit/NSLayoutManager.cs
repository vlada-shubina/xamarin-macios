// Copyright 2015 Xamarin, Inc.
#if !WATCH

#if !MONOMAC
using NSFont=UIKit.UIFont;
#endif

using System;
using ObjCRuntime;
using Foundation;
using CoreGraphics;
using System.Runtime.Versioning;

#if MONOMAC
namespace AppKit {
#else
namespace UIKit {
#endif
	partial class NSLayoutManager {
#if !XAMCORE_4_0 && MONOMAC
#if NET
		[UnsupportedOSPlatform ("macos10.11")]
#if MONOMAC
		[Obsolete ("Starting with macos10.11.", DiagnosticId = "BI1234", UrlFormat = "https://github.com/xamarin/xamarin-macios/wiki/Obsolete")]
#endif
#else
		[Deprecated (PlatformName.MacOSX, 10, 11)]
#endif
		public CGRect [] GetRectArray (NSRange glyphRange, NSRange selectedGlyphRange, NSTextContainer textContainer)
		{
			if (textContainer == null)
				throw new ArgumentNullException ("textContainer");

			nuint rectCount;
			var retHandle = GetRectArray (glyphRange, selectedGlyphRange, textContainer.Handle, out rectCount);
			var returnArray = new CGRect [rectCount];

			unsafe {
				float *ptr = (float*) retHandle;
				for (nuint i = 0; i < rectCount; ++i) {
					returnArray [i] = new CGRect (ptr [0], ptr [1], ptr [2], ptr [3]);
					ptr += 4;
				}
			}
			return returnArray;
		}
#endif // MONOMAC

#if !NET && MONOMAC
		[Obsolete ("Use 'GetIntAttribute' instead.")]
		public virtual nint IntAttributeforGlyphAtIndex (nint attributeTag, nint glyphIndex)
		{
			return GetIntAttribute (attributeTag, glyphIndex);
		}
#endif
	}
}

#endif // !WATCH
