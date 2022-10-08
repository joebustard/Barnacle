using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class PGramMaker : MakerBase
    {
        private double angle;
        private double bevel;
        private double height;
        private double length;
        private double width;

        public PGramMaker(double l, double h, double w, double a, double b)
        {
            length = l;
            height = h;
            width = w;
            angle = a;
            bevel = b;
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            Point p0 = new Point(-length / 2, 0);
            Point p1 = new Point(length / 2, 0);
            double theta = (90 - angle) * Math.PI / 180.0;
            double x = height * Math.Sin(theta);
            double y = height * Math.Cos(theta);
            Point p2 = new Point((length / 2) + x, y);
            Point p3 = new Point((-length / 2) + x, y);

            // front
            int v0 = AddVertice(new Point3D(p0.X + bevel, p0.Y + bevel, width / 2));
            int v1 = AddVertice(new Point3D(p1.X - bevel, p1.Y + bevel, width / 2));
            int v2 = AddVertice(new Point3D(p2.X - bevel, p2.Y - bevel, width / 2));
            int v3 = AddVertice(new Point3D(p3.X + bevel, p3.Y - bevel, width / 2));

            Faces.Add(v0);
            Faces.Add(v1);
            Faces.Add(v2);

            Faces.Add(v0);
            Faces.Add(v2);
            Faces.Add(v3);

            // back
            int v4 = AddVertice(new Point3D(p0.X, p0.Y, -width / 2));
            int v5 = AddVertice(new Point3D(p3.X, p3.Y, -width / 2));
            int v6 = AddVertice(new Point3D(p2.X, p2.Y, -width / 2));
            int v7 = AddVertice(new Point3D(p1.X, p1.Y, -width / 2));

            Faces.Add(v4);
            Faces.Add(v5);
            Faces.Add(v6);

            Faces.Add(v4);
            Faces.Add(v6);
            Faces.Add(v7);

            // left side
            Faces.Add(v0);
            Faces.Add(v3);
            Faces.Add(v4);

            Faces.Add(v4);
            Faces.Add(v3);
            Faces.Add(v5);

            // right side
            Faces.Add(v1);
            Faces.Add(v7);
            Faces.Add(v2);

            Faces.Add(v7);
            Faces.Add(v6);
            Faces.Add(v2);

            // bottom side
            Faces.Add(v0);
            Faces.Add(v4);
            Faces.Add(v1);

            Faces.Add(v4);
            Faces.Add(v7);
            Faces.Add(v1);

            // top side
            Faces.Add(v2);
            Faces.Add(v6);
            Faces.Add(v3);

            Faces.Add(v3);
            Faces.Add(v6);
            Faces.Add(v5);
        }
    }
}