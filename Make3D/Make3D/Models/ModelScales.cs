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

namespace Barnacle.Models
{
    public class ModelScales
    {
        public static Dictionary<string, double> Standard = new Dictionary<string, double>();

        public static void Initialise()
        {
            Standard["2"] = 2;
            Standard["1 3/4"] = 1.75;
            Standard["1 1/2"] = 1.5;
            Standard["1 1/4"] = 1.25;
            Standard["1"] = 1.0;
            Standard["1/2"] = 1.0 / 2.0;
            Standard["1/3"] = 1.0 / 3.0;
            Standard["1/4"] = 1.0 / 4.0;
            Standard["1/5"] = 1.0 / 5.0;
            Standard["1/6"] = 1.0 / 6.0;
            Standard["1/10"] = 1.0 / 10.0;
            Standard["1/12"] = 1.0 / 12.0;
            Standard["1/16"] = 1.0 / 16.0;
            Standard["1/24"] = 1.0 / 24;
            Standard["1/32"] = 1.0 / 32.0;
            Standard["1/35"] = 1.0 / 35.0;
            Standard["1/48"] = 1.0 / 48.0;
            Standard["1/50"] = 1.0 / 50.0;
            Standard["1/72"] = 1.0 / 72.0;
            Standard["1/100"] = 1.0 / 100.0;
            Standard["1/144"] = 1.0 / 144.0;
            Standard["1/200"] = 1.0 / 200.0;
            Standard["1/300"] = 1.0 / 300.0;
            Standard["1/400"] = 1.0 / 400.0;
            Standard["OO"] = 1.0 / 76.2;
            Standard["HO"] = 1.0 / 87;
            Standard["N"] = 1.0 / 148;
            Standard["Z"] = 1.0 / 200;
        }

        internal static double ConversionFactor(string baseScale, string exportScale)
        {
            double res = 1.0;
            if (Standard.ContainsKey(baseScale) && Standard.ContainsKey(exportScale))
            {
                double startScale = Standard[baseScale];
                double endScale = Standard[exportScale];
                res = 1.0 / startScale;
                res = res * endScale;
            }
            return res;
        }

        internal static List<string> ScaleNames()
        {
            List<string> names = new List<string>();
            foreach (string a in Standard.Keys)
            {
                names.Add(a);
            }
            return names;
        }
    }
}