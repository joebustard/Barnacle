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

namespace Barnacle.Models.LoopSmoothing
{
    internal class LoopPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public LoopCoord UpdatedPosition { get; set; }

        public Int32Collection Edges { get; set; }

        public LoopPoint(Point3D p)
        {
            X = p.X;
            Y = p.Y;
            Z = p.Z;
            Edges = new Int32Collection();
        }

        public LoopPoint(LoopCoord cc)
        {
            X = cc.X;
            Y = cc.Y;
            Z = cc.Z;
            Edges = new Int32Collection();
        }

        public LoopPoint(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
            Edges = new Int32Collection();
        }
    }
}