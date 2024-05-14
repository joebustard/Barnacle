/**************************************************************************
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

using System;

namespace Barnacle.Dialogs.MeshEditor
{
    internal class SculptingTool
    {
        private double piByTwo = Math.PI / 2.0;

        public SculptingTool()
        {
            Radius = 2.5;
            Scaler = 0.5;
        }

        public double Radius { get; set; }
        public double Scaler { get; set; }

        public virtual double Force(double v)
        {
            double res = 0;
            if (v > 0 && v <= Radius)
            {
                res = Math.Cos((v / Radius) * piByTwo) * Scaler;
            }
            return res;
        }
    }
}