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
using System.Windows.Media;

namespace Barnacle.UserControls
{
    public class AvailableColour
    {
        public String Name { get; set; }
        public string Title { get; set; }
        public Color Colour { get; set; }

        public AvailableColour(string n, string t, Color c)
        {
            Name = n;
            Title = t;
            Colour = c;
        }

        public AvailableColour(string name)
        {
            Name = name;
            Title = name.Substring(0, 1);
            for (int i = 1; i < name.Length; i++)
            {
                if (char.IsUpper(name[i]))
                {
                    Title += " ";
                }
                Title += name[i];
            }

            Colour = (Color)ColorConverter.ConvertFromString(Name);
        }
    }
}