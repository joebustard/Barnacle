
using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Object3DLib
{
    public partial class PrimitiveGenerator
    {
      public static void GenerateRightBracket(ref Point3DCollection pnts, ref Int32Collection indices, ref Vector3DCollection normals)
      {
        double [] v =
        {
			-0.500,1.000,-0.500,
			-0.500,0.000,0.500,
			-0.500,1.000,0.500,
			-0.500,0.000,-0.500,
			0.500,0.000,0.500,
			0.500,0.000,-0.500,
			0.500,0.200,0.500,
			0.500,0.200,-0.500,
			-0.300,0.200,0.500,
			-0.300,0.200,-0.500,
			-0.300,1.000,0.500,
			-0.300,1.000,-0.500,

        };

        int [] f =
        {
			0,1,2,
			0,3,1,
			3,4,1,
			3,5,4,
			5,6,4,
			5,7,6,
			7,8,6,
			7,9,8,
			9,10,8,
			9,11,10,
			11,2,10,
			11,0,2,
			0,2,2,
			0,0,2,
			3,7,5,
			1,4,6,
			3,9,7,
			1,6,8,
			0,9,3,
			2,1,8,
			11,0,0,
			10,2,2,
			0,11,9,
			2,8,10,

        };

        BuildPrimitive(pnts, indices, v, f);
      }
    }
}
