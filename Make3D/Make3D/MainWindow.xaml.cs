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

using asdflibrary;
using Barnacle.Dialogs;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Barnacle.Models;
using Barnacle.Models.BufferedPolyline;
using System.Collections.Generic;

namespace Barnacle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.RestoreSizeAndLocation();
            PrepareUndo();
        }

        private void PrepareUndo()
        {
            string fld = System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BarnacleUndo";
            undoer.Initialise(fld);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.SaveSizeAndLocation();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            IInputElement ele = Keyboard.FocusedElement;
            if (!(ele is TextBox))
            {
                NotificationManager.Notify("KeyDown", e);
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            NotificationManager.Notify("KeyUp", e);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // check if we have been opened with a project file name on the command line
            string[] args = Environment.GetCommandLineArgs();
            string startProject = "";
            foreach (string s in args)
            {
                if (s.EndsWith(".bmf"))
                {
                    startProject = s;
                    break;
                }
            }
            if (startProject != "")
            {
                NotificationManager.Notify("ReloadProject", startProject);
            }

            InfoWindow.Instance().Owner = this;
        }
    }
}