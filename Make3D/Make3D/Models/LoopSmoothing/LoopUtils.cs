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

namespace Barnacle.Models.LoopSmoothing
{
    internal class LoopUtils
    {
        private static double TwoPi = Math.PI * 2.0;
        private static double fiveEigths = 5.0 / 8.0;

        public static double Beta(double n)
        {
            double q1 = (3.0 + 2.0 * Math.Cos(TwoPi / n));
            double res = (1.0 / n) * (fiveEigths - (q1 * q1) / 64.0);
            return res;
        }
    }
}