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

using System.Windows.Controls;
using System.Windows.Threading;

namespace Barnacle.Views
{
    /// <summary>
    /// Interaction logic for ObjectPropertiesView.xaml
    /// </summary>
    public partial class ObjectPropertiesView : UserControl
    {
        private DispatcherTimer tabTimer;

        public ObjectPropertiesView()
        {
            InitializeComponent();
            tabTimer = new DispatcherTimer();
            tabTimer.Interval = new System.TimeSpan(0, 0, 0, 0, 500);
            tabTimer.Tick += TabTimer_Tick;
        }

        private void TabTimer_Tick(object sender, System.EventArgs e)
        {
            // reset the focus back to the main view
            NotificationManager.Notify("RefocusEditor", null);
        }

        private void TextBox_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Tab)
            {
                tabTimer.Start();
            }
        }
    }
}