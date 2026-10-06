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
using System.Windows.Threading;

namespace Barnacle.UserControls
{
    /// <summary>
    /// Interaction logic for PlaneDirectionControl.xaml
    /// </summary>
    public partial class PlaneDirectionControl : UserControl
    {
        public enum Direction
        {
            None, Up, Down, Left, Right
        }

        public delegate void PlaneDirectionChanged(Direction direction);

        public PlaneDirectionChanged OnUpdated;
        private DispatcherTimer timer;
        private Direction selectedDirection;

        public PlaneDirectionControl()
        {
            InitializeComponent();
            OnUpdated = null;
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 1);
            timer.Tick += Timer_Tick;
            selectedDirection = Direction.None;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            ReportChange();
        }

        private void ReportChange()
        {
            if (OnUpdated != null)
            {
                OnUpdated(selectedDirection);
            }
            //         timer.Start();
        }

        private void DownButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DownScaleTransform.ScaleX = 0.9;
            DownScaleTransform.ScaleY = 0.9;
            selectedDirection = Direction.Down;
            ReportChange();
        }

        private void DownButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            timer.Stop();
            DownScaleTransform.ScaleX = 1.0;
            DownScaleTransform.ScaleY = 1.0;
        }

        private void LeftButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            LeftScaleTransform.ScaleX = 0.9;
            LeftScaleTransform.ScaleY = 0.9;
            selectedDirection = Direction.Left;
            ReportChange();
        }

        private void LeftButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            timer.Stop();
            LeftScaleTransform.ScaleX = 1.0;
            LeftScaleTransform.ScaleY = 1.0;
        }

        private void RightButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RightScaleTransform.ScaleX = 0.9;
            RightScaleTransform.ScaleY = 0.9;
            selectedDirection = Direction.Right;
            ReportChange();
        }

        private void RightButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            timer.Stop();
            RightScaleTransform.ScaleX = 1.0;
            RightScaleTransform.ScaleY = 1.0;
        }

        private void UpButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            UpScaleTransform.ScaleX = 0.9;
            UpScaleTransform.ScaleY = 0.9;
            selectedDirection = Direction.Up;
            ReportChange();
        }

        private void UpButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            timer.Stop();
            UpScaleTransform.ScaleX = 1.0;
            UpScaleTransform.ScaleY = 1.0;
        }
    }
}