using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Object3DLib
{
    public partial class PrimitiveGenerator
    {
        public static void GenerateShallowUBeam(ref Point3DCollection pnts, ref Int32Collection indices, ref Vector3DCollection normals)
        {
            double[] v =
            {
            -0.500,0.000,-0.625,
            -0.500,1.000,0.375,
            -0.500,1.000,-0.625,
            -0.500,0.000,0.375,
            0.500,1.000,0.375,
            0.500,0.000,0.375,
            0.500,1.000,0.175,
            0.500,0.000,0.175,
            0.25,1.000,0.175,
            0.25,0.000,0.175,
            0.25,1.000,-0.425,
            0.25,0.000,-0.425,
            0.500,1.000,-0.425,
            0.500,0.000,-0.425,
            0.500,1.000,-0.625,
            0.500,0.000,-0.625,
        };

            int[] f =
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
            13,14,12,
            13,15,14,
            15,2,14,
            15,0,2,
            0,2,2,
            0,0,2,
            3,7,5,
            1,4,6,
            3,9,7,
            1,6,8,
            0,9,3,
            2,1,8,
            0,11,9,
            2,8,10,
            0,13,11,
            2,10,12,
            0,15,13,
            2,12,14,
            0,0,15,
            2,14,2,
        };

            BuildPrimitive(pnts, indices, v, f);
        }
    }
}