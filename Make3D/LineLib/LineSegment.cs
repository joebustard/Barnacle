using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Make3D.LineLib
{
    public class LineSegment : FlexiSegment
    {
        public LineSegment(int p0, int p1)
        {
            P0 = p0;
            P1 = p1;
        }

        public int P0 { get; set; }
        public int P1 { get; set; }

        public override void DeletePoints(List<FlexiPoint> points)
        {
            points.RemoveAt(P1);
        }

        public override void DisplayPoints(List<Point> res, List<FlexiPoint> pnts)
        {
            res.Add(pnts[P1].Point);
        }

        public override int End()
        {
            return P1;
        }

        public Point GetCoord(double t, List<FlexiPoint> points)
        {
            double x = points[P0].Point.X + t * (points[P1].Point.X - points[P0].Point.X);
            double y = points[P0].Point.Y + t * (points[P1].Point.Y - points[P0].Point.Y);

            return new Point(x, y);
        }

        public override void GetSegmentPoints(List<FlexiPoint> res, List<FlexiPoint> pnts)
        {
            res.Add(pnts[P1]);
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
        }

        public override int Start()
        {
            return P0;
        }

        public override string ToString()
        {
            string s = "L," + P0.ToString() + "," + P1.ToString();

            return s;
        }
    }
}