using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Make3D.Models
{
    internal class VectorUtils
    {
        // Return vectors along the coordinate axes.
        public static Vector3D XVector(double length = 1)
        {
            return new Vector3D(length, 0, 0);
        }

        public static Vector3D YVector(double length = 1)
        {
            return new Vector3D(0, length, 0);
        }

        public static Vector3D ZVector(double length = 1)
        {
            return new Vector3D(0, 0, length);
        }
    }
}