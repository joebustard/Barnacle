// **************************************************************************
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Barnacle.LineLib
{
    internal class FlexiArcSegment : FlexiSegment
    {
        private const double subSegmentRatio = 0.05;

        public FlexiArcSegment()
        {
            P0 = -1;
            P1 = -1;
            P2 = -1;
            Radius = 1;
        }

        public FlexiArcSegment(int point1, int point2, int point3)
        {
            P0 = point1;
            P1 = point2;
            P2 = point3;
            IsLargeArc = false;
        }

        public bool Clockwise
        {
            get;
            set;
        }

        public bool IsLargeArc
        {
            get; set;
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

        public double Radius
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

        public Point FindPointOnLine(
            Point pt, Point p1, Point p2)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            if ((dx == 0) && (dy == 0))
            {
                // It's a point not a line segment.
                return p1;
            }

            // Calculate the t that minimizes the distance.
            double t = ((pt.X - p1.X) * dx + (pt.Y - p1.Y) * dy) /
                (dx * dx + dy * dy);

            return new Point(p1.X + t * dx, p1.Y + t * dy);
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
            FlexiPoint fs = points[P0];
            FlexiPoint fc = points[P1];
            FlexiPoint fe = points[P2];

            Point pt1 = new Point(fs.X, fs.Y);
            Point pt2 = new Point(fe.X, fe.Y);

            // Get info about chord that connects both points
            Point midPoint = new Point((pt1.X + pt2.X) / 2, (pt1.Y + pt2.Y) / 2);
            Vector vect = pt2 - pt1;
            double halfChord = vect.Length / 2;
            if (halfChord > Radius)
            {
                Radius = halfChord;
            }

            // Get vector from chord to center
            Vector vectRotated;

            // (comparing two Booleans here!)
            if (IsLargeArc == Clockwise)
                vectRotated = new Vector(-vect.Y, vect.X);
            else
                vectRotated = new Vector(vect.Y, -vect.X);

            vectRotated.Normalize();

            // Distance from chord to center
            double centerDistance = Math.Sqrt(Radius * Radius - halfChord * halfChord);

            // Calculate center point
            Point center = midPoint + centerDistance * vectRotated;

            // Get angles from center to the two points
            double angle1 = Math.Atan2(pt1.Y - center.Y, pt1.X - center.X);
            double angle2 = Math.Atan2(pt2.Y - center.Y, pt2.X - center.X);

            // (another comparison of two Booleans!)
            if (angle1 < 0)
            {
                angle1 += 2 * Math.PI;
            }

            if (angle2 < 0)
            {
                angle2 += 2 * Math.PI;
            }
            if (Clockwise)
            {
                double tmp = angle1;
                angle1 = angle2;
                angle2 = tmp;
                t = 1 - t;
            }
            /*
            if (IsLargeArc == (Math.Abs(angle2 - angle1) < Math.PI))
            {
                if (angle1 < angle2)
                    angle1 += 2 * Math.PI;
                else
                    angle2 += 2 * Math.PI;
            }
            */
            if (angle2 < angle1)
            {
                angle2 += 2 * Math.PI;
            }
            double angle = angle1 + t * (angle2 - angle1);
            double x = center.X + Radius * Math.Cos(angle);
            double y = center.Y + Radius * Math.Sin(angle);

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
            string s = "A," + P0.ToString() + "," + P1.ToString() + "," + P2.ToString() + " " + Radius.ToString();

            return s;
        }

        internal override void Expand(List<FlexiSegment> segs, ObservableCollection<FlexiPoint> flexiPoints)
        {
            int np0 = P0 + 1;
            int np1 = np0 + 1;
            int np2 = np1 + 1;
            int np3 = np2 + 1;
            FlexiPoint fp0 = new FlexiPoint(flexiPoints[P0].X, flexiPoints[P0].Y);
            FlexiPoint fp1 = new FlexiPoint(flexiPoints[P2].X, flexiPoints[P2].Y);
            flexiPoints.Insert(P2, fp1);
            flexiPoints.Insert(np0, fp0);
            int index = segs.IndexOf(this);
            if (index >= 0 && index < segs.Count)
            {
                for (int i = index + 1; i < segs.Count; i++)
                {
                    segs[i].PointInserted(P2, 2);
                }
                LineSegment preSeg = new LineSegment(P0, np0);
                LineSegment postSeg = new LineSegment(np2, np3);

                segs.Insert(index + 1, postSeg);
                segs.Insert(index, preSeg);
                P0 = np0;
                P1 = np1;
                P2 = np2;
            }
            // renumber the points
            // might as well just do all of them
            for (int ind = 0; ind < flexiPoints.Count; ind++)
            {
                flexiPoints[ind].Id = ind;
            }
        }

        internal override void MoveSegment(ObservableCollection<FlexiPoint> points, double dx, double dy)
        {
            points[P0].Offset(dx, dy);
            points[P1].Offset(dx, dy);
            points[P2].Offset(dx, dy);
        }

        internal Point Snap(Point p, ObservableCollection<FlexiPoint> points)
        {
            FlexiPoint fs = points[P0];
            FlexiPoint fc = points[P1];
            FlexiPoint fe = points[P2];

            Point pt1 = new Point(fs.X, fs.Y);
            Point pt2 = new Point(fe.X, fe.Y);

            // Get info about chord that connects both points
            Point midPoint = new Point((pt1.X + pt2.X) / 2, (pt1.Y + pt2.Y) / 2);
            Vector vect = pt2 - pt1;
            double halfChord = vect.Length / 2;
            double currentRad = Radius;
            if (halfChord >= currentRad)
            {
                currentRad = halfChord + 1;
            }

            // Get vector from chord to center
            Vector vectRotated;

            // (comparing two Booleans here!)
            if (IsLargeArc == Clockwise)
                vectRotated = new Vector(-vect.Y, vect.X);
            else
                vectRotated = new Vector(vect.Y, -vect.X);

            vectRotated.Normalize();

            // Distance from chord to center
            double centerDistance = Math.Sqrt(currentRad * currentRad - halfChord * halfChord);
            Point center = midPoint + centerDistance * vectRotated;
            // we now have two points at least 1 mm apart at
            // right angles to the bisector
            // find the point on this line closest to the input point p
            Point np = FindPointOnLine(p, center, midPoint);
            Radius = Distance(np, pt1);

            return np;
        }

        internal override string ToOutline(ObservableCollection<FlexiPoint> flexiPoints)
        {
            string code = "A";
            if (Clockwise)
            {
                code += "C";
            }
            if (IsLargeArc)
            {
                code += "L";
            }
            return $"{code} {flexiPoints[P1].X:F07},{flexiPoints[P1].Y:F07} {flexiPoints[P2].X:F07},{flexiPoints[P2].Y:F07}  ";
        }

        internal override string ToPath(ObservableCollection<FlexiPoint> points, ref double ox, ref double oy, bool absolute)
        {
            string res = "";

            string code = "RA";
            if (absolute)
            {
                code = "A";
            }
            if (Clockwise)
            {
                code += "C";
            }
            if (IsLargeArc)
            {
                code += "L";
            }

            if (absolute)
            {
                res = $"{code} {points[P1].X:F07},{points[P1].Y:F07} {points[P2].X:F07},{points[P2].Y:F07} ";
            }
            else
            {
                res = $"{code} {points[P1].X - ox:F07},{points[P1].Y - oy:F07} {points[P2].X - ox:F07},{points[P2].Y - oy:F07} ";
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

        private double GetAngle(FlexiPoint origin, FlexiPoint p)
        {
            double res = Math.Atan2(p.Y - origin.Y, p.X - origin.X);
            return res;
        }
    }
}