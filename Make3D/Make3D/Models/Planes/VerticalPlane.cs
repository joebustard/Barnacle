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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace Barnacle.Models
{
    internal class VerticalPlane : PlaneControl
    {
        internal VerticalPlane(double height, double depth) : base(height, depth, true)
        {
        }

        public override void SetLocation(double x, double y, double z)
        {
            SetLocation(x);
        }

        public void SetLocation(double v)
        {
            double over = 10;
            double y = width;
            double z = depth / 2;
  
            points = new Point3DCollection(20);
            Point3D point;
            //top of the floor
            point = new Point3D(v, -over, z);
            points.Add(point);
            point = new Point3D(v, y + over, z);
            points.Add(point);
            point = new Point3D(v, y + over, -z);
            points.Add(point);
            point = new Point3D(v, -over, -z);
            points.Add(point);
            return;
         
        }
    }
}