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

using System.Collections.Generic;
using System.Windows;

namespace Barnacle.Models.BufferedPolyline
{
    public class Segment
    {
        public Segment(Point s, Point e, Point c, bool dir)
        {
            Start = s;
            End = e;
            ExtensionCentre = c;
            Extensions = null;
            Outbound = dir;
        }

        public Point End { get; set; }
        public Point ExtensionCentre { get; set; }
        public List<Segment> Extensions { get; set; }
        public bool Outbound { get; set; }
        public Point Start { get; set; }
        public int Twin { get; internal set; }
    }
}