using MeshDecimator.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs.SdfModel.Primitives
{
    internal class SdfCylinder : SdfPrimitive
    {
        public SdfCylinder()
        {
            Size = new Vector3D(20, 20, 20);
            Position = new Point3D(0, 0, 0);
        }

        public override String ToString()
        {
            return "Cylinder";
        }

        internal override double GetSdfValue(Point3D p)
        {
            if (Rotation != null)
            {
                p = Rotate(p);
            }
            double r = Size.X / 2;
            double h = Size.Y;
            Vector2d v1 = new Vector2d(p.X, p.Z);

            // Calculate the distance from the point to the circular base (xz plane)
            double dist = v1.Magnitude - r;

            // Calculate the distance from the point to the top/bottom caps (y axis)
            double capDist = Math.Abs(p.Y) - (h / 2);

            // Determine if the point is inside or outside
            double result = Math.Max(dist, capDist);

            return result;
        }
    }
}