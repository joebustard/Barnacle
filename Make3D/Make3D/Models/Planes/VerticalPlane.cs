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

using MathsLib;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace Barnacle.Models
{
    internal class VerticalPlane : PlaneControl
    {
        internal VerticalPlane(PlaneEquation planeEquation, double radius) : base(planeEquation, radius)
        {
        }

        public override void MoveTo(double x, double y, double z)
        {
            equation.Origin.X = x;
            base.MoveTo(equation.Origin.X, equation.Origin.Y, equation.Origin.Z);
        }
    }
}