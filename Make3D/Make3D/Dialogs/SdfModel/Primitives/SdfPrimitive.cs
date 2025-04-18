using Barnacle.Dialogs.SdfModel.Primitives;
using Barnacle.Object3DLib;
using MeshDecimator.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs.SdfModel
{
    internal class SdfPrimitive
    {
        public Point3D Position
        {
            get;
            set;
        }

        public double[,] Rotation
        {
            get;
            set;
        }

        public double RotX
        {
            get; set;
        }

        public double RotY
        {
            get; set;
        }

        public double RotZ
        {
            get; set;
        }

        public Vector3D Size
        {
            get;
            set;
        }

        public override String ToString()
        {
            return "";
        }

        internal Vector3D Abs(Point3D p)
        {
            Vector3D v = new Vector3D(Math.Abs(p.X), Math.Abs(p.Y), Math.Abs(p.Z));
            return v;
        }

        internal virtual void AdjustBounds(Bounds3D bounds)
        {
            double l = Size.X / 2;
            double h = Size.Y / 2;
            double w = Size.Z / 2;
            if (Rotation != null)
            {
                l = Size.X;
                h = Size.Y;
                w = Size.Z;
            }
            //            else
            //{
            bounds.Adjust(new Point3D(Position.X - l, Position.Y - h, Position.Z - w));
            bounds.Adjust(new Point3D(Position.X + l, Position.Y + h, Position.Z + w));
            //            }
        }

        internal virtual double GetSdfValue(Point3D xyZ)
        {
            return double.MaxValue;
        }

        internal Vector2d max(Vector2d v, double m)
        {
            return new Vector2d((v.x > m) ? v.x : m, (v.y > m) ? v.y : m);
        }

        internal Point3D Rotate(Point3D p)
        {
            Point3D res = Rotations.Multiply(Rotation, p);
            return res;
        }

        internal virtual string ToParameters()
        {
            string s = ToString() + ",";
            s += Position.X.ToString() + ",";
            s += Position.Y.ToString() + ",";
            s += Position.Z.ToString() + ",";
            s += Size.X.ToString() + ",";
            s += Size.Y.ToString() + ",";
            s += Size.Z.ToString();
            return s;
        }

        protected Point3D RotatePoint(double r1, double r2, double r3, Point3D cp)
        {
            Point3D tmp = new Point3D();
            r1 = DegToRad(r1);
            r2 = DegToRad(r2);
            r2 = DegToRad(r3);
            float cosa = (float)Math.Cos(r2);
            float sina = (float)Math.Sin(r2);

            float cosb = (float)Math.Cos(r1);
            float sinb = (float)Math.Sin(r1);

            float cosc = (float)Math.Cos(r3);
            float sinc = (float)Math.Sin(r3);

            float Axx = cosa * cosb;
            float Axy = cosa * sinb * sinc - sina * cosc;
            float Axz = cosa * sinb * cosc + sina * sinc;

            float Ayx = sina * cosb;
            float Ayy = sina * sinb * sinc + cosa * cosc;
            float Ayz = sina * sinb * cosc - cosa * sinc;

            float Azx = -sinb;
            float Azy = cosb * sinc;
            float Azz = cosb * cosc;

            tmp.X = Axx * cp.X + Axy * cp.Y + Axz * cp.Z;
            tmp.Y = Ayx * cp.X + Ayy * cp.Y + Ayz * cp.Z;
            tmp.Z = Azx * cp.X + Azy * cp.Y + Azz * cp.Z;

            return tmp;
        }

        private double DegToRad(double v)
        {
            return (v * Math.PI / 180.0);
        }
    }
}