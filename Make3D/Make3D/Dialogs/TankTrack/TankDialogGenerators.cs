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

using Barnacle.Object3DLib;
using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    public partial class TankDialog2 : BaseModellerDialog
    {
        internal void GenerateLinkPart(System.Windows.Point p1, System.Windows.Point p2, Point3DCollection vertices, Int32Collection faces, double width, double thickness, Link link)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double midx = p1.X + dx / 2.0;
            double midy = p1.Y + dy / 2.0;
            double zrot = Math.Atan2(dy, dx) + Math.PI;
            Object3D rawLink = link.ScaledModel.Clone();
            rawLink.Position = new Point3D(midx, midy, 0);
            rawLink.RotateRad(new Point3D(0, 0, zrot));
            rawLink.Remesh();
            int faceoffset = vertices.Count;
            foreach (Point3D p3 in rawLink.AbsoluteObjectVertices)
            {
                vertices.Add(p3);
            }

            foreach (int f in rawLink.TriangleIndices)
            {
                faces.Add(f + faceoffset);
            }
        }
    }
}