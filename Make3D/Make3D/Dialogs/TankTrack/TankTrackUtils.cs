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
        public static double Distance(System.Windows.Point p1, System.Windows.Point p2)
        {
            double d = Math.Sqrt((p2.X - p1.X) * (p2.X - p1.X) +
                                   (p2.Y - p1.Y) * (p2.Y - p1.Y));

            return d;
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