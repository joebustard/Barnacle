using Make3D.Models;
using Make3D.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows.Media;
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
                if (selected == false)
                {
                    SelectionWeight = 0;
                }
                SetModelMaterials();

            }
        }

        public double SelectionWeight { get; set; }
        public void SetModelMaterials()
        {
            // don't try to update model materials if one of them isn't
            // defined yet
            if (model != null && SelectedFrontMaterial != null)
            {
                if (selected && SelectionWeight >= 100)
                {
                    model.Material = SelectedFrontMaterial;
                }
                else
                 if (selected && SelectionWeight < 100)
                {
                    //model.Material = PartSelectedFrontMaterial;
                    SolidColorBrush bh = SelectedFrontMaterial.Brush as SolidColorBrush;
                    SolidColorBrush bl = UnselectedFrontMaterial.Brush as SolidColorBrush;

                    int dr = (bh.Color.R - bl.Color.R) *(int)SelectionWeight /100  ;
                    int dg = (bh.Color.G - bl.Color.G) *(int)SelectionWeight  /100 ;
                    int db = (bh.Color.B - bl.Color.B) * (int)SelectionWeight / 100;
                    Color c = Color.FromRgb((byte)( bl.Color.R +dr),
                                            (byte)(bl.Color.G + dg),
                                            (byte)(bl.Color.B + db));
                    SolidColorBrush br = new SolidColorBrush();
                    br.Color = c;
                    DiffuseMaterial mt = new DiffuseMaterial();
                    mt.Color= c;
                    mt.Brush = br;
                    model.Material = mt;
                
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

        public DiffuseMaterial PartSelectedFrontMaterial { get; internal set; }

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

                res = true;
            }
            return res;
        }

        private void SelectNeighbours(List<MeshTriangle> visited, double v)
        {
            visited.Add(this);
            if (v > 1)
            {
                if (SelectionWeight < 100)
                {
                    SelectionWeight += v;
                }
                Selected = true;
                if (NeighbourP0P1 != null && !visited.Contains(NeighbourP0P1))
                {
                    NeighbourP0P1.SelectNeighbours(visited, SelectionWeight / 2);
                }

                if (NeighbourP1P2 != null && !visited.Contains(NeighbourP1P2))
                {
                    NeighbourP1P2.SelectNeighbours(visited, SelectionWeight / 2);
                }

                if (NeighbourP2P0 != null && !visited.Contains(NeighbourP2P0))
                {
                    NeighbourP2P0.SelectNeighbours(visited, SelectionWeight / 2);
                }
            }
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

        internal void SelectAllNeighbours()
        {
            List<MeshTriangle> visited = new List<MeshTriangle>();
            SelectNeighbours(visited,100);
        }
    }
}