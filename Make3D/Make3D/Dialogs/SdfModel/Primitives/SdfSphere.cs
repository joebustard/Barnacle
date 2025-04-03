using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs.SdfModel.Primitives
{
    internal class SdfSphere : SdfPrimitive
    {
        private double Radius = 10;
        private Point3D Zero;

        public SdfSphere()
        {
            Position = new Point3D(0, 0, 0);
            Zero = new Point3D(0, 0, 0);
        }

        public override String ToString()
        {
            return "Sphere";
        }

        internal override void AdjustBounds(Bounds3D bounds)
        {
            Radius = Size.X / 2;
            bounds.Adjust(new Point3D(Position.X - Radius, Position.Y - Radius, Position.Z - Radius));
            bounds.Adjust(new Point3D(Position.X + Radius, Position.Y + Radius, Position.Z + Radius));
        }

        internal override double GetSdfValue(Point3D xyz)
        {
            return BaseModellerDialog.Distance3D(Zero, xyz) - Radius;
        }
    }
}