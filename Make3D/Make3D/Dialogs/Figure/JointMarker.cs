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

            if (Bone != null)
            {
                Bone.XRot += rot.X;
                Bone.YRot += rot.Y;
                Bone.ZRot += rot.Z;
                Dirty = true;
            }
        }
    }
}