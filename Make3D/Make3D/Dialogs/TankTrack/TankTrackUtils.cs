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

namespace Barnacle.Dialogs
{
    internal class TankTrackUtils
    {
        public static void CentreGuideLinkPolygon(System.Windows.Point p1, System.Windows.Point p2, ref List<System.Windows.Point> outter, ref List<System.Windows.Point> inner, bool firstCall, double thickness, double guideSize, double spudSize)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;

            if (dx == 0 && dy != 0)
            {
                if (p2.Y > p1.Y)
                {
                    // vertical downwards
                    if (firstCall)
                    {
                        outter.Add(new System.Windows.Point(p1.X + thickness, p1.Y));
                    }
                    outter.Add(new System.Windows.Point(p1.X + thickness, p1.Y + 0.2 * dy));
                    outter.Add(new System.Windows.Point(p1.X + thickness, p1.Y + 0.3 * dy));
                    outter.Add(new System.Windows.Point(p1.X + thickness, p1.Y + 0.5 * dy));
                    outter.Add(new System.Windows.Point(p1.X + thickness, p1.Y + 0.6 * dy));
                    outter.Add(new System.Windows.Point(p1.X + thickness, p1.Y + 0.75 * dy));
                    outter.Add(new System.Windows.Point(p1.X + thickness - spudSize, p1.Y + 0.87 * dy));
                    outter.Add(new System.Windows.Point(p1.X + thickness, p2.Y));

                    if (firstCall)
                    {
                        inner.Add(new System.Windows.Point(p1.X - thickness, p1.Y));
                    }
                    inner.Add(new System.Windows.Point(p1.X - thickness, p1.Y + 0.2 * dy));
                    inner.Add(new System.Windows.Point(p1.X - thickness - guideSize, p1.Y + 0.3 * dy));
                    inner.Add(new System.Windows.Point(p1.X - thickness - guideSize, p1.Y + 0.5 * dy));
                    inner.Add(new System.Windows.Point(p1.X - thickness, p1.Y + 0.6 * dy));
                    inner.Add(new System.Windows.Point(p1.X - thickness, p1.Y + 0.75 * dy));
                    inner.Add(new System.Windows.Point(p1.X - thickness, p1.Y + 0.87 * dy));
                    inner.Add(new System.Windows.Point(p1.X - thickness, p2.Y));
                }
                else
                {
                    // vertical upwards
                    if (firstCall)
                    {
                        outter.Add(new System.Windows.Point(p1.X - thickness, p1.Y));
                    }
                    outter.Add(new System.Windows.Point(p1.X - thickness, p1.Y + 0.2 * dy));
                    outter.Add(new System.Windows.Point(p1.X - thickness, p1.Y + 0.3 * dy));
                    outter.Add(new System.Windows.Point(p1.X - thickness, p1.Y + 0.5 * dy));
                    outter.Add(new System.Windows.Point(p1.X - thickness, p1.Y + 0.6 * dy));
                    outter.Add(new System.Windows.Point(p1.X - thickness, p1.Y + 0.75 * dy));
                    outter.Add(new System.Windows.Point(p1.X - thickness - spudSize, p1.Y + 0.87 * dy));
                    outter.Add(new System.Windows.Point(p1.X - thickness, p2.Y));

                    if (firstCall)
                    {
                        inner.Add(new System.Windows.Point(p1.X + thickness, p1.Y));
                    }
                    inner.Add(new System.Windows.Point(p1.X + thickness, p1.Y + 0.2 * dy));
                    inner.Add(new System.Windows.Point(p1.X + thickness, p1.Y + 0.3 * dy));
                    inner.Add(new System.Windows.Point(p1.X + thickness, p1.Y + 0.5 * dy));
                    inner.Add(new System.Windows.Point(p1.X + thickness, p1.Y + 0.6 * dy));
                    inner.Add(new System.Windows.Point(p1.X + thickness, p1.Y + 0.75 * dy));
                    inner.Add(new System.Windows.Point(p1.X + thickness, p1.Y + 0.87 * dy));
                    inner.Add(new System.Windows.Point(p1.X + thickness, p2.Y));
                }
            }
            else
            if (dy == 0)
            {
                if (p2.X - p1.X > 0)
                {
                    // Horizontal Left to right
                    if (firstCall)
                    {
                        outter.Add(new System.Windows.Point(p1.X, p1.Y - thickness));
                    }
                    outter.Add(new System.Windows.Point(p1.X + 0.2 * dx, p1.Y - thickness));
                    outter.Add(new System.Windows.Point(p1.X + 0.3 * dx, p1.Y - thickness));
                    outter.Add(new System.Windows.Point(p1.X + 0.5 * dx, p1.Y - thickness));
                    outter.Add(new System.Windows.Point(p1.X + 0.4 * dx, p1.Y - thickness));
                    outter.Add(new System.Windows.Point(p1.X + 0.75 * dx, p1.Y - thickness));
                    outter.Add(new System.Windows.Point(p1.X + 0.87 * dx, p1.Y - thickness - spudSize));
                    outter.Add(new System.Windows.Point(p2.X, p2.Y - thickness));

                    if (firstCall)
                    {
                        inner.Add(new System.Windows.Point(p1.X, p1.Y + thickness));
                    }
                    inner.Add(new System.Windows.Point(p1.X + 0.2 * dx, p1.Y + thickness));
                    inner.Add(new System.Windows.Point(p1.X + 0.3 * dx, p1.Y + thickness));
                    inner.Add(new System.Windows.Point(p1.X + 0.5 * dx, p1.Y + thickness));
                    inner.Add(new System.Windows.Point(p1.X + 0.6 * dx, p1.Y + thickness));
                    inner.Add(new System.Windows.Point(p1.X + 0.75 * dx, p1.Y + thickness));
                    inner.Add(new System.Windows.Point(p1.X + 0.87 * dx, p1.Y + thickness));
                    inner.Add(new System.Windows.Point(p2.X, p2.Y + thickness));
                }
                else
                {
                    // Horizontal right to Left
                    if (firstCall)
                    {
                        outter.Add(new System.Windows.Point(p1.X, p1.Y - +thickness));
                    }
                    outter.Add(new System.Windows.Point(p1.X + 0.2 * dx, p1.Y + thickness));
                    outter.Add(new System.Windows.Point(p1.X + 0.3 * dx, p1.Y + thickness));
                    outter.Add(new System.Windows.Point(p1.X + 0.5 * dx, p1.Y + thickness));
                    outter.Add(new System.Windows.Point(p1.X + 0.6 * dx, p1.Y + thickness));
                    outter.Add(new System.Windows.Point(p1.X + 0.75 * dx, p1.Y + thickness));
                    outter.Add(new System.Windows.Point(p1.X + 0.87 * dx, p1.Y + thickness + spudSize));
                    outter.Add(new System.Windows.Point(p2.X, p2.Y + thickness));

                    if (firstCall)
                    {
                        inner.Add(new System.Windows.Point(p1.X, p1.Y - thickness));
                    }
                    inner.Add(new System.Windows.Point(p1.X + 0.2 * dx, p1.Y - thickness));
                    inner.Add(new System.Windows.Point(p1.X + 0.3 * dx, p1.Y - thickness));
                    inner.Add(new System.Windows.Point(p1.X + 0.5 * dx, p1.Y - thickness));
                    inner.Add(new System.Windows.Point(p1.X + 0.6 * dx, p1.Y - thickness));
                    inner.Add(new System.Windows.Point(p1.X + 0.75 * dx, p1.Y - thickness));
                    inner.Add(new System.Windows.Point(p1.X + 0.87 * dx, p1.Y - thickness));
                    inner.Add(new System.Windows.Point(p2.X, p2.Y - thickness));
                }
            }
            else
            {
                double sign = -1;
                if (dx > 0 && dy < 0)
                {
                    sign = 1;
                }
                else
                if (dx > 0 && dy > 0)
                {
                    sign = -1;
                }
                else
                if (dx < 0 && dy < 0)
                {
                    sign = 1;
                }
                else
                if (dx < 0 && dy > 0)
                {
                    sign = -1;
                }
                System.Windows.Point o1 = Perpendicular(p1, p2, 0.0, -sign * thickness);
                System.Windows.Point o2 = Perpendicular(p1, p2, 0.2, -sign * thickness);
                System.Windows.Point o3 = Perpendicular(p1, p2, 0.3, -sign * thickness);
                System.Windows.Point o4 = Perpendicular(p1, p2, 0.5, -sign * thickness);
                System.Windows.Point o5 = Perpendicular(p1, p2, 0.6, -sign * thickness);
                System.Windows.Point o6 = Perpendicular(p1, p2, 0.75, -sign * thickness);
                System.Windows.Point o7 = Perpendicular(p1, p2, 0.87, -sign * (thickness + spudSize));
                System.Windows.Point o8 = Perpendicular(p1, p2, 1.0, -sign * thickness);
                if (firstCall)
                {
                    outter.Add(o1);
                }
                outter.Add(o2);
                outter.Add(o3);
                outter.Add(o4);
                outter.Add(o5);
                outter.Add(o6);
                outter.Add(o7);
                outter.Add(o8);

                System.Windows.Point i1 = Perpendicular(p1, p2, 0.0, sign * thickness);
                System.Windows.Point i2 = Perpendicular(p1, p2, 0.2, sign * thickness);
                System.Windows.Point i3 = Perpendicular(p1, p2, 0.3, sign * (thickness + guideSize));
                System.Windows.Point i4 = Perpendicular(p1, p2, 0.5, sign * (thickness + guideSize));
                System.Windows.Point i5 = Perpendicular(p1, p2, 0.6, sign * thickness);
                System.Windows.Point i6 = Perpendicular(p1, p2, 0.75, sign * thickness);
                System.Windows.Point i7 = Perpendicular(p1, p2, 0.87, sign * thickness);
                System.Windows.Point i8 = Perpendicular(p1, p2, 1.0, sign * thickness);
                if (firstCall)
                {
                    inner.Add(i1);
                }
                inner.Add(i2);
                inner.Add(i3);
                inner.Add(i4);
                inner.Add(i5);
                inner.Add(i6);
                inner.Add(i7);
                inner.Add(i8);
            }
        }

