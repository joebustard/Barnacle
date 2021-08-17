using Make3D.Object3DLib;
using PolygonTriangulationLib;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class MakerBase
    {
        private Int32Collection tris;
        private Point3DCollection vertices;

        public MakerBase()
        {
            vertices = new Point3DCollection();
            tris = new Int32Collection();
        }

        public Int32Collection Faces
        {
            get
            {
                return tris;
            }
            set
            {
                tris = value;
            }
        }

        public Point3DCollection Vertices
        {
            get
            {
                return vertices;
            }
            set
            {
                vertices = value;
            }
        }

        protected int AddVertice(double x, double y, double z)
        {
            int res = -1;
            for (int i = 0; i < vertices.Count; i++)
            {
                if (PointUtils.equals(vertices[i], x, y, z))
                {
                    res = i;
                    break;
                }
            }

            if (res == -1)
            {
                vertices.Add(new Point3D(x, y, z));
                res = vertices.Count - 1;
            }
            return res;
        }

        protected int AddVertice(Point3D v)
        {
            int res = -1;
            for (int i = 0; i < vertices.Count; i++)
            {
                if (PointUtils.equals(vertices[i], v.X, v.Y, v.Z))
                {
                    res = i;
                    break;
                }
            }

            if (res == -1)
            {
                vertices.Add(new Point3D(v.X, v.Y, v.Z));
                res = vertices.Count - 1;
            }
            return res;
        }

        protected void TriangulatePerimiter(List<Point> points, double thickness)
        {
            TriangulationPolygon ply = new TriangulationPolygon();
            List<System.Drawing.PointF> pf = new List<System.Drawing.PointF>();
            foreach (Point p in points)
            {
                pf.Add(new System.Drawing.PointF((float)p.X, (float)p.Y));
            }
            ply.Points = pf.ToArray();
            List<Triangle> tris = ply.Triangulate();
            foreach (Triangle t in tris)
            {
                int c0 = AddVertice(t.Points[0].X, thickness, t.Points[0].Y);
                int c1 = AddVertice(t.Points[1].X, thickness, t.Points[1].Y);
                int c2 = AddVertice(t.Points[2].X, thickness, t.Points[2].Y);
                Faces.Add(c0);
                Faces.Add(c2);
                Faces.Add(c1);

                c0 = AddVertice(t.Points[0].X, 0.0, t.Points[0].Y);
                c1 = AddVertice(t.Points[1].X, 0.0, t.Points[1].Y);
                c2 = AddVertice(t.Points[2].X, 0.0, t.Points[2].Y);
                Faces.Add(c0);
                Faces.Add(c1);
                Faces.Add(c2);
            }
        }
    }
}