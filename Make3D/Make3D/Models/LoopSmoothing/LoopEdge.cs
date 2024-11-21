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

namespace Barnacle.Models.LoopSmoothing
{
    internal class LoopEdge
    {
        public LoopEdge()
        {
            F1 = -1;
            F2 = -1;
        }

        public int Id { get; set; }
        public int End { get; set; }

        public LoopCoord Ep
        {
            get; set;
        }

        public int F1 { get; set; }
        public int F2 { get; set; }
        public int OppositePoint1 { get; set; }
        public int OppositePoint2 { get; set; }
        public int Start { get; set; }

        internal void MakeEdgePoint(List<LoopFace> catFaces, List<LoopPoint> catPoints)
        {
            LoopCoord cc = new LoopCoord();

            cc.X = (3.0 * (catPoints[Start].X + catPoints[End].X) / 8.0) + ((catPoints[OppositePoint1].X + catPoints[OppositePoint2].X) / 8.0);
            cc.Y = (3.0 * (catPoints[Start].Y + catPoints[End].Y) / 8.0) + ((catPoints[OppositePoint1].Y + catPoints[OppositePoint2].Y) / 8.0);
            cc.Z = (3.0 * (catPoints[Start].Z + catPoints[End].Z) / 8.0) + ((catPoints[OppositePoint1].Z + catPoints[OppositePoint2].Z) / 8.0);
            Ep = cc;
        }

        internal void MakeUnweightedEdgePoint(List<LoopFace> catFaces, List<LoopPoint> catPoints)
        {
            LoopCoord cc = new LoopCoord();

            cc.X = (catPoints[Start].X + catPoints[End].X) / 2.0;
            cc.Y = (catPoints[Start].Y + catPoints[End].Y) / 2.0;
            cc.Z = (catPoints[Start].Z + catPoints[End].Z) / 2.0;
            Ep = cc;
        }
    }
}