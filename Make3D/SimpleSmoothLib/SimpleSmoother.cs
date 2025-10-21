using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace SimpleSmoothLib
{
    public class SimpleSmoother
    {
        public Int32Collection[] Neighbours;
        private VertexAverage[] averagePoints;
        private Int32Collection faces;
        private Point3DCollection vertices;

        public SimpleSmoother(Point3DCollection v, Int32Collection f)
        {
            vertices = v;
            faces = f;
            averagePoints = new VertexAverage[vertices.Count];
            Neighbours = new Int32Collection[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                Neighbours[i] = new Int32Collection();
            }

            for (int findex = 0; findex < faces.Count; findex += 3)
            {
                if (findex + 2 < faces.Count)
                {
                    int a = faces[findex];
                    int b = faces[findex + 1];
                    int c = faces[findex + 2];
                    CheckAdd(a, b, c);
                    CheckAdd(b, a, c);
                    CheckAdd(c, b, a);
                }
            }
        }

        public void Smooth(int iterations, double mu)
        {
            for (int iter = 0; iter < iterations; iter++)
            {
                double mu2 = mu;
                for (int pass = 0; pass < 2; pass++)
                {
                    averagePoints = new VertexAverage[vertices.Count];
                    for (int i = 0; i < vertices.Count; i++)
                    {
                        averagePoints[i] = new VertexAverage();
                    }

                    for (int i = 0; i < vertices.Count; i++)
                    {
                        foreach (int n in Neighbours[i])
                        {
                            averagePoints[i].Add(vertices[n]);
                        }
                    }

                    for (int i = 0; i < vertices.Count; i++)
                    {
                        vertices[i] = Move(i, mu2);
                    }

                    mu2 = -mu - 0.01;
                }
            }
        }

        private void CheckAdd(int a, int b, int c)
        {
            if (!Neighbours[a].Contains(b)) Neighbours[a].Add(b);
            if (!Neighbours[a].Contains(c)) Neighbours[a].Add(c);
        }

        private Point3D Move(int i, double mu)
        {
            Point3D res = vertices[i];

            if (averagePoints[i].N > 0)
            {
                Point3D ap = averagePoints[i].GetAverage();
                res.X = res.X + (ap.X - res.X) * mu;
                res.Y = res.Y + (ap.Y - res.Y) * mu;
                res.Z = res.Z + (ap.Z - res.Z) * mu;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Point with no neighbours");
            }
            return res;
        }
    }
}