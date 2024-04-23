using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Barnacle.Models.BufferedPolyline
{
    public class BufferedPolyline
    {
        public List<CoreSegment> CoreSegments { get; set; }
        public List<Segment> SideA { get; set; }
        public List<Segment> SideB { get; set; }

        public double BufferRadius
        {
            get; set;
        }

        public BufferedPolyline()
        {
            CoreSegments = new List<CoreSegment>();
            SideA = new List<Segment>();
            SideB = new List<Segment>();
            BufferRadius = 1;
        }

        public BufferedPolyline(List<Point> pnts)
        {
            SideA = new List<Segment>();
            SideB = new List<Segment>();
            BufferRadius = 2;
            CoreSegments = new List<CoreSegment>();
            if (pnts != null && pnts.Count > 1)
            {
                for (int i = 0; i < pnts.Count - 1; i++)
                {
                    CoreSegment seg = new CoreSegment(new Point(pnts[i].X, pnts[i].Y), new Point(pnts[i + 1].X, pnts[i + 1].Y));
                    CoreSegments.Add(seg);
                }
            }
        }

        public void GenerateBuffer(double br)
        {
            BufferRadius = br;
            GenerateBuffer();
        }

        public void GenerateBuffer()
        {
            /*
            for reference points are orientated around the core like this

            p0 ------------------>--------- p1

            ==================================

            p3 -----------<---------------- p2
            */
            if (CoreSegments != null)
            {
                foreach (CoreSegment seg in CoreSegments)
                {
                    // as far as we know at this point the segment doesn't need to fill in its end
                    // with curve points
                    seg.FillA = false;
                    seg.FillB = false;
                    double dx = seg.End.X - seg.Start.X;
                    double dy = seg.End.Y - seg.Start.Y;
                    Point p0 = new Point(0, 0);
                    Point p1 = new Point(0, 0);
                    Point p2 = new Point(0, 0);
                    Point p3 = new Point(0, 0);

                    // is the line horizontal
                    if (dy != 0)
                    {
                        double len = Math.Sqrt((dx * dx) + (dy * dy));
                        double mag = BufferRadius / len;
                        double dx2 = -dy * mag;
                        double dy2 = dx * mag;

                        p0.X = seg.Start.X + dx2;
                        p0.Y = seg.Start.Y + dy2;

                        p1.X = seg.End.X + dx2;
                        p1.Y = seg.End.Y + dy2;

                        dx2 = dy * mag;
                        dy2 = -dx * mag;

                        p2.X = seg.End.X + dx2;
                        p2.Y = seg.End.Y + dy2;

                        p3.X = seg.Start.X + dx2;
                        p3.Y = seg.Start.Y + dy2;
                    }
                    else
                    {
                        // the segment is horizontal
                        p0.X = seg.Start.X;
                        p0.Y = seg.Start.Y + BufferRadius;

                        p1.X = seg.End.X;
                        p1.Y = seg.Start.Y + BufferRadius;

                        p2.X = seg.End.X;
                        p2.Y = seg.End.Y - BufferRadius;

                        p3.X = seg.Start.X;
                        p3.Y = seg.Start.Y - BufferRadius;
                    }

                    Segment left = new Segment(p0, p1);
                    SideA.Add(left);
                    Segment right = new Segment(p2, p3);
                    SideB.Insert(0, right);
                    System.Diagnostics.Debug.WriteLine($"Segment corners {p0.X},{p0.Y}   {p1.X},{p1.Y}  {p2.X},{p2.Y}  {p3.X},{p3.Y}");
                }

                CheckForIntercepts(SideA, true);
                CheckForIntercepts(SideB, false);
            }
        }

        private void CheckForIntercepts(List<Segment> side, bool sideIsA)
        {
            for (int i = 0; i < side.Count - 1; i++)
            {
                // ignore if the end of the first side segment just matches the start of the next
                if (!PointsEqual(side[i].End, side[i + 1].Start))
                {
                    Point? crossPoint = GetIntersectionPoint(side[i].Start, side[i].End, side[i + 1].Start, side[i + 1].End);
                    if (crossPoint != null)
                    {
                        side[i].End = (Point)crossPoint;
                        side[i + 1].Start = (Point)crossPoint;
                        System.Diagnostics.Debug.WriteLine($"Move end of segment");
                    }
                    else
                    {
                        // need to fill gap between
                        if (sideIsA)
                        {
                            CoreSegments[i].FillA = true;
                        }
                        else
                        {
                            CoreSegments[side.Count - i - 1].FillB = true;
                        }
                    }
                }
            }
        }

        private bool PointsEqual(Point p1, Point p2)
        {
            bool res = false;
            if (IsEqual(p1.X, p2.X) && IsEqual(p1.Y, p2.Y))
            {
                res = true;
            }
            return res;
        }

        private const double EquityTolerance = 0.0000001d;

        private bool IsEqual(double d1, double d2)
        {
            return Math.Abs(d1 - d2) <= EquityTolerance;
        }

        //math logic from http://www.wyrmtale.com/blog/2013/115/2d-line-intersection-in-c
        public Point? GetIntersectionPoint(Point l1p1, Point l1p2, Point l2p1, Point l2p2)
        {
            double A1 = l1p2.Y - l1p1.Y;
            double B1 = l1p1.X - l1p2.X;
            double C1 = A1 * l1p1.X + B1 * l1p1.Y;
            double A2 = l2p2.Y - l2p1.Y;
            double B2 = l2p1.X - l2p2.X;
            double C2 = A2 * l2p1.X + B2 * l2p1.Y;
            //lines are parallel
            double det = A1 * B2 - A2 * B1;
            if (IsEqual(det, 0d))
            {
                return null; //parallel lines
            }
            else
            {
                double x = (B2 * C1 - B1 * C2) / det;
                double y = (A1 * C2 - A2 * C1) / det;
                bool online1 = ((Math.Min(l1p1.X, l1p2.X) < x || IsEqual(Math.Min(l1p1.X, l1p2.X), x))
                    && (Math.Max(l1p1.X, l1p2.X) > x || IsEqual(Math.Max(l1p1.X, l1p2.X), x))
                    && (Math.Min(l1p1.Y, l1p2.Y) < y || IsEqual(Math.Min(l1p1.Y, l1p2.Y), y))
                    && (Math.Max(l1p1.Y, l1p2.Y) > y || IsEqual(Math.Max(l1p1.Y, l1p2.Y), y))
                    );
                bool online2 = ((Math.Min(l2p1.X, l2p2.X) < x || IsEqual(Math.Min(l2p1.X, l2p2.X), x))
                    && (Math.Max(l2p1.X, l2p2.X) > x || IsEqual(Math.Max(l2p1.X, l2p2.X), x))
                    && (Math.Min(l2p1.Y, l2p2.Y) < y || IsEqual(Math.Min(l2p1.Y, l2p2.Y), y))
                    && (Math.Max(l2p1.Y, l2p2.Y) > y || IsEqual(Math.Max(l2p1.Y, l2p2.Y), y))
                    );
                if (online1 && online2)
                    return new Point(x, y);
            }
            return null; //intersection is at out of at least one segment.
        }
    }
}