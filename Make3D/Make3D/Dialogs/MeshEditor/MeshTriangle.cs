using Make3D.Models;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace Make3D.Dialogs.MeshEditor
{
    public class MeshTriangle
    {
        public int P0 { get; set; }
        public int P1 { get; set; }
        public int P2 { get; set; }

        public DiffuseMaterial SelectedFrontMaterial
        {
            get;
            set;
        }

        public DiffuseMaterial UnselectedFrontMaterial
        {
            get;
            set;
        }

        public DiffuseMaterial BackMaterial
        {
            get;
            set;
        }

        public MeshTriangle NeighbourP0P1 { get; set; }
        public MeshTriangle NeighbourP1P2 { get; set; }
        public MeshTriangle NeighbourP2P0 { get; set; }
        private bool selected;

        public bool Selected
        {
            get { return selected; }

            set
            {
                selected = value;
                SetModelMaterials();
            }
        }

        private void SetModelMaterials()
        {
            // don't try to update model materials if one of them isn't
            // defined yet
            if (model != null && SelectedFrontMaterial != null)
            {
                if (selected)
                {
                    model.Material = SelectedFrontMaterial;
                }
                else
                {
                    model.Material = UnselectedFrontMaterial;
                }
                model.BackMaterial = BackMaterial;
            }
        }

        public object Tag { get; set; }
        private GeometryModel3D model;

        public GeometryModel3D Model
        {
            get { return model; }
            set
            {
                if (model != value)
                {
                    model = value;
                }
            }
        }

        public MeshTriangle()
        {
            P0 = -1;
            P1 = -1;
            P2 = -1;
            NeighbourP0P1 = null;
            NeighbourP1P2 = null;
            NeighbourP2P0 = null;
            Selected = false;
            Tag = null;
            Model = null;
            SelectedFrontMaterial = null;
            UnselectedFrontMaterial = null;
            BackMaterial = null;
        }

        private int AddPoint(Point3DCollection positions, Point3D v)
        {
            int res = -1;
            for (int i = 0; i < positions.Count; i++)
            {
                if (PointUtils.equals(positions[i], v.X, v.Y, v.Z))
                {
                    res = i;
                    break;
                }
            }

            if (res == -1)
            {
                positions.Add(new Point3D(v.X, v.Y, v.Z));
                res = positions.Count - 1;
            }
            return res;
        }

        internal void CreateModel(System.Collections.Generic.List<MeshVertex> vertices)
        {
            model = new GeometryModel3D();
            MeshGeometry3D faces = new MeshGeometry3D();

            int v0 = AddPoint(faces.Positions, vertices[P0].Position);
            int v1 = AddPoint(faces.Positions, vertices[P1].Position);
            int v2 = AddPoint(faces.Positions, vertices[P2].Position);

            faces.TriangleIndices.Add(v0);
            faces.TriangleIndices.Add(v1);
            faces.TriangleIndices.Add(v2);

            model.Geometry = faces;
            Tag = model;
        }

        internal bool CheckHit(GeometryModel3D m, bool shift)
        {
            bool res = false;
            if (m == model)
            {
                if (shift == true)
                {
                    Selected = true;
                }
                else
                {
                    Selected = !Selected;
                }
                res = true;
            }
            return res;
        }

        internal void MovePoint(int pindex, List<MeshVertex> vertices)
        {
            if (pindex == P0)
            {
                (model.Geometry as MeshGeometry3D).Positions[0] = vertices[pindex].Position;
            }
            else
            if (pindex == P1)
            {
                (model.Geometry as MeshGeometry3D).Positions[1] = vertices[pindex].Position;
            }
            else
             if (pindex == P2)
            {
                (model.Geometry as MeshGeometry3D).Positions[2] = vertices[pindex].Position;
            }
        }
    }
}