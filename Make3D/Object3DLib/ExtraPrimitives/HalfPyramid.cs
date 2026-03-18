
using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Object3DLib
{
    public partial class PrimitiveGenerator
    {
      public static void GenerateHalfPyramid(ref Point3DCollection pnts, ref Int32Collection indices, ref Vector3DCollection normals)
      {
        double [] v =
        {
			0.500,0.000,0.500,
			0.000,1.000,-0.500,
			-0.500,0.000,0.500,
			0.167,0.667,-0.500,
			0.500,0.000,-0.500,
			-0.500,0.000,-0.500,
			0.000,0.000,-0.500,

        };

        int [] f =
        {
			0,1,2,
			1,0,3,
			0,4,3,
			5,2,1,
			2,6,0,
			4,0,6,
			3,5,1,
			2,5,6,
			3,4,6,
			5,3,6,

        };

        BuildPrimitive(pnts, indices, v, f);
      }
    }
}
