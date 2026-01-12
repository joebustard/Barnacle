using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace MakerLib
{
    public class HoneyCombGrilleMaker : MakerBase
    {
        private double beamLength;
        private double beamWidth;

        // stack used for the centre points of the forks used to make the skeleton
        private List<Point> centreStack;

        private double edgeSize;
        private double grilleHeight;
        private double grilleLength;
        private double grilleThickness;
        private double leftThicknessAdjustment = 0.002;
        private double rightThicknessAdjustment = 0.003;
        private bool showEdge;
        private List<SkeletonSegment> skeletonSegments;
        private double straightThicknessAdjustment = 0.001;
        private List<Point> visited;

        public HoneyCombGrilleMaker()
        {
            paramLimits = new ParamLimits();
            SetLimits();
            centreStack = new List<Point>();
            skeletonSegments = new List<SkeletonSegment>();
            visited = new List<Point>();
        }

        private enum SegIncline
        {
            Straight,
            Left,
            Right
        };

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            Point3D minPoint = new Point3D(-1, -1, -1);
            Point3D maxPoint = new Point3D(grilleLength + 1, grilleHeight + 1, grilleThickness + 1);
            CreateOctree(minPoint, maxPoint);
            // calculate offsets to fork points
            double forkOff = beamLength * Math.Sin(Math.PI / 4.0);

            centreStack.Clear();
            visited.Clear();
            skeletonSegments.Clear();
            AddForkCentre(grilleLength / 2, grilleHeight / 2);
            do
            {
                Point cp = PullStack();
                if (!visited.Contains(cp))
                {
                    Point p0 = new Point(cp.X, cp.Y - beamLength);
                    Point p1 = new Point(cp.X - forkOff, cp.Y + forkOff);
                    Point p2 = new Point(cp.X + forkOff, cp.Y + forkOff);
                    AddSkeletonSegment(p0, cp, SegIncline.Straight);
                    AddSkeletonSegment(cp, p1, SegIncline.Left);
                    AddSkeletonSegment(cp, p2, SegIncline.Right);
                    visited.Add(cp);

                    // calcuate the centres of the neigbouring sets of forks
                    Point ncp0 = new Point(p0.X - forkOff, p0.Y - forkOff);
                    Point ncp1 = new Point(p0.X + forkOff, p0.Y - forkOff);
                    Point ncp2 = new Point(p1.X, p1.Y + beamLength);
                    Point ncp3 = new Point(p2.X, p2.Y + beamLength);
                    // if any of these are inside the shape then add them to the queue
                    if (Inside(ncp0) || Inside(ncp1) || Inside(ncp2) || Inside(ncp3))
                    {
                        AddForkCentre(ncp0.X, ncp0.Y);
                        AddForkCentre(ncp1.X, ncp1.Y);
                        AddForkCentre(ncp2.X, ncp2.Y);
                        AddForkCentre(ncp3.X, ncp3.Y);
                    }
                }
            } while (centreStack.Count > 0);

            SolidsFromSegments(skeletonSegments, beamWidth);
            if (showEdge)
            {
                CreateEdge();
            }
        }

        public void SetValues(double grilleLength,
                              double grillHeight,
                              double grillThickness,
                              double beamLength,
                              double beamWidth,
                              double edgeSize,
                              bool showEdge)
        {
            this.grilleLength = grilleLength;
            this.grilleHeight = grillHeight;
            this.grilleThickness = grillThickness;
            this.beamLength = beamLength;
            this.beamWidth = beamWidth;
            this.edgeSize = edgeSize;
            this.showEdge = showEdge;
            if (this.edgeSize < beamWidth + 0.1)
            {
                this.edgeSize = beamWidth + 0.1;
            }
        }

        private void AddForkCentre(double x, double y)
        {
            centreStack.Add(new Point(x, y));
        }

        private void AddSkeletonSegment(Point p1, Point p2, SegIncline incline)
        {
            bool found = false;
            foreach (SkeletonSegment s in skeletonSegments)
            {
                if (dequals(s.start.X, p1.X) && dequals(s.start.Y, p1.Y))
                {
                    if (dequals(s.end.X, p2.X) && dequals(s.end.Y, p2.Y))
                    {
                        found = true;
                    }
                }
                else
                if (dequals(s.start.X, p2.X) && dequals(s.start.Y, p2.Y))
                {
                    if (dequals(s.end.X, p1.X) && dequals(s.end.Y, p1.Y))
                    {
                        found = true;
                    }
                }
            }
            if (!found)
            {
                SkeletonSegment sk = new SkeletonSegment();
                sk.start = p1;
                sk.end = p2;
                sk.incline = incline;
                sk.clipped = false;
                skeletonSegments.Add(sk);
            }
        }

        private void CreateEdge()
        {
            Point p0 = new Point(0, 0);
            Point p1 = new Point(0, grilleHeight);
            Point p2 = new Point(grilleLength, grilleHeight);
            Point p3 = new Point(grilleLength, 0);

            Point p4 = new Point(edgeSize, edgeSize);
            Point p5 = new Point(edgeSize, grilleHeight - edgeSize);
            Point p6 = new Point(grilleLength - edgeSize, grilleHeight - edgeSize);
            Point p7 = new Point(grilleLength - edgeSize, edgeSize);

            int v0 = AddVerticeOctTree(p0.X, p0.Y, 0);
            int v1 = AddVerticeOctTree(p1.X, p1.Y, 0);
            int v2 = AddVerticeOctTree(p2.X, p2.Y, 0);
            int v3 = AddVerticeOctTree(p3.X, p3.Y, 0);

            int v4 = AddVerticeOctTree(p4.X, p4.Y, 0);
            int v5 = AddVerticeOctTree(p5.X, p5.Y, 0);
            int v6 = AddVerticeOctTree(p6.X, p6.Y, 0);
            int v7 = AddVerticeOctTree(p7.X, p7.Y, 0);

            AddFace(v0, v1, v4);
            AddFace(v4, v1, v5);

            AddFace(v5, v1, v6);
            AddFace(v6, v1, v2);

            AddFace(v7, v6, v2);
            AddFace(v7, v2, v3);

            AddFace(v0, v4, v7);
            AddFace(v3, v0, v7);

            int v8 = AddVerticeOctTree(p0.X, p0.Y, grilleThickness);
            int v9 = AddVerticeOctTree(p1.X, p1.Y, grilleThickness);
            int v10 = AddVerticeOctTree(p2.X, p2.Y, grilleThickness);
            int v11 = AddVerticeOctTree(p3.X, p3.Y, grilleThickness);

            int v12 = AddVerticeOctTree(p4.X, p4.Y, grilleThickness);
            int v13 = AddVerticeOctTree(p5.X, p5.Y, grilleThickness);
            int v14 = AddVerticeOctTree(p6.X, p6.Y, grilleThickness);
            int v15 = AddVerticeOctTree(p7.X, p7.Y, grilleThickness);

            AddFace(v8, v12, v9);
            AddFace(v12, v13, v9);

            AddFace(v13, v14, v9);
            AddFace(v14, v10, v9);

            AddFace(v15, v10, v14);
            AddFace(v15, v11, v10);

            AddFace(v8, v15, v12);
            AddFace(v11, v15, v8);

            // close left outside
            AddFace(v0, v8, v1);
            AddFace(v8, v9, v1);

            // left inside'
            AddFace(v4, v5, v12);
            AddFace(v12, v5, v13);

            // top outside
            AddFace(v1, v9, v2);
            AddFace(v9, v10, v2);

            // top inside
            AddFace(v13, v5, v6);
            AddFace(v13, v6, v14);

            // Right outside
            AddFace(v2, v10, v3);
            AddFace(v3, v10, v11);

            // right inside
            AddFace(v6, v7, v15);
            AddFace(v6, v15, v14);

            // bottom outside
            AddFace(v3, v11, v12);
            AddFace(v3, v12, v0);

            // bottom inside
            AddFace(v12, v7, v4);
            AddFace(v12, v11, v7);
        }

        private bool dequals(double d1, double d2)
        {
            bool res = false;
            if (Math.Abs(d1 - d2) < 0.0000001)
            {
                res = true;
            }
            //  System.Diagnostics.Debug.WriteLine($" {Math.Abs(d1-d2)}");
            return res;
        }

        private bool Inside(Point p)
        {
            bool res = false;
            if (p.X >= 0 && p.Y >= 0)
            {
                if (p.X < grilleLength && p.Y < grilleHeight)
                {
                    res = true;
                }
            }
            return res;
        }

        private Point PullStack()
        {
            Point p = centreStack[0];
            centreStack.RemoveAt(0);
            return p;
        }

        private void SetLimits()
        {
            paramLimits.AddLimit("GrilleLength", 1, 250);
            paramLimits.AddLimit("GrillHeight", 1, 250);
            paramLimits.AddLimit("GrillThickness", 1, 259);
            paramLimits.AddLimit("BeamLength", 1, 250);
            paramLimits.AddLimit("BeamWidth", 1, 250);
            paramLimits.AddLimit("EdgeSize", 1, 260);
        }

        private void SolidsFromSegments(List<SkeletonSegment> skeletonSegments, double segWidth)
        {
            double segoff = segWidth * Math.Sin(Math.PI / 4.0);
            Extents bounds = new Extents();
            bounds.Left = segoff;
            bounds.Right = grilleLength - segoff;
            bounds.Top = grilleHeight - segoff;
            bounds.Bottom = segoff;
            List<SkeletonSegment> clippedSegs = new List<SkeletonSegment>();
            foreach (SkeletonSegment s in skeletonSegments)
            {
                var v = CohenSutherland.CohenSutherlandLineClip(bounds, s.start, s.end);
                if (v != null)
                {
                    double sx = v.Item1.X;
                    double sy = v.Item1.Y;
                    double ex = v.Item2.X;
                    double ey = v.Item2.Y;
                    SkeletonSegment ns = new SkeletonSegment();
                    ns.start = new Point(sx, sy);
                    ns.closeStart = true;
                    ns.end = new Point(ex, ey);
                    ns.closeEnd = true;
                    ns.incline = s.incline;
                    for (int i = 0; i < clippedSegs.Count; i++)
                    {
                        if (dequals(ns.start.X, clippedSegs[i].end.X) &&
                            dequals(ns.start.Y, clippedSegs[i].end.Y))
                        {
                            clippedSegs[i].DontCloseEnd();
                            ns.DontCloseStart();
                        }

                        if (dequals(ns.end.X, clippedSegs[i].start.X) &&
                            dequals(ns.end.Y, clippedSegs[i].start.Y))
                        {
                            clippedSegs[i].DontCloseStart();
                            ns.DontCloseEnd();
                        }
                    }
                    clippedSegs.Add(ns);
                }
            }
            foreach (SkeletonSegment s in clippedSegs)
            {
                double sx = s.start.X;
                double sy = s.start.Y;
                double ex = s.end.X;
                double ey = s.end.Y;
                switch (s.incline)
                {
                    case SegIncline.Straight:
                        {
                            // Straights always run vertically down.
                            // Bottom (i.e. end ) is always flat, top is point
                            Point p0 = new Point(sx - segoff, sy);
                            Point p1 = new Point(ex - segoff, ey);
                            // pointy bit
                            Point p2 = new Point(ex, ey + segoff);
                            Point p3 = new Point(ex + segoff, ey);
                            Point p4 = new Point(sx + segoff, sy);

                            int v0 = AddVerticeOctTree(p0.X, p0.Y, straightThicknessAdjustment);
                            int v1 = AddVerticeOctTree(p1.X, p1.Y, straightThicknessAdjustment);
                            int v2 = AddVerticeOctTree(p2.X, p2.Y, straightThicknessAdjustment);
                            int v3 = AddVerticeOctTree(p3.X, p3.Y, straightThicknessAdjustment);
                            int v4 = AddVerticeOctTree(p4.X, p4.Y, straightThicknessAdjustment);

                            int v5 = AddVerticeOctTree(p0.X, p0.Y, grilleThickness - straightThicknessAdjustment);
                            int v6 = AddVerticeOctTree(p1.X, p1.Y, grilleThickness - straightThicknessAdjustment);
                            int v7 = AddVerticeOctTree(p2.X, p2.Y, grilleThickness - straightThicknessAdjustment);
                            int v8 = AddVerticeOctTree(p3.X, p3.Y, grilleThickness - straightThicknessAdjustment);
                            int v9 = AddVerticeOctTree(p4.X, p4.Y, grilleThickness - straightThicknessAdjustment);
                            // left
                            AddFace(v0, v5, v1);
                            AddFace(v5, v6, v1);
                            // right
                            AddFace(v3, v8, v4);
                            AddFace(v8, v9, v4);
                            // back
                            AddFace(v0, v1, v3);
                            AddFace(v0, v3, v4);
                            AddFace(v1, v2, v3);
                            // front
                            AddFace(v5, v8, v6);
                            AddFace(v5, v9, v8);
                            AddFace(v8, v7, v6);

                            if (s.closeStart)
                            {
                                AddFace(v0, v4, v5);
                                AddFace(v4, v9, v5);
                            }

                            if (s.closeEnd)
                            {
                                AddFace(v1, v6, v2);
                                AddFace(v2, v6, v7);

                                AddFace(v2, v7, v3);
                                AddFace(v3, v7, v8);
                            }
                        }
                        break;

                    case SegIncline.Left:
                        {
                            // start is at the bottom

                            Point p0 = new Point(sx - segoff, sy);
                            Point p1 = new Point(ex - segoff, ey);

                            Point p2 = new Point(ex + segoff, ey);
                            Point p3 = new Point(sx, sy + segoff);

                            int v0 = AddVerticeOctTree(p0.X, p0.Y, leftThicknessAdjustment);
                            int v1 = AddVerticeOctTree(p1.X, p1.Y, leftThicknessAdjustment);
                            int v2 = AddVerticeOctTree(p2.X, p2.Y, leftThicknessAdjustment);
                            int v3 = AddVerticeOctTree(p3.X, p3.Y, leftThicknessAdjustment);

                            int v4 = AddVerticeOctTree(p0.X, p0.Y, grilleThickness - leftThicknessAdjustment);
                            int v5 = AddVerticeOctTree(p1.X, p1.Y, grilleThickness - leftThicknessAdjustment);
                            int v6 = AddVerticeOctTree(p2.X, p2.Y, grilleThickness - leftThicknessAdjustment);
                            int v7 = AddVerticeOctTree(p3.X, p3.Y, grilleThickness - leftThicknessAdjustment);

                            // left
                            AddFace(v0, v4, v1);
                            AddFace(v4, v5, v1);
                            // right
                            AddFace(v2, v7, v3);
                            AddFace(v2, v6, v7);
                            // back
                            AddFace(v0, v1, v3);
                            AddFace(v1, v2, v3);

                            // front
                            AddFace(v5, v4, v7);
                            AddFace(v5, v7, v6);

                            if (s.closeStart)
                            {
                                AddFace(v0, v3, v4);
                                AddFace(v3, v7, v4);
                            }

                            if (s.closeEnd)
                            {
                                AddFace(v1, v5, v2);
                                AddFace(v2, v5, v6);
                            }
                        }
                        break;

                    case SegIncline.Right:
                        {
                            // start is at the bottom

                            Point p0 = new Point(sx - segoff, sy);
                            Point p1 = new Point(ex - segoff, ey);

                            Point p2 = new Point(ex + segoff, ey);
                            Point p3 = new Point(sx + segoff, sy);

                            int v0 = AddVerticeOctTree(p0.X, p0.Y, rightThicknessAdjustment);
                            int v1 = AddVerticeOctTree(p1.X, p1.Y, rightThicknessAdjustment);
                            int v2 = AddVerticeOctTree(p2.X, p2.Y, rightThicknessAdjustment);
                            int v3 = AddVerticeOctTree(p3.X, p3.Y, rightThicknessAdjustment);

                            int v4 = AddVerticeOctTree(p0.X, p0.Y, grilleThickness - rightThicknessAdjustment);
                            int v5 = AddVerticeOctTree(p1.X, p1.Y, grilleThickness - rightThicknessAdjustment);
                            int v6 = AddVerticeOctTree(p2.X, p2.Y, grilleThickness - rightThicknessAdjustment);
                            int v7 = AddVerticeOctTree(p3.X, p3.Y, grilleThickness - rightThicknessAdjustment);

                            // left
                            AddFace(v0, v4, v1);
                            AddFace(v4, v5, v1);
                            // right
                            AddFace(v2, v7, v3);
                            AddFace(v2, v6, v7);
                            // back
                            AddFace(v0, v1, v3);
                            AddFace(v1, v2, v3);

                            // front
                            AddFace(v5, v4, v7);
                            AddFace(v5, v7, v6);

                            if (s.closeStart)
                            {
                                AddFace(v0, v3, v4);
                                AddFace(v3, v7, v4);
                            }

                            if (s.closeEnd)
                            {
                                AddFace(v1, v5, v2);
                                AddFace(v2, v5, v6);
                            }
                        }
                        break;
                }
            }
        }

        private struct SkeletonSegment
        {
            public bool clipped;
            public bool closeEnd;
            public bool closeStart;
            public Point end;
            public SegIncline incline;
            public Point start;

            internal void DontCloseEnd()
            {
                closeEnd = false;
            }

            internal void DontCloseStart()
            {
                closeStart = false;
            }
        }
    }
}