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

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for DecimatorSettings.xaml
    /// </summary>
    public partial class DecimatorSettings : Window
    {
        public DecimatorSettings()
        {
            InitializeComponent();
            OriginalFaceCount = 0;
            TargetFaceCount = 0;
        }

        public int OriginalFaceCount
        {
            get; set;
        }

        public int TargetFaceCount
        {
            get; set;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            TargetFaceCount = Convert.ToInt32(TargetBox.Text);
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            OriginalFacesLabel.Content = $"Object has {OriginalFaceCount} faces.";
            TargetBox.Text = (OriginalFaceCount * 90 / 100).ToString();
        }
    }
}