using Barnacle.Object3DLib;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs.Figure
{
    internal class JointMarker : Object3D
    {
        public delegate void BoneRotate(Bone bn);

        public BoneRotate OnBoneRotated { get; set; }

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
                if (OnBoneRotated != null)
                {
                    OnBoneRotated(Bone);
                }
                Dirty = true;
            }
        }
    }
}