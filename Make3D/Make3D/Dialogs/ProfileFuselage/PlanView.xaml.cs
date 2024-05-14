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
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for ViewManager.xaml
    /// </summary>
    public partial class PlanView : UserControl, INotifyPropertyChanged
    {
        private string commandText;

        public string CommandText
        {
            get { return commandText; }

            set
            {
                if (value != commandText)
                {
                    commandText = value;

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

        private string imageFilePath;

        public string ImageFilePath
        {
            get { return imageFilePath; }

            set
            {
                if (value != imageFilePath)
                {
                    imageFilePath = value;
                    PathControl.ImagePath = imageFilePath;
                    PathControl.FetchImage();
                }
            }
        }

        public delegate void CommandHandler(string command);

        public CommandHandler OnCommandHandler;

        public event PropertyChangedEventHandler PropertyChanged;

        public PlanView()
        {
            InitializeComponent();
            CommandText = "Load It";
            DataContext = this;
        }

        public void Clear()
        {
            imageFilePath = "";
            PathControl.Clear();
        }

        private void LoadClicked(object sender, RoutedEventArgs e)
        {
            OnCommandHandler?.Invoke(CommandText);
        }

        internal void RenderFlexipath(ref Bitmap bmp, out double tlx, out double tly, out double brx, out double bry)
        {
            PathControl.RenderFlexipath(ref bmp, out tlx, out tly, out brx, out bry);
        }
    }
}