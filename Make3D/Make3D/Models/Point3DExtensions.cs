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
using System.Windows.Media.Media3D;

namespace Barnacle.Models
{
    public static class Point3DExtensions
    {
        public static double Distance(this Point3D s, Point3D e)
        {

            double xd = e.X - s.X;
            double yd = e.Y - s.Y;
            double zd = e.Z - s.Z;
            double res = Math.Sqrt(xd * xd + yd * yd + zd * zd);
            return res;
        }

        public static Point3D MidPoint( this Point3D s, Point3D e)
        {
            return (new Point3D((s.X + e.X) / 2, (s.Y + e.Y) / 2, (s.Z + e.Z) / 2));
        }
    }
}
