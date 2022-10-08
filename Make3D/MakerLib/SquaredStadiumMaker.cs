using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class SquaredStadiumMaker : MakerBase
    {
        private double endSize;
        private double gap;
        private double height;
        private double overrun;
        private double radius;

        public SquaredStadiumMaker(double r, double l, double es, double h, double ov)
        {
            radius = r;
            endSize = es;
            height = h;
            gap = l;
            overrun = ov;
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            Vertices = pnts;
            Faces = faces;
            pnts.Clear();
            faces.Clear();
            GenerateBasicShape();
        }

        private void GenerateBasicShape()
        {
            double halfend = endSize / 2.0;
            double dx = gap / 2.0;
            double pi2 = Math.PI / 2.0;
            double cx1 = gap;
            double cy1 = 0;

            double theta = -Math.PI;
            double dt = Math.PI / 100.0;
            overrun = (Math.PI * overrun) / 180;
            List<Point> perimeter = new List<Point>();
            theta = -pi2 - overrun;
            while (theta <= pi2 + overrun)
            {
                double x = cx1 + radius * Math.Cos(theta);
                double y = cy1 + radius * Math.Sin(theta);

                perimeter.Add(new Point(x, y));
                theta += dt;
            }
            perimeter.Add(new Point(0, +halfend));
            perimeter.Add(new Point(0, -halfend));

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
    }
}