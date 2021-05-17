using Make3D.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Make3D.Dialogs.Figure
{
    internal class JointMarker : Object3D
    {
        public JointMarker()
        {
            Bone = null;
            Dirty = false;
        }

        public Bone Bone { get; set; }
        public bool Dirty { get; set; }

        public override void Rotate(Point3D rot)
        {
            System.Diagnostics.Debug.WriteLine($"ad rot {rot.X},{rot.Y},{rot.Z}");

            double r1 = rot.X;
            double r2 = rot.Y;
            double r3 = rot.Z;
            if (Bone != null)
            {
                Bone.XRot += r1;
                Bone.YRot += r2;
                Bone.ZRot += r3;
                Dirty = true;
            }
        }
    }
}