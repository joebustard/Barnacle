using Barnacle.LineLib;
using PolygonLibrary;
using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class ShapedBrickWallMaker : MakerBase
    {
        private double brickDepth;
        private double brickHeight;
        private double brickLength;
        private double lx;
        private double ly;
        private double mortarGap;
        private String path;
        private List<Point> pathPoints;
        private double rx;
        private double ry;
        private double wallWidth;

        public ShapedBrickWallMaker(string path, double brickLength, double brickHeight, double brickDepth, double wallWidth, double mortarGap)
        {
            this.path = path;
            this.brickLength = brickLength;
            this.brickHeight = brickHeight;
            this.brickDepth = brickDepth;
            this.mortarGap = mortarGap;
            this.wallWidth = wallWidth;

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

            FlexiPath fp = new FlexiPath();
            fp.FromString(this.path);
            pathPoints = fp.DisplayPoints();

            foreach (Point p in pathPoints)
            {
                lx = Math.Min(lx, p.X);
                ly = Math.Min(ly, p.Y);
                rx = Math.Max(rx, p.X);
                ry = Math.Max(ry, p.Y);
            }
            List<Point> tmp = new List<Point>();
            foreach (Point p in pathPoints)
            {
                tmp.Add(new Point(p.X, ry - p.Y));
            }
            double backWall = wallWidth - brickDepth;
            if (backWall < 1)
            {
                backWall = 1;
            }
            Point[] crns;
            ConvexPolygon2D path = new ConvexPolygon2D(tmp.ToArray());
            ConvexPolygon2DHelper interceptor = new ConvexPolygon2DHelper();
            double py = 0;

            bool offsetBrick = false;
            while (py <= ry)
            {
                double px = lx;
                if (offsetBrick)
                {
                    px -= brickLength / 2.0;
                }
                offsetBrick = !offsetBrick;
                while (px < rx)
                {
                    // brick

                    ConvexPolygon2D rect = RectPoly(px, py, brickLength, brickHeight);
                    ConvexPolygon2D intercept = interceptor.GetIntersectionOfPolygons(path, rect);
                    crns = intercept.Corners;
                    if (crns.GetLength(0) >= 3)
                    {
                        TriangulateSurface(crns, brickDepth);

                        for (int i = 0; i < crns.GetLength(0); i++)
                        {
                            int j = (i + 1) % crns.GetLength(0);
                            int v0 = AddVertice(crns[i].X, crns[i].Y, 0);
                            int v1 = AddVertice(crns[j].X, crns[j].Y, 0);
                            int v2 = AddVertice(crns[i].X, crns[i].Y, brickDepth);
                            int v3 = AddVertice(crns[j].X, crns[j].Y, brickDepth);

                            Faces.Add(v0);
                            Faces.Add(v1);
                            Faces.Add(v2);

                            Faces.Add(v1);
                            Faces.Add(v3);
                            Faces.Add(v2);
                        }
                    }

                    // horizontal gap running top length of brick
                    ConvexPolygon2D rect2 = RectPoly(px, py + brickHeight, brickLength + mortarGap, mortarGap);
                    ConvexPolygon2D intercept2 = interceptor.GetIntersectionOfPolygons(path, rect2);
                    crns = intercept2.Corners;
                    Point mid2 = Centroid(crns);
                    if (IsPointInPolygon(mid2, tmp))
                    {
                        TriangulateSurface(crns, 0);
                    }
                    px += brickLength;

                    // side gap
                    rect = RectPoly(px, py, mortarGap, brickHeight);
                    intercept = interceptor.GetIntersectionOfPolygons(path, rect);
                    crns = intercept.Corners;
                    if (crns.GetLength(0) >= 3)
                    {
                        Point mid = Centroid(crns);
                        if (IsPointInPolygon(mid, tmp))
                        {
                            TriangulateSurface(crns, 0);
                        }
                    }
                    px += mortarGap;
                }
                py += brickHeight;
                py += mortarGap;
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
    }
}