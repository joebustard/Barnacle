﻿// **************************************************************************
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

namespace Barnacle.UserControls.SideBevelSelector
{
    /// <summary>
    /// Interaction logic for SideBevelSelector.xaml
    /// </summary>
    public partial class SideBevelSelector : UserControl, INotifyPropertyChanged
    {
        public ImageSource bottomImage;
        public ImageSource leftImage;
        public ImageSource rightImage;
        public ImageSource topImage;
        private bool bottomBevelled;
        private bool leftBevelled;
        private bool rightBevelled;
        private bool topBevelled;

        public SideBevelSelector()
        {
            InitializeComponent();
            topBevelled = false;
            bottomBevelled = false;
            leftBevelled = false;
            rightBevelled = false;
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool BottomBevelled
        {
            get
            {
                return bottomBevelled;
            }
            set
            {
                if (value != bottomBevelled)
                {
                    bottomBevelled = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ImageSource BottomImage
        {
            get
            {
                return bottomImage;
            }

            set
            {
                if (bottomImage != value)
                {
                    bottomImage = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool LeftBevelled
        {
            get
            {
                return leftBevelled;
            }
            set
            {
                if (value != leftBevelled)
                {
                    leftBevelled = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ImageSource LeftImage
        {
            get
            {
                return leftImage;
            }

            set
            {
                if (leftImage != value)
                {
                    leftImage = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool RightBevelled
        {
            get
            {
                return rightBevelled;
            }
            set
            {
                if (value != rightBevelled)
                {
                    rightBevelled = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ImageSource RightImage
        {
            get
            {
                return rightImage;
            }

            set
            {
                if (rightImage != value)
                {
                    rightImage = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool TopBevelled
        {
            get
            {
                return topBevelled;
            }
            set
            {
                if (value != topBevelled)
                {
                    topBevelled = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ImageSource TopImage
        {
            get
            {
                return topImage;
            }

            set
            {
                if (topImage != value)
                {
                    topImage = value;
                    NotifyPropertyChanged();
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

        internal void SetAll(bool leftBevel, bool topBevel, bool rightBevel, bool bottomBevel)
        {
            leftBevelled = leftBevel;
            rightBevelled = rightBevel;
            topBevelled = topBevel;
            bottomBevelled = bottomBevel;
            UpdateShapes();
        }

        private void BottomClick(object sender, RoutedEventArgs e)
        {
            BottomBevelled = !BottomBevelled;
            UpdateShapes();
        }

        private Uri ImageUri(string p)
        {
            return new Uri(@"pack://application:,,,/Images/Buttons/" + p);
        }

        private void LeftClick(object sender, RoutedEventArgs e)
        {
            LeftBevelled = !LeftBevelled;
            UpdateShapes();
        }

        private void RightClick(object sender, RoutedEventArgs e)
        {
            RightBevelled = !RightBevelled;
            UpdateShapes();
        }

        private void TopClick(object sender, RoutedEventArgs e)
        {
            TopBevelled = !TopBevelled;
            UpdateShapes();
        }

        private void UpdateShapes()
        {
            if (topBevelled)
            {
                BitmapImage icon = new BitmapImage(ImageUri("TopBevelled.png"));
                TopImage = icon;
            }
            else
            {
                BitmapImage icon = new BitmapImage(ImageUri("TopNotBevelled.png"));
                TopImage = icon;
            }

            if (bottomBevelled)
            {
                BitmapImage icon = new BitmapImage(ImageUri("BottomBevelled.png"));
                BottomImage = icon;
            }
            else
            {
                BitmapImage icon = new BitmapImage(ImageUri("BottomNotBevelled.png"));
                BottomImage = icon;
            }

            if (leftBevelled)
            {
                BitmapImage icon = new BitmapImage(ImageUri("LeftBevelled.png"));
                LeftImage = icon;
            }
            else
            {
                BitmapImage icon = new BitmapImage(ImageUri("LeftNotBevelled.png"));
                LeftImage = icon;
            }

            if (rightBevelled)
            {
                BitmapImage icon = new BitmapImage(ImageUri("RightBevelled.png"));
                RightImage = icon;
            }
            else
            {
                BitmapImage icon = new BitmapImage(ImageUri("RightNotBevelled.png"));
                RightImage = icon;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateShapes();
        }
    }
}