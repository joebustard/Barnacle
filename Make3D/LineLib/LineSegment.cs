using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Make3D.LineLib
{
    public class LineSegment : PathSegment
    {
        /*
        public LineSegment(Point point1, Point point2)
        {
            P0 = new PathPoint(point1);
            P1 = new PathPoint(point2);
            Offset = new Point(0, 0);
        }

        public LineSegment(PathPoint point1, PathPoint point2)
        {
            P0 = point1;
            P1 = point2;
            Offset = new Point(0, 0);
        }

        public Point Offset { get; set; }
        public PathPoint P0 { get; set; }
        public PathPoint P1 { get; set; }

        public override PathPoint End()
        {
            return P1;
        }

        public Point GetCoord(double t, bool addOffset = true)
        {
            double x = P0.X + t * (P1.X - P0.X);
            double y = P0.Y + t * (P1.Y - P0.Y);
            if (addOffset)
            {
                x += Offset.X;
                y += Offset.Y;
            }
            return new Point(x, y);
        }

        public override PathPoint Start()
        {
            return P0;
        }
        */
    }
}