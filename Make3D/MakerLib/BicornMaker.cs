using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class BicornMaker : MakerBase
    {
        private bool doubled;
        private double height;
        private double radius1;
        private double radius2;

        public BicornMaker(double r1, double r2, double h, bool db)
        {
            radius1 = r1;
            radius2 = r2;
            height = h;
            doubled = db;
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            if (radius1 > 0 && height > 0 && radius2 > 0)
            {
                GenerateOne(1);
                if (doubled)
                {
                    GenerateOne(-1);
                }
            }
        }

        private void GenerateOne(int sgn)
        {
            double st;
            double ct;
            List<System.Windows.Point> edge = new List<System.Windows.Point>();
            for (double theta = 0; theta < Math.PI * 2; theta += 0.05)
            {
                st = Math.Sin(theta);
                ct = Math.Cos(theta);
                double x = radius1 * st;
                double y = (sgn * radius2) * (ct * ct) * (2 + ct);
                y = y / (3 + (st * st));
                edge.Add(new System.Windows.Point(x, y));
            }

            for (int i = 0; i < edge.Count; i++)
            {
                int j = i + 1;
                if (j >= edge.Count)
                {
                    j = 0;
                }
                int v1 = AddVertice(edge[i].X, 0, edge[i].Y);
                int v2 = AddVertice(edge[j].X, 0, edge[j].Y);
                int v3 = AddVertice(edge[j].X, height, edge[j].Y);
                int v4 = AddVertice(edge[i].X, height, edge[i].Y);

                if (sgn == 1)
                {
                    Faces.Add(v1);
                    Faces.Add(v2);
                    Faces.Add(v3);

                    Faces.Add(v1);
                    Faces.Add(v3);
                    Faces.Add(v4);
                }
                else
                {
                    Faces.Add(v1);
                    Faces.Add(v3);
                    Faces.Add(v2);

                    Faces.Add(v1);
                    Faces.Add(v4);
                    Faces.Add(v3);
                }
            }
            TriangulatePerimiter(edge, height);
        }
    }
}