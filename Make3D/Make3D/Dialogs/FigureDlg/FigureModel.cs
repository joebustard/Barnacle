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

namespace Barnacle.Dialogs.Figure
{
    public class FigureModel
    {
        public FigureModel(string name, string figureModelName, double ls, double ws, double hs)
        {
            this.BoneName = name;
            this.FigureModelName = figureModelName;
            LScale = ls;
            WScale = ws;
            HScale = hs;
        }

        public String BoneName { get; set; }
        public String FigureModelName { get; set; }
        public double HScale { get; set; }
        public double LScale { get; set; }
        public double WScale { get; set; }
    }
}