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

namespace Barnacle.Models
{
    public class MultiPasteConfig
    {
        public String Direction { get; set; }
        public int Repeats { get; set; }
        public double Spacing { get; set; }
        public double AlternatingOffset { get; internal set; }

        public MultiPasteConfig()
        {
            Repeats = 2;
            Spacing = 1.0;
            Direction = "X";
        }
    }
}