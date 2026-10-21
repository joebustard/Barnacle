
using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Object3DLib
{
    public partial class PrimitiveGenerator
    {
      public static void GenerateArrowWideBase(ref Point3DCollection pnts, ref Int32Collection indices, ref Vector3DCollection normals)
      {
        double [] v =
        {
			0.444,0.000,-0.500,
			1.111,0.000,0.500,
			0.444,0.000,0.500,
			1.111,0.000,-0.500,
			1.111,0.625,0.500,
			1.111,0.625,-0.500,
			1.277,0.625,0.500,
			1.277,0.625,-0.500,
			0.777,1.000,0.500,
			0.777,1.000,-0.500,
			0.277,0.625,0.500,
			0.277,0.625,-0.500,
			0.444,0.625,0.500,
			0.444,0.625,-0.500,

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
			11,12,10,
			11,13,12,
			13,2,12,
			13,0,2,
			0,2,2,
			0,0,2,
			0,5,3,
			2,1,4,
			5,9,7,
			4,6,8,
			0,9,5,
			2,4,8,
			9,13,11,
			8,10,12,
			0,13,9,
			2,8,12,
			0,0,13,
			2,12,2,

        };

        BuildPrimitive(pnts, indices, v, f);
      }
    }
}
