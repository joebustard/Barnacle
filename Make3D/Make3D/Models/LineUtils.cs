using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;

namespace Barnacle.Models
{
    internal class LineUtils
    {
        public static void CoplanerTest()
        {
            List<PointF> points = new List<PointF>();
            points.Add(new PointF(0.0F, 0.0F));
            points.Add(new PointF(0.0F, 0.5F));
            points.Add(new PointF(0.0F, 1.0F));
            points.Add(new PointF(1.0F, 1.0F));
            points.Add(new PointF(1.0F, 0.0F));
            points.Add(new PointF(0.5F, 0.0F));
            points = RemoveCoplanarSegments(points);
        }

        // Find the point of intersection between
        // the lines p1 --> p2 and p3 --> p4.
        public static void FindIntersection(
            PointF p1, PointF p2, PointF p3, PointF p4,
            out bool lines_intersect, out bool segments_intersect,
            out PointF intersection,
            out PointF close_p1, out PointF close_p2)
        {
            // Get the segments' parameters.
            float dx12 = p2.X - p1.X;
            float dy12 = p2.Y - p1.Y;
            float dx34 = p4.X - p3.X;
            float dy34 = p4.Y - p3.Y;

            // Solve for t1 and t2
            float denominator = (dy12 * dx34 - dx12 * dy34);

            float t1 =
                ((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34)
                    / denominator;
            if (float.IsInfinity(t1))
            {
                // The lines are parallel (or close enough to it).
                lines_intersect = false;
                segments_intersect = false;
                intersection = new PointF(float.NaN, float.NaN);
                close_p1 = new PointF(float.NaN, float.NaN);
                close_p2 = new PointF(float.NaN, float.NaN);
                return;
            }
            lines_intersect = true;

            float t2 =
                ((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12)
                    / -denominator;

            // Find the point of intersection.
            intersection = new PointF(p1.X + dx12 * t1, p1.Y + dy12 * t1);

            // The segments intersect if t1 and t2 are between 0 and 1.
            segments_intersect =
                ((t1 >= 0) && (t1 <= 1) &&
                 (t2 >= 0) && (t2 <= 1));

            // Find the closest points on the segments.
            if (t1 < 0)
            {
                t1 = 0;
            }
            else if (t1 > 1)
            {
                t1 = 1;
            }

            if (t2 < 0)
            {
                t2 = 0;
            }
            else if (t2 > 1)
            {
                t2 = 1;
            }

            close_p1 = new PointF(p1.X + dx12 * t1, p1.Y + dy12 * t1);
            close_p2 = new PointF(p3.X + dx34 * t2, p3.Y + dy34 * t2);
        }

        // Find the point of intersection between
        // the lines p1 --> p2 and p3 --> p4.
        public static bool DoSegmentsIntersect(
            PointF p1, PointF p2, PointF p3, PointF p4)
        {
            bool intersect = false;
            PointF intersection;
            // Get the segments' parameters.
            float dx12 = p2.X - p1.X;
            float dy12 = p2.Y - p1.Y;
            float dx34 = p4.X - p3.X;
            float dy34 = p4.Y - p3.Y;

            // Solve for t1 and t2
            float denominator = (dy12 * dx34 - dx12 * dy34);

            float t1 =
                ((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34)
                    / denominator;
            if (float.IsInfinity(t1))
            {
                return false;
            }

            float t2 =
                ((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12)
                    / -denominator;

            // Find the point of intersection.
            intersection = new PointF(p1.X + dx12 * t1, p1.Y + dy12 * t1);

            // The segments intersect if t1 and t2 are between 0 and 1.
            intersect =
                ((t1 >= 0) && (t1 <= 1) &&
                 (t2 >= 0) && (t2 <= 1));
            return intersect;
        }

        // Return points representing an enlarged polygon.
        public static List<PointF> GetEnlargedPolygon(
            List<PointF> old_points, float offset)
        {
            List<PointF> enlarged_points = new List<PointF>();
            int num_points = old_points.Count;
            for (int j = 0; j < num_points; j++)
            {
                // Find the new location for point j.
                // Find the points before and after j.
                int i = (j - 1);
                if (i < 0) i += num_points;
                int k = (j + 1) % num_points;

                // Move the points by the offset.
                Vector v1 = new Vector(
                    old_points[j].X - old_points[i].X,
                    old_points[j].Y - old_points[i].Y);
                v1.Normalize();
                v1 *= offset;
                Vector n1 = new Vector(-v1.Y, v1.X);

                PointF pij1 = new PointF(
                    (float)(old_points[i].X + n1.X),
                    (float)(old_points[i].Y + n1.Y));
                PointF pij2 = new PointF(
                    (float)(old_points[j].X + n1.X),
                    (float)(old_points[j].Y + n1.Y));

                Vector v2 = new Vector(
                    old_points[k].X - old_points[j].X,
                    old_points[k].Y - old_points[j].Y);
                v2.Normalize();
                v2 *= offset;
                Vector n2 = new Vector(-v2.Y, v2.X);

                PointF pjk1 = new PointF(
                    (float)(old_points[j].X + n2.X),
                    (float)(old_points[j].Y + n2.Y));
                PointF pjk2 = new PointF(
                    (float)(old_points[k].X + n2.X),
                    (float)(old_points[k].Y + n2.Y));

                // See where the shifted lines ij and jk intersect.
                bool lines_intersect, segments_intersect;
                PointF poi, close1, close2;
                FindIntersection(pij1, pij2, pjk1, pjk2,
                    out lines_intersect, out segments_intersect,
                    out poi, out close1, out close2);
                System.Diagnostics.Debug.Assert(lines_intersect,
                    "Edges " + i + "-->" + j + " and " +
                    j + "-->" + k + " are parallel");

                enlarged_points.Add(poi);
            }

            return enlarged_points;
        }

        public static double Gradient(PointF p1, PointF p2)
        {
            double res = 0;
            if (p2.X == p1.X)
            {
                res = double.PositiveInfinity;
            }
            else
            {
                res = (p2.Y - p1.Y) / (p2.X - p1.X);
            }
            return res;
        }

        public static List<PointF> RemoveCoplanarSegments(List<PointF> points)
        {
            bool found = true;
            List<PointF> res = new List<PointF>();

            while (found)
            {
                found = false;
                res.Clear();
                if (points.Count < 3)
                {
                    res = points;
                }
                else
                {
                    bool coplaner = false;
                    int i;
                    for (i = 0; i < points.Count - 2; i += 1)
                    {
                        coplaner = IsCoPlanar(points[i], points[i + 1], points[i + 2]);

                        if (!coplaner)
                        {
                            res.Add(points[i]);
                        }
                        else
                        {
                            res.Add(points[i]);
                            i++; // Skips the midpoint
                            found = true;
                        }
                    }
                    if (i + 1 < points.Count)
                    {
                        coplaner = IsCoPlanar(points[i], points[i + 1], points[0]);
                    }
                    else
                    {
                        coplaner = true;
                    }
                    if (!coplaner)
                    {
                        res.Add(points[i]);
                        res.Add(points[i + 1]);
                    }
                    else
                    {
                        res.Add(points[i]);
                    }
                }
                if (found)
                {
                    points.Clear();
                    foreach (PointF p in res)
                    {
                        points.Add(p);
                    }
                }
            }
            return res;
        }

        private static bool IsCoPlanar(PointF p0, PointF p1, PointF p2)
        {
            bool coplaner = false;
            double g1 = Gradient(p0, p1);
            double g2 = Gradient(p1, p2);

            if (double.IsPositiveInfinity(g1) || double.IsPositiveInfinity(g2))
            {
                if (double.IsPositiveInfinity(g1) && double.IsPositiveInfinity(g2))
                {
                    coplaner = true;
                }
            }
            else
            {
                if (Math.Abs(g1 - g2) < 0.0000001)
                {
                    coplaner = true;
                }
            }

            return coplaner;
        }

        // assumes polygon DOES not have repeated points
        public static bool IsPointInPolygon(PointF point, List<PointF> polygon)
        {
            int polyCorners = polygon.Count;
            int i = 0;
            int j = polyCorners - 1;
            bool oddNodes = false;

            for (i = 0; i < polyCorners; i++)
            {
                if (polygon[i].Y < point.Y && polygon[j].Y >= point.Y
                || polygon[j].Y < point.Y && polygon[i].Y >= point.Y)
                {
                    if (polygon[i].X + (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < point.X)
                    {
                        oddNodes = !oddNodes;
                    }
                }
                j = i;
            }

            return oddNodes;
        }
    }
}