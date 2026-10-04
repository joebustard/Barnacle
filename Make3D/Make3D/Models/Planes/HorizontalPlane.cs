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

using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Models
{
    internal class HorizontalPlane : PlaneControl
    {
        internal HorizontalPlane(double width, double depth) : base(width, depth)
        {
        }

        public override void SetLocation(double ox, double oy, double oz)
        {
            double x = width / 2; // floor width / 2
            double y = oy;
            double z = depth / 2; // floor length / 2
       
            points = new Point3DCollection(20);
            Point3D point;
            //top of the floor
            point = new Point3D(-x, y, z);// HorizontalPlane Index - 0
            points.Add(point);
            point = new Point3D(x, y, z);// HorizontalPlane Index - 1
            points.Add(point);
            point = new Point3D(x, y, -z);// HorizontalPlane Index - 2
            points.Add(point);
            point = new Point3D(-x, y, -z);// HorizontalPlane Index - 3
            points.Add(point);
            return;
           
        }
    }
}