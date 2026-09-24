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
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace Barnacle.Models.Adorners
{
    internal class LengthAndHeightAdorner
    {
        public delegate void UserEditedLengthOrHeight(double length, double height);
        public UserEditedLengthOrHeight OnDimensionsChangedByUser;

        private Point anchor;
        public Point Anchor
        {
            get
            {
                return anchor;
            }
            set
            {
                anchor = value;
            }
        }
        private double length;
        private double height;
        public double Length
        {
            get
            {
                return length;
            }
            set
            {
                length = value;
            }
        }
        public double Height
        {
            get
            {
                return height;
            }
            set
            {
                height = value;
            }
        }
        public Canvas Overlay
        {
            get; internal set;
        }
        public LengthAndHeightAdorner() : base()
        {
            OnDimensionsChangedByUser = null;
            length = 0;
            height = 0;
        }
        private TextBox lengthTextBox;
        private TextBox heightTextBox;
        internal void Display(UIElementCollection children)
        {
            double ll = 4;
            // create the anchor mark which is actually a pair of  horizontal and vertical lines
            Addline(Anchor.X, Anchor.Y, Anchor.X + ll, Anchor.Y, children);
            Addline(Anchor.X, Anchor.Y, Anchor.X, Anchor.Y + ll, children);

            // line at top right
            Addline(Anchor.X, Anchor.Y - height, Anchor.X + ll, Anchor.Y - height, children);

            // vertical connector
            Addline(Anchor.X + ll / 2, Anchor.Y - height, Anchor.X + ll / 2, Anchor.Y, children);

            // top v arrow
            Addline(Anchor.X + ll / 2, Anchor.Y - height, Anchor.X + ll, Anchor.Y - height + ll / 2, children);
            Addline(Anchor.X + ll / 2, Anchor.Y - height, Anchor.X, Anchor.Y - height + ll / 2, children);


            // bottom v arrow
            Addline(Anchor.X + ll / 2, Anchor.Y, Anchor.X + ll, Anchor.Y - ll / 2, children);
            Addline(Anchor.X + ll / 2, Anchor.Y, Anchor.X, Anchor.Y - ll / 2, children);


            // line at bottom left
            Addline(Anchor.X - length, Anchor.Y, Anchor.X - length, Anchor.Y + ll, children);

            // horizontal connector
            Addline(Anchor.X - length, Anchor.Y + ll / 2, Anchor.X, Anchor.Y + ll / 2, children);

            // left arrow
            Addline(Anchor.X - length, Anchor.Y + ll / 2, Anchor.X - length + ll / 2, Anchor.Y, children);
            Addline(Anchor.X - length, Anchor.Y + ll / 2, Anchor.X - length + ll / 2, Anchor.Y + ll, children);

            // right arrow
            Addline(Anchor.X, Anchor.Y + ll / 2, Anchor.X - ll / 2, Anchor.Y, children);
            Addline(Anchor.X, Anchor.Y + ll / 2, Anchor.X - ll / 2, Anchor.Y + ll, children);

            double lx = Anchor.X - length / 2;
            if (lx < Anchor.X - length)
            {
                lx = Anchor.X - length;
            }
            lengthTextBox = AddTextBox(lx, Anchor.Y + 5, length, children);
            heightTextBox = AddTextBox(Anchor.X + ll, Anchor.Y - height / 2, height, children);
        }

        private TextBox AddTextBox(double x, double y, double v, UIElementCollection children)
        {
            TextBox tbx = new TextBox();
            tbx.Width = 80;
            tbx.FontSize = 16;
            tbx.Background = Brushes.Cornsilk;
            Canvas.SetLeft(tbx, ToPixelX(x));
            Canvas.SetTop(tbx, ToPixelY(y));
            tbx.Text = v.ToString();
            children.Add(tbx);
            tbx.TextAlignment = TextAlignment.Center;
            tbx.LostFocus += Tbx_LostFocus;
            return tbx;
        }

        private void Tbx_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {

                TextBox tbx = sender as TextBox;
                if (tbx != null)
                {
                    double v = Convert.ToDouble(tbx.Text);
                    double l = length;
                    double h = height;
                    // only notify if anything has actually changed
                    if (v > 0 && (OnDimensionsChangedByUser != null))
                    {
                        if (tbx == lengthTextBox && v != l)
                        {
                            l = v;
                        }
                        if (tbx == heightTextBox && v != h)
                        {
                            h = v;
                        }
                        if (length != l || height != h)
                        {

                            OnDimensionsChangedByUser(l, h);
                        }
                    }
                }

            }
            catch (Exception ex) { }
        }



        private void Addline(double x1, double y1, double x2, double y2, UIElementCollection children)
        {
            Line ln = new Line();
            ln.Stroke = Brushes.Black;
            ln.StrokeThickness = 3;

            ln.Fill = Brushes.Black;

            ln.X1 = ToPixelX(x1);
            ln.Y1 = ToPixelY(y1);
            ln.X2 = ToPixelX(x2);
            ln.Y2 = ToPixelY(y2);
            children.Add(ln);
        }

        public double ToPixelX(double x)
        {
            DpiScale sc = VisualTreeHelper.GetDpi(Overlay);
            double res = sc.PixelsPerInchX * x / 25.4;
            return res;
        }
        public double ToPixelY(double y)
        {
            DpiScale sc = VisualTreeHelper.GetDpi(Overlay);
            double res = sc.PixelsPerInchY * y / 25.4;
            return res;
        }
    }
}
