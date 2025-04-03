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
    internal class SdfTriangle : SdfPrimitive
    {
        public SdfTriangle()
        {
            Size = new Vector3D(20, 20, 20);
            Position = new Point3D(0, 0, 0);
        }

        public override String ToString()
        {
            return "Triangle";
        }

        internal override void AdjustBounds(Bounds3D bounds)
        {
            double l = Size.X * 1.5;
            double h = Size.Y * 1.5;
            double w = Size.Z * 1.5;
            bounds.Adjust(new Point3D(Position.X - l, Position.Y - h, Position.Z - w));
            bounds.Adjust(new Point3D(Position.X + l, Position.Y + h, Position.Z + w));
        }

        internal override double GetSdfValue(Point3D p)
        {
            if (Rotation != null)
            {
                p = Rotate(p);
            }
            Vector2d h = new Vector2d(Size.X / 2, Size.Y * 0.5);

            Vector3D q = Abs(p);
            double result = Math.Max(q.Z - h.y, Math.Max(q.X * 0.55 + p.Y * 0.2, -p.Y) - h.x * 0.335);
            return result;
        }
    }
}