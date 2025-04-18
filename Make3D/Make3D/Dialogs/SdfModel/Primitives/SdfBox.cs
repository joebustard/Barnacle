using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Media3D;
using static Barnacle.Dialogs.ImageMarker;

namespace Barnacle.Dialogs.SdfModel.Primitives
{
    internal class SdfBox : SdfPrimitive
    {
        public SdfBox()
        {
            Size = new Vector3D(20, 20, 20);
            Position = new Point3D(0, 0, 0);
        }

        public override String ToString()
        {
            return "Box";
        }

        /*
        internal override void AdjustBounds(Bounds3D bounds)
        {
            double l = Size.X / 2;
            double h = Size.Y / 2;
            double w = Size.Z / 2;
            if (Rotation != null)
            {
                AdjustOffset(-l, -h, -w, bounds);
                AdjustOffset(l, h, w, bounds);
                AdjustOffset(-l, -h, -w, bounds);
                AdjustOffset(l, h, w, bounds);
                AdjustOffset(-l, -h, -w, bounds);
                AdjustOffset(l, h, w, bounds);
                AdjustOffset(-l, -h, -w, bounds);
                AdjustOffset(l, h, w, bounds);
            }
            else
            {
                bounds.Adjust(new Point3D(Position.X - l, Position.Y - h, Position.Z - w));
                bounds.Adjust(new Point3D(Position.X + l, Position.Y + h, Position.Z + w));
            }
        }
        */

        internal override double GetSdfValue(Point3D p)
        {
            if (Rotation != null)
            {
                p = Rotate(p);
            }
            double x = Math.Max(p.X - (Size.X / 2), -p.X - (Size.X / 2));
            double y = Math.Max(p.Y - (Size.Y / 2), -p.Y - (Size.Y / 2));
            double z = Math.Max(p.Z - (Size.Z / 2), -p.Z - (Size.Z / 2));

            double d = x;
            d = Math.Max(d, y);
            d = Math.Max(d, z);
            return d;
        }

        private void AdjustOffset(double x, double y, double z, Bounds3D bounds)
        {
            Point3D p = new Point3D(x, y, z);
            p = RotatePoint(RotX, RotY, RotZ, p);
            bounds.Adjust(new Point3D(Position.X + p.X, Position.Y + p.Y, Position.Z + p.Z));
        }
    }
}