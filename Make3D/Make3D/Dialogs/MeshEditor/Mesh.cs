using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs.MeshEditor
{
    public class Mesh
    {
        private DiffuseMaterial backMaterial;
        private Bounds3D bounds;
        private Model3DGroup modelGroup;
        private MeshOctTree octTree;
        private DiffuseMaterial partSelectedFaceMaterial;
        private DiffuseMaterial selectedFaceMaterial;
        private List<double> selectedForces;
        private DiffuseMaterial selectedPointMaterial;
        private Int32Collection selectedPoints;
        private DiffuseMaterial unselectedFaceMaterial;
        private DiffuseMaterial unselectedPointMaterial;

        public Mesh()
        {
            Faces = new List<MeshTriangle>();
            Vertices = new List<MeshVertex>();
            CreateMaterials();
            modelGroup = null;
            ShowAllPoints = false;
            bounds = new Bounds3D();
            selectedPoints = new Int32Collection();
            selectedForces = new List<double>();
        }

        public List<MeshTriangle> Faces { get; set; }
        public bool ShowAllPoints { get; set; }
        public List<MeshVertex> Vertices { get; set; }

        public void Clear()
        {
            Faces.Clear();
            Vertices.Clear();
            bounds = new Bounds3D();
        }

        internal void AddFace(int v1, int v2, int v3)
        {
            MeshTriangle f = new MeshTriangle();
            f.P0 = v1;
            f.P1 = v2;
            f.P2 = v3;
            f.CreateModel(Vertices);
            f.SelectedFrontMaterial = selectedFaceMaterial;
            f.PartSelectedFrontMaterial = partSelectedFaceMaterial;
            f.UnselectedFrontMaterial = unselectedFaceMaterial;
            f.BackMaterial = backMaterial;
            f.Selected = false;
            f.SetModelMaterials();
            Faces.Add(f);
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
            bounds.Adjust(point3D);
            return Vertices.Count - 1;
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
                    if (shift == true)
                    {
                        tri.Selected = true;
                        tri.SelectionWeight = 100;
                        tri.SetModelMaterials();
                        Vertices[tri.P0].Selected = true;
                        Vertices[tri.P1].Selected = true;
                        Vertices[tri.P2].Selected = true;
                        lastSelectedTriangle = tri;
                    }
                    else
                    {
                        if (tri.Selected == false)
                        {
                            DeselectAll();
                            tri.SelectionWeight = 100;
                            tri.Selected = true;
                            tri.SetModelMaterials();
                            Vertices[tri.P0].Selected = true;
                            Vertices[tri.P1].Selected = true;
                            Vertices[tri.P2].Selected = true;
                            lastSelectedTriangle = tri;
                        }
                        else
                        {
                            lastSelectedTriangle = tri;
                            tri.SelectAllNeighbours();
                        }
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
                        DeselectAll();
                        lastSelectedPoint = Vertices.IndexOf(vx);
                        vx.Selected = true;
                        break;
                    }
                }
            }
            return found;
        }

        internal void DivideLongSideSelectedFaces()
        {
            List<MeshTriangle> tmp = new List<MeshTriangle>();
            List<MeshTriangle> skip = new List<MeshTriangle>();

            Point3D p0;
            Point3D p1;
            Point3D p2;
            Point3D splitPoint;
            MeshTriangle neighbour = null;
            foreach (MeshTriangle tri in Faces)
            {
                if (tri.Selected == true)
                {
                    p0 = Vertices[tri.P0].Position;
                    p1 = Vertices[tri.P1].Position;
                    p2 = Vertices[tri.P2].Position;

                    double d0 = Dist3D(p0, p1);
                    double d1 = Dist3D(p1, p2);
                    double d2 = Dist3D(p2, p0);
                    int splitting = 0;
                    double longest = d0;
                    splitPoint = Midpoint(p0, p1);
                    neighbour = tri.NeighbourP0P1;
                    if (d1 > longest)
                    {
                        splitting = 1;
                        longest = d1;
                        splitPoint = Midpoint(p1, p2);
                        neighbour = tri.NeighbourP1P2;
                    }

                    if (d2 > longest)
                    {
                        splitting = 2;
                        longest = d2;
                        splitPoint = Midpoint(p2, p0);
                        neighbour = tri.NeighbourP2P0;
                    }

                    int mp = AddVertex(splitPoint);
                    if (splitting == 0)
                    {
                        MeshTriangle nt = new MeshTriangle();
                        nt.P0 = tri.P0;
                        nt.P1 = mp;
                        nt.P2 = tri.P2;

                        SetupNewFace(nt);
                        tmp.Add(nt);

                        nt = new Dialogs.MeshEditor.MeshTriangle();
                        nt.P0 = mp;
                        nt.P1 = tri.P1;
                        nt.P2 = tri.P2;
                        SetupNewFace(nt);
                        tmp.Add(nt);
                    }

                    if (splitting == 1)
                    {
                        MeshTriangle nt = new MeshTriangle();
                        nt.P0 = tri.P0;
                        nt.P1 = tri.P1;
                        nt.P2 = mp;

                        SetupNewFace(nt);
                        tmp.Add(nt);

                        nt = new Dialogs.MeshEditor.MeshTriangle();
                        nt.P0 = tri.P0;
                        nt.P1 = mp;
                        nt.P2 = tri.P2;
                        SetupNewFace(nt);
                        tmp.Add(nt);
                    }
                    if (splitting == 2)
                    {
                        MeshTriangle nt = new MeshTriangle();
                        nt.P0 = tri.P0;
                        nt.P1 = tri.P1;
                        nt.P2 = mp;

                        SetupNewFace(nt);
                        tmp.Add(nt);

                        nt = new Dialogs.MeshEditor.MeshTriangle();
                        nt.P0 = tri.P1;
                        nt.P1 = tri.P2;
                        nt.P2 = mp;
                        SetupNewFace(nt);
                        tmp.Add(nt);
                    }

                    int neighbourSplitting = 0;
                    if (neighbour != null)
                    {
                        if (neighbour.NeighbourP1P2 == tri)
                        {
                            neighbourSplitting = 1;
                        }
                        if (neighbour.NeighbourP2P0 == tri)
                        {
                            neighbourSplitting = 2;
                        }

                        if (neighbourSplitting == 0)
                        {
                            MeshTriangle nt = new MeshTriangle();
                            nt.P0 = neighbour.P0;
                            nt.P1 = mp;
                            nt.P2 = neighbour.P2;

                            SetupNewFace(nt);
                            tmp.Add(nt);

                            nt = new Dialogs.MeshEditor.MeshTriangle();
                            nt.P0 = mp;
                            nt.P1 = neighbour.P1;
                            nt.P2 = neighbour.P2;
                            SetupNewFace(nt);
                            tmp.Add(nt);
                        }

                        if (neighbourSplitting == 1)
                        {
                            MeshTriangle nt = new MeshTriangle();
                            nt.P0 = neighbour.P0;
                            nt.P1 = neighbour.P1;
                            nt.P2 = mp;

                            SetupNewFace(nt);
                            tmp.Add(nt);

                            nt = new Dialogs.MeshEditor.MeshTriangle();
                            nt.P0 = neighbour.P0;
                            nt.P1 = mp;
                            nt.P2 = neighbour.P2;
                            SetupNewFace(nt);
                            tmp.Add(nt);
                        }
                        if (neighbourSplitting == 2)
                        {
                            MeshTriangle nt = new MeshTriangle();
                            nt.P0 = neighbour.P0;
                            nt.P1 = neighbour.P1;
                            nt.P2 = mp;

                            SetupNewFace(nt);
                            tmp.Add(nt);

                            nt = new Dialogs.MeshEditor.MeshTriangle();
                            nt.P0 = neighbour.P1;
                            nt.P1 = neighbour.P2;
                            nt.P2 = mp;
                            SetupNewFace(nt);
                            tmp.Add(nt);
                        }
                        neighbour.Selected = false;
                        skip.Add(neighbour);
                    }
                }
                else
                {
                    tmp.Add(tri);
                }
            }
            foreach (MeshTriangle f in skip)
            {
                if (tmp.Contains(f))
                {
                    tmp.Remove(f);
                }
            }
            Faces = tmp;
            foreach (MeshTriangle tri in Faces)
            {
                tri.NeighbourP0P1 = null;
                tri.NeighbourP1P2 = null;
                tri.NeighbourP2P0 = null;
            }
            FindNeighbours();
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
                    nt.PartSelectedFrontMaterial = partSelectedFaceMaterial;
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
                    nt.PartSelectedFrontMaterial = partSelectedFaceMaterial;
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
                    nt.PartSelectedFrontMaterial = partSelectedFaceMaterial;
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

        internal void FindNeighbours()
        {
            foreach (MeshTriangle tri in Faces)
            {
                if (tri.NeighbourP0P1 == null)
                {
                    tri.NeighbourP0P1 = FindNeighbourTriangle(tri, tri.P0, tri.P1);
                }
                if (tri.NeighbourP1P2 == null)
                {
                    tri.NeighbourP1P2 = FindNeighbourTriangle(tri, tri.P1, tri.P2);
                }
                if (tri.NeighbourP2P0 == null)
                {
                    tri.NeighbourP2P0 = FindNeighbourTriangle(tri, tri.P2, tri.P0);
                }
            }
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
            else
            {
                List<MeshVertex> showers = new List<MeshVertex>();
                foreach (MeshTriangle tri in Faces)
                {
                    if (tri.Selected)
                    {
                        MeshVertex vx = Vertices[tri.P0];
                        if (!showers.Contains(vx))
                        {
                            showers.Add(vx);
                        }
                        vx = Vertices[tri.P1];
                        if (!showers.Contains(vx))
                        {
                            showers.Add(vx);
                        }
                        vx = Vertices[tri.P2];
                        if (!showers.Contains(vx))
                        {
                            showers.Add(vx);
                        }
                    }
                }
                foreach (MeshVertex vx in showers)
                {
                    modelGroup.Children.Add(vx.Model);
                }

                // diagnostics test
                foreach (MeshVertex vx in Vertices)
                {
                    if (vx.Selected)
                    {
                        modelGroup.Children.Add(vx.Model);
                    }
                }
            }
            return modelGroup;
        }

        internal void Initialise()
        {
            FindNeighbours();
            LinkPointsToTriangles();
            octTree = new MeshOctTree(Vertices, bounds.Lower, bounds.Upper, 6);
            // OctNode nd = octTree.FindNodeAround(pnts[4]);
        }

        internal void LinkPointsToTriangles()
        {
            for (int i = 0; i < Faces.Count; i++)

            {
                MeshTriangle tri = Faces[i];
                tri.SetNormal(Vertices);
                tri.MakeVerticesReferToThis(Vertices);
                // while you are at it calculate the midpoint
                tri.CalculateMidPoint(Vertices);
            }
        }

        internal void MoveControlPoints()
        {
            Log($"Move ControlPoints Starting with ${Vertices.Count} vertices");
            List<MeshTriangle> trisToMove = new List<MeshTriangle>();
            for (int i = 0; i < selectedPoints.Count; i++)
            {
                // create a normal for the point that is the average
                // of all the normals of triangles that refer to it
                Vector3D pointNormal = new Vector3D(0, 0, 0);
                int pIndex = selectedPoints[i];
                Log($"Move Control Point {pIndex}");
                // look at every triangle that we think refers to this point
                foreach (MeshTriangle tri in Vertices[pIndex].UsedInTriangles)
                {
                    if (!trisToMove.Contains(tri))
                    {
                        trisToMove.Add(tri);
                        pointNormal += tri.Normal;
                    }
                }
                double len = pointNormal.Length;
                if (len > 0)
                {
                    pointNormal.X /= len;
                    pointNormal.Y /= len;
                    pointNormal.Z /= len;
                    double f = selectedForces[i];
                    MovePoint(pIndex, new Point3D(pointNormal.X * f, pointNormal.Y * f, pointNormal.Z * f));
                }
            }
            Log($"Smoothing {trisToMove.Count} triangles");
            SmoothTriangles(trisToMove);
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

        internal void MoveSelectedTriangles(Point3D positionChange)
        {
            // we have to do it this way to avoid moving the same point multiple times if it is
            // referenced in multiple triangles
            List<MeshTriangle> movingTris = new List<MeshTriangle>();
            foreach (MeshTriangle f in Faces)
            {
                if (f.Selected)
                {
                    movingTris.Add(f);
                }
            }

            // sort by selection weight, highest first
            bool swapped = false;
            do
            {
                swapped = false;
                for (int i = 0; i < movingTris.Count - 1; i++)
                {
                    if (movingTris[i].SelectionWeight < movingTris[i + 1].SelectionWeight)
                    {
                        MeshTriangle tmp = movingTris[i];
                        movingTris[i] = movingTris[i + 1];
                        movingTris[i + 1] = tmp;
                        swapped = true;
                    }
                }
            } while (swapped == true);

            Int32Collection pnts = new Int32Collection();
            foreach (MeshTriangle f in movingTris)
            {
                double scale = f.SelectionWeight / 100.0;
                Point3D delta = new Point3D(positionChange.X * scale,
                                            positionChange.Y * scale,
                                            positionChange.Z * scale);
                if (!pnts.Contains(f.P0))
                {
                    AddPoint(pnts, f.P0);
                    Vertices[f.P0].MovePosition(delta);
                }
                if (!pnts.Contains(f.P1))
                {
                    AddPoint(pnts, f.P1);
                    Vertices[f.P1].MovePosition(delta);
                }
                if (!pnts.Contains(f.P2))
                {
                    AddPoint(pnts, f.P2);
                    Vertices[f.P2].MovePosition(delta);
                }
            }

            foreach (int pindex in pnts)
            {
                foreach (MeshTriangle tri in Faces)
                {
                    tri.MovePoint(pindex, Vertices);
                }
            }
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
                if (v)
                {
                    tri.SelectionWeight = 100;
                }
                else
                {
                    tri.SelectionWeight = 0;
                }
            }
        }

        internal bool SelectToolPoints(SculptingTool tool, Point3D pos)
        {
            double radius = tool.Radius;
            DeselectAll();
            octTree.FindPointsInRadius(radius, pos, selectedPoints, selectedForces);

            // diagnostic, just select all the found points

            for (int i = 0; i < selectedPoints.Count; i++)
            {
                int ind = selectedPoints[i];
                Vertices[ind].Selected = true;
                // forces are currently just distance from tool centre
                selectedForces[i] = tool.Force(selectedForces[i]);
            }
            return selectedPoints.Count > 0;
        }

        private void AddPoint(Int32Collection s, int v)
        {
            if (!s.Contains(v))
            {
                s.Add(v);
            }
        }

        private DiffuseMaterial CreateMaterial(SolidColorBrush brush)
        {
            DiffuseMaterial res = new DiffuseMaterial();
            res.Brush = brush;
            res.Color = brush.Color;
            return res;
        }

        private void CreateMaterials()
        {
            backMaterial = CreateMaterial(Brushes.Black);
            selectedFaceMaterial = CreateMaterial(Brushes.LightYellow);
            partSelectedFaceMaterial = CreateMaterial(Brushes.Yellow);
            unselectedFaceMaterial = CreateMaterial(Brushes.CornflowerBlue);
            selectedPointMaterial = CreateMaterial(Brushes.Red); ;
            unselectedPointMaterial = CreateMaterial(Brushes.DarkBlue);
        }

        private void DeselectAll()
        {
            foreach (MeshTriangle f in Faces)
            {
                f.SelectionWeight = 0;
                f.Selected = false;
            }
            foreach (MeshVertex mv in Vertices)
            {
                mv.Selected = false;
            }
            selectedForces.Clear();
            selectedPoints.Clear();
        }

        private double Dist3D(Point3D p0, Point3D p1)
        {
            double dx = p1.X - p0.X;
            double dy = p1.Y - p0.Y;
            double dz = p1.Z - p0.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        private MeshTriangle FindNeighbourTriangle(MeshTriangle tri, int p0, int p1)
        {
            Dialogs.MeshEditor.MeshTriangle res = null;
            foreach (MeshTriangle nd in Faces)
            {
                if (tri != nd)
                {
                    if (nd.NeighbourP0P1 == null)
                    {
                        if ((nd.P0 == p0 && nd.P1 == p1) ||
                            (nd.P0 == p1 && nd.P1 == p0))
                        {
                            res = nd;
                            nd.NeighbourP0P1 = tri;
                            break;
                        }
                    }

                    if (nd.NeighbourP1P2 == null)
                    {
                        if ((nd.P1 == p0 && nd.P2 == p1) ||
                            (nd.P1 == p1 && nd.P2 == p0))
                        {
                            res = nd;
                            nd.NeighbourP1P2 = tri;
                            break;
                        }
                    }

                    if (nd.NeighbourP2P0 == null)
                    {
                        if ((nd.P2 == p0 && nd.P0 == p1) ||
                            (nd.P2 == p1 && nd.P0 == p0))
                        {
                            res = nd;
                            nd.NeighbourP2P0 = tri;
                            break;
                        }
                    }
                }
            }
            if (res == null)
            {
                System.Diagnostics.Debug.WriteLine($"Cant find neighbour of side {p0} to {p1}");
            }
            return res;
        }

        private void Log(string s)
        {
            System.Diagnostics.Debug.WriteLine(s);
        }

        private Point3D Midpoint(Point3D p0, Point3D p1)
        {
            double dx = p1.X - p0.X;
            double dy = p1.Y - p0.Y;
            double dz = p1.Z - p0.Z;
            Point3D res = new Point3D(p0.X + dx / 2, p0.Y + dy / 2, p0.Z + dz / 2);
            return res;
        }

        private void SetupNewFace(MeshTriangle nt)
        {
            nt.CreateModel(Vertices);
            nt.SelectedFrontMaterial = selectedFaceMaterial;
            nt.UnselectedFrontMaterial = unselectedFaceMaterial;
            nt.BackMaterial = backMaterial;
            nt.PartSelectedFrontMaterial = partSelectedFaceMaterial;
            nt.Selected = false;
        }

        private void SmoothTriangles(List<MeshTriangle> trisToSmooth)
        {
            foreach (MeshTriangle mt in Faces)
            {
                if (trisToSmooth.Contains(mt))
                {
                    mt.Selected = true;
                }
                else
                {
                    mt.Selected = false;
                }
            }
            // recalculate the mid points but only for the ones we are going to smooth
            // Others keep the same midpoints they had last time
            // Cant calculate the edge split points until then
            foreach (MeshTriangle mt in trisToSmooth)
            {
                mt.CalculateMidPoint(Vertices);
            }
            List<MeshTriangle> needModels = new List<MeshTriangle>();

            foreach (MeshTriangle mt in trisToSmooth)
            {
                mt.CalculateEdgeSplitPoints(Vertices);
            }
            foreach (MeshTriangle mt in trisToSmooth)
            {
                // Split this face into four new
                mt.Subdivide(Vertices, Faces, octTree, needModels);

                // remove the original, first stop any Vertices pointing at this face
                mt.DereferencePoints(Vertices);

                // remove face from main list
                Faces.Remove(mt);
            }

            foreach (MeshTriangle tri in Faces)
            {
                tri.NeighbourP0P1 = null;
                tri.NeighbourP1P2 = null;
                tri.NeighbourP2P0 = null;
            }
            FindNeighbours();

            foreach (MeshTriangle mt in needModels)
            {
                mt.Selected = false;
                SetupNewFace(mt);
            }
        }

        public void DiagnosticSplitSelectedFaces()
        {
            List<MeshTriangle> trisToSmooth = new List<MeshTriangle>();
            foreach (MeshTriangle t in Faces)
            {
                if (t.Selected)

                {
                    trisToSmooth.Add(t);
                }
            }
            SmoothTriangles(trisToSmooth);
        }
    }
}