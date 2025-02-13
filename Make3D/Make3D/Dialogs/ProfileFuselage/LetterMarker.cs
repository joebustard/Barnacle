﻿/**************************************************************************
*   Copyright (c) 2024 Joe Bustard <barnacle3d@gmailcom>                  *
*                                                                         *
*   This file is part of the Barnacle 3D application.                     *
*                                                                         *
*   This application is free software; you can redistribute it and/or     *
*   modify it under the terms of the GNU Library General Public           *
*   License as published by the Free Software Foundation; either          *
*   version 2 of the License, or (at your option) any later version.      *
*                                                                         *
*   This application is distributed in the hope that it will be useful,   *
*   but WITHOUT ANY WARRANTY; without even the implied warranty of        *
*   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
*   GNU Library General Public License for more details.                  *
*                                                                         *
**************************************************************************/

using System.Windows;

namespace Barnacle.Dialogs
{
    public class LetterMarker
    {
        public LetterMarker(string l, Point pos)
        {
            Letter = l;
            Position = pos;
        }

        public string Letter { get; set; }
        public Point Position { get; set; }
        public RibAndPlanEditControl Rib { get; set; }

        internal void Dump()
        {
            System.Diagnostics.Debug.WriteLine($"Marker {Letter} at {Position.X},{Position.Y}");
        }
    }
}