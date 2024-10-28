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

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for ConfirmObjectNameDlg.xaml
    /// </summary>
    public partial class ConfirmObjectNameDlg : Window, INotifyPropertyChanged
    {
        private String leftName;

        private string objectName;

        private String rightName;

        public ConfirmObjectNameDlg()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string LeftName
        {
            get { return leftName; }
            set
            {
                if (value != leftName)
                {
                    leftName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string ObjectName
        {
            get { return objectName; }

            set
            {
                if (value != objectName)
                {
                    objectName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string RightName
        {
            get { return rightName; }
            set
            {
                if (value != rightName)
                {
                    rightName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void LeftNameClicked(object sender, RoutedEventArgs e)
        {
            ObjectName = LeftName;
            DialogResult = true;
            Close();
        }

        private void RightNameClicked(object sender, RoutedEventArgs e)
        {
            ObjectName = RightName;
            DialogResult = true;
            Close();
        }
    }
}