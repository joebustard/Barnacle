﻿// **************************************************************************
// *   Copyright (c) 2024 Joe Bustard <barnacle3d@gmailcom>                  *
// *                                                                         *
// *   This file is part of the Barnacle 3D application.                     *
// *                                                                         *
// *   This application is free software. You can redistribute it and/or     *
// *   modify it under the terms of the GNU Library General Public           *
// *   License as published by the Free Software Foundation. Either          *
// *   version 2 of the License, or (at your option) any later version.      *
// *                                                                         *
// *   This application is distributed in the hope that it will be useful,   *
// *   but WITHOUT ANY WARRANTY. Without even the implied warranty of        *
// *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
// *   GNU Library General Public License for more details.                  *
// *                                                                         *
// *************************************************************************

using System;
using System.Collections.Generic;
using System.Windows;

namespace Barnacle.Models.BufferedPolyline
{
    public class BufferedPolyline
    {
        private const double EquityTolerance = 0.0000001d;

        private double bufferRadius;

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

        public double BufferRadius
        {
            get
            {
                return bufferRadius;
            }

            set
            {
                if (value != bufferRadius && value > 0)
                {
                    bufferRadius = value;
                }
            }
        }

        public int LastOutBoundIndex
        {
            get; set;
        }

        private List<CoreSegment> coreSegments
        {
            get; set;
        }

        private List<Segment> sideSegments
        {
            get; set;
        }

