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
            bounds.Adjust(new Point3D(Position.X - l, Position.Y - h, Position.Z - w));
            bounds.Adjust(new Point3D(Position.X + l, Position.Y + h, Position.Z + w));
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
    }
}