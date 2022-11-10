using Barnacle.LineLib;
using Barnacle.Object3DLib;
using PolygonLibrary;
using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class ShapedTiledRoofMaker : MakerBase
    {
        private double tileLength;
        private double tileHeight;
        private double tileDepth;
        private double mortarGap;
        private double roofWidth;
        private double lx;
        private double ly;
        private double rx;
        private double ry;
        private String path;
        private List<Point> pathPoints;

        public ShapedTiledRoofMaker(string path, double tileLength, double tileHeight, double tileDepth, double mortarGap, double roofWidth)
        {
            this.path = path;
            this.tileLength = tileLength;
            this.tileHeight = tileHeight;
            this.tileDepth = tileDepth;
            this.mortarGap = mortarGap;
            this.roofWidth = roofWidth;
            lx = double.MaxValue;
            ly = double.MaxValue;
            rx = double.MinValue;
            ry = double.MinValue;
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;

            // get the path as a list of points
            FlexiPath fp = new FlexiPath();
            fp.FromString(this.path);
            pathPoints = fp.DisplayPoints();

            // calculate path bounds
            foreach (Point p in pathPoints)
            {
                lx = Math.Min(lx, p.X);
                ly = Math.Min(ly, p.Y);
                rx = Math.Max(rx, p.X);
                ry = Math.Max(ry, p.Y);
            }

            // turn path upside down (2D path has zero at top and y increase downwards, 3D has 0 at bottom and Y increases upwards)
            List<Point> tmp = new List<Point>();
            foreach (Point p in pathPoints)
            {
                tmp.Add(new Point(p.X, ry - p.Y));
            }

            // calculate where the back of the shape should be, parameters may make the front stick through the
            // back. we can't allow that.
            double backWall = roofWidth - tileDepth;
            if (backWall < 1)
            {
                backWall = 1;
            }

            Point[] crns;
            // We do the interception checks using ConverxPolygon2Ds
            ConvexPolygon2D path = new ConvexPolygon2D(tmp.ToArray());
            ConvexPolygon2DHelper interceptor = new ConvexPolygon2DHelper();
            double py = 0;

            bool offsetTile = false;
            while (py <= ry)
            {
                double px = lx;

                // alternate rows of tiles should be offset
                if (offsetTile)
                {
                    px -= tileLength / 2.0;
                }
                offsetTile = !offsetTile;

                while (px < rx)
                {
                    // tile

                    ConvexPolygon2D rect = RectPoly(px, py, tileLength, tileHeight);
                    ConvexPolygon2D intercept = interceptor.GetIntersectionOfPolygons(path, rect);
                    crns = intercept.Corners;
                    if (crns.GetLength(0) >= 3)
                    {
                        TriangulateSlopingSurface(px, py, crns, tileHeight, tileDepth);
                        int l = crns.GetLength(0);
                        double[] zeds = new double[l];
                        for (int i = 0; i < l; i++)
                        {
                            double rely = crns[i].Y - py;

                            zeds[i] = tileDepth - ((rely / tileHeight) * tileDepth);
                        }

                        for (int i = 0; i < crns.GetLength(0); i++)
                        {
                            int j = (i + 1) % crns.GetLength(0);

                            int v0 = AddVertice(crns[i].X, crns[i].Y, 0);
                            int v1 = AddVertice(crns[j].X, crns[j].Y, 0);
                            int v2 = AddVertice(crns[i].X, crns[i].Y, zeds[i]);
                            int v3 = AddVertice(crns[j].X, crns[j].Y, zeds[j]);

                            Faces.Add(v0);
                            Faces.Add(v1);
                            Faces.Add(v2);

                            Faces.Add(v1);
                            Faces.Add(v3);
                            Faces.Add(v2);
                        }
                    }

                    px += tileLength;

                    // side gap
                    rect = RectPoly(px, py, mortarGap, tileHeight);
                    intercept = interceptor.GetIntersectionOfPolygons(path, rect);
                    crns = intercept.Corners;
                    if (crns.GetLength(0) >= 3)
                    {
                        // only include the polygon if its centroid is in the path shape.
                        // This avoids spurious edge triangles
                        Point mid = Centroid(crns);
                        if (IsPointInPolygon(mid, tmp))
                        {
                            TriangulateSurface(crns, 0);
                        }
                    }
                    px += mortarGap;
                }
                py += tileHeight;
            }

            // do the back

            TriangulateSurface(tmp.ToArray(), -backWall, true);

            // sides
            for (int i = 0; i < tmp.Count; i++)
            {
                int j = (i + 1) % tmp.Count;
                int v0 = AddVertice(tmp[i].X, tmp[i].Y, -backWall);
                int v1 = AddVertice(tmp[j].X, tmp[j].Y, -backWall);
                int v2 = AddVertice(tmp[i].X, tmp[i].Y, 0);
                int v3 = AddVertice(tmp[j].X, tmp[j].Y, 0);

                Faces.Add(v0);
                Faces.Add(v2);
                Faces.Add(v1);

                Faces.Add(v1);
                Faces.Add(v2);
                Faces.Add(v3);
            }
        }

        protected void TriangulateSlopingSurface(double px, double py, Point[] points, double height, double depth, bool reverse = false)
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
                Point[] rel = new Point[3];
                rel[0] = new Point(t.Points[0].X - px, t.Points[0].Y - py);
                rel[1] = new Point(t.Points[1].X - px, t.Points[1].Y - py);
                rel[2] = new Point(t.Points[2].X - px, t.Points[2].Y - py);
                double[] zeds = new double[3];
                for (int i = 0; i < 3; i++)
                {
                    zeds[i] = depth - ((rel[i].Y / height) * depth);
                }
                int c0 = AddVertice(t.Points[0].X, t.Points[0].Y, zeds[0]);
                int c1 = AddVertice(t.Points[1].X, t.Points[1].Y, zeds[1]);
                int c2 = AddVertice(t.Points[2].X, t.Points[2].Y, zeds[2]);
                if (reverse)
                {
                    Faces.Add(c0);
                    Faces.Add(c2);
                    Faces.Add(c1);
                }
                else
                {
                    Faces.Add(c0);
                    Faces.Add(c1);
                    Faces.Add(c2);
                }
            }
        }
    }
}