using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Object3DLib
{
    public partial class PrimitiveGenerator
    {
        public static void GenerateStar6(ref Point3DCollection pnts, ref Int32Collection indices, ref Vector3DCollection normals)
        {
            double[] v =
            {
            0.289,0.000,0.000,
            0.500,0.000,0.250,
            0.250,-0.500,0.125,
            0.144,0.000,0.217,
            0.250,0.500,0.125,
            0.000,0.000,0.500,
            0.000,-0.500,0.250,
            -0.144,0.000,0.217,
            0.000,0.500,0.250,
            -0.500,0.000,0.250,
            -0.250,-0.500,0.125,
            -0.289,0.000,0.000,
            -0.250,0.500,0.125,
            -0.500,0.000,-0.250,
            -0.250,-0.500,-0.125,
            -0.144,0.000,-0.217,
            -0.250,0.500,-0.125,
            0.000,0.000,-0.500,
            0.000,-0.500,-0.250,
            0.144,0.000,-0.217,
            0.000,0.500,-0.250,
            0.500,0.000,-0.250,
            0.250,-0.500,-0.125,
            0.250,0.500,-0.125,
            0.000,-0.500,0.000,
            0.000,0.500,0.000,
        };

            int[] f =
            {
            0,1,2,
            2,1,3,
            0,4,1,
            4,3,1,
            3,5,6,
            6,5,7,
            3,8,5,
            8,7,5,
            7,9,10,
            10,9,11,
            7,12,9,
            12,11,9,
            11,13,14,
            14,13,15,
            11,16,13,
            16,15,13,
            15,17,18,
            18,17,19,
            15,20,17,
            20,19,17,
            19,21,22,
            22,21,0,
            19,23,21,
            23,0,21,
            0,2,24,
            2,3,24,
            0,25,4,
            4,25,3,
            3,6,24,
            6,7,24,
            3,25,8,
            8,25,7,
            7,10,24,
            10,11,24,
            7,25,12,
            12,25,11,
            11,14,24,
            14,15,24,
            11,25,16,
            16,25,15,
            15,18,24,
            18,19,24,
            15,25,20,
            20,25,19,
            19,22,24,
            22,0,24,
            19,25,23,
            23,25,0,
        };

            BuildPrimitive(pnts, indices, v, f);
        }
    }
}