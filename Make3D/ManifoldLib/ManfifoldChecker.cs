using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ManifoldLib
{
    public class ManifoldChecker
    {
        public Point3DCollection Points { get; set; }
        public Int32Collection Indices { get; set; }
        public bool IsManifold { get; set; }
        public int NumberOfDuplicatedVertices { get; set; }
        public int NumberOfBadlyOrientatedEdges = 0;
        public int NumbeOfUnconnectedFaces { get; set; }
        private List<Vertex> Vertices;
        private List<Face> Faces;
        private VertexTreeNode treeRoot;

        public ManifoldChecker()
        {
            Points = null;
            Indices = null;
            Vertices = new List<Vertex>();
            Faces = new List<Face>();
        }

        public void Check()
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
                        AddPointToTree(i, Points[i]);
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
                    tmp[i].NewNumber = i;
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
                                tmp[j].NewNumber = j;
                                j++;
                                NumberOfDuplicatedVertices++;
                            }
                        }
                    }
                    i = j;
                    // System.Diagnostics.Debug.WriteLine($"{v.OriginalNumber}, {v.Pos.X},{v.Pos.Y},{v.Pos.Z}");
                }
            }
            return tmp;
        }

        private void AddPointToTree(int i, Point3D p)
        {
            Vertex v = new Vertex();
            v.Pos = new Point3D(p.X, p.Y, p.Z);
            v.DuplicateOf = -1;
            v.OriginalNumber = i;
            v.NewNumber = i;
            if (treeRoot == null)
            {
                treeRoot = new VertexTreeNode(v);
            }
            else
            {
                treeRoot.Add(v);
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
                        AddPointToTree(i, Points[i]);
                    }
                    Vertices = SortTree();
                }

                Faces.Clear();
                for (int i = 0; i < Indices.Count; i += 3)
                {
                    Face f = new Face(Indices[i], Indices[i + 1], Indices[i + 2]);
                    Faces.Add(f);
                }

                int removed = 0;
                for (int i = 0; i < Vertices.Count; i++)
                {
                    if (Vertices[i].DuplicateOf != -1)
                    {
                        removed++;
                    }
                    else
                    {
                        Vertices[i].NewNumber -= removed;
                    }
                }

                foreach (Face f in Faces)
                {
                    foreach (Edge e in f.Edges)
                    {
                        if (Vertices[e.P0].DuplicateOf != -1)
                        {
                            e.P0 = Vertices[e.P0].NewNumber;
                        }

                        if (Vertices[e.P1].DuplicateOf != -1)
                        {
                            e.P1 = Vertices[e.P1].NewNumber;
                        }
                    }
                }
            }

            List<Vertex> tmp = new List<Vertex>();
            for (int i = 0; i < Vertices.Count; i++)
            {
                if (Vertices[i].DuplicateOf == -1)
                {
                    tmp.Add(Vertices[i]);
                }
            }
            Vertices = tmp;
            VerticesToPoints();
        }

        private void VerticesToPoints()
        {
            Points.Clear();
            foreach (Vertex v in Vertices)
            {
                Points.Add(v.Pos);
            }
            Indices.Clear();
            foreach (Face f in Faces)
            {
                foreach (Edge e in f.Edges)
                {
                    Indices.Add(e.P0);
                }
            }
        }

        private void AddVertice(Point3D p)
        {
            int res = -1;

            for (int i = 0; i < Vertices.Count; i++)
            {
                if (PointUtils.equals(Vertices[i].Pos, p.X, p.Y, p.Z))
                {
                    res = i;
                    break;
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
    }
}