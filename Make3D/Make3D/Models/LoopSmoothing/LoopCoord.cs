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

namespace Barnacle.Models.LoopSmoothing
{
    internal class LoopCoord
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public LoopCoord()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        public LoopCoord(double v1, double v2, double v3)
        {
            X = v1;
            Y = v2;
            Z = v3;
        }

        public static LoopCoord operator +(LoopCoord a, LoopCoord b)
        {
            LoopCoord res = new LoopCoord();
            res.X = a.X + b.X;
            res.Y = a.Y + b.Y;
            res.Z = a.Z + b.Z;
            return res;
        }
    }
}