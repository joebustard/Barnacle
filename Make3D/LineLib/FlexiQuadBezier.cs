using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace Barnacle.LineLib
{
    public class FlexiQuadBezier : FlexiSegment
    {
        private const double subSegmentRatio = 0.05;

        public FlexiQuadBezier()
        {
            P0 = -1;
            P1 = -1;
            P2 = -1;
        }

        public FlexiQuadBezier(int point1, int point2, int point3)
        {
            P0 = point1;
            P1 = point2;
            P2 = point3;
        }

        public int P0
        {
            get; set;
        }

        public int P1
        {
            get; set;
        }

        public int P2
        {
            get; set;
        }

        public override void DeletePoints(ObservableCollection<FlexiPoint> points)
        {
            points.RemoveAt(P2);
            points.RemoveAt(P1);
        }

        public override void Deselect(ObservableCollection<FlexiPoint> points)
        {
            Selected = false;
            DeselectHide(P0, points);
            DeselectHide(P1, points);
            DeselectHide(P2, points);
        }

        public override void DisplayPoints(List<Point> res, ObservableCollection<FlexiPoint> pnts)
        {
            double dt = subSegmentRatio;
            AddDisplayPoint(res, pnts[P0].X, pnts[P0].Y);

            for (double t = 0; t <= 1; t += dt)
            {
                Point p = GetCoord(t, pnts);
                AddDisplayPoint(res, p.X, p.Y);
            }
            AddDisplayPoint(res, pnts[P2].X, pnts[P2].Y);
        }

        public override void DisplayPointsF(List<System.Drawing.PointF> res, ObservableCollection<FlexiPoint> pnts)
        {
            double dt = 0.1;
            for (double t = 0; t <= 1; t += dt)
            {
                Point p = GetCoord(t, pnts);
                res.Add(new System.Drawing.PointF((float)p.X, (float)p.Y));
            }
        }

        public override int DisplayPointsInSegment()
        {
            return (int)(1 / subSegmentRatio);
        }

        public override double DistToPoint(Point position, ObservableCollection<FlexiPoint> points)
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
            return P2;
        }

        public double FindT(Point position, ObservableCollection<FlexiPoint> pnts)
        {
            double minT = Double.MaxValue;
            double minDist = Double.MaxValue;

            double dt = 0.05;
            for (double t = 0; t <= 1; t += dt)
            {
                Point p = GetCoord(t, pnts);
                double dis = Distance(position, p);
                if (dis < minDist)
                {
                    minDist = dis;
                    minT = t;
                }
            }

            return minT;
        }

        public Point GetCoord(double t, ObservableCollection<FlexiPoint> points)
        {
            FlexiPoint p0 = points[P0];
            FlexiPoint p1 = points[P1];
            FlexiPoint p2 = points[P2];

            double x = p1.X + Math.Pow(1 - t, 2) * (p0.X - p1.X) +
                        Math.Pow(t, 2) * (p2.X - p1.X);

            double y = p1.Y + Math.Pow(1 - t, 2) * (p0.Y - p1.Y) +
                        Math.Pow(t, 2) * (p2.Y - p1.Y);

            return new Point(x, y);
        }

        public override void GetSegmentPoints(List<FlexiPoint> res, ObservableCollection<FlexiPoint> pnts)
        {
            res.Add(pnts[P1]);
            res.Add(pnts[P2]);
        }

        public override int NumberOfPoints()
        {
            return 3;
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
        }

        public override void PointsRemoved(int n)
        {
            P0 -= n;
            P1 -= n;
            if (P2 != 0)
            {
                P2 -= n;
            }
        }

        public void ResetControlPoints(ObservableCollection<FlexiPoint> points)
        {
            Point p1;

            p1 = new Point(points[P0].X + (0.5 * (points[P2].X - points[P0].X)), points[P0].Y + (0.5 * (points[P2].Y - points[P0].Y)));

            points[P1].X = p1.X;
            points[P1].Y = p1.Y;
        }

        public override void Select(ObservableCollection<FlexiPoint> points)
        {
            Selected = true;
            points[P0].Selected = true;
            points[P1].Selected = true;
            points[P2].Selected = true;
            points[P0].Visible = true;
            points[P1].Visible = true;
            points[P2].Visible = true;
        }

        public override int Start()
        {
            return P0;
        }

        public override string ToString()
        {
            string s = "Q," + P0.ToString() + "," + P1.ToString() + "," + P2.ToString();

            return s;
        }

        internal override string ToOutline(ObservableCollection<FlexiPoint> flexiPoints)
        {
            return $"Q {flexiPoints[P1].X:F07},{flexiPoints[P1].Y:F07} {flexiPoints[P2].X:F07},{flexiPoints[P2].Y:F07}  ";
        }

        internal override string ToPath(ObservableCollection<FlexiPoint> points, ref double ox, ref double oy, bool absolute)
        {
            string res = "";
            if (absolute)
            {
                res = $"Q {points[P1].X:F07},{points[P1].Y:F07} {points[P2].X:F07},{points[P2].Y:F07} ";
            }
            else
            {
                res = $"RQ {points[P1].X - ox:F07},{points[P1].Y - oy:F07} {points[P2].X - ox:F07},{points[P2].Y - oy:F07} ";
            }
            ox = points[P2].X;
            oy = points[P2].Y;
            return res;
        }

        private double Distance(Point p1, Point p2)
        {
            return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) +
                               (p1.Y - p2.Y) * (p1.Y - p2.Y));
        }
    }
}