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

        public override void Rotate(Point3D RotateBy)
        {
            //double r1 = DegreesToRad(RotateBy.Y);
            //double r2 = DegreesToRad(RotateBy.Z);
            //double r3 = DegreesToRad(RotateBy.X);
            double r1 = RotateBy.Z;
            double r2 = RotateBy.Y;
            double r3 = RotateBy.X;
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