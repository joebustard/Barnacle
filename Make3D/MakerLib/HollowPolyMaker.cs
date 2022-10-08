using Barnacle.Object3DLib;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class HollowPolyMaker : MakerBase
    {
        private double height;
        private List<System.Windows.Point> points;
        private double wallThickness;

        public HollowPolyMaker(List<System.Windows.Point> pnts, double h, double wth)
        {
            height = h;
            points = pnts;
            wallThickness = wth;
        }

        public HollowPolyMaker(List<double> pnts, double h, double wth)
        {
            height = h;
            wallThickness = wth;
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
            List<PointF> outerPolygon = new List<PointF>();
            List<PointF> innerPolygon = new List<PointF>();

            if (points != null && points.Count > 2 && height > 0)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    outerPolygon.Add(new PointF((float)points[i].X, (float)points[i].Y));
                    innerPolygon.Add(new PointF((float)points[i].X, (float)points[i].Y));
                }
                outerPolygon = LineUtils.RemoveCoplanarSegments(outerPolygon);
                innerPolygon = LineUtils.RemoveCoplanarSegments(innerPolygon);

                innerPolygon = LineUtils.GetEnlargedPolygon(innerPolygon, -(float)wallThickness);
                List<System.Windows.Point> tmp = new List<System.Windows.Point>();
                for (int i = outerPolygon.Count - 1; i >= 0; i--)
                {
                    tmp.Add(new System.Windows.Point(outerPolygon[i].X, outerPolygon[i].Y));
                }
                // generate side triangles so original points are already in list
                for (int i = 0; i < tmp.Count; i++)
                {
                    CreateSideFace(tmp, i);
                }

                tmp.Clear();
                for (int i = 0; i < innerPolygon.Count; i++)
                {
                    tmp.Add(new System.Windows.Point(innerPolygon[i].X, innerPolygon[i].Y));
                }
                // generate inside wall triangles
                for (int i = 0; i < tmp.Count; i++)
                {
                    CreateSideFace(tmp, i);
                }

                for (int i = 0; i < outerPolygon.Count; i++)
                {
                    int j = i + 1;
                    if (j == outerPolygon.Count)
                    {
                        j = 0;
                    }
                    int c0 = AddVertice(outerPolygon[i].X, outerPolygon[i].Y, 0.0);
                    int c1 = AddVertice(innerPolygon[i].X, innerPolygon[i].Y, 0.0);
                    int c2 = AddVertice(innerPolygon[j].X, innerPolygon[j].Y, 0.0);
                    int c3 = AddVertice(outerPolygon[j].X, outerPolygon[j].Y, 0.0);
                    Faces.Add(c0);
                    Faces.Add(c2);
                    Faces.Add(c1);

                    Faces.Add(c0);
                    Faces.Add(c3);
                    Faces.Add(c2);

                    c0 = AddVertice(outerPolygon[i].X, outerPolygon[i].Y, height);
                    c1 = AddVertice(innerPolygon[i].X, innerPolygon[i].Y, height);
                    c2 = AddVertice(innerPolygon[j].X, innerPolygon[j].Y, height);
                    c3 = AddVertice(outerPolygon[j].X, outerPolygon[j].Y, height);
                    Faces.Add(c0);
                    Faces.Add(c1);
                    Faces.Add(c2);

                    Faces.Add(c0);
                    Faces.Add(c2);
                    Faces.Add(c3);
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