//
// ARCompat.cs: Compatibility functions
//
// Authors:
//	Vincent Dondain <vidondai@microsoft.com>
//
// Copyright 2017 Microsoft Inc. All rights reserved.
//

using System;
#if NET
using Vector2 = global::System.Numerics.Vector2;
using Vector3 = global::CoreGraphics.NVector3;
#else
using Vector2 = global::OpenTK.Vector2;
using Vector3 = global::OpenTK.NVector3;
#endif
#nullable enable

namespace ARKit {

#if !XAMCORE_4_0 && IOS
	public partial class ARFaceGeometry {

		[Obsolete ("Use the 'GetVertices' method instead.")]
		public virtual Vector3 Vertices { get; }

		[Obsolete ("Use the 'GetTextureCoordinates' method instead.")]
		public virtual Vector2 TextureCoordinates { get; }

		[Obsolete ("Use the 'GetTriangleIndices' method instead.")]
		public virtual short TriangleIndices { get; }
	}
#endif
}
