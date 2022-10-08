using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class StadiumMaker : MakerBase
    {
        private double gap;
        private double height;
        private double radius1;
        private double radius2;
        private string shape;

        public StadiumMaker(string s, double r1, double r2, double g, double h)
        {
            shape = s;
            radius1 = r1;
            radius2 = r2;
            height = h;
            gap = g;
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            Vertices = pnts;
            Faces = faces;
            pnts.Clear();
            faces.Clear();
            switch (shape.ToLower())
            {
                case "flat":
                    {
                        GenerateBasicShape();
                    }
                    break;

                case "overflat5":
                    {
                        GenerateBasicShape(5);
                    }
                    break;

                case "overflat10":
                    {
                        GenerateBasicShape(10);
                    }
                    break;

                case "overflat20":
                    {
                        GenerateBasicShape(20);
                    }
                    break;

                case "sausage":
                    {
                        GenerateSausageShape();
                    }
                    break;
            }
        }

        private void GenerateBasicShape(double over = 0)
        {
            double dx = gap / 2.0;
            //double cy1 = -dx - radius1;
            //double cy2 = dx + radius2;

            double cy1 = -dx;
            double cy2 = dx;

            double cx1 = 0;
            double cx2 = 0;

            double theta = 0;
            double dt = Math.PI / 100.0;
            over = (Math.PI * over) / 180;
            List<Point> perimeter = new List<Point>();
            theta = over;
            while (theta <= Math.PI - over)
            {
                double x = cx2 + radius2 * Math.Cos(theta);
                double y = cy2 + radius2 * Math.Sin(theta);

                perimeter.Add(new Point(x, y));
                theta += dt;
            }

            while (theta <= 2.0 * Math.PI + over)
            {
                double x = cx1 + radius1 * Math.Cos(theta);
                double y = cy1 + radius1 * Math.Sin(theta);

                perimeter.Add(new Point(x, y));
                theta += dt;
            }
            Vertices.Clear();
            Faces.Clear();

            for (int i = 0; i < perimeter.Count; i++)
            {
                int j = i + 1;
                if (j == perimeter.Count)
                {
                    j = 0;
                }

                int v0 = AddVertice(perimeter[i].X, 0, perimeter[i].Y);
                int v1 = AddVertice(perimeter[i].X, height, perimeter[i].Y);
                int v2 = AddVertice(perimeter[j].X, height, perimeter[j].Y);
                int v3 = AddVertice(perimeter[j].X, 0, perimeter[j].Y);

                Faces.Add(v0);
                Faces.Add(v1);
                Faces.Add(v2);

                Faces.Add(v0);
                Faces.Add(v2);
                Faces.Add(v3);
            }
            TriangulatePerimiter(perimeter, height);
        }

        private void GenerateSausageShape()
        {
            double dx = gap / 2.0;
            double cy1 = -dx - radius1;
            double cy2 = dx + radius2;

            double cx1 = 0;
            double cx2 = 0;

            double theta = 0;
            double dt = Math.PI / 20.0;

            List<Point> perimeter = new List<Point>();
            theta = Math.PI / 2;
            while (theta >= 0)
            {
                double x = cx2 + radius2 * Math.Cos(theta);
                double y = cy2 + radius2 * Math.Sin(theta);

                perimeter.Add(new Point(x, y));
                theta -= dt;
            }

            theta = 2.0 * Math.PI;

            while (theta >= 1.5 * Math.PI)
            {
                double x = cx1 + radius1 * Math.Cos(theta);
                double y = cy1 + radius1 * Math.Sin(theta);

                perimeter.Add(new Point(x, y));
                theta -= dt;
            }
            Vertices.Clear();
            Faces.Clear();

            double phi = 0;
            double phi2 = 0;
            double numberOfFacets = 72;
            double dphi = (2 * Math.PI) / numberOfFacets;

            double px1 = 0;
            double py1 = 0;

            double px2 = 0;
            double py2 = 0;

            double px3 = 0;
            double py3 = 0;

            double px4 = 0;
            double py4 = 0;

            while (phi <= (2 * Math.PI) - dphi)
            {
                phi2 = phi + dphi;

                for (int i = 0; i < perimeter.Count - 1; i++)
                {
                    int j = i + 1;
                    if (j == perimeter.Count)
                    {
                        j = 0;
                    }
                    px1 = perimeter[i].X * Math.Cos(phi);
                    py1 = perimeter[i].X * Math.Sin(phi);

                    px2 = perimeter[i].X * Math.Cos(phi2);
                    py2 = perimeter[i].X * Math.Sin(phi2);

                    px3 = perimeter[j].X * Math.Cos(phi2);
                    py3 = perimeter[j].X * Math.Sin(phi2);

                    px4 = perimeter[j].X * Math.Cos(phi);
                    py4 = perimeter[j].X * Math.Sin(phi);

                    int v0 = AddVertice(px1, py1, perimeter[i].Y);
                    int v1 = AddVertice(px2, py2, perimeter[i].Y);
                    int v2 = AddVertice(px3, py3, perimeter[j].Y);
                    int v3 = AddVertice(px4, py4, perimeter[j].Y);

                    Faces.Add(v0);
                    Faces.Add(v2);
                    Faces.Add(v1);

                    Faces.Add(v0);
                    Faces.Add(v3);
                    Faces.Add(v2);
                }
                phi += dphi;
            }
        }
    }
}