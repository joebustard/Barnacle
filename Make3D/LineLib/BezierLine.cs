using System;

using System.Windows;

namespace Make3D.LineLib
{
    public class BezierLine : PathSegment
    {
        /*
        public BezierLine()
        {
            Offset = new Point(0, 0);
        }

        public BezierLine(Point point1, Point point2)
        {
            P0 = new PathPoint(point1);
            P3 = new PathPoint(point2);
            Offset = new Point(0, 0);
        }

        public BezierLine(PathPoint point1, PathPoint point2)
        {
            P0 = point1;
            P3 = point2;
            Offset = new Point(0, 0);
        }

        public BezierLine(Point point1, Point point2, Point point3, Point point4)
        {
            P0 = new PathPoint(point1);
            P1 = new PathPoint(point2);
            P2 = new PathPoint(point3);
            P3 = new PathPoint(point4);
            Offset = new Point(0, 0);
        }

        public BezierLine(PathPoint point1, PathPoint point2, PathPoint point3, PathPoint point4)
        {
            P0 = point1;
            P1 = point2;
            P2 = point3;
            P3 = point4;
            Offset = new Point(0, 0);
        }

        public Point Offset { get; set; }
        public int P0 { get; set; }
        public int P1 { get; set; }
        public int P2 { get; set; }
        public int P3 { get; set; }

        public override int End()
        {
            return P3;
        }

        public Point GetCoord(double t, bool addOffset = true)
        {
            double x = Math.Pow(1 - t, 3) * P0.X +
                        (3 * Math.Pow(1 - t, 2) * t * P1.X) +
                        (3 * (1 - t) * t * t * P2.X) +
                        Math.Pow(t, 3) * P3.X;

            double y = Math.Pow(1 - t, 3) * P0.Y +
                        (3 * Math.Pow(1 - t, 2) * t * P1.Y) +
                        (3 * (1 - t) * t * t * P2.Y) +
                        Math.Pow(t, 3) * P3.Y;
            if (addOffset)
            {
                x += Offset.X;
                y += Offset.Y;
            }
            return new Point(x, y);
        }

        public void SetControlPoints()
        {
            Point p1;
            Point p2;
            p1 = new Point(P0.X + (0.25 * (P3.X - P0.X)), P0.Y + (0.25 * (P3.Y - P0.Y)));
            p2 = new Point(P0.X + (0.75 * (P3.X - P0.X)), P0.Y + (0.75 * (P3.Y - P0.Y)));
            P1 = new PathPoint(p1);
            P2 = new PathPoint(p2);
        }

        public void SetOffset(double x, double y)
        {
            Offset = new Point(x, y);
        }

        public void SetPoints(double x0, double y0, double x1, double y1, double x2, double y2, double x3, double y3)
        {
            P0 = new PathPoint(x0, y0);
            P1 = new PathPoint(x1, y1);
            P2 = new PathPoint(x2, y2);
            P3 = new PathPoint(x3, y3);
        }

        public override PathPoint Start()
        {
            return P0;
        }
        */
    }
}