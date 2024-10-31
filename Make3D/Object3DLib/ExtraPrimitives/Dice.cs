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
                -.3,-.5,-.5, //9
                -.3,.5,-.5, //10
                -.3,.5,.5, // 11
                -.3,-.5,.5, // 12

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
                .3,-.5,-.5, //22
                .3,.5,-.5, //23
                .3,.5,.5, // 24
                .3,-.5,.5, // 25
                // centres
                0,-.5,0, // 26
                0,0,-0.5, // 27
                0,0,0.5, // 28
                0.0,0.5,0.0 // 29 top
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
                19,25,20,
                // bottom
                26,7,8,
                26,8,9,
                26,9,22,
                26,22,21,
                26,21,20,
                26,20,25,
                26,25,12,
                26,12,7,
                // back
                27,1,2,
                27,2,10,
                27,10,23,
                27,23,15,
                27,15,14,
                27,14,22,
                27,22,9,
                27,9,1,
                // front
               28,5,6,
                28,11,5,
                28,24,11,
                28,18,24,
                28,19,18,
                28,25,19,
                28,12,25,
                28,6,12,
                // top
                29,10,3,
                29,23,10,
                29,16,23,
                29,17,16,
                29,24,17,
                29,11,24,
                29,4,11,
                29,3,4
            };

            BuildPrimitive(pnts, indices, v, f);
        }
    }
}