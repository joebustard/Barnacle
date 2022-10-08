using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class TrapezoidMaker : MakerBase
    {
        private double bevel;
        private double bottomLength;
        private double height;
        private double topLength;
        private double width;

        public TrapezoidMaker(double lt, double lb, double h, double w, double b)
        {
            topLength = lt;
            height = h;
            width = w;
            bottomLength = lb;
            bevel = b;
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            Point p0 = new Point(-bottomLength / 2, 0);
            Point p1 = new Point(bottomLength / 2, 0);

            Point p2 = new Point((topLength / 2), height);
            Point p3 = new Point((-topLength / 2), height);

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