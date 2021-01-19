using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Make3D.Dialogs.MeshEditor
{
    public class Mesh
    {
        public List<MeshTriangle> Faces { get; set; }
        public List<MeshVertex> Vertices { get; set; }

        private DiffuseMaterial backMaterial;

        private DiffuseMaterial selectedFaceMaterial;

        private DiffuseMaterial unselectedFaceMaterial;

        private DiffuseMaterial unselectedPointMaterial;

        private DiffuseMaterial selectedPointMaterial;

        private Model3DGroup modelGroup;

        public bool ShowAllPoints { get; set; }

        public Mesh()
        {
            Faces = new List<MeshTriangle>();
            Vertices = new List<MeshVertex>();
            CreateMaterials();
            modelGroup = null;
            ShowAllPoints = false;
        }

        public void Clear()
        {
            Faces.Clear();
            Vertices.Clear();
        }

        private void CreateMaterials()
        {
            backMaterial = CreateMaterial(Brushes.Black);
            selectedFaceMaterial = CreateMaterial(Brushes.Yellow);
            unselectedFaceMaterial = CreateMaterial(Brushes.CornflowerBlue);
            selectedPointMaterial = CreateMaterial(Brushes.Red); ;
            unselectedPointMaterial = CreateMaterial(Brushes.DarkBlue);
        }

        private DiffuseMaterial CreateMaterial(SolidColorBrush brush)
        {
            DiffuseMaterial res = new DiffuseMaterial();
            res.Brush = brush;
            res.Color = brush.Color;
            return res;
        }

        internal int AddVertex(Point3D point3D)
        {
            MeshVertex vx = new MeshVertex();
            vx.Position = point3D;
            vx.BackMaterial = backMaterial;
            vx.SelectedMaterial = selectedPointMaterial;
            vx.UnselectedMaterial = unselectedPointMaterial;
            Vertices.Add(vx);
            vx.CreateModel();
            return Vertices.Count - 1;
        }

        internal void AddFace(int v1, int v2, int v3)
        {
            MeshTriangle f = new MeshTriangle();
            f.P0 = v1;
            f.P1 = v2;
            f.P2 = v3;
            f.CreateModel(Vertices);
            f.SelectedFrontMaterial = selectedFaceMaterial;
            f.UnselectedFrontMaterial = unselectedFaceMaterial;
            f.BackMaterial = backMaterial;
            f.Selected = false;
            Faces.Add(f);
        }

        internal void FindNeighbours()
        {
            foreach (MeshTriangle tri in Faces)
            {
                tri.NeighbourP0P1 = FindNeighbourTriangle(tri, tri.P0, tri.P1);
                tri.NeighbourP1P2 = FindNeighbourTriangle(tri, tri.P1, tri.P2);
                tri.NeighbourP2P0 = FindNeighbourTriangle(tri, tri.P2, tri.P0);
            }
        }

        private MeshTriangle FindNeighbourTriangle(MeshTriangle tri, int p0, int p1)
        {
            Dialogs.MeshEditor.MeshTriangle res = null;
            foreach (MeshTriangle nd in Faces)
            {
                if (tri != nd)
                {
                    if ((nd.P0 == p0 && nd.P1 == p1) ||
                        (nd.P0 == p1 && nd.P1 == p0))
                    {
                        res = nd;
                        break;
                    }

                    if ((nd.P1 == p0 && nd.P2 == p1) ||
                        (nd.P1 == p1 && nd.P2 == p0))
                    {
                        res = nd;
                        break;
                    }

                    if ((nd.P2 == p0 && nd.P0 == p1) ||
                        (nd.P0 == p1 && nd.P2 == p0))
                    {
                        res = nd;
                        break;
                    }
                }
            }
            return res;
        }

        internal Model3D GetModels()
        {
            if (modelGroup == null)
            {
                modelGroup = new Model3DGroup();
            }
            else
            {
                modelGroup.Children.Clear();
            }
            foreach (MeshTriangle tri in Faces)
            {
                modelGroup.Children.Add(tri.Model);
            }

            if (ShowAllPoints)
            {
                foreach (MeshVertex vx in Vertices)
                {
                    modelGroup.Children.Add(vx.Model);
                }
            }
            return modelGroup;
        }

        internal bool CheckHit(GeometryModel3D m, bool shift, ref int lastSelectedPoint, ref MeshTriangle lastSelectedTriangle)
        {
            bool found = false;
            lastSelectedPoint = -1;
            lastSelectedTriangle = null;

            foreach (MeshTriangle tri in Faces)
            {
                if (tri.CheckHit(m, shift))
                {
                    if (tri.Selected)
                    {
                        Vertices[tri.P0].Selected = true;
                        Vertices[tri.P1].Selected = true;
                        Vertices[tri.P2].Selected = true;
                        lastSelectedTriangle = tri;
                    }
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                foreach (MeshVertex vx in Vertices)
                {
                    if (vx.CheckHit(m, shift))
                    {
                        found = true;
                        lastSelectedPoint = Vertices.IndexOf(vx);
                        break;
                    }
                }
            }
            return found;
        }

        internal void SelectAll(bool v)
        {
            foreach (MeshVertex vx in Vertices)
            {
                vx.Selected = v;
            }
            foreach (MeshTriangle tri in Faces)
            {
                tri.Selected = v;
            }
        }

        internal void MovePoint(int pindex, Point3D positionChange)
        {
            if (pindex >= 0 && pindex < Vertices.Count)
            {
                Vertices[pindex].MovePosition(positionChange);

                foreach (MeshTriangle tri in Faces)
                {
                    tri.MovePoint(pindex, Vertices);
                }
            }
        }

        internal void Export(Point3DCollection vertices, Int32Collection faces)
        {
            foreach (MeshVertex vx in Vertices)
            {
                vertices.Add(vx.Position);
            }
            foreach (MeshTriangle tri in Faces)
            {
                faces.Add(tri.P0);
                faces.Add(tri.P1);
                faces.Add(tri.P2);
            }
        }

        internal void DivideSelectedFaces()
        {
            List<MeshTriangle> tmp = new List<MeshTriangle>();
            foreach (MeshTriangle tri in Faces)
            {
                if (tri.Selected == true)
                {
                    double cx = Vertices[tri.P0].Position.X + Vertices[tri.P1].Position.X + Vertices[tri.P2].Position.X;
                    double cy = Vertices[tri.P0].Position.Y + Vertices[tri.P1].Position.Y + Vertices[tri.P2].Position.Y;
                    double cz = Vertices[tri.P0].Position.Z + Vertices[tri.P1].Position.Z + Vertices[tri.P2].Position.Z;
                    cx = cx / 3.0;
                    cy = cy / 3.0;
                    cz = cz / 3.0;
                    Point3D np = new Point3D(cx, cy, cz);

                    int mp = AddVertex(np);
                    MeshTriangle nt = new MeshTriangle();
                    nt.P0 = tri.P0;
                    nt.P1 = tri.P1;
                    nt.P2 = mp;

                    nt.CreateModel(Vertices);
                    nt.SelectedFrontMaterial = selectedFaceMaterial;
                    nt.UnselectedFrontMaterial = unselectedFaceMaterial;
                    nt.BackMaterial = backMaterial;
                    nt.Selected = false;
                    tmp.Add(nt);

                    nt = new Dialogs.MeshEditor.MeshTriangle();
                    nt.P0 = tri.P1;
                    nt.P1 = tri.P2;
                    nt.P2 = mp;
                    nt.CreateModel(Vertices);
                    nt.SelectedFrontMaterial = selectedFaceMaterial;
                    nt.UnselectedFrontMaterial = unselectedFaceMaterial;
                    nt.BackMaterial = backMaterial;
                    nt.Selected = false;
                    tmp.Add(nt);

                    nt = new Dialogs.MeshEditor.MeshTriangle();
                    nt.P0 = tri.P2;
                    nt.P1 = tri.P0;
                    nt.P2 = mp;
                    nt.CreateModel(Vertices);
                    nt.SelectedFrontMaterial = selectedFaceMaterial;
                    nt.UnselectedFrontMaterial = unselectedFaceMaterial;
                    nt.BackMaterial = backMaterial;
                    nt.Selected = false;
                    tmp.Add(nt);
                }
                else
                {
                    tmp.Add(tri);
                }
            }
            Faces = tmp;
            FindNeighbours();
        }

        internal void MoveSelectedTriangles(Point3D positionChange)
        {
            // we have to do it this way to avoid moving the same point multiple times if it is
            // referenced in multiple triangles
            Int32Collection pnts = new Int32Collection();
            foreach (MeshTriangle f in Faces)
            {
                if (f.Selected)
                {
                    AddPoint(pnts, f.P0);
                    AddPoint(pnts, f.P1);
                    AddPoint(pnts, f.P2);
                }
            }

            foreach (int pindex in pnts)
            {
                Vertices[pindex].MovePosition(positionChange);

                foreach (MeshTriangle tri in Faces)
                {
                    tri.MovePoint(pindex, Vertices);
                }
            }
        }

        private void AddPoint(Int32Collection s, int v)
        {
            if (!s.Contains(v))
            {
                s.Add(v);
            }
        }
    }
}