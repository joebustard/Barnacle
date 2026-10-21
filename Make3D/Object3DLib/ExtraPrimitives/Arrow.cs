
using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Object3DLib
{
    public partial class PrimitiveGenerator
    {
      public static void GenerateArrow(ref Point3DCollection pnts, ref Int32Collection indices, ref Vector3DCollection normals)
      {
        double [] v =
        {
			-0.167,0.000,-0.782,
			0.167,0.000,0.218,
			-0.167,0.000,0.218,
			0.167,0.000,-0.782,
			0.167,0.625,0.218,
			0.167,0.625,-0.782,
			0.500,0.625,0.218,
			0.500,0.625,-0.782,
			0.000,1.000,0.218,
			0.000,1.000,-0.782,
			-0.500,0.625,0.218,
			-0.500,0.625,-0.782,
			-0.167,0.625,0.218,
			-0.167,0.625,-0.782,

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
			5,9,7,
			4,6,8,
			3,9,5,
			1,4,8,
			9,13,11,
			8,10,12,
			3,13,9,
			1,8,12,
			13,0,0,
			12,2,2,
			0,13,3,
			2,1,12,

        };

        BuildPrimitive(pnts, indices, v, f);
      }
    }
}
