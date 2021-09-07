using System;
using System.Collections.Generic;
using System.Windows;

namespace Make3D.LineLib
{
    public class FlexiBezier : FlexiSegment
    {
        public FlexiBezier()
        {
            P0 = -1;
            P1 = -1;
            P2 = -1;
            P3 = -1;
        }

        public FlexiBezier(int point1, int point2, int point3, int point4)
        {
            P0 = point1;
            P1 = point2;
            P2 = point3;
            P3 = point4;
        }

        public int P0 { get; set; }
        public int P1 { get; set; }
        public int P2 { get; set; }
        public int P3 { get; set; }

        public override void DeletePoints(List<FlexiPoint> points)
        {
            points.RemoveAt(P3);
            points.RemoveAt(P2);
            points.RemoveAt(P1);
        }

        public override void Deselect(List<FlexiPoint> points)
        {
            Selected = false;
            points[P0].Selected = false;
            points[P1].Selected = false;
            points[P2].Selected = false;
            points[P3].Selected = false;
        }

        public override void DisplayPoints(List<Point> res, List<FlexiPoint> pnts)
        {
            double dt = 0.1;
            for (double t = dt; t <= 1; t += dt)
            {
                res.Add(GetCoord(t, pnts));
            }
        }

        public override double DistToPoint(Point position, List<FlexiPoint> points)
        {
            double minDist = double.MaxValue;
            double t0 = 0.0;
            double dt = 0.1;
            double t1;
            Point pt0 = GetCoord(t0, points);
            t1 = t0 + dt;
            while (t1 < 1.0)
            {
                Point pt1 = GetCoord(t1, points);
                double dist = DistToLine.FindDistanceToLine(position, pt0, pt1);
                if (dist < minDist)
                {
                    minDist = dist;
                }
                pt0 = pt1;
                t1 += dt;
            }
            return minDist;
        }

        public override int End()
        {
            return P3;
        }

        public Point GetCoord(double t, List<FlexiPoint> points)
        {
            Point p0 = points[P0].Point;
            Point p1 = points[P1].Point;
            Point p2 = points[P2].Point;
            Point p3 = points[P3].Point;
            double x = Math.Pow(1 - t, 3) * p0.X +
                        (3 * Math.Pow(1 - t, 2) * t * p1.X) +
                        (3 * (1 - t) * t * t * p2.X) +
                        Math.Pow(t, 3) * p3.X;

            double y = Math.Pow(1 - t, 3) * p0.Y +
                        (3 * Math.Pow(1 - t, 2) * t * p1.Y) +
                        (3 * (1 - t) * t * t * p2.Y) +
                        Math.Pow(t, 3) * p3.Y;

            return new Point(x, y);
        }

        public override void GetSegmentPoints(List<FlexiPoint> res, List<FlexiPoint> pnts)
        {
            res.Add(pnts[P1]);
            res.Add(pnts[P2]);
            res.Add(pnts[P3]);
        }

        public override void PointInserted(int v2, int n)
        {
            if (P0 >= v2)
            {
                P0 += n;
            }

            if (P1 >= v2)
            {
                P1 += n;
            }

            if (P2 >= v2)
            {
                P2 += n;
            }

            if (P3 >= v2)
            {
                P3 += n;
            }
        }

        public void ResetControlPoints(List<FlexiPoint> points)
        {
            Point p1;
            Point p2;
            p1 = new Point(points[P0].Point.X + (0.25 * (points[P3].Point.X - points[P0].Point.X)), points[P0].Point.Y + (0.25 * (points[P3].Point.Y - points[P0].Point.Y)));
            p2 = new Point(points[P0].Point.X + (0.75 * (points[P3].Point.X - points[P0].Point.X)), points[P0].Point.Y + (0.75 * (points[P3].Point.Y - points[P0].Point.Y)));
            points[P1].Point = p1;
            points[P2].Point = p2;
        }

        public override void Select(List<FlexiPoint> points)
        {
            Selected = true;
            points[P0].Selected = true;
            points[P1].Selected = true;
            points[P2].Selected = true;
            points[P3].Selected = true;
        }

        public override int Start()
        {
            return P0;
        }

        public override string ToString()
        {
            string s = "C," + P0.ToString() + "," + P1.ToString() + "," + P2.ToString() + "," + P3.ToString();

            return s;
        }
    }
}