using Barnacle.LineLib;
using PolygonTriangulationLib;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class SquirkleMaker : MakerBase
    {
        private int blc;
        private int brc;
        private double depth;
        private double length;
        private double squirkleheight;
        private int tlc;
        private int trc;

        public SquirkleMaker(int tl, int tr, int bl, int br, double l, double h, double w)
        {
            tlc = tl;
            trc = tr;
            blc = bl;
            brc = br;
            length = l;
            squirkleheight = h;
            depth = w;
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;

            double h = 0.25;
            double l = 0.25;
            string yseg = h.ToString();
            string xseg = l.ToString();

            string pathtext = "M 0,0 ";

            pathtext += "RV -" + yseg + " ";
            // bl
            if (blc == 0)
            {
                pathtext += "RV -" + yseg + " ";
                pathtext += "RH " + xseg + " ";
            }
            if (blc == 1)
            {
                pathtext += "RQ 0,-" + yseg + " " + xseg + ",-" + yseg + " ";
            }
            if (blc == 2)
            {
                pathtext += "RQ " + xseg + ",0 " + xseg + ",-" + yseg + " ";
            }
            pathtext += "RH " + xseg + " ";

            // br
            pathtext += "RH " + xseg + " ";
            if (brc == 0)
            {
                pathtext += "RH " + xseg + " ";
                pathtext += "RV " + yseg + " ";
            }
            if (brc == 1)
            {
                pathtext += "RQ " + xseg + ",0 " + xseg + "," + yseg + " ";
            }
            if (brc == 2)
            {
                pathtext += "RQ 0," + yseg + " " + xseg + "," + yseg + " ";
            }
            pathtext += "RV " + yseg + " ";

            // tr
            pathtext += "RV " + yseg + " ";
            if (trc == 0)
            {
                pathtext += "RV " + yseg + " ";
                pathtext += "RH -" + xseg + " ";
            }
            if (trc == 1)
            {
                pathtext += "RQ 0," + yseg + " -" + xseg + "," + yseg + " ";
            }
            if (trc == 2)
            {
                pathtext += "RQ -" + xseg + ",0 -" + xseg + "," + yseg + " ";
            }
            pathtext += "RH -" + xseg + " ";

            // tl
            pathtext += "RH -" + xseg + " ";
            if (tlc == 0)
            {
                pathtext += "RH -" + xseg + " ";
                pathtext += "RV -" + yseg + " ";
            }
            if (tlc == 1)
            {
                pathtext += "RQ -" + yseg + ",0 -" + xseg + ",-" + yseg + " ";
            }
            if (tlc == 2)
            {
                pathtext += "RQ 0,-" + xseg + " -" + yseg + ",-" + xseg + " ";
            }
            pathtext += "RV -" + xseg + " ";

            GenerateFromPath(pathtext);
        }

        private void CreateSideFace(List<System.Windows.Point> pnts, int i, bool autoclose = true)
        {
            int v = i + 1;

            if (v == pnts.Count)
            {
                v = 0;
            }

            int c0 = AddVertice(pnts[i].X, pnts[i].Y, 0.0);
            int c1 = AddVertice(pnts[i].X, pnts[i].Y, depth);
            int c2 = AddVertice(pnts[v].X, pnts[v].Y, depth);
            int c3 = AddVertice(pnts[v].X, pnts[v].Y, 0.0);
            Faces.Add(c0);
            Faces.Add(c2);
            Faces.Add(c1);

            Faces.Add(c0);
            Faces.Add(c3);
            Faces.Add(c2);
        }

        private void GenerateFromPath(string pathtext)
        {
            FlexiPath flexiPath = new FlexiPath();
            flexiPath.InterpretTextPath(pathtext);
            List<System.Windows.Point> points = flexiPath.DisplayPoints();
            List<System.Windows.Point> tmp = new List<System.Windows.Point>();
            for (int i = 0; i < points.Count; i++)
            {
                tmp.Add(new System.Windows.Point(points[i].X * length, points[i].Y * squirkleheight));
            }
            // generate side triangles so original points are already in list
            for (int i = 0; i < tmp.Count; i++)
            {
                CreateSideFace(tmp, i);
            }
            // triangulate the basic polygon
            TriangulationPolygon ply = new TriangulationPolygon();
            List<PointF> pf = new List<PointF>();
            foreach (System.Windows.Point p in tmp)
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

                c0 = AddVertice(t.Points[0].X, t.Points[0].Y, depth);
                c1 = AddVertice(t.Points[1].X, t.Points[1].Y, depth);
                c2 = AddVertice(t.Points[2].X, t.Points[2].Y, depth);
                Faces.Add(c0);
                Faces.Add(c1);
                Faces.Add(c2);
            }
        }
    }
}