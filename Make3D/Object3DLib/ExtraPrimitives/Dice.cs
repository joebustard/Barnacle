using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Object3DLib
{
    public partial class PrimitiveGenerator
    {
        public static void GenerateDice(ref Point3DCollection pnts, ref Int32Collection indices, ref Vector3DCollection normals)
        {
            double[] v =
            {
                // left side
                -.5,0,0, // 0
                -0.5,-.3,-.5, // 1
                -.5,.3,-.5, // 2
                -.5,.5,-.3, // 3
                -.5,.5,.3, // 4
                -.5,.3,.5, // 5
                -.5,-.3,.5, // 6
                -.5,-.5,.3, // 7
                -.5,-.5,-.3, // 8
                // camfer points
                -.4,-.5,-.5, //9
                -.4,.5,-.5, //10
                -.4,.5,.5, // 11
                -.4,-.5,.5, // 12

                // right side
                .5,0,0, // 13
                0.5,-.3,-.5, // 14
                .5,.3,-.5, // 15
                .5,.5,-.3, // 16
                .5,.5,.3, // 17
                .5,.3,.5, // 18
                .5,-.3,.5, // 19
                .5,-.5,.3, // 20
                .5,-.5,-.3, // 21
                // camfer points
                .4,-.5,-.5, //22
                .4,.5,-.5, //23
                .4,.5,.5, // 24
                .4,-.5,.5 // 25
            };

            int[] f =
            {
                // left
                0,2,1,
                0,3,2,
                0,4,3,
                0,5,4,
                0,6,5,
                0,7,6,
                0,8,7,
                0,1,8,
                8,1,9,
                2,3,10,
                5,11,4,
                7,12,6,
                // right
                13,14,15,
                13,15,16,
                13,16,17,
                13,17,18,
                13,18,19,
                13,19,20,
                13,20,21,
                21,14,13,
                21,22,14,
                15,23,16,
                17,24,18,
                19,25,20
            };

            for (int i = 0; i < v.GetLength(0); i += 3)
            {
                pnts.Add(new Point3D(v[i], v[i + 1], v[i + 2]));
            }
            for (int j = 0; j < f.GetLength(0); j++)
            {
                indices.Add(f[j]);
            }
        }
    }
}