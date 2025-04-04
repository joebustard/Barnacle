﻿// **************************************************************************
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Barnacle.UserControls
{
    public class PenSetting
    {
        public PenSetting(int v1, SolidColorBrush br, double op)
        {
            this.StrokeThickness = v1;
            this.Brush = br;
            this.Opacity = op;
            DashPattern = new DoubleCollection();
        }

        public double Opacity { get; set; }
        public int StrokeThickness { get; set; }
        public SolidColorBrush Brush { get; set; }
        public DoubleCollection DashPattern { get; set; }
    }
}