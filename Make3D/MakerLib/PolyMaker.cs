using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class PolyMaker : MakerBase
    {
        private double height;
        private List<System.Windows.Point> points;

        public PolyMaker(List<System.Windows.Point> pnts, double h)
        {
            height = h;
            points = pnts;
        }

        public PolyMaker(List<double> pnts, double h)
        {
            height = h;
            points = new List<System.Windows.Point>();
            for (int i = 0; i < pnts.Count; i += 2)
            {
                if (i + 1 < pnts.Count)
                {
                    System.Windows.Point p = new System.Windows.Point(pnts[i], pnts[i + 1]);
                    points.Add(p);
                }
            }
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            if (points != null && points.Count > 2 && height > 0)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    CreateSideFace(points, i);
                }
                // triangulate the basic polygon
                TriangulationPolygon ply = new TriangulationPolygon();
                List<PointF> pf = new List<PointF>();
                foreach (System.Windows.Point p in points)
                {
                    pf.Add(new PointF((float)p.X, (float)p.Y));
                }
                ply.Points = pf.ToArray();
                List<Triangle> tris = ply.Triangulate();
                foreach (Triangle t in tris)
                {
                    int c0 = AddVertice(t.Points[0].X, t.Points[0].Y, 0.0);
                    int c1 = AddVertice(t.Points[1].X, t.Points[1].Y, 0.0);
                    int c2 = AddVertice(t.Points[2].X, t.Points[2].Y, 0.0);
                    Faces.Add(c0);
                    Faces.Add(c2);
                    Faces.Add(c1);

                    c0 = AddVertice(t.Points[0].X, t.Points[0].Y, height);
                    c1 = AddVertice(t.Points[1].X, t.Points[1].Y, height);
                    c2 = AddVertice(t.Points[2].X, t.Points[2].Y, height);
                    Faces.Add(c0);
                    Faces.Add(c1);
                    Faces.Add(c2);
                }
            }
        }

        private void CreateSideFace(List<System.Windows.Point> pnts, int i)
        {
            int v = i + 1;
            if (v == pnts.Count)
            {
                v = 0;
            }

            int c0 = AddVertice(pnts[i].X, pnts[i].Y, 0.0);
            int c1 = AddVertice(pnts[i].X, pnts[i].Y, height);
            int c2 = AddVertice(pnts[v].X, pnts[v].Y, height);
            int c3 = AddVertice(pnts[v].X, pnts[v].Y, 0.0);
            Faces.Add(c0);
            Faces.Add(c2);
            Faces.Add(c1);

            Faces.Add(c0);
            Faces.Add(c3);
            Faces.Add(c2);
        }
    }
}