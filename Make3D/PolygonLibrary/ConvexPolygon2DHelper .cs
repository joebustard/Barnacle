using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PolygonLibrary
{
    public class ConvexPolygon2DHelper
    {
        private const double EquityTolerance = 0.000000001d;

        private static bool IsEqual(double d1, double d2)
        {
            return Math.Abs(d1 - d2) <= EquityTolerance;
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

        // taken from https://wrf.ecse.rpi.edu//Research/Short_Notes/pnpoly.html
        public bool IsPointInsidePoly(Point test, ConvexPolygon2D poly)
        {
            int i;
            int j;
            bool result = false;
            for (i = 0, j = poly.Corners.Length - 1; i < poly.Corners.Length; j = i++)
            {
                if ((poly.Corners[i].Y > test.Y) != (poly.Corners[j].Y > test.Y) &&
                    (test.X < (poly.Corners[j].X - poly.Corners[i].X) * (test.Y - poly.Corners[i].Y) / (poly.Corners[j].Y - poly.Corners[i].Y) + poly.Corners[i].X))
                {
                    result = !result;
                }
            }
            return result;
        }

        public Point[] GetIntersectionPoints(Point l1p1, Point l1p2, ConvexPolygon2D poly)
        {
            List<Point> intersectionPoints = new List<Point>();
            for (int i = 0; i < poly.Corners.Length; i++)
            {
                int next = (i + 1 == poly.Corners.Length) ? 0 : i + 1;
                Point? ip = GetIntersectionPoint(l1p1, l1p2, poly.Corners[i], poly.Corners[next]);
                if (ip != null) intersectionPoints.Add((Point)ip);
            }
            return intersectionPoints.ToArray();
        }

        private void AddPoints(List<Point> pool, Point[] newpoints)
        {
            foreach (Point np in newpoints)
            {
                bool found = false;
                foreach (Point p in pool)
                {
                    if (IsEqual(p.X, np.X) && IsEqual(p.Y, np.Y))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found) pool.Add(np);
            }
        }

        private Point[] OrderClockwise(Point[] points)
        {
            double mX = 0;
            double my = 0;
            foreach (Point p in points)
            {
                mX += p.X;
                my += p.Y;
            }
            mX /= points.Length;
            my /= points.Length;
            return points.OrderBy(v => Math.Atan2(v.Y - my, v.X - mX)).ToArray();
        }

        public ConvexPolygon2D GetIntersectionOfPolygons(ConvexPolygon2D poly1, ConvexPolygon2D poly2)
        {
            List<Point> clippedCorners = new List<Point>();
            //Add  the corners of poly1 which are inside poly2
            for (int i = 0; i < poly1.Corners.Length; i++)
            {
                if (IsPointInsidePoly(poly1.Corners[i], poly2))
                    AddPoints(clippedCorners, new Point[] { poly1.Corners[i] });
            }
            //Add the corners of poly2 which are inside poly1
            for (int i = 0; i < poly2.Corners.Length; i++)
            {
                if (IsPointInsidePoly(poly2.Corners[i], poly1))
                    AddPoints(clippedCorners, new Point[] { poly2.Corners[i] });
            }
            //Add  the intersection points
            for (int i = 0, next = 1; i < poly1.Corners.Length; i++, next = (i + 1 == poly1.Corners.Length) ? 0 : i + 1)
            {
                AddPoints(clippedCorners, GetIntersectionPoints(poly1.Corners[i], poly1.Corners[next], poly2));
            }

            return new ConvexPolygon2D(OrderClockwise(clippedCorners.ToArray()));
        }
    }
}