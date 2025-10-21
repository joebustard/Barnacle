using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Object3DLib
{
    public partial class PrimitiveGenerator
    {
        public static void GenerateTetrahedron(ref Point3DCollection pnts, ref Int32Collection indices, ref Vector3DCollection normals)
        {
            double[] v =
            {
            0.0,0.0,-0.500,
            0.0,0.0,0.500,
            0.0,1.000,0.0,
            1.0,0.0,0.0,
        };

            int[] f =
            {
            0,1,2,
            1,3,2,
            0,2,3,
            0,3,1
        };

            BuildPrimitive(pnts, indices, v, f);
        }
    }
}