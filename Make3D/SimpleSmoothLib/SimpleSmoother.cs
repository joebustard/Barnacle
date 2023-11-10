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
        private VertexAverage[] averagePoints;
        private Point3DCollection vertices;
        private Int32Collection faces;

        public SimpleSmoother(Point3DCollection v, Int32Collection f)
        {
            vertices = v;
            faces = f;
            averagePoints = new VertexAverage[vertices.Count];
        }

        public void Smooth(int iterations, double mu)
        {
            for (int iter = 0; iter < iterations; iter++)
            {
                for (int pass = 0; pass < 2; pass++)
                {
                    averagePoints = new VertexAverage[vertices.Count];
                    for (int i = 0; i < vertices.Count; i++)
                    {
                        averagePoints[i] = new VertexAverage();
                    }

                    for (int findex = 0; findex < faces.Count; findex += 3)
                    {
                        if (findex + 2 < faces.Count)
                        {
                            int a = faces[findex];
                            int b = faces[findex + 1];
                            int c = faces[findex + 2];
                            averagePoints[a].Add(vertices[b]);
                            averagePoints[a].Add(vertices[c]);

                            averagePoints[b].Add(vertices[a]);
                            averagePoints[b].Add(vertices[c]);

                            averagePoints[c].Add(vertices[a]);
                            averagePoints[c].Add(vertices[b]);
                        }
                    }

                    for (int i = 0; i < vertices.Count; i++)
                    {
                        vertices[i] = Move(i, mu);
                    }

                    mu = -mu;
                }
            }
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