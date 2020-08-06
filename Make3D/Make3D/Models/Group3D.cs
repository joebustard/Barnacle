using Net3dBool;
using System.Windows.Media.Media3D;

namespace Make3D.Models
{
    public class Group3D : Object3D
    {
        private BooleanModeller modeller;
        private Object3D leftObject;

        public Object3D LeftObject
        {
            get { return leftObject; }
            set
            {
                if (leftObject != value)
                {
                    leftObject = value;
                }
            }
        }

        private Object3D rightObject;

        public Object3D RightObject
        {
            get { return rightObject; }
            set
            {
                if (rightObject != value)
                {
                    rightObject = value;
                }
            }
        }

        private Solid leftSolid;
        private Solid rightSolid;

        public Group3D()
        {
            leftObject = null;
            leftSolid = null;

            rightObject = null;
            rightSolid = null;
        }

        internal void Init()
        {
            if (leftObject != null && rightObject != null)
            {
                Position = new Point3D(leftObject.Position.X, leftObject.Position.Y, leftObject.Position.Z);
                Scale = new Scale3D(leftObject.Scale.X + rightObject.Scale.X,
                leftObject.Scale.Y + rightObject.Scale.Y,
                leftObject.Scale.Z + rightObject.Scale.Z);
                Color = leftObject.Color;
            }
        }

        internal override void Remesh()
        {
            absoluteBounds = new Bounds3D();
            leftObject.RelativeToAbsolute();
            rightObject.RelativeToAbsolute();
            leftSolid = new Solid(leftObject.AbsoluteObjectVertices, leftObject.TriangleIndices);
            rightSolid = new Solid(rightObject.AbsoluteObjectVertices, rightObject.TriangleIndices);
            modeller = new BooleanModeller(leftSolid, rightSolid);
            Solid result = null;
            switch (PrimType)
            {
                case "groupunion":
                    {
                        result = modeller.GetUnion();
                    }
                    break;
            }

            if (result != null)
            {
                AbsoluteObjectVertices = new Point3DCollection();
                TriangleIndices = new System.Windows.Media.Int32Collection();
                Vector3D[] vc = result.GetVertices();
                foreach (Vector3D v in vc)
                {
                    Point3D p = new Point3D(v.X, v.Y, v.Z);
                    AbsoluteObjectVertices.Add(p);
                    absoluteBounds.Adjust(p);
                }
                int[] ids = result.GetIndices();
                for (int i = 0; i < ids.Length; i++)
                {
                    TriangleIndices.Add(ids[i]);
                }
                SetMesh();
            }
        }
    }
}