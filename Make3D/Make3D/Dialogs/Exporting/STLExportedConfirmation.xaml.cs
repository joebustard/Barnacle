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

using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for STLExportedConfirmation.xaml
    /// </summary>
    public partial class STLExportedConfirmation : Window
    {
        public STLExportedConfirmation()
        {
            InitializeComponent();
        }

        public string ExportPath { get; internal set; }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void OpenFileClick(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(ExportPath);
            startInfo.WindowStyle = ProcessWindowStyle.Minimized;
            Process.Start(startInfo);
            DialogResult = true;
            Close();
        }

        private void OpenFolderClick(object sender, RoutedEventArgs e)
        {
            string pth = "";
            if (ExportPath.Contains(","))
            {
                string[] lines = ExportPath.Split(',');
                pth = System.IO.Path.GetDirectoryName(lines[0]);
            }
            else
            {
                pth = System.IO.Path.GetDirectoryName(ExportPath);
            }
            if (Directory.Exists(pth))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo("explorer.exe");
                startInfo.Arguments = pth;
                startInfo.WindowStyle = ProcessWindowStyle.Normal;
                Process.Start(startInfo);
            }
            DialogResult = true;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string pth = "";
            if (ExportPath.Contains(","))
            {
                OpenFileButton.IsEnabled = false;
                string[] lines = ExportPath.Split(',');
                pth = System.IO.Path.GetDirectoryName(lines[0]);
                ExportLabel.Content = "Models have been exported to";
            }
            else
            {
                ExportLabel.Content = "Model has been exported to";
                pth = ExportPath;
            }
            PathLabel.Content = pth;
        }
    }
}