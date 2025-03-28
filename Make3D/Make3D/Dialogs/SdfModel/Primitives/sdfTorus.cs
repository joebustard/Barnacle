using Barnacle.Object3DLib;
using MeshDecimator.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs.SdfModel.Primitives
{
    internal class SdfTorus : SdfPrimitive
    {
        public SdfTorus()
        {
            Size = new Vector3D(20, 20, 20);
            Position = new Point3D(0, 0, 0);
            Thickness = 3;
        }

        public double Thickness
        {
            get;
            set;
        }

        public override String ToString()
        {
            return "Torus";
        }

        internal override void AdjustBounds(Bounds3D bounds)
        {
            double l = Size.X / 2;
            double h = Thickness;
            double w = Size.X;
            bounds.Adjust(new Point3D(Position.X - l, Position.Y - h, Position.Z - w));
            bounds.Adjust(new Point3D(Position.X + l, Position.Y + h, Position.Z + w));
            bounds.Expand(new Point3D(2 * Thickness, 2 * Thickness, 3 * Thickness));
        }

        internal override double GetSdfValue(Point3D p)
        {
            p = p - new Vector3D(Position.X, Position.Y, Position.Z);
            if (Rotation != null)
            {
                p = Rotate(p);
            }
            Vector2d t = new Vector2d(Size.X / 2 - Thickness, Thickness);
            Vector2d tmp = new Vector2d(p.X, p.Z);
            Vector2d q = new Vector2d(tmp.Magnitude - Math.Abs(t.x), p.Y);

            double d = q.Magnitude - t.y;
            return d;
        }

        internal override string ToParameters()
        {
            string s = ToString() + ",";
            s += Position.X.ToString() + ",";
            s += Position.Y.ToString() + ",";
            s += Position.Z.ToString() + ",";
            s += Size.X.ToString() + ",";
            s += Size.Y.ToString() + ",";
            s += Size.Z.ToString() + ",";
            s += Thickness.ToString();
            return s;
        }
    }
}