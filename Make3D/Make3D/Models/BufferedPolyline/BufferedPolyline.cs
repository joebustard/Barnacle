using System;
using System.Collections.Generic;
using System.Windows;

namespace Barnacle.Models.BufferedPolyline
{
    public class BufferedPolyline
    {
        private const double EquityTolerance = 0.0000001d;

        public BufferedPolyline()
        {
            coreSegments = new List<CoreSegment>();
            sideSegments = new List<Segment>();
            BufferRadius = 1;
        }

        public BufferedPolyline(List<Point> pnts)
        {
            sideSegments = new List<Segment>();
            BufferRadius = 2;
            coreSegments = new List<CoreSegment>();
            if (pnts != null && pnts.Count > 1)
            {
                for (int i = 0; i < pnts.Count - 1; i++)
                {
                    CoreSegment seg = new CoreSegment(new Point(pnts[i].X, pnts[i].Y), new Point(pnts[i + 1].X, pnts[i + 1].Y));
                    coreSegments.Add(seg);
                }
            }
        }

        private double bufferRadius;

        public double BufferRadius
        {
            get { return bufferRadius; }

            set
            {
                if (value != bufferRadius && value > 0)
                {
                    bufferRadius = value;
                }
            }
        }

        private List<CoreSegment> coreSegments { get; set; }
        private List<Segment> sideSegments { get; set; }

        public List<Point> GenerateBufferOutline(double br)
        {
            if (br > 0)
            {
                BufferRadius = br;
            }
            else
            {
                BufferRadius = 1;
            }
            return GenerateBufferOutline();
        }

        /// <summary>
        /// Assuming we have been given a polyline and its been stored in the Coresegments, generate
        /// the outline, offset by BufferRadius
        /// </summary>
        /// <returns></returns>
        public List<Point> GenerateBufferOutline()
        {
            List<Point> outline = new List<Point>();
            /*
            for reference,
            points are orientated around the core like this

            p0 ------------------>--------- p1

            ==================================

            p1 -----------<---------------- p0
            */
            if (coreSegments != null)
            {
                foreach (CoreSegment seg in coreSegments)
                {
                    Point sp = seg.Start;
                    Point se = seg.End;

                    GetOffsetEdge(sp, se);
                }
                for (int si = coreSegments.Count - 1; si >= 0; si--)
                {
                    CoreSegment seg = coreSegments[si];
                    Point sp = seg.End;
                    Point se = seg.Start;
                    GetOffsetEdge(sp, se, false);
                }

                FixOutlineConnections(sideSegments);

                ExtractOutlinePoints(outline, sideSegments);
            }
            return outline;
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

        /// <summary>
        /// Fill the gap between the start of one line segment and an other with segments following
        /// an arc from one point to another.
        /// </summary>
        /// <param name="extensions"></param>
        /// <param name="centre"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="outbound"></param>
        private void CreateCurvedLinks(List<Segment> extensions, Point centre, Point p1, Point p2, bool outbound)
        {
            // Generate a curve of segments
            double dy1 = p1.Y - centre.Y;
            double dx1 = p1.X - centre.X;

            double startAngle = Math.Atan2(dy1, dx1);

            double dy2 = p2.Y - centre.Y;
            double dx2 = p2.X - centre.X;

            double endAngle = Math.Atan2(dy2, dx2);
            int numdiv = 4;
            double theta;
            Point? oldp = null;

            if (startAngle < endAngle)
            {
                startAngle += Math.PI * 2;
            }
            double da = (startAngle - endAngle) / numdiv;
            for (int i = 0; i < numdiv; i++)
            {
                theta = startAngle - (da * i);

                Point p = new Point();
                p.X = BufferRadius * Math.Cos(theta) + centre.X;
                p.Y = BufferRadius * Math.Sin(theta) + centre.Y;
                if (oldp != null)
                {
                    // dont care about rotation centre for this, just set to dummy
                    Segment sg = new Segment((Point)oldp, p, new Point(0, 0), outbound);
                    extensions.Add(sg);
                }
                oldp = p;
            }
        }

        /// <summary>
        /// Add a new point to the end of a list as long as it doesn't cause duplicates
        /// </summary>
        /// <param name="outline"></param>
        /// <param name="pt"></param>
        private void AddNoDup(List<Point> outline, Point pt)
        {
            int last = outline.Count - 1;
            if (!PointsEqual(outline[last], pt))
            {
                outline.Add(pt);
            }
        }

        private void ExtractOutlinePoints(List<Point> outline, List<Segment> side)
        {
            foreach (Segment seg in side)
            {
                if (outline.Count == 0)
                {
                    outline.Add(seg.Start);
                    outline.Add(seg.End);
                }
                else
                {
                    AddNoDup(outline, seg.Start);
                    AddNoDup(outline, seg.End);
                }
                if (seg.Extensions != null)
                {
                    ExtractOutlinePoints(outline, seg.Extensions);
                }
            }
        }

        /// <summary>
        /// If two edges are already linked up do nothing If they intercept set the end of one and
        /// the start of the other to the interception point so they are linked up If there ia gap
        /// between the points fill it with small segments that go round the circumference of a
        /// circle between the gaps
        /// </summary>
        /// <param name="side"></param>
        private void FixOutlineConnections(List<Segment> side)
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
                    }
                    else
                    {
                        side[i].Extensions = new List<Segment>();
                        if (side[i].Outbound == side[i + 1].Outbound)
                        {
                            // Add curve section to the segment as extension curves

                            CreateCurvedLinks(side[i].Extensions, side[i].ExtensionCentre, side[i].End, side[i + 1].Start, side[i].Outbound);
                        }
                        else
                        {
                            // probably at the end of the outbound so just close off with a straight
                            // edge only the start and ens matter
                            Segment ext = new Segment(side[i].End, side[i + 1].Start, side[i].ExtensionCentre, true);
                        }
                    }
                }
            }
        }

        private void DumpSegments(List<Segment> side)
        {
            foreach (Segment seg in side)
            {
                System.Diagnostics.Debug.WriteLine($"{seg.Start.X},{seg.Start.Y}   {seg.End.X},{seg.End.Y}");

                if (seg.Extensions != null)
                {
                    foreach (Segment ext in seg.Extensions)
                    {
                        System.Diagnostics.Debug.WriteLine($"{ext.Start.X},{ext.Start.Y} {ext.End.X},{ext.End.Y}");
                    }
                }
            }
        }

        /// <summary>
        /// Given a line defined by a start point and end point Calculate the position of a line,
        /// offset by the buffer radius to the left Add it to the list of sides
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="se"></param>
        /// <param name="outbound"></param>
        private void GetOffsetEdge(Point sp, Point se, bool outbound = true)
        {
            double dx = se.X - sp.X;
            double dy = se.Y - sp.Y;
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

                p0.X = sp.X + dx2;
                p0.Y = sp.Y + dy2;

                p1.X = se.X + dx2;
                p1.Y = se.Y + dy2;
            }
            else
            {
                // the segment is horizontal
                if (sp.X < se.X)
                {
                    p0.X = sp.X;
                    p0.Y = sp.Y + BufferRadius;

                    p1.X = se.X;
                    p1.Y = se.Y + BufferRadius;
                }
                else
                {
                    p0.X = sp.X;
                    p0.Y = sp.Y - BufferRadius;

                    p1.X = se.X;
                    p1.Y = se.Y - BufferRadius;
                }
            }

            Segment left = new Segment(p0, p1, se, outbound);

            sideSegments.Add(left);
        }

        private bool IsEqual(double d1, double d2)
        {
            return Math.Abs(d1 - d2) <= EquityTolerance;
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
    }
}