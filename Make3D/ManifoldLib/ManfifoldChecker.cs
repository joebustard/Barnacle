using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ManifoldLib
{
    public class ManifoldChecker
    {
        public int NumberOfBadlyOrientatedEdges = 0;
        private List<Face> Faces;
        private VertexTreeNode treeRoot;
        private List<Vertex> Vertices;

        public ManifoldChecker()
        {
            Points = new Point3DCollection();
            Indices = null;
            Vertices = new List<Vertex>();
            Faces = new List<Face>();
        }

        public Int32Collection Indices { get; set; }
        public bool IsManifold { get; set; }
        public int NumbeOfUnconnectedFaces { get; set; }
        public int NumberOfDuplicatedVertices { get; set; }
        public int NumberOfNonExistentVertices { get; set; }
        public int NumberOfUnReferencedVertices { get; set; }
        public Point3DCollection Points { get; set; }

        public void Check()
        {
            IsManifold = false;
            NumberOfDuplicatedVertices = 0;
            NumberOfNonExistentVertices = 0;
            treeRoot = null;
            if ((Indices != null) && (Indices.Count >= 3))
            {
                if ((Points != null) && (Points.Count >= 3))
                {
                    Vertices.Clear();

                    // list may be already be sorted which would result in a very
                    // deep, thin tree and break the stack
                    // Doesn't hurt to insert in sections
                    if (Points.Count > 0 && Points.Count < 10)
                    {
                        for (int i = 0; i < Points.Count; i++)
                        {
                            AddPointToTree(i, Points[i]);
                        }
                    }
                    else
                    {
                        int low = 0;
                        int high = Points.Count - 1;
                        int mid = (high - low) / 2;
                        int mid1 = mid - 1;
                        int mid2 = mid + 1;
                        bool more = true;
                        bool[] used = new bool[Points.Count];
                        for (int i = 0; i < Points.Count; i++)
                        {
                            used[i] = false;
                        }
                        AddPointToTree(mid, Points[mid]);
                        used[mid] = true;
                        do
                        {
                            //System.Diagnostics.Debug.WriteLine($"{low},{mid1},{mid2},{high}");
                            if (low < Points.Count && !used[low])
                            {
                                AddPointToTree(low, Points[low]);
                                used[low] = true;
                            }

                            if (mid1 >= 0 && !used[mid1])
                            {
                                AddPointToTree(mid1, Points[mid1]);
                                used[mid1] = true;
                            }
                            if (high >= 0 && !used[high])
                            {
                                AddPointToTree(high, Points[high]);
                                used[high] = true;
                            }
                            if (mid2 <= Points.Count - 1 && !used[mid2])
                            {
                                AddPointToTree(mid2, Points[mid2]);
                                used[mid2] = true;
                            }
                            low++;
                            mid1--;
                            mid2++;
                            high--;
                            if (mid1 <= 0 && mid2 >= Points.Count)
                            {
                                more = false;
                            }
                        } while (more);
                    }

                    SortTree();

                    foreach (Point3D p in Points)
                    {
                        AddVertice(p);
                    }
                    if (NumberOfDuplicatedVertices == 0)
                    {
                        IsManifold = true;
                    }
                }

                Faces.Clear();
                for (int i = 0; i < Indices.Count; i += 3)
                {
                    Face f = new Face(Indices[i], Indices[i + 1], Indices[i + 2]);
                    Faces.Add(f);

                    UpdateVertexFaceReferences(Indices[i]);
                    UpdateVertexFaceReferences(Indices[i + 1]);
                    UpdateVertexFaceReferences(Indices[i + 2]);
                }
                NumberOfBadlyOrientatedEdges = 0;
                NumbeOfUnconnectedFaces = 0;

                foreach (Face f in Faces)
                {
                    f.CountSharedEdges(Faces);
                    NumberOfBadlyOrientatedEdges += f.NumberOfBadSharedEdges;
                    if (f.NumberOfGoodSharedEdges != 3)
                    {
                        NumbeOfUnconnectedFaces++;
                    }
                }

                NumberOfUnReferencedVertices = 0;
                foreach (Vertex v in Vertices)
                {
                    if (v.FaceReferencs == 0)
                    {
                        NumberOfUnReferencedVertices++;
                    }
                }
            }
        }

        public void RemoveDuplicateVertices()
        {
            IsManifold = false;
            NumberOfDuplicatedVertices = 0;
            treeRoot = null;
            if ((Indices != null) && (Indices.Count >= 3))
            {
                if ((Points != null) && (Points.Count >= 3))
                {
                    Vertices.Clear();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        if (Indices.Contains(i))
                        {
                            Vertex v = new Vertex();
                            v.Pos = new Point3D(Points[i].X, Points[i].Y, Points[i].Z);
                            v.OriginalNumber = i;
                            v.NewNumber = -1;
                            InsertVertice(v);
                        }
                    }

                    int dupof = 0;
                    int newid = 0;
                    Vertices[0].NewNumber = newid;
                    for (int i = 1; i < Vertices.Count; i++)
                    {
                        if (PointUtils.equals(Vertices[dupof].Pos, Vertices[i].Pos))
                        {
                            Vertices[i].DuplicateOf = Vertices[dupof].OriginalNumber;
                        }
                        else
                        {
                            dupof = i;
                            newid++;
                        }
                        Vertices[i].NewNumber = newid;
                    }

                    Faces.Clear();
                    for (int i = 0; i < Indices.Count; i += 3)
                    {
                        Face f = new Face(Indices[i], Indices[i + 1], Indices[i + 2]);
                        Faces.Add(f);
                    }

                    foreach (Face f in Faces)
                    {
                        foreach (Edge e in f.Edges)
                        {
                            for (int i = 0; i < Vertices.Count; i++)
                            {
                                if (Vertices[i].OriginalNumber == e.P0)
                                {
                                    e.NP0 = Vertices[i].NewNumber;
                                }
                                if (Vertices[i].OriginalNumber == e.P1)
                                {
                                    e.NP1 = Vertices[i].NewNumber;
                                }
                            }
                        }
                    }
                }

                VerticesToPoints();
            }
        }

        public void RemoveUnreferencedVertices()
        {
            if ((Indices != null) && (Indices.Count >= 3))
            {
                if ((Points != null) && (Points.Count >= 3))
                {
                    Vertices.Clear();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        if (Indices.Contains(i))
                        {
                            AddPointToTree(i, Points[i]);
                        }
                    }

                    Vertices = SortTree();

                    System.Diagnostics.Debug.WriteLine("Faces before");
                    Faces.Clear();
                    for (int i = 0; i < Indices.Count; i += 3)
                    {
                        Face f = new Face(Indices[i], Indices[i + 1], Indices[i + 2]);
                        Faces.Add(f);
                        f.Dump(Vertices);
                    }
                    int neo = 0;
                    for (int i = 0; i < Vertices.Count; i++)
                    {
                        if (Vertices[i].DuplicateOf == -1)
                        {
                            Vertices[i].NewNumber = neo;
                            neo++;
                        }
                        else
                        {
                            Vertices[i].NewNumber = -1;
                        }
                    }

                    System.Diagnostics.Debug.WriteLine("------");
                    for (int i = 0; i < Vertices.Count; i++)
                    {
                        Vertices[i].Dump();
                    }

                    foreach (Face f in Faces)
                    {
                        foreach (Edge e in f.Edges)
                        {
                            for (int i = 0; i < Vertices.Count; i++)
                            {
                                if (Vertices[i].OriginalNumber == e.P0 && Vertices[i].NewNumber != -1)
                                {
                                    e.NP0 = Vertices[i].NewNumber;
                                }
                                if (Vertices[i].OriginalNumber == e.P1 && Vertices[i].NewNumber != -1)
                                {
                                    e.NP1 = Vertices[i].NewNumber;
                                }
                            }
                        }
                    }
                    System.Diagnostics.Debug.WriteLine(" Creating tmp");
                    List<Vertex> tmp = new List<Vertex>();
                    for (int i = 0; i < Vertices.Count; i++)
                    {
                        if (Vertices[i].DuplicateOf == -1)
                        {
                            tmp.Add(Vertices[i]);
                            Vertices[i].Dump();
                        }
                    }
                    Vertices = tmp;
                    System.Diagnostics.Debug.WriteLine("Faces after");
                    foreach (Face f in Faces)
                    {
                        f.Dump(Vertices);
                    }
                }

                VerticesToPoints();
            }
        }

        private void AddPointToTree(int i, Point3D p)
        {
            Vertex v = new Vertex();
            v.Pos = new Point3D(p.X, p.Y, p.Z);
            v.DuplicateOf = -1;
            v.OriginalNumber = i;
            v.NewNumber = -1;

            treeRoot = VertexTreeNode.Add(v, treeRoot);
        }

        private void AddVertice(Point3D p)
        {
            int res = -1;

            if (Vertices.Count > 0 && Math.Abs(p.X - Vertices[Vertices.Count - 1].Pos.X) < 0.00001)
            {
                for (int i = 0; i < Vertices.Count; i++)
                {
                    if (PointUtils.equals(Vertices[i].Pos, p.X, p.Y, p.Z))
                    {
                        res = i;
                        break;
                    }
                }
            }
            // we always add the point even if its a duplicate
            Vertex v = new Vertex();
            v.Pos = new Point3D(p.X, p.Y, p.Z);
            v.DuplicateOf = res;
            v.OriginalNumber = Vertices.Count;
            v.NewNumber = Vertices.Count;
            Vertices.Add(v);

            if (res != -1)
            {
                v.NewNumber = res;
                NumberOfDuplicatedVertices++;
                IsManifold = false;
            }
        }

        private void InsertVertice(Vertex v)
        {
            if (Vertices.Count >= 3)
            {
                if (Vertices[0].GreaterOrEqual(v))
                {
                    Vertices.Insert(0, v);
                }
                else
                {
                    if (v.GreaterOrEqual(Vertices[Vertices.Count - 1]))
                    {
                        Vertices.Add(v);
                    }
                    else
                    {
                        InsertVertice(v, 0, Vertices.Count - 1);
                    }
                }
            }
            else
            if (Vertices.Count == 2)
            {
                if (Vertices[0].GreaterOrEqual(v))
                {
                    Vertices.Insert(0, v);
                }
                else
                {
                    if (v.GreaterOrEqual(Vertices[1]))
                    {
                        Vertices.Add(v);
                    }
                    else
                    {
                        Vertices.Insert(1, v);
                    }
                }
            }
            else
            if (Vertices.Count == 1)
            {
                if (Vertices[0].GreaterOrEqual(v))
                {
                    Vertices.Insert(0, v);
                }
                else
                {
                    Vertices.Add(v);
                }
            }
            else
            if (Vertices.Count == 0)
            {
                Vertices.Add(v);
            }
        }

        private void InsertVertice(Vertex v, int low, int high)
        {
            int mid = low + (high - low) / 2;
            if ((high - low) < 1000)
            {
                for (int i = low; i < high; i++)
                {
                    if (v.GreaterOrEqual(Vertices[i]) && (Vertices[i + 1].GreaterOrEqual(v)))
                    {
                        Vertices.Insert(i + 1, v);
                        break;
                    }
                }
            }
            else
            {
                if (v.GreaterOrEqual(Vertices[mid]))
                {
                    InsertVertice(v, mid, high);
                }
                else
                {
                    InsertVertice(v, low, mid);
                }
            }
        }

        private List<Vertex> SortTree()
        {
            const double tolerence = 1E-8;
            treeRoot.Sort();
            List<Vertex> tmp = new List<Vertex>();
            treeRoot.AddToList(tmp);
            for (int i = 0; i < tmp.Count - 1;)
            {
                if (i < tmp.Count - 1)
                {
                    Vertex v = tmp[i];
                    // tmp[i].NewNumber = i;
                    int j = i + 1;

                    bool more = true;
                    while (more)
                    {
                        more = false;
                        if (j < tmp.Count)
                        {
                            Vertex v2 = tmp[j];

                            if ((Math.Abs(v2.Pos.X - v.Pos.X) < tolerence) &&
                            (Math.Abs(v2.Pos.Y - v.Pos.Y) < tolerence) &&
                            (Math.Abs(v2.Pos.Z - v.Pos.Z) < tolerence))
                            {
                                // System.Diagnostics.Debug.WriteLine($"{v2.OriginalNumber}, {v2.Pos.X},{v2.Pos.Y},{v2.Pos.Z}");
                                // System.Diagnostics.Debug.WriteLine($" duplicate of ${i}");
                                more = true;
                                tmp[j].DuplicateOf = i;
                                //   tmp[j].NewNumber = j;
                                j++;
                                NumberOfDuplicatedVertices++;
                            }
                        }
                    }
                    i = j;
                    // System.Diagnostics.Debug.WriteLine($"{v.OriginalNumber}, {v.Pos.X},{v.Pos.Y},{v.Pos.Z}");
                }
            }
            /*
            foreach (Vertex ve in tmp)
            {
                ve.Dump();
            }
            */
            return tmp;
        }

        private void UpdateVertexFaceReferences(int vi)
        {
            if (vi >= 0 && vi < Vertices.Count)
            {
                Vertices[vi].FaceReferencs++;
            }
            else
            {
                NumberOfNonExistentVertices++;
            }
        }

        private void VerticesToPoints()
        {
            Points.Clear();
            foreach (Vertex v in Vertices)
            {
                if (v.DuplicateOf == -1)
                {
                    Points.Add(v.Pos);
                }
            }
            Indices.Clear();
            foreach (Face f in Faces)
            {
                foreach (Edge e in f.Edges)
                {
                    Indices.Add(e.NP0);
                }
            }
        }
    }
}