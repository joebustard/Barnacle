using Barnacle.LineLib;
using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    public partial class TrackDialog : BaseModellerDialog
    {
        private System.Windows.Point[] m1LinkConnectorCoords =
    {
         new System.Windows.Point(0.6,0.05),
         new System.Windows.Point(0.65,0.9),
         new System.Windows.Point(0.8,0.9),
         new System.Windows.Point(0.86,3),
         new System.Windows.Point(0.93,3),
         new System.Windows.Point(1,0.9),
         new System.Windows.Point(1.15,0.9),
         new System.Windows.Point(1.2,0.05),

         new System.Windows.Point(1.2,-0.05),

         new System.Windows.Point(1.15,-0.9),
         new System.Windows.Point(0.65,-0.9),
         new System.Windows.Point(0.6,-0.05),
     };

        private System.Windows.Point[] m1MainPolyCoords =
    {
         new System.Windows.Point(0.0,0.05),
         new System.Windows.Point(0.05,0.25),
         new System.Windows.Point(0.1,1.0),
         new System.Windows.Point(0.8,1.0),
         new System.Windows.Point(0.98,0.1),

          new System.Windows.Point(0.98,-0.1),
         new System.Windows.Point(0.8,-1.0),
         new System.Windows.Point(0.1,-1.0),
         new System.Windows.Point(0.05,-0.25),
         new System.Windows.Point(0.0,-0.05),
        };

        internal void GenerateLinkPart(System.Windows.Point p1, System.Windows.Point p2, Point3DCollection vertices, Int32Collection faces, bool firstCall, double width, double thickness, Link link)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            foreach (LinkPart lp in link.Parts)
            {
                String pth = lp.PathText;
                FlexiPath fp = new FlexiPath();
                if (fp.FromTextPath(pth))
                {
                    List<System.Windows.Point> rawprofile = fp.DisplayPoints();
                    List<System.Windows.Point> linkProfile = new List<System.Windows.Point>();
                    // rawprofile should have the basic shape of the part
                    // horizontal, with coordinates in the range 0 to 1
                    if (Math.Abs(dx) < 0.000001 && dy > 0.0001)
                    {
                        VerticalUp(lp.X, lp.Y, p1, p2, rawprofile, linkProfile, thickness);
                    }
                    else if (Math.Abs(dx) < 0.000001 && dy < 0.0001)
                    {
                        VerticalDown(lp.X, lp.Y, p1, p2, rawprofile, linkProfile, thickness);
                    }
                    else if (dx > 0.0001 && Math.Abs(dy) < 0.0000011)
                    {
                        HorizontalRight(lp.X, lp.Y, p1, p2, rawprofile, linkProfile, thickness);
                    }
                    else if (dx < 0.0001 && Math.Abs(dy) < 0.000001)
                    {
                        HorizontalLeft(lp.X, lp.Y, p1, p2, rawprofile, linkProfile, thickness);
                    }
                    else
                    {
                        NonOrthogonal(lp.X, lp.Y, p1, p2, rawprofile, linkProfile, thickness);
                    }

                    double partBackZ = (lp.Z - lp.W / 2.0) * width;
                    double partFrontZ = (lp.Z + lp.W / 2.0) * width;
                    MakeFacesForLinkPart(linkProfile, partBackZ, partFrontZ);
                }
            }
        }

        private static void HorizontalLeft(double xo, double yo, System.Windows.Point p1, System.Windows.Point p2, List<System.Windows.Point> rawProfile, List<System.Windows.Point> rotatedProfile, double height)
        {
            double dx = p2.X - p1.X;
            foreach (System.Windows.Point rawp in rawProfile)
            {
                double x = p1.X + ((rawp.X + xo) * dx);
                double y = p1.Y - ((rawp.Y + yo) * height);
                rotatedProfile.Add(new System.Windows.Point(x, y));
            }
        }

        private static void HorizontalRight(double xo, double yo, System.Windows.Point p1, System.Windows.Point p2, List<System.Windows.Point> rawProfile, List<System.Windows.Point> rotatedProfile, double height)
        {
            double dx = p2.X - p1.X;
            foreach (System.Windows.Point rawp in rawProfile)
            {
                double x = p1.X + ((rawp.X + xo) * dx);
                double y = p1.Y + ((rawp.Y + yo) * height);
                rotatedProfile.Add(new System.Windows.Point(x, y));
            }
        }

        private static void NonOrthogonal(double xo, double yo, System.Windows.Point p1, System.Windows.Point p2, List<System.Windows.Point> rawProfile, List<System.Windows.Point> rotatedProfile, double height)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double dist = TankTrackUtils.Distance(p1, p2);
            double sign = -1;
            // right down
            if (dx > 0 && dy < 0)
            {
                sign = 1;
            }
            else
            if (dx > 0 && dy > 0)
            {
                sign = -1;
            }
            else
            if (dx < 0 && dy < 0)
            {
                sign = 1;
            }
            else
            if (dx < 0 && dy > 0)
            {
                sign = -1;
            }

            foreach (System.Windows.Point rawp in rawProfile)
            {
                //double x = p1.X + (rawp.X * dx);
                //double y = p1.Y + (rawp.Y * height);
                System.Windows.Point o1 = TankTrackUtils.Perpendicular(p1, p2, rawp.X + xo, sign * (rawp.Y + yo) * height);
                rotatedProfile.Add(o1);
            }
        }

        private static void VerticalDown(double xo, double yo, System.Windows.Point p1, System.Windows.Point p2, List<System.Windows.Point> rawProfile, List<System.Windows.Point> rotatedProfile, double height)
        {
            double dy = p2.Y - p1.Y;
            foreach (System.Windows.Point rawp in rawProfile)
            {
                double x = p1.X + ((rawp.Y + yo) * height);
                double y = p1.Y + ((rawp.X + xo) * dy);
                rotatedProfile.Add(new System.Windows.Point(x, y));
            }
        }

        private static void VerticalUp(double xo, double yo, System.Windows.Point p1, System.Windows.Point p2, List<System.Windows.Point> rawProfile, List<System.Windows.Point> rotatedProfile, double height)
        {
            double dy = p2.Y - p1.Y;
            foreach (System.Windows.Point rawp in rawProfile)
            {
                // yes this is meant to look odd
                double x = p1.X - ((rawp.Y + yo) * height);
                double y = p1.Y + ((rawp.X + xo) * dy);
                rotatedProfile.Add(new System.Windows.Point(x, y));
            }
        }

        private void CreateInnerFace(List<System.Windows.Point> ply, int i)
        {
            int v = i + 1;
            if (v == ply.Count)
            {
                v = 0;
            }

            int c0 = AddVertice(ply[i].X, ply[i].Y, 0.0);
            int c1 = AddVertice(ply[i].X, ply[i].Y, trackWidth);
            int c2 = AddVertice(ply[v].X, ply[v].Y, trackWidth);
            int c3 = AddVertice(ply[v].X, ply[v].Y, 0.0);
            Faces.Add(c0);
            Faces.Add(c1);
            Faces.Add(c2);

            Faces.Add(c0);
            Faces.Add(c2);
            Faces.Add(c3);
        }

        private void CreateOutsideFace(List<System.Windows.Point> ply, int i)
        {
            int v = i + 1;
            if (v == ply.Count)
            {
                v = 0;
            }

            int c0 = AddVertice(ply[i].X, ply[i].Y, 0.0);
            int c1 = AddVertice(ply[i].X, ply[i].Y, trackWidth);
            int c2 = AddVertice(ply[v].X, ply[v].Y, trackWidth);
            int c3 = AddVertice(ply[v].X, ply[v].Y, 0.0);
            Faces.Add(c0);
            Faces.Add(c2);
            Faces.Add(c1);

            Faces.Add(c0);
            Faces.Add(c3);
            Faces.Add(c2);
        }

        private void GenerateCentreGuideTrack()
        {
            if (outerPolygon != null)
            {
                outerPolygon.Clear();
                innerPolygon.Clear();
                bool firstCall = true;
                for (int i = 0; i < trackPath.Count; i++)
                {
                    int j = i + 1;
                    if (j >= trackPath.Count)
                    {
                        j = 0;
                    }
                    System.Windows.Point p1 = trackPath[i];
                    System.Windows.Point p2 = trackPath[j];
                    TankTrackUtils.CentreGuideLinkPolygon(p1, p2, ref outerPolygon, ref innerPolygon, firstCall, thickness, guideSize, spudSize);
                    firstCall = false;
                }
            }
        }

        private void GenerateFaces()
        {
            Faces.Clear();
            Vertices.Clear();
            MakeFacesFrommOuterAndInner();
        }

        private void GenerateM1LinkConnectors()
        {
        }

        private void GenerateM1Main()
        {
            if (outerPolygon != null)
            {
                for (int i = 0; i < trackPath.Count; i++)
                {
                    int j = i + 1;
                    if (j >= trackPath.Count)
                    {
                        j = 0;
                    }
                    List<System.Windows.Point> linkProfile = new List<System.Windows.Point>();

                    System.Windows.Point p1 = trackPath[i];
                    System.Windows.Point p2 = trackPath[j];

                    GetLinkPartProfile(p1, p2, ref linkProfile, m1MainPolyCoords, thickness);
                    double partBackZ = 0;
                    double partFrontZ = trackWidth;
                    MakeFacesForLinkPart(linkProfile, partBackZ, partFrontZ);

                    GetLinkPartProfile(p1, p2, ref linkProfile, m1LinkConnectorCoords, thickness + guideSize);
                    partBackZ = trackWidth;
                    partFrontZ = trackWidth + 1;
                    MakeFacesForLinkPart(linkProfile, partBackZ, partFrontZ);

                    GetLinkPartProfile(p1, p2, ref linkProfile, m1LinkConnectorCoords, thickness + guideSize);
                    partBackZ = -1;
                    partFrontZ = 0;
                    MakeFacesForLinkPart(linkProfile, partBackZ, partFrontZ);
                }
            }
        }

        private void GenerateM1Track()
        {
            Faces.Clear();
            Vertices.Clear();
            GenerateM1Main();
            GenerateM1LinkConnectors();
        }

        private void GenerateSimpleTrack(int subType)
        {
            if (outerPolygon != null)
            {
                outerPolygon.Clear();
                innerPolygon.Clear();
                bool firstCall = true;
                for (int i = 0; i < trackPath.Count; i++)
                {
                    int j = i + 1;
                    if (j >= trackPath.Count)
                    {
                        j = 0;
                    }
                    System.Windows.Point p1 = trackPath[i];
                    System.Windows.Point p2 = trackPath[j];
                    switch (subType)
                    {
                        case 0:
                            {
                                SimpleLinkPolygon(p1, p2, ref outerPolygon, ref innerPolygon, firstCall, 1);
                            }
                            break;

                        case 1:
                            {
                                SimpleLinkPolygon(p1, p2, ref outerPolygon, ref innerPolygon, firstCall, -1);
                            }
                            break;
                    }

                    firstCall = false;
                }
            }
        }

        private void GetLinkPartProfile(System.Windows.Point p1, System.Windows.Point p2, ref List<System.Windows.Point> poly, System.Windows.Point[] shape, double size)
        {
            poly.Clear();
            for (int i = 0; i < shape.GetLength(0); i++)
            {
                System.Windows.Point po = shape[i];

                System.Windows.Point o1 = Perpendicular2(p1, p2, po.X, po.Y * size);
                if (localImage == null)
                {
                    // flipping coordinates so have to reverse polygon too
                    o1.Y = -o1.Y;
                }
                else
                {
                    o1.X = ToMM(o1.X);
                    o1.Y = ToMM(-o1.Y);
                }

                poly.Add(o1);
            }
        }

        private void MakeFacesForLinkPart(List<System.Windows.Point> linkProfile, double partBackZ, double partFrontZ)
        {
            // make faces for this single link part
            List<PointF> pf = new List<PointF>();
            foreach (System.Windows.Point p in linkProfile)
            {
                pf.Add(new PointF((float)p.X, (float)p.Y));
            }
            TriangulationPolygon ply = new TriangulationPolygon();
            ply.Points = pf.ToArray();
            List<Triangle> tris = ply.Triangulate();
            foreach (Triangle t in tris)
            {
                int c0 = AddVertice(t.Points[0].X, t.Points[0].Y, partBackZ);
                int c1 = AddVertice(t.Points[1].X, t.Points[1].Y, partBackZ);
                int c2 = AddVertice(t.Points[2].X, t.Points[2].Y, partBackZ);
                Faces.Add(c0);
                Faces.Add(c2);
                Faces.Add(c1);

                c0 = AddVertice(t.Points[0].X, t.Points[0].Y, partFrontZ);
                c1 = AddVertice(t.Points[1].X, t.Points[1].Y, partFrontZ);
                c2 = AddVertice(t.Points[2].X, t.Points[2].Y, partFrontZ);
                Faces.Add(c0);
                Faces.Add(c1);
                Faces.Add(c2);
            }

            for (int k = 0; k < linkProfile.Count; k++)
            {
                int l = k + 1;
                if (l >= linkProfile.Count)
                {
                    l = 0;
                }
                int c0 = AddVertice(linkProfile[k].X, linkProfile[k].Y, partBackZ);
                int c1 = AddVertice(linkProfile[l].X, linkProfile[l].Y, partBackZ);
                int c2 = AddVertice(linkProfile[l].X, linkProfile[l].Y, partFrontZ);
                int c3 = AddVertice(linkProfile[k].X, linkProfile[k].Y, partFrontZ);

                Faces.Add(c0);
                Faces.Add(c1);
                Faces.Add(c2);

                Faces.Add(c0);
                Faces.Add(c2);
                Faces.Add(c3);
            }
        }

        private void MakeFacesFrommOuterAndInner()
        {
            List<System.Windows.Point> otmp = new List<System.Windows.Point>();
            List<System.Windows.Point> itmp = new List<System.Windows.Point>();
            if (outerPolygon != null)
            {
                double top = 0;
                for (int i = 0; i < outerPolygon.Count; i++)
                {
                    if (outerPolygon[i].Y > top)
                    {
                        top = outerPolygon[i].Y;
                    }
                }
                for (int i = 0; i < outerPolygon.Count; i++)
                {
                    if (localImage == null)
                    {
                        // flipping coordinates so have to reverse polygon too
                        otmp.Insert(0, new System.Windows.Point(outerPolygon[i].X, top - outerPolygon[i].Y));
                    }
                    else
                    {
                        double x = ToMM(outerPolygon[i].X);
                        double y = ToMM(top - outerPolygon[i].Y);
                        otmp.Insert(0, new System.Windows.Point(x, y));
                    }
                }

                // generate side triangles so original points are already in list
                for (int i = 0; i < outerPolygon.Count; i++)
                {
                    CreateOutsideFace(otmp, i);
                }
                itmp.Clear();
                for (int i = 0; i < innerPolygon.Count; i++)
                {
                    if (localImage == null)
                    {
                        // flipping coordinates so have to reverse polygon too
                        itmp.Insert(0, new System.Windows.Point(innerPolygon[i].X, top - innerPolygon[i].Y));
                    }
                    else
                    {
                        double x = ToMM(innerPolygon[i].X);
                        double y = ToMM(top - innerPolygon[i].Y);
                        itmp.Insert(0, new System.Windows.Point(x, y));
                    }
                }

                for (int i = 0; i < innerPolygon.Count; i++)
                {
                    CreateInnerFace(itmp, i);
                }

                for (int i = 0; i < innerPolygon.Count; i++)
                {
                    int v = i + 1;
                    if (v == innerPolygon.Count)
                    {
                        v = 0;
                    }

                    int c0 = AddVertice(itmp[i].X, itmp[i].Y, 0.0);
                    int c1 = AddVertice(otmp[i].X, otmp[i].Y, 0.0);
                    int c2 = AddVertice(otmp[v].X, otmp[v].Y, 0.0);
                    int c3 = AddVertice(itmp[v].X, itmp[v].Y, 0.0);
                    Faces.Add(c0);
                    Faces.Add(c2);
                    Faces.Add(c1);

                    Faces.Add(c0);
                    Faces.Add(c3);
                    Faces.Add(c2);
                }

                for (int i = 0; i < innerPolygon.Count; i++)
                {
                    int v = i + 1;
                    if (v == innerPolygon.Count)
                    {
                        v = 0;
                    }

                    int c0 = AddVertice(itmp[i].X, itmp[i].Y, trackWidth);
                    int c1 = AddVertice(otmp[i].X, otmp[i].Y, trackWidth);
                    int c2 = AddVertice(otmp[v].X, otmp[v].Y, trackWidth);
                    int c3 = AddVertice(itmp[v].X, itmp[v].Y, trackWidth);
                    Faces.Add(c0);
                    Faces.Add(c1);
                    Faces.Add(c2);

                    Faces.Add(c0);
                    Faces.Add(c2);
                    Faces.Add(c3);
                }
                CentreVertices();
            }
        }

        private void SimpleLinkPolygon(System.Windows.Point p1, System.Windows.Point p2, ref List<System.Windows.Point> outter, ref List<System.Windows.Point> inner, bool firstCall, int spudSign)
        {
            double spudDim = spudSign * spudSize;
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;

            if (dx == 0 && dy != 0)
            {
                if (p2.Y > p1.Y)
                {
                    // vertical downwards
                    if (firstCall)
                    {
                        outter.Add(new System.Windows.Point(p1.X + thickness, p1.Y));
                    }
                    outter.Add(new System.Windows.Point(p1.X + thickness, p1.Y + 0.75 * dy));
                    outter.Add(new System.Windows.Point(p1.X + thickness - spudDim, p1.Y + 0.87 * dy));
                    outter.Add(new System.Windows.Point(p1.X + thickness, p2.Y));

                    if (firstCall)
                    {
                        inner.Add(new System.Windows.Point(p1.X - thickness, p1.Y));
                    }
                    inner.Add(new System.Windows.Point(p1.X - thickness, p1.Y + 0.75 * dy));
                    inner.Add(new System.Windows.Point(p1.X - thickness, p1.Y + 0.87 * dy));
                    inner.Add(new System.Windows.Point(p1.X - thickness, p2.Y));
                }
                else
                {
                    // vertical upwards
                    if (firstCall)
                    {
                        outter.Add(new System.Windows.Point(p1.X - thickness, p1.Y));
                    }
                    outter.Add(new System.Windows.Point(p1.X - thickness, p1.Y + 0.75 * dy));
                    outter.Add(new System.Windows.Point(p1.X - thickness - spudDim, p1.Y + 0.87 * dy));
                    outter.Add(new System.Windows.Point(p1.X - thickness, p2.Y));

                    if (firstCall)
                    {
                        inner.Add(new System.Windows.Point(p1.X + thickness, p1.Y));
                    }
                    inner.Add(new System.Windows.Point(p1.X + thickness, p1.Y + 0.75 * dy));
                    inner.Add(new System.Windows.Point(p1.X + thickness, p1.Y + 0.87 * dy));
                    inner.Add(new System.Windows.Point(p1.X + thickness, p2.Y));
                }
            }
            else
            if (dy == 0)
            {
                if (p2.X - p1.X > 0)
                {
                    // Horizontal Left to right
                    if (firstCall)
                    {
                        outter.Add(new System.Windows.Point(p1.X, p1.Y - thickness));
                    }
                    outter.Add(new System.Windows.Point(p1.X + 0.75 * dx, p1.Y - thickness));
                    outter.Add(new System.Windows.Point(p1.X + 0.87 * dx, p1.Y - thickness - spudDim));
                    outter.Add(new System.Windows.Point(p2.X, p2.Y - thickness));

                    if (firstCall)
                    {
                        inner.Add(new System.Windows.Point(p1.X, p1.Y + thickness));
                    }
                    inner.Add(new System.Windows.Point(p1.X + 0.75 * dx, p1.Y + thickness));
                    inner.Add(new System.Windows.Point(p1.X + 0.87 * dx, p1.Y + thickness));
                    inner.Add(new System.Windows.Point(p2.X, p2.Y + thickness));
                }
                else
                {
                    // Horizontal right to Left
                    if (firstCall)
                    {
                        outter.Add(new System.Windows.Point(p1.X, p1.Y - +thickness));
                    }
                    outter.Add(new System.Windows.Point(p1.X + 0.75 * dx, p1.Y + thickness));
                    outter.Add(new System.Windows.Point(p1.X + 0.87 * dx, p1.Y + thickness + spudDim));
                    outter.Add(new System.Windows.Point(p2.X, p2.Y + thickness));

                    if (firstCall)
                    {
                        inner.Add(new System.Windows.Point(p1.X, p1.Y - thickness));
                    }
                    inner.Add(new System.Windows.Point(p1.X + 0.75 * dx, p1.Y - thickness));
                    inner.Add(new System.Windows.Point(p1.X + 0.87 * dx, p1.Y - thickness));
                    inner.Add(new System.Windows.Point(p2.X, p2.Y - thickness));
                }
            }
            else
            {
                double sign = -1;
                if (dx > 0 && dy < 0)
                {
                    sign = 1;
                }
                else
                if (dx > 0 && dy > 0)
                {
                    sign = -1;
                }
                else
                if (dx < 0 && dy < 0)
                {
                    sign = 1;
                }
                else
                if (dx < 0 && dy > 0)
                {
                    sign = -1;
                }
                System.Windows.Point o1 = TankTrackUtils.Perpendicular(p1, p2, 0.0, -sign * thickness);
                System.Windows.Point o2 = TankTrackUtils.Perpendicular(p1, p2, 0.75, -sign * thickness);
                System.Windows.Point o3 = TankTrackUtils.Perpendicular(p1, p2, 0.87, -sign * (thickness + spudDim));
                System.Windows.Point o4 = TankTrackUtils.Perpendicular(p1, p2, 1.0, -sign * thickness);
                if (firstCall)
                {
                    outter.Add(o1);
                }
                outter.Add(o2);
                outter.Add(o3);
                outter.Add(o4);

                System.Windows.Point i1 = TankTrackUtils.Perpendicular(p1, p2, 0.0, sign * thickness);
                System.Windows.Point i2 = TankTrackUtils.Perpendicular(p1, p2, 0.75, sign * thickness);
                System.Windows.Point i3 = TankTrackUtils.Perpendicular(p1, p2, 0.87, sign * thickness);
                System.Windows.Point i4 = TankTrackUtils.Perpendicular(p1, p2, 1.0, sign * thickness);
                if (firstCall)
                {
                    inner.Add(i1);
                }
                inner.Add(i2);
                inner.Add(i3);
                inner.Add(i4);
            }
        }
    }
}