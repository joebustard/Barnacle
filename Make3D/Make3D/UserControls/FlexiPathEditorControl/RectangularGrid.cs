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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Barnacle.UserControls
{
    public class RectangularGrid : PointGrid
    {
        public override void CreateMarkers(DpiScale sc)
        {
            gridMarkers?.Clear();
            double x = 0;
            double y = 0;

            gridXPixels = (pixelsPerInchX / 25.4) * gridXMM;

            gridYPixels = (pixelsPerInchY / 25.4) * gridYMM;

            while (x < actualWidth)
            {
                y = 0;
                while (y < actualHeight)
                {
                    Ellipse el = new Ellipse();
                    Canvas.SetLeft(el, x - 1);
                    Canvas.SetTop(el, y - 1);
                    el.Width = 5;
                    el.Height = 5;
                    el.Fill = Brushes.CadetBlue;
                    el.Stroke = Brushes.Black;
                    if (gridMarkers != null)
                    {
                        gridMarkers.Add(el);
                    }
                    y += gridYPixels;
                }
                x += gridXPixels;
            }
        }

        public override Point SnapPositionToMM(Point pos)
        {
            double gx = pos.X;
            double gy = pos.Y;

            gx = pos.X / gridXPixels;
            gx = Math.Round(gx) * gridXPixels;
            gy = pos.Y / gridYPixels;
            gy = Math.Round(gy) * gridYPixels;

            return new Point(ToMMX(gx), ToMMY(gy));
        }

        public override void SetGridIntervals(double gridXMM, double gridYMM)
        {
            this.gridXMM = gridXMM;
            this.gridYMM = gridYMM;
        }
    }
}