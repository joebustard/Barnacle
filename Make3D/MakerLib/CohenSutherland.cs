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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MakerLib
{
    // <summary>
    /// The Cohen Sutherland line clipping algorithm
    /// http://en.wikipedia.org/wiki/Cohen%E2%80%93Sutherland_algorithm
    /// </summary>
    public class CohenSutherland
    {
        private const byte BOTTOM = 4;

        /// <summary>
        /// Bitfields used to partition the space into 9 regiond
        /// </summary>
        private const byte INSIDE = 0; // 0000

        private const byte LEFT = 1;   // 0001
        private const byte RIGHT = 2;  // 0010

        // 0100
        private const byte TOP = 8;    // 1000

        /// <summary>
        /// Cohen–Sutherland clipping algorithm clips a line from
        /// P0 = (x0, y0) to P1 = (x1, y1) against a rectangle with
        /// diagonal from (xmin, ymin) to (xmax, ymax).
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="y0""</param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <returns>a list of two points in the resulting clipped line, or zero</returns>
        public static Tuple<Point, Point> CohenSutherlandLineClip(Extents extents,
                                                                  Point p0,
                                                                  Point p1)
        {
            double x0 = p0.X;
            double y0 = p0.Y;
            double x1 = p1.X;
            double y1 = p1.Y;

            // compute outcodes for P0, P1, and whatever point lies outside the clip rectangle
            byte outcode0 = CohenSutherland.ComputeOutCode(extents, x0, y0);
            byte outcode1 = CohenSutherland.ComputeOutCode(extents, x1, y1);
            bool accept = false;

            while (true)
            {
                // Bitwise OR is 0. Trivially accept and get out of loop
                if ((outcode0 | outcode1) == 0)
                {
                    accept = true;
                    break;
                }
                // Bitwise AND is not 0. Trivially reject and get out of loop
                else if ((outcode0 & outcode1) != 0)
                {
                    break;
                }
                else
                {
                    // failed both tests, so calculate the line segment to clip
                    // from an outside point to an intersection with clip edge
                    double x, y;

                    // At least one endpoint is outside the clip rectangle; pick it.
                    byte outcodeOut = (outcode0 != 0) ? outcode0 : outcode1;

                    // Now find the intersection point;
                    // use formulas y = y0 + slope * (x - x0), x = x0 + (1 / slope) * (y - y0)
                    if ((outcodeOut & TOP) != 0)
                    {   // point is above the clip rectangle
                        x = x0 + (x1 - x0) * (extents.Top - y0) / (y1 - y0);
                        y = extents.Top;
                    }
                    else if ((outcodeOut & BOTTOM) != 0)
                    { // point is below the clip rectangle
                        x = x0 + (x1 - x0) * (extents.Bottom - y0) / (y1 - y0);
                        y = extents.Bottom;
                    }
                    else if ((outcodeOut & RIGHT) != 0)
                    {  // point is to the right of clip rectangle
                        y = y0 + (y1 - y0) * (extents.Right - x0) / (x1 - x0);
                        x = extents.Right;
                    }
                    else if ((outcodeOut & LEFT) != 0)
                    {   // point is to the left of clip rectangle
                        y = y0 + (y1 - y0) * (extents.Left - x0) / (x1 - x0);
                        x = extents.Left;
                    }
                    else
                    {
                        x = double.NaN;
                        y = double.NaN;
                    }

                    // Now we move outside point to intersection point to clip
                    // and get ready for next pass.
                    if (outcodeOut == outcode0)
                    {
                        x0 = x;
                        y0 = y;
                        outcode0 = CohenSutherland.ComputeOutCode(extents, x0, y0);
                    }
                    else
                    {
                        x1 = x;
                        y1 = y;
                        outcode1 = CohenSutherland.ComputeOutCode(extents, x1, y1);
                    }
                }
            }

            // return the clipped line
            return (accept) ? new Tuple<Point, Point>(new Point(x0, y0), new Point(x1, y1)) : null;
        }

        /// <summary>
        /// Compute the bit code for a point (x, y) using the clip rectangle
        /// bounded diagonally by (xmin, ymin), and (xmax, ymax)
        /// ASSUME THAT xmax , xmin , ymax and ymin are global constants.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static byte ComputeOutCode(Extents extents, double x, double y)
        {
            // initialised as being inside of clip window
            byte code = INSIDE;

            if (x < extents.Left)           // to the left of clip window
                code |= LEFT;
            else if (x > extents.Right)     // to the right of clip window
                code |= RIGHT;
            if (y < extents.Bottom)         // below the clip window
                code |= BOTTOM;
            else if (y > extents.Top)       // above the clip window
                code |= TOP;

            return code;
        }
    }

    public class Extents
    {
        public double Bottom;
        public double Left;
        public double Right;
        public double Top;
    }
}