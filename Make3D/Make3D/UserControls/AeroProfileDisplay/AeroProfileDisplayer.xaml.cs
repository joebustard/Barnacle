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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Barnacle.UserControls
{
    /// <summary>
    /// Interaction logic for AeroProfileDisplayer.xaml
    /// </summary>
    public partial class AeroProfileDisplayer : UserControl
    {
        private List<Point> profilePnts;

        public List<Point> ProfilePnts
        {
            get { return profilePnts; }
            set
            {
                if (value != profilePnts)
                {
                    profilePnts = value;
                    if (profilePnts != null && profilePnts.Count > 0)
                    {
                        Refresh();
                    }
                }
            }
        }

        public void Refresh()
        {
            profileCanvas.Children.Clear();

            double border = 8;
            double hc = profileCanvas.ActualHeight / 2.0;
            double h = (profileCanvas.ActualHeight - 2 * border);
            double w = (profileCanvas.ActualWidth - 2 * border);
            double yscale = w / h;
            double xscale = 1;

            if (profilePnts != null && profilePnts.Count > 0)
            {
                Polyline pln = new Polyline();
                pln.Stroke = Brushes.Black;
                pln.StrokeThickness = 2;
                pln.Points = new PointCollection();
                pln.Fill = Brushes.Red;
                double miny = double.MaxValue;
                double maxy = double.MinValue;

                foreach (Point p in profilePnts)
                {
                    miny = Math.Min(p.Y, miny);
                    maxy = Math.Max(p.Y, maxy);
                }
                double ydiff = maxy - miny;
                while (ydiff * yscale > 1.0)
                {
                    yscale = yscale * 0.9;
                    xscale = xscale * 0.9;
                }
                foreach (Point p in profilePnts)
                {
                    Point n = new Point(p.X * w * xscale + border, (-p.Y * h * yscale) + hc);
                    pln.Points.Add(n);
                }
                profileCanvas.Children.Add(pln);
            }

            Line ln = new Line();
            ln.Stroke = Brushes.Blue;
            ln.StrokeThickness = 2;
            DoubleCollection dc = new DoubleCollection();
            dc.Add(0.25);
            dc.Add(0.5);
            dc.Add(0.25);
            ln.StrokeDashArray = dc;
            ln.X1 = border;
            ln.Y1 = hc;
            ln.X2 = w - border;
            ln.Y2 = hc;

            profileCanvas.Children.Add(ln);
        }

        public AeroProfileDisplayer()
        {
            InitializeComponent();
            ProfilePnts = null;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (profilePnts != null)
            {
                Refresh();
            }
        }
    }
}