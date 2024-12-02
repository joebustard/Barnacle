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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for CornerShape.xaml
    /// </summary>
    public partial class CornerShape : UserControl, INotifyPropertyChanged
    {
        public ModeChanged OnModeChanged;
        private ImageSource currentImage;
        private CornerLocation location;

        private int mode;

        public CornerShape()
        {
            InitializeComponent();
            DataContext = this;
            location = CornerLocation.TopLeft;
            Mode = 0;
            SetShapeImage(Mode);
        }

        public delegate void ModeChanged(int mode);

        public event PropertyChangedEventHandler PropertyChanged;

        public enum CornerLocation
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }

        public ImageSource CurrentImage
        {
            get
            {
                return currentImage;
            }
            set
            {
                if (currentImage != value)
                {
                    currentImage = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public CornerLocation Location
        {
            get
            {
                return location;
            }
            set
            {
                if (value != location)
                {
                    location = value;
                    SetShapeImage(Mode);
                }
            }
        }

        public int Mode
        {
            get
            {
                return mode;
            }
            set
            {
                if (mode != value)
                {
                    mode = value;
                    SetShapeImage(mode);
                }
            }
        }

        public virtual void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Mode = Mode + 1;
            if (Mode >= 3)
            {
                Mode = 0;
            }
            SetShapeImage(Mode);
            if (OnModeChanged != null)
            {
                OnModeChanged(Mode);
            }
        }

        private Uri ImageUri(string p)
        {
            return new Uri(@"pack://application:,,,/Images/Buttons/" + p);
        }

        private void SetShapeImage(int mode)
        {
            string im = "";
            switch (location)
            {
                case CornerLocation.TopLeft:
                    {
                        if (Mode == 0) im = "TopLeft0";
                        if (Mode == 1) im = "TopLeft1";
                        if (Mode == 2) im = "TopLeft2";
                    }
                    break;

                case CornerLocation.TopRight:
                    {
                        if (Mode == 0) im = "TopRight0";
                        if (Mode == 1) im = "TopRight1";
                        if (Mode == 2) im = "TopRight2";
                    }
                    break;

                case CornerLocation.BottomLeft:
                    {
                        if (Mode == 0) im = "BottomLeft0";
                        if (Mode == 1) im = "BottomLeft1";
                        if (Mode == 2) im = "BottomLeft2";
                    }
                    break;

                case CornerLocation.BottomRight:
                    {
                        if (Mode == 0) im = "BottomRight0";
                        if (Mode == 1) im = "BottomRight1";
                        if (Mode == 2) im = "BottomRight2";
                    }
                    break;
            }
            if (im != "")
            {
                im = im + ".png";
                BitmapImage icon = new BitmapImage(ImageUri(im));
                CurrentImage = icon;
            }
        }
    }
}