using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Media3D;

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
        private bool showEdge;
        private List<SkeletonSegment> skeletonSegments;
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
                    AddSkeletonSegment(cp, p0, SegIncline.Straight);
                    AddSkeletonSegment(cp, p1, SegIncline.Left);
                    AddSkeletonSegment(cp, p2, SegIncline.Right);
                    visited.Add(cp);

                    // calcuate the centres of the neigbouring sets of forks
                    Point ncp0 = new Point(p0.X - forkOff, p0.Y - forkOff);
                    Point ncp1 = new Point(p0.X + forkOff, p0.Y - forkOff);
                    Point ncp2 = new Point(p1.X, p1.Y + beamLength);
                    Point ncp3 = new Point(p2.X, p2.Y + beamLength);
                    // if any of these are inside the shape then add them to the queue
                    if (Inside(ncp0) || Inside(ncp0) || Inside(ncp0) || Inside(ncp0))
                    {
                        AddForkCentre(ncp0.X, ncp0.Y);
                        AddForkCentre(ncp1.X, ncp1.Y);
                        AddForkCentre(ncp2.X, ncp2.Y);
                        AddForkCentre(ncp3.X, ncp3.Y);
                    }
                }
            } while (centreStack.Count > 0);

            SolidsFromSegments(skeletonSegments, beamWidth);
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

        private bool dequals(double d1, double d2)
        {
            bool res = false;
            if (Math.Abs(d1 - d2) < 0.0000001)
            {
                res = true;
            }
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
            Point3D minPoint = new Point3D(-1, -1, -1);
            Point3D maxPoint = new Point3D(grilleLength + 1, grilleHeight + 1, grilleThickness + 1);
            CreateOctree(minPoint, maxPoint);
            foreach (SkeletonSegment s in skeletonSegments)
            {
                switch (s.incline)
                {
                    case SegIncline.Straight:
                        {
                            // Straights always run vertically down.
                            // Bottom (i.e. end ) is always flat, top is point
                            Point p0 = new Point(s.end.X - segoff, s.end.Y);
                            Point p1 = new Point(s.start.X - segoff, s.start.Y);
                            // pointy bit
                            Point p2 = new Point(s.start.X, s.start.Y + segoff);
                            Point p3 = new Point(s.start.X + segoff, s.start.Y);
                            Point p4 = new Point(s.end.X + segoff, s.end.Y);

                            int v0 = AddVerticeOctTree(p0.X, p0.Y, 0);
                            int v1 = AddVerticeOctTree(p1.X, p1.Y, 0);
                            int v2 = AddVerticeOctTree(p2.X, p2.Y, 0);
                            int v3 = AddVerticeOctTree(p3.X, p3.Y, 0);
                            int v4 = AddVerticeOctTree(p4.X, p4.Y, 0);

                            int v5 = AddVerticeOctTree(p0.X, p0.Y, grilleThickness);
                            int v6 = AddVerticeOctTree(p1.X, p1.Y, grilleThickness);
                            int v7 = AddVerticeOctTree(p2.X, p2.Y, grilleThickness);
                            int v8 = AddVerticeOctTree(p3.X, p3.Y, grilleThickness);
                            int v9 = AddVerticeOctTree(p4.X, p4.Y, grilleThickness);
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
                        }
                        break;

                    case SegIncline.Left:
                        {
                            // start is at the bottom

                            Point p0 = new Point(s.start.X - segoff, s.start.Y);
                            Point p1 = new Point(s.end.X - segoff, s.end.Y - segoff);

                            Point p2 = new Point(s.end.X + segoff, s.end.Y);
                            Point p3 = new Point(s.start.X, s.start.Y + segoff);

                            int v0 = AddVerticeOctTree(p0.X, p0.Y, 0);
                            int v1 = AddVerticeOctTree(p1.X, p1.Y, 0);
                            int v2 = AddVerticeOctTree(p2.X, p2.Y, 0);
                            int v3 = AddVerticeOctTree(p3.X, p3.Y, 0);

                            int v4 = AddVerticeOctTree(p0.X, p0.Y, grilleThickness);
                            int v5 = AddVerticeOctTree(p1.X, p1.Y, grilleThickness);
                            int v6 = AddVerticeOctTree(p2.X, p2.Y, grilleThickness);
                            int v7 = AddVerticeOctTree(p3.X, p3.Y, grilleThickness);

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
                        }
                        break;
                }
            }
        }

        private struct SkeletonSegment
        {
            public bool clipped;
            public Point end;
            public SegIncline incline;
            public Point start;
        }
    }
}