        public List<CurvePoint> GenerateBufferCurvePoints()
        {
            sideSegments.Clear();
            LastOutBoundIndex = -1;
            List<CurvePoint> curvePoints = new List<CurvePoint>();
            if (coreSegments != null)
            {
                // construct the raw out bound edges
                foreach (CoreSegment seg in coreSegments)
                {
                    Point sp = new Point(seg.Start.X, seg.Start.Y);
                    Point se = new Point(seg.End.X, seg.End.Y);

                    GetOffsetEdge(sp, se);
                }
                // construct the raw inbound edges
                for (int si = coreSegments.Count - 1; si >= 0; si--)
                {
                    CoreSegment seg = coreSegments[si];
                    Point sp = new Point(seg.End.X, seg.End.Y);
                    Point se = new Point(seg.Start.X, seg.Start.Y);
                    GetOffsetEdge(sp, se, false);

                    // twin the new inbound with its equivalent out bound
                    sideSegments[sideSegments.Count - 1].Twin = si;
                    sideSegments[si].Twin = sideSegments.Count - 1;
                }

                FixOutlineConnections(sideSegments);

                // create the first point of the outline
                CurvePoint cp = new CurvePoint();
                cp.radius = bufferRadius;
                cp.point = new Point(coreSegments[0].Start.X, coreSegments[0].Start.Y);
                cp.direction = new Vector(coreSegments[0].End.X - coreSegments[0].Start.X, coreSegments[0].End.Y - coreSegments[0].Start.Y);
                cp.angle = Math.Atan2(cp.direction.Y, cp.direction.X);
                curvePoints.Add(cp);
                LoggerLib.Logger.LogLine($"GenerateBufferCurvePoints ");

                for (int i = 0; i < coreSegments.Count; i++)
                {
                    int twin = sideSegments[i].Twin;
                    bool addEnd = true;
                    if (sideSegments[twin].Extensions != null && sideSegments[twin].Extensions.Count > 1)
                    {
                        LoggerLib.Logger.LogLine($"Twin  {twin} has {sideSegments[twin].Extensions.Count} extensions");
                        Segment opposite = sideSegments[i];
                        Point opPoint = new Point(opposite.Start.X, opposite.Start.Y);
                        for (int j = sideSegments[twin].Extensions.Count - 1; j > 0; j--)
                        {
                            Segment fs = sideSegments[twin].Extensions[j];
                            Point midPoint = new Point(0, 0);
                            midPoint.X = opPoint.X + (fs.End.X - opPoint.X) * 0.5;
                            midPoint.Y = opPoint.Y + (fs.End.Y - opPoint.Y) * 0.5;

                            Segment fs2 = sideSegments[twin].Extensions[j - 1];
                            Point midPoint2 = new Point(0, 0);
                            midPoint2.X = opPoint.X + (fs2.End.X - opPoint.X) * 0.5;
                            midPoint2.Y = opPoint.Y + (fs2.End.Y - opPoint.Y) * 0.5;
                            cp.radius = Distance(midPoint, opPoint);
                            cp.point = new Point(midPoint.X, midPoint.Y);
                            cp.direction = new Vector(midPoint2.X - midPoint.X, midPoint2.Y - midPoint.Y);
                            cp.angle = Math.Atan2(cp.direction.Y, cp.direction.X);
                            curvePoints.Add(cp);
                        }
                    }
                    LoggerLib.Logger.LogLine($"Check SideSegment {i}");
                    // does the side segment to the left have any curve extensions
                    if (sideSegments[i].Extensions != null && sideSegments[i].Extensions.Count > 1)
                    {
                        LoggerLib.Logger.LogLine($"Side {i} has {sideSegments[i].Extensions.Count} extensions");
                        Segment opposite = sideSegments[twin];
                        Point midPoint;
                        Point midPoint2 = new Point(0, 0);
                        Point opPoint = opposite.Start;
                        for (int j = 0; j < sideSegments[i].Extensions.Count - 1; j++)
                        {
                            Segment fs = sideSegments[i].Extensions[j];
                            midPoint = new Point(0, 0);
                            midPoint.X = opPoint.X + (fs.Start.X - opPoint.X) * 0.5;
                            midPoint.Y = opPoint.Y + (fs.Start.Y - opPoint.Y) * 0.5;

                            Segment fs2 = sideSegments[i].Extensions[j + 1];
                            midPoint2 = new Point(0, 0);
                            midPoint2.X = opPoint.X + (fs2.Start.X - opPoint.X) * 0.5;
                            midPoint2.Y = opPoint.Y + (fs2.Start.Y - opPoint.Y) * 0.5;
                            cp.radius = Distance(midPoint, opPoint);
                            cp.point = new Point(midPoint.X, midPoint.Y);
                            cp.direction = new Vector(midPoint2.X - midPoint.X, midPoint2.Y - midPoint.Y);
                            cp.angle = Math.Atan2(cp.direction.Y, cp.direction.X);
                            curvePoints.Add(cp);
                            if (sideSegments[i].Outbound)
                            {
                                LastOutBoundIndex = curvePoints.Count;
                            }
                        }
                        if (i < coreSegments.Count - 1)
                        {
                            LoggerLib.Logger.LogLine($"Adding cp at midpoint2");
                            cp.radius = bufferRadius;
                            cp.point = new Point(midPoint2.X, midPoint2.Y);
                            cp.direction = new Vector(coreSegments[i + 1].End.X - cp.point.X, coreSegments[i + 1].End.Y - cp.point.Y);
                            cp.angle = Math.Atan2(cp.direction.Y, cp.direction.X);
                            curvePoints.Add(cp);
                            if (sideSegments[i].Outbound)
                            {
                                LastOutBoundIndex = curvePoints.Count;
                            }
                        }
                        addEnd = false;
                    }

                    if (addEnd)
                    {
                        LoggerLib.Logger.LogLine($"Side {i} no extensions");
                        // no extensions at all
                        cp.radius = bufferRadius;
                        cp.point = new Point(coreSegments[i].End.X, coreSegments[i].End.Y);
                        cp.direction = new Vector(coreSegments[i].End.X - coreSegments[i].Start.X, coreSegments[i].End.Y - coreSegments[i].Start.Y);
                        cp.angle = Math.Atan2(cp.direction.Y, cp.direction.X);
                        curvePoints.Add(cp);
                        if (sideSegments[i].Outbound)
                        {
                            LastOutBoundIndex = curvePoints.Count;
                        }
                    }
                }
                LoggerLib.Logger.LogLine($"add end");
                int last = coreSegments.Count - 1;
                cp.radius = bufferRadius;
                cp.point = new Point(coreSegments[last].End.X, coreSegments[last].End.Y);
                cp.direction = new Vector(coreSegments[last].End.X - coreSegments[last].Start.X, coreSegments[last].End.Y - coreSegments[last].Start.Y);
                cp.angle = Math.Atan2(cp.direction.Y, cp.direction.X);
                curvePoints.Add(cp);

                LoggerLib.Logger.LogLine("-------------");
                foreach (CurvePoint cp1 in curvePoints)
                {
                    LoggerLib.Logger.LogLine($"cp {cp1.point.X},{cp1.point.Y}, v= {cp1.direction.X},{cp1.direction.Y}, r = {cp1.radius}, a = {cp1.angle}");
                }
            }
            return curvePoints;
        }

        public List<Point> GenerateBufferOutline(double br)
        {
            LastOutBoundIndex = -1;
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
            LastOutBoundIndex = -1;
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
            int numdiv = 12;
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

        private double Distance(Point p1, Point p2)
        {
            double dist = (p2.X - p1.X) * (p2.X - p1.X) +
                 (p2.Y - p1.Y) * (p2.Y - p1.Y);
            return Math.Sqrt(dist);
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

        private void ExtractOutlinePoints(List<Point> outline, List<Segment> side)
        {
            foreach (Segment seg in side)
            {
                if (outline.Count == 0)
                {
                    outline.Add(seg.Start);
                    outline.Add(seg.End);
                    if (seg.Outbound)
                    {
                        LastOutBoundIndex = outline.Count - 1;
                    }
                }
                else
                {
                    AddNoDup(outline, seg.Start);
                    AddNoDup(outline, seg.End);
                    if (seg.Outbound)
                    {
                        LastOutBoundIndex = outline.Count - 1;
                    }
                }
                if (seg.Extensions != null)
                {
                    ExtractOutlinePoints(outline, seg.Extensions);
                    if (seg.Outbound)
                    {
                        LastOutBoundIndex = outline.Count - 1;
                    }
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

        public struct CurvePoint
        {
            public double angle;
            public Vector direction;
            public Point point;
            public double radius;
        }
    }
}