        public static double Distance(System.Windows.Point p1, System.Windows.Point p2)
        {
            double d = Math.Sqrt((p2.X - p1.X) * (p2.X - p1.X) +
                                   (p2.Y - p1.Y) * (p2.Y - p1.Y));

            return d;
        }

        public static System.Windows.Point Perpendicular(System.Windows.Point p1, System.Windows.Point p2, double t, double distanceFromLine)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double grad = dy / dx;
            double perp = -1.0 / grad;
            double sgn = Math.Sign(distanceFromLine);
            distanceFromLine = Math.Abs(distanceFromLine);
            System.Windows.Point tp = new System.Windows.Point(p1.X + t * dx, p1.Y + t * dy);
            double x = tp.X + sgn * Math.Sqrt((distanceFromLine * distanceFromLine) / (1.0 + (1.0 / (grad * grad))));
            double y = tp.Y + perp * (x - tp.X);
            System.Windows.Point res = new System.Windows.Point(x, y);
            return res;
        }

        public static System.Drawing.PointF PerpendicularF(System.Windows.Point p1, System.Windows.Point p2, double t, double distanceFromLine)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double grad = dy / dx;
            double perp = -1.0 / grad;
            double sgn = Math.Sign(distanceFromLine);
            distanceFromLine = Math.Abs(distanceFromLine);
            System.Windows.Point tp = new System.Windows.Point(p1.X + t * dx, p1.Y + t * dy);
            double x = tp.X + sgn * Math.Sqrt((distanceFromLine * distanceFromLine) / (1.0 + (1.0 / (grad * grad))));
            double y = tp.Y + perp * (x - tp.X);
            System.Drawing.PointF res = new System.Drawing.PointF((float)x, (float)y);
            return res;
        }

        public static double PolygonLength(List<System.Windows.Point> points)
        {
            double res = 0;
            for (int i = 0; i < points.Count; i++)
            {
                int j = i + 1;
                if (j == points.Count)
                {
                    j = 0;
                }
                System.Windows.Point p1 = points[i];
                System.Windows.Point p2 = points[j];
                res += Distance(p1, p2);
            }
            return res;
        }
    }
}