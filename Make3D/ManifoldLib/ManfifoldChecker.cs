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
            if ((Indices != null) && (Indices.Count >= 3))
            {
                if ((Points != null) && (Points.Count >= 3))
                {
                    Vertices.Clear();
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

        public void RemoveDuplicateVertices()
        {
            Check();

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

            for (int i = 0; i < Vertices.Count; i++)
            {
                if (Vertices[i].NewNumber != Vertices[i].OriginalNumber)
                {
                    foreach (Face f in Faces)
                    {
                        foreach (Edge e in f.Edges)
                        {
                            if (e.P0 == Vertices[i].OriginalNumber)
                            {
                                e.P0 = Vertices[i].NewNumber;
                            }

                            if (e.P1 == Vertices[i].OriginalNumber)
                            {
                                e.P1 = Vertices[i].NewNumber;
                            }
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