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
using System.Windows.Threading;
using Barnacle.Views;
using FileUtils;
using Barnacle.ViewModels;
using System.IO;

namespace Barnacle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string autoModelFile = "";
        private DispatcherTimer autoRunTimer = null;

        private string autoScriptToRun = "";

        private bool autoSlice = false;
        private string autoSlicePrinter = "";
        private string autoSliceProfile = "";
        private string autoStartProject = "";
        //        private bool minimiseOnStart = false;

        public MainWindow()
        {
            InitializeComponent();
            this.RestoreSizeAndLocation();
            PrepareUndo();
        }

        private void AutoRunScript()
        {
            NotificationManager.Notify("StartWithOldProjectNoLoad", autoStartProject);
            NotificationManager.Notify("AutoRunScript", autoScriptToRun);
        }

        private void AutoRunTimer_Tick(object sender, EventArgs e)
        {
            autoRunTimer?.Stop();
            AutoRunScript();

            // set the exit code so any batch program knows if autorun succeeded
            int res = -1;
            if (DefaultView.AutoRunResult)
            {
                res = 0;
            }
            Application.Current.Shutdown(res);
        }

        private void AutoSlice()
        {
            NotificationManager.Notify("StartWithOldProjectNoLoad", autoStartProject);
            int res = (this.DataContext as MainWindowViewModel).AutoSlice(autoSlicePrinter, autoSliceProfile, autoModelFile);
            Application.Current.Shutdown(res);
        }

        private void AutoSliceTimer_Tick(object sender, EventArgs e)
        {
            autoRunTimer?.Stop();
            AutoSlice();

            // set the exit code so any batch program knows if autorun succeeded
            int res = -1;
            if (DefaultView.AutoRunResult)
            {
                res = 0;
            }
            Application.Current.Shutdown(res);
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
            // save the current window state IF we are not auto running
            // i.e. only record the size and position selected by the user if we are not autorunning
            if (autoRunTimer == null)
            {
                this.SaveSizeAndLocation();
            }
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
            // create the default folders if needed
            PathManager.CreateDefaultFolders();

            // check if we have been opened with a project file name on the command line
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                string s = args[i];
                string l = s.ToLower();
                if (l == "-m")
                {
                    //minimiseOnStart = true;
                    Application.Current.MainWindow.WindowState = WindowState.Minimized;
                }
                if (s.EndsWith(".bmf") && autoStartProject == "")
                {
                    autoStartProject = s;
                }
                if (s.EndsWith(".lmp") && autoScriptToRun == "")
                {
                    autoScriptToRun = s;
                }
                if (s.EndsWith(".txt") && autoModelFile == "")
                {
                    autoModelFile = s;
                }
                if (l == "-slice")
                {
                    autoSlice = true;
                }
                if (l == "-profile")
                {
                    autoSliceProfile = args[++i];
                }
                if (l == "-printer")
                {
                    autoSlicePrinter = args[++i];
                }
            }
            if (autoStartProject != "" && autoScriptToRun != "")
            {
                // command line asks us to load a project and run a script in it
                // We can't do that from here as things haven't fully started yet
                // so delay for a few seconds
                autoRunTimer = new DispatcherTimer();
                autoRunTimer.Interval = new TimeSpan(0, 0, 1);
                autoRunTimer.Tick += AutoRunTimer_Tick;
                autoRunTimer.Start();
            }
            else
            if (autoStartProject != "" && autoSlice && autoModelFile != "")
            {
                // command line is asking us to load a project and file and to slice it
                // We can't do that from here as things haven't fully started yet
                // so delay for a few seconds
                autoRunTimer = new DispatcherTimer();
                autoRunTimer.Interval = new TimeSpan(0, 0, 1);
                autoRunTimer.Tick += AutoSliceTimer_Tick;
                autoRunTimer.Start();
            }

            InfoWindow.Instance().Owner = this;
        }
    }
}