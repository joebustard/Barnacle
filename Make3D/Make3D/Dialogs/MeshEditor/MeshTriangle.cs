using Barnacle.Models;
using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs.MeshEditor
{
    public class MeshTriangle
    {
        private const double em1 = 3.0 / 8.0;
        private const double em2 = 1.0 / 8.0;
        private Point3D midPoint;
        private GeometryModel3D model;
        private Point3D P0P1Mid;
        private Point3D P1P2Mid;
        private Point3D P2P0Mid;
        private bool selected;

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
            midPoint = new Point3D(0, 0, 0);
        }

        public DiffuseMaterial BackMaterial
        {
            get;
            set;
        }

        public GeometryModel3D Model
        {
            get
            {
                return model;
            }
            set
            {
                if (model != value)
                {
                    model = value;
                }
            }
        }

        public MeshTriangle NeighbourP0P1 { get; set; }
        public MeshTriangle NeighbourP1P2 { get; set; }
        public MeshTriangle NeighbourP2P0 { get; set; }
        public Vector3D Normal { get; set; }
        public int P0 { get; set; }
        public int P1 { get; set; }
        public int P2 { get; set; }
        public DiffuseMaterial PartSelectedFrontMaterial { get; internal set; }

        public bool Selected
        {
            get
            {
                return selected;
            }

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

        public DiffuseMaterial SelectedFrontMaterial
        {
            get;
            set;
        }

        public double SelectionWeight { get; set; }

        public object Tag { get; set; }

        public DiffuseMaterial UnselectedFrontMaterial
        {
            get;
            set;
        }

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

                    int dr = (bh.Color.R - bl.Color.R) * (int)SelectionWeight / 100;
                    int dg = (bh.Color.G - bl.Color.G) * (int)SelectionWeight / 100;
                    int db = (bh.Color.B - bl.Color.B) * (int)SelectionWeight / 100;
                    Color c = Color.FromRgb((byte)(bl.Color.R + dr),
                                            (byte)(bl.Color.G + dg),
                                            (byte)(bl.Color.B + db));
                    SolidColorBrush br = new SolidColorBrush();
                    br.Color = c;
                    DiffuseMaterial mt = new DiffuseMaterial();
                    mt.Color = c;
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

        public void SetNormal(List<MeshVertex> points)
        {
            Point3D v0 = points[P0].Position;
            Point3D v1 = points[P1].Position;
            Point3D v2 = points[P2].Position;
            Vector3D vector0 = new Vector3D(v1.X - v0.X, v1.Y - v0.Y, v1.Z - v0.Z);

            Vector3D vector1 = new Vector3D(v2.X - v1.X, v2.Y - v1.Y, v2.Z - v1.Z);

            Vector3D vn = Vector3D.CrossProduct(vector0, vector1);
            double l = vn.Length;
            Normal = new Vector3D(vn.X / l, vn.Y / l, vn.Z / l);
        }

        internal void CalculateEdgeSplitPoints(List<MeshVertex> vertices)
        {
            P0P1Mid = EdgeMid(vertices[P0], vertices[P1], midPoint, NeighbourP0P1.midPoint);
            P1P2Mid = EdgeMid(vertices[P1], vertices[P2], midPoint, NeighbourP1P2.midPoint);
            P2P0Mid = EdgeMid(vertices[P2], vertices[P0], midPoint, NeighbourP2P0.midPoint);
        }

        internal void CalculateMidPoint(List<MeshVertex> verts)
        {
            midPoint.X = (verts[P0].Position.X + verts[P1].Position.X + verts[P2].Position.X) / 3.0;
            midPoint.Y = (verts[P0].Position.Y + verts[P1].Position.Y + verts[P2].Position.Y) / 3.0;
            midPoint.Z = (verts[P0].Position.Z + verts[P1].Position.Z + verts[P2].Position.Z) / 3.0;
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

        internal void DereferencePoints(List<MeshVertex> vertices)
        {
            vertices[P0].NotUsedInTri(this);
            vertices[P1].NotUsedInTri(this);
            vertices[P2].NotUsedInTri(this);
        }

        internal void MakeVerticesReferToThis(List<MeshVertex> vrts)
        {
            vrts[P0].UsedInTri(this);
            vrts[P1].UsedInTri(this);
            vrts[P2].UsedInTri(this);
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
            SelectNeighbours(visited, 100);
        }

        internal void Subdivide(List<MeshVertex> vertices, List<MeshTriangle> faces, MeshOctTree octTree, List<MeshTriangle> needModels)
        {
           
            int ind0 = octTree.PointPresent(P0P1Mid);
            if (ind0 == -1)
            {
                MeshVertex mv1 = new MeshVertex();
                mv1.Position = P0P1Mid;
                ind0 = vertices.Count;
                octTree.AddPoint(ind0, mv1.Position);
                vertices.Add(mv1);
                mv1.CreateModel();
            }

            int ind1 = octTree.PointPresent(P1P2Mid);
            if (ind1 == -1)
            {
                MeshVertex mv2 = new MeshVertex();
                mv2.Position = P1P2Mid;
                ind1 = vertices.Count;
                octTree.AddPoint(ind1, mv2.Position);
                vertices.Add(mv2);
                mv2.CreateModel();
            }

            int ind2 = octTree.PointPresent(P2P0Mid);
            if (ind2 == -1)
            {
                MeshVertex mv3 = new MeshVertex();
                mv3.Position = P2P0Mid;
                ind2 = vertices.Count;
                octTree.AddPoint(ind2, mv3.Position);
                vertices.Add(mv3);
                mv3.CreateModel();
            }
            MeshTriangle tri1 = new MeshTriangle();
            tri1.P0 = this.P0;
            tri1.P1 = ind0;
            tri1.P2 = ind2;
       
            faces.Add(tri1);
            needModels.Add(tri1);
            tri1.MakeVerticesReferToThis(vertices);

            MeshTriangle tri2 = new MeshTriangle();
            tri2.P0 = ind0;
            tri2.P1 = this.P1;
            tri2.P2 = ind1;
            
            faces.Add(tri2);
            needModels.Add(tri2);
            tri2.MakeVerticesReferToThis(vertices);

            MeshTriangle tri3 = new MeshTriangle();
            tri3.P0 = ind1;
            tri3.P1 = this.P2;
            tri3.P2 = ind2;
            
            faces.Add(tri3);
            needModels.Add(tri3);
            tri3.MakeVerticesReferToThis(vertices);

            MeshTriangle tri4 = new MeshTriangle();
            tri4.P0 = ind0;
            tri4.P1 = ind1;
            tri4.P2 = ind2;
            faces.Add(tri4);
            
            needModels.Add(tri4);
            tri4.MakeVerticesReferToThis(vertices);

           SplitSideTriangle(faces, NeighbourP0P1, P0, P1, ind0, vertices, needModels, tri1, tri2);
           SplitSideTriangle(faces, NeighbourP1P2, P1, P2, ind1, vertices, needModels, tri2, tri3);
           SplitSideTriangle(faces, NeighbourP2P0, P2, P0, ind2, vertices, needModels, tri3, tri1);
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

        private Point3D EdgeMid(MeshVertex v1, MeshVertex v2, Point3D m1, Point3D m2)
        {
            Point3D res = new Point3D();
            res.X = em1 * (v1.Position.X + v2.Position.X) + em2 * (m1.X + m2.X);
            res.Y = em1 * (v1.Position.Y + v2.Position.Y) + em2 * (m1.Y + m2.Y);
            res.Z = em1 * (v1.Position.Z + v2.Position.Z) + em2 * (m1.Z + m2.Z);
            return res;
        }

        private void Log(string s)
        {
            System.Diagnostics.Debug.WriteLine(s);
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

        private void SplitSideTriangle(List<MeshTriangle> faces, MeshTriangle neighbour, int v0, int v1, int vn, List<MeshVertex> vertices, List<MeshTriangle> needModels, MeshTriangle tri1, MeshTriangle tri2)
        {
           
            // Only split the neighbour if its NOT selected, Yes thats right
            if (!neighbour.Selected)
            {
                int splitSide = -1;
                if ((neighbour.P0 == v0) && (neighbour.P1 == v1))
                {
                    splitSide = 0;
                }
                else
                if ((neighbour.P0 == v1) && (neighbour.P1 == v0))
                {
                    splitSide = 0;
                }
                else
                if ((neighbour.P1 == v0) && (neighbour.P2 == v1))
                {
                    splitSide = 1;
                }
                else
                if ((neighbour.P1 == v1) && (neighbour.P2 == v0))
                {
                    splitSide = 1;
                }
                else
                if ((neighbour.P2 == v0) && (neighbour.P0 == v1))
                {
                    splitSide = 2;
                }
                else
                if ((neighbour.P2 == v1) && (neighbour.P0 == v0))
                {
                    splitSide = 2;
                }

                // which side are we spliting
                if (splitSide == 0)
                {
                    MeshTriangle n1 = new MeshTriangle();
                    n1.P0 = neighbour.P0;
                    n1.P1 = vn;
                    n1.P2 = neighbour.P2;
                    faces.Add(n1);
                    needModels.Add(n1);
                    

                    MeshTriangle n2 = new MeshTriangle();
                    n2.P0 = vn;
                    n2.P1 = neighbour.P1;
                    n2.P2 = neighbour.P2;
                    faces.Add(n2);
                    needModels.Add(n2);
                    
                }
                else
                if (splitSide == 1)
                {
                    MeshTriangle n1 = new MeshTriangle();
                    n1.P0 = neighbour.P1;
                    n1.P1 = vn;
                    n1.P2 = neighbour.P0;
                    faces.Add(n1);
                    needModels.Add(n1);
                    

                    MeshTriangle n2 = new MeshTriangle();
                    n2.P0 = vn;
                    n2.P1 = neighbour.P2;
                    n2.P2 = neighbour.P0;
                    faces.Add(n2);
                    needModels.Add(n2);
                    
                }
                else
                if (splitSide == 2)
                {
                    MeshTriangle n1 = new MeshTriangle();
                    n1.P0 = neighbour.P2;
                    n1.P1 = vn;
                    n1.P2 = neighbour.P1;
                    faces.Add(n1);
                    needModels.Add(n1);
                    //    n1.NeighbourP2P0 = tri2;

                    MeshTriangle n2 = new MeshTriangle();
                    n2.P0 = vn;
                    n2.P1 = neighbour.P0;
                    n2.P2 = neighbour.P1;
                    faces.Add(n2);
                    needModels.Add(n2);
                    
                }
                else
                {
                    Log($"ERROR Cant work out which side to split");
                }

                // get rid of the neightbour as its not valid anymore
                neighbour.DereferencePoints(vertices);
                needModels.Remove(neighbour);
                faces.Remove(neighbour);
            }
            else
            {
                Log($"Ignore neighour is selected so will split himself");
            }
        }
    }
}