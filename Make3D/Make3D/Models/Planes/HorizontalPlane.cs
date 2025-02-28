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
    internal class HorizontalPlane : Plane
    {
        internal HorizontalPlane(double planeLevel, double width, double depth) : base(planeLevel, width, depth)
        {
        }

        public override void SetLocation(double v)
        {
            double x = width / 2; // floor width / 2
            double z = depth / 2; // floor length / 2
            double thick = -.1; // give the floor some depth so it's not a 2 dimensional plane

            points = new Point3DCollection(20);
            Point3D point;
            //top of the floor
            point = new Point3D(-x, v, z);// HorizontalPlane Index - 0
            points.Add(point);
            point = new Point3D(x, v, z);// HorizontalPlane Index - 1
            points.Add(point);
            point = new Point3D(x, v, -z);// HorizontalPlane Index - 2
            points.Add(point);
            point = new Point3D(-x, v, -z);// HorizontalPlane Index - 3
            points.Add(point);
            //front side
            point = new Point3D(-x, v, z);// HorizontalPlane Index - 4
            points.Add(point);
            point = new Point3D(-x, v - thick, z);// HorizontalPlane Index - 5
            points.Add(point);
            point = new Point3D(x, v - thick, z);// HorizontalPlane Index - 6
            points.Add(point);
            point = new Point3D(x, v, z);// HorizontalPlane Index - 7
            points.Add(point);
            //right side
            point = new Point3D(x, v, z);// HorizontalPlane Index - 8
            points.Add(point);
            point = new Point3D(x, v - thick, z);// HorizontalPlane Index - 9
            points.Add(point);
            point = new Point3D(x, v - thick, -z);// HorizontalPlane Index - 10
            points.Add(point);
            point = new Point3D(x, v, -z);// HorizontalPlane Index - 11
            points.Add(point);
            //back side
            point = new Point3D(x, v, -z);// HorizontalPlane Index - 12
            points.Add(point);
            point = new Point3D(x, v - thick, -z);// HorizontalPlane Index - 13
            points.Add(point);
            point = new Point3D(-x, v - thick, -z);// HorizontalPlane Index - 14
            points.Add(point);
            point = new Point3D(-x, v, -z);// HorizontalPlane Index - 15
            points.Add(point);
            //left side
            point = new Point3D(-x, v, -z);// HorizontalPlane Index - 16
            points.Add(point);
            point = new Point3D(-x, v - thick, -z);// HorizontalPlane Index - 17
            points.Add(point);
            point = new Point3D(-x, v - thick, z);// HorizontalPlane Index - 18
            points.Add(point);
            point = new Point3D(-x, v, z);// HorizontalPlane Index - 19
            points.Add(point);
        }
    }
}