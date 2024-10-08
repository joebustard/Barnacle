﻿using System;
using System.Windows;

namespace Barnacle.LineLib
{
    public class DistToLine
    {
        // Calculate the distance between
        // point pt and the segment p1 --> p2.
        public static double FindClosestToLine(
            Point pt, Point p1, Point p2, out Point closest)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            if ((dx == 0) && (dy == 0))
            {
                // It's a point not a line segment.
                closest = p1;
                dx = pt.X - p1.X;
                dy = pt.Y - p1.Y;
                return Math.Sqrt(dx * dx + dy * dy);
            }

            // Calculate the t that minimizes the distance.
            double t = ((pt.X - p1.X) * dx + (pt.Y - p1.Y) * dy) /
                (dx * dx + dy * dy);

            // See if this represents one of the segment's
            // end points or a point in the middle.
            if (t < 0)
            {
                closest = new Point(p1.X, p1.Y);
                dx = pt.X - p1.X;
                dy = pt.Y - p1.Y;
            }
            else if (t > 1)
            {
                closest = new Point(p2.X, p2.Y);
                dx = pt.X - p2.X;
                dy = pt.Y - p2.Y;
            }
            else
            {
                closest = new Point(p1.X + t * dx, p1.Y + t * dy);
                dx = pt.X - closest.X;
                dy = pt.Y - closest.Y;
            }

            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Find the t of the point relative to a line seqment
        /// </summary>
        /// <param name="pt"> point whose t we want</param>
        /// <param name="p1">one end of the segment</param>
        /// <param name="p2">other end of the segment</param>
        /// <returns></returns>
        public static double FindTOfClosestToLine(
          Point pt, Point p1, Point p2)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            if ((dx == 0) && (dy == 0))
            {
                return double.MinValue;
            }

            // Calculate the t that minimizes the distance.
            double t = ((pt.X - p1.X) * dx + (pt.Y - p1.Y) * dy) /
                (dx * dx + dy * dy);
            return t;
        }

        public static double FindDistanceToLine(
           Point pt, Point p1, Point p2)
        {
            Point closest;
            return FindClosestToLine(pt, p1, p2, out closest);
        }
    }
}