using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Barnacle.Models
{
    public static class Point3DExtensions
    {
        public static double Distance(this Point3D s, Point3D e)
        {

            double xd = e.X - s.X;
            double yd = e.Y - s.Y;
            double zd = e.Z - s.Z;
            double res = Math.Sqrt(xd * xd + yd * yd + zd * zd);
            return res;
        }

        public static Point3D MidPoint( this Point3D s, Point3D e)
        {
            return (new Point3D((s.X + e.X) / 2, (s.Y + e.Y) / 2, (s.Z + e.Z) / 2));
        }
    }
}
