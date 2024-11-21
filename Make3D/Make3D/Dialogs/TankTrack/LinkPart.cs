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

using Barnacle.LineLib;
using PolygonTriangulationLib;
using System;
using System.Collections.Generic;

namespace Barnacle.Dialogs
{
    public class LinkPart
    {
        private FlexiPath flexipath;
        private List<System.Drawing.PointF> profile;
        private List<Triangle> triangles;

        public LinkPart()
        {
            PathText = "";
            profile = null;
            triangles = null;
        }

        public String Name { get; set; }
        public String PathText { get; set; }

        public List<System.Drawing.PointF> Profile
        {
            get
            {
                if (profile == null)
                {
                    if (PathText != "")
                    {
                        FlexiPath fp = new FlexiPath();
                        if (fp.InterpretTextPath(PathText))
                        {
                            profile = fp.DisplayPointsF();
                            GenerateTriangles();
                        }
                    }
                }
                return profile;
            }
        }

        public List<Triangle> Triangles
        {
            get { return triangles; }
        }

        public double W { get; set; }

        public double X { get; set; }

        public double Y { get; set; }

        public double Z { get; set; }

        private void GenerateTriangles()
        {
            TriangulationPolygon ply = new TriangulationPolygon();
            // ply.Points = pf.ToArray();
            ply.Points = profile.ToArray();
            triangles = ply.Triangulate();
        }
    }
}