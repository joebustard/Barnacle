using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Object3DLib
{
    public partial class PrimitiveGenerator
    {
        public static void GenerateStellateOcto(ref Point3DCollection pnts, ref Int32Collection indices, ref Vector3DCollection normals)
        {
            double[] v =
            {
            0.000,0.000,-0.433,
            0.500,0.500,-0.500,
            0.433,0.000,0.000,
            0.000,0.433,0.000,
            -0.500,0.500,-0.500,
            -0.433,0.000,0.000,
            -0.500,-0.500,-0.500,
            0.000,-0.433,0.000,
            0.500,-0.500,-0.500,
            0.000,0.000,0.433,
            0.500,-0.500,0.500,
            0.500,0.500,0.500,
            -0.500,0.500,0.500,
            -0.500,-0.500,0.500,
        };

            int[] f =
            {
            0,1,2,
            2,1,3,
            3,1,0,
            0,4,3,
            3,4,5,
            5,4,0,
            0,6,5,
            5,6,7,
            7,6,0,
            0,8,7,
            7,8,2,
            2,8,0,
            9,10,2,
            2,10,7,
            7,10,9,
            9,11,3,
            3,11,2,
            2,11,9,
            9,12,5,
            5,12,3,
            3,12,9,
            9,13,7,
            7,13,5,
            5,13,9,
        };

            BuildPrimitive(pnts, indices, v, f);
        }
    }
}