﻿using System;
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
            pixelsPerInchX = sc.PixelsPerInchX;
            pixelsPerInchY = sc.PixelsPerInchY;
            gridXPixels = (sc.PixelsPerInchX / 25.4) * gridXMM;

            gridYPixels = (sc.PixelsPerInchY / 25.4) * gridYMM;

            while (x < actualWidth)
            {
                y = 0;
                while (y < actualHeight)
                {
                    Ellipse el = new Ellipse();
                    Canvas.SetLeft(el, x - 1);
                    Canvas.SetTop(el, y - 1);
                    el.Width = 3;
                    el.Height = 3;
                    el.Fill = Brushes.AliceBlue;
                    el.Stroke = Brushes.CadetBlue;
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