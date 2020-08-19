using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Make3D.Models
{
    public class Bounds3D
    {
        private Point3D lower;
        public Point3D Lower
        {
            get { return lower; }
            set { lower = value; }
        }

        private Point3D upper;
        public Point3D Upper
        {
            get { return upper; }
            set { upper = value; }
        }

        public double Width
        { get { return Upper.X - Lower.X; }
        }

        public Bounds3D()
        {
            lower.X = double.MaxValue;
            lower.Y = double.MaxValue;
            lower.Z = double.MaxValue;

            upper.X = double.MinValue;
            upper.Y = double.MinValue;
            upper.Z = double.MinValue;
        }

        public void Zero()
        {
            lower.X = 0;
            lower.Y = 0;
            lower.Z = 0;

            upper.X = 0;
            upper.Y = 0;
            upper.Z = 0;
        }
        public static  Bounds3D operator+ (Bounds3D a, Bounds3D b)
        {
            Bounds3D res = a;
            res.Adjust(b.Lower);
            res.Adjust(b.Upper);
            return res;
        }

        public void Adjust(Point3D ap)
        {
            
            if (ap.X < lower.X)
            {
                lower.X = ap.X;
            }
            if (ap.Y < lower.Y)
            {
                lower.Y = ap.Y;
            }
            if (ap.Z < lower.Z)
            {
                lower.Z = ap.Z;
            }

           
            if (ap.X > upper.X)
            {
                upper.X = ap.X;
            }
            if (ap.Y > upper.Y)
            {
                upper.Y = ap.Y;
            }
            if (ap.Z > upper.Z)
            {
                upper.Z = ap.Z;
            }
           
        }

        internal Point3D MidPoint()
        {
            Point3D res = new Point3D();
            res.X = lower.X + (upper.X - lower.X) / 2;
            res.Y = lower.Y + (upper.Y - lower.Y) / 2;
            res.Z = lower.Z + (upper.Z - lower.Z) / 2;
            return res;
        }

        internal Point3D Size()
        {
            Point3D res = new Point3D();
            res.X = upper.X - lower.X;
            res.Y = upper.Y - lower.Y;
            res.Z = upper.Z - lower.Z;
            return res;
        }
    }
}
