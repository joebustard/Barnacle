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

        public void AddFace(int c0, int c1, int c2)
        {
            Faces.Add(c0);
            Faces.Add(c1);
            Faces.Add(c2);
        }

        internal static double Distance3D(Point3D p1, Point3D p2)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double dz = p2.Z - p1.Z;
            return Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
        }

        internal void SweepPolarProfilePhi(List<PolarCoordinate> polarProfile, double cx, double cy, double sweepRange, int numSegs, bool clear = true)
        {
            // now we have a lovely copy of the profile in polar coordinates.
            if (clear)
            {
                Vertices.Clear();
                Faces.Clear();
            }

            double sweep = sweepRange * (Math.PI * 2.0) / 360.0;
            double da = sweep / (numSegs - 1);
            for (int i = 0; i < numSegs; i++)
            {
                double a = da * i;
                int j = i + 1;
                if (j == numSegs)
                {
                    if (sweepRange == 360)
                    {
                        j = 0;
                    }
                    else
                    {
                        // dont connect end to start if the sweep doesn't go all the way round
                        break;
                    }
                }
                double b = da * j;

                for (int index = 0; index < polarProfile.Count; index++)
                {
                    int index2 = index + 1;
                    if (index2 == polarProfile.Count)
                    {
                        index2 = 0;
                    }
                    PolarCoordinate pc1 = polarProfile[index].Clone();
                    PolarCoordinate pc2 = polarProfile[index2].Clone();
                    PolarCoordinate pc3 = polarProfile[index2].Clone();
                    PolarCoordinate pc4 = polarProfile[index].Clone();
                    pc1.Phi -= a;
                    pc2.Phi -= a;
                    pc3.Phi -= b;
                    pc4.Phi -= b;

                    Point3D p1 = pc1.GetPoint3D();
                    Point3D p2 = pc2.GetPoint3D();
                    Point3D p3 = pc3.GetPoint3D();
                    Point3D p4 = pc4.GetPoint3D();

                    int v1 = AddVertice(p1);
                    int v2 = AddVertice(p2);
                    int v3 = AddVertice(p3);
                    int v4 = AddVertice(p4);

                    Faces.Add(v1);
                    Faces.Add(v2);
                    Faces.Add(v3);

                    Faces.Add(v1);
                    Faces.Add(v3);
                    Faces.Add(v4);
                }
            }
            return;
        }

        internal void SweepPolarProfileTheta(List<PolarCoordinate> polarProfile, double cx, double cy, double sweepRange, int numSegs, bool clear = true, bool flipAxies = false, bool invert = false)
        {
            // now we have a lovely copy of the profile in polar coordinates.
            if (clear)
            {
                Vertices.Clear();
                Faces.Clear();
            }

            double sweep = sweepRange * (Math.PI * 2.0) / 360.0;
            double da = sweep / (numSegs - 1);
            for (int i = 0; i < numSegs; i++)
            {
                double a = da * i;
                int j = i + 1;
                if (j == numSegs)
                {
                    if (sweepRange == 360)
                    {
                        j = 0;
                    }
                    else
                    {
                        // dont connect end to start if the sweep doesn't go all the way round
                        break;
                    }
                }
                double b = da * j;

                for (int index = 0; index < polarProfile.Count; index++)
                {
                    int index2 = index + 1;
                    if (index2 == polarProfile.Count)
                    {
                        index2 = 0;
                    }
                    PolarCoordinate pc1 = polarProfile[index].Clone();
                    PolarCoordinate pc2 = polarProfile[index2].Clone();
                    PolarCoordinate pc3 = polarProfile[index2].Clone();
                    PolarCoordinate pc4 = polarProfile[index].Clone();
                    pc1.Theta -= a;
                    pc2.Theta -= a;
                    pc3.Theta -= b;
                    pc4.Theta -= b;

                    Point3D p1 = pc1.GetPoint3D();
                    Point3D p2 = pc2.GetPoint3D();
                    Point3D p3 = pc3.GetPoint3D();
                    Point3D p4 = pc4.GetPoint3D();
                    if (flipAxies)
                    {
                        FlipAxies(ref p1);
                        FlipAxies(ref p2);
                        FlipAxies(ref p3);
                        FlipAxies(ref p4);
                    }
                    int v1 = AddVertice(p1);
                    int v2 = AddVertice(p2);
                    int v3 = AddVertice(p3);
                    int v4 = AddVertice(p4);
                    if (invert)
                    {
                        Faces.Add(v1);
                        Faces.Add(v3);
                        Faces.Add(v2);

                        Faces.Add(v1);
                        Faces.Add(v4);
                        Faces.Add(v3);
                    }
                    else
                    {
                        Faces.Add(v1);
                        Faces.Add(v2);
                        Faces.Add(v3);

                        Faces.Add(v1);
                        Faces.Add(v3);
                        Faces.Add(v4);
                    }
                }
            }
            if (sweepRange != 360.0)
            {
                // both ends will be open.
                Point3D centreOfProfile = new Point3D(cx, 0, cy);
                for (int index = 0; index < polarProfile.Count; index++)
                {
                    int index2 = index + 1;
                    if (index2 == polarProfile.Count)
                    {
                        index2 = 0;
                    }
                    PolarCoordinate pc1 = polarProfile[index].Clone();
                    PolarCoordinate pc2 = polarProfile[index2].Clone();
                    PolarCoordinate pc3 = new PolarCoordinate(0, 0, 0);
                    pc3.SetPoint3D(centreOfProfile);

                    Point3D p1 = pc1.GetPoint3D();
                    Point3D p2 = pc2.GetPoint3D();
                    Point3D p3 = pc3.GetPoint3D();

                    int v1 = AddVertice(p1);
                    int v2 = AddVertice(p2);
                    int v3 = AddVertice(p3);
                    if (invert)
                    {
                        Faces.Add(v1);
                        Faces.Add(v2);
                        Faces.Add(v3);
                    }
                    else
                    {
                        Faces.Add(v1);
                        Faces.Add(v3);
                        Faces.Add(v2);
                    }
                }

                for (int index = 0; index < polarProfile.Count; index++)
                {
                    int index2 = index + 1;
                    if (index2 == polarProfile.Count)
                    {
                        index2 = 0;
                    }
                    PolarCoordinate pc1 = polarProfile[index].Clone();
                    PolarCoordinate pc2 = polarProfile[index2].Clone();
                    PolarCoordinate pc3 = new PolarCoordinate(0, 0, 0);
                    pc3.SetPoint3D(centreOfProfile);
                    pc1.Theta -= sweep;
                    pc2.Theta -= sweep;
                    pc3.Theta -= sweep;

                    Point3D p1 = pc1.GetPoint3D();
                    Point3D p2 = pc2.GetPoint3D();
                    Point3D p3 = pc3.GetPoint3D();

                    int v1 = AddVertice(p1);
                    int v2 = AddVertice(p2);
                    int v3 = AddVertice(p3);

                    if (invert)
                    {
                        Faces.Add(v1);
                        Faces.Add(v3);
                        Faces.Add(v2);
                    }
                    else
                    {
                        Faces.Add(v1);
                        Faces.Add(v2);
                        Faces.Add(v3);
                    }
                }
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

        protected int AddVertice(Point3DCollection pnts, double x, double y, double z)
        {
            int res = -1;
            for (int i = 0; i < pnts.Count; i++)
            {
                if (PointUtils.equals(pnts[i], x, y, z))
                {
                    res = i;
                    break;
                }
            }

            if (res == -1)
            {
                pnts.Add(new Point3D(x, y, z));
                res = pnts.Count - 1;
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

        protected void AppendShape(Object3D bs, Point3DCollection pnts, Int32Collection faces)
        {
            Int32Collection altFaces = new Int32Collection();
            foreach (int org in bs.TriangleIndices)
            {
                Point3D p = bs.AbsoluteObjectVertices[org];
                int apped = AddVertice(pnts, p.X, p.Y, p.Z);
                altFaces.Add(apped);
            }

            foreach (int i in altFaces)
            {
                faces.Add(i);
            }
        }

        protected Point Centroid(Point[] crns)
        {
            double x = 0;
            double y = 0;
            if (crns.GetLength(0) > 0)
            {
                foreach (Point p in crns)
                {
                    x += p.X;
                    y += p.Y;
                }
                x /= crns.GetLength(0);
                y /= crns.GetLength(0);
            }
            return new Point(x, y);
        }

        protected void CopyShape(Object3D bs, Point3DCollection pnts, Int32Collection faces)
        {
            foreach (Point3D ps in bs.AbsoluteObjectVertices)
            {
                pnts.Add(new Point3D(ps.X, ps.Y, ps.Z));
            }
            foreach (int i in bs.TriangleIndices)
            {
                faces.Add(i);
            }
        }

        protected void FlipAxies(ref Point3D p1)
        {
            p1 = new Point3D(p1.X, p1.Z, p1.Y);
        }

        protected bool IsPointInPolygon(Point point, List<Point> polygon)
        {
            int polyCorners = polygon.Count;
            int i = 0;
            int j = polyCorners - 1;
            bool oddNodes = false;

            for (i = 0; i < polyCorners; i++)
            {
                if (polygon[i].Y < point.Y && polygon[j].Y >= point.Y
                || polygon[j].Y < point.Y && polygon[i].Y >= point.Y)
                {
                    if (polygon[i].X + (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < point.X)
                    {
                        oddNodes = !oddNodes;
                    }
                }
                j = i;
            }

            return oddNodes;
        }

        protected ConvexPolygon2D RectPoly(double px, double py, double l, double h)
        {
            Point[] rct = new Point[4];
            rct[0].X = px;
            rct[0].Y = py;

            rct[1].X = px;
            rct[1].Y = py + h;

            rct[2].X = px + l;
            rct[2].Y = py + h;

            rct[3].X = px + l;
            rct[3].Y = py;

            return new ConvexPolygon2D(rct);
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

        protected void TriangulatePerimiter(List<System.Drawing.PointF> points, double thickness)
        {
            TriangulationPolygon ply = new TriangulationPolygon();
            List<System.Drawing.PointF> pf = new List<System.Drawing.PointF>();
            foreach (System.Drawing.PointF p in points)
            {
                pf.Add(new System.Drawing.PointF(p.X, p.Y));
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

        protected void TriangulateSurface(Point[] points, double z, bool reverse = false)
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
                int c0 = AddVertice(t.Points[0].X, t.Points[0].Y, z);
                int c1 = AddVertice(t.Points[1].X, t.Points[1].Y, z);
                int c2 = AddVertice(t.Points[2].X, t.Points[2].Y, z);
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