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

using Barnacle.UserControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Barnacle.UserControls
{
    /// <summary>
    /// Interaction logic for GridSettings.xaml
    /// </summary>
    public partial class GridSettingsDlg : Window, INotifyPropertyChanged
    {
        public GridSettings Settings { get; set; }

        public GridSettingsDlg()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void DefaultButton_Click(object sender, RoutedEventArgs e)
        {
            PolarAngle = 10;
            Polar10Checked = true;
            Rect10Checked = true;
            LineThickness = 4;
            LineColour = ColourPicker.FindAvailableColour(Colors.Yellow);
            LineOpacity = 0.5;

        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (Settings != null)
            {
                if (Rect2Point5Checked) Settings.RectangularGridSize = 2.5;
                if (Rect5Checked) Settings.RectangularGridSize = 5;
                if (Rect10Checked) Settings.RectangularGridSize = 10;
                if (Rect15Checked) Settings.RectangularGridSize = 15;
                if (Rect20Checked) Settings.RectangularGridSize = 20;
                if (Rect25Checked) Settings.RectangularGridSize = 25;
                if (Polar5Checked) Settings.PolarGridRadius = 5;
                if (Polar10Checked) Settings.PolarGridRadius = 10;
                if (Polar15Checked) Settings.PolarGridRadius = 15;
                if (Polar20Checked) Settings.PolarGridRadius = 20;
                if (polarAngle > 0 && polarAngle < 120)
                {
                    Settings.PolarGridAngle = polarAngle;
                }
            }
            DialogResult = true;
            Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool polar5Checked;

        public bool Polar5Checked
        {
            get { return polar5Checked; }
            set
            {
                if (value != polar5Checked)
                {
                    polar5Checked = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private bool polar10Checked;

        public bool Polar10Checked
        {
            get { return polar10Checked; }
            set
            {
                if (value != polar10Checked)
                {
                    polar10Checked = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private bool polar15Checked;

        public bool Polar15Checked
        {
            get { return polar15Checked; }
            set
            {
                if (value != polar15Checked)
                {
                    polar15Checked = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private bool polar20Checked;

        public bool Polar20Checked
        {
            get { return polar20Checked; }
            set
            {
                if (value != polar20Checked)
                {
                    polar20Checked = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private bool rect2Point5Checked;

        public bool Rect2Point5Checked
        {
            get { return rect2Point5Checked; }
            set
            {
                if (value != rect2Point5Checked)
                {
                    rect2Point5Checked = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private bool rect5Checked;

        public bool Rect5Checked
        {
            get { return rect5Checked; }
            set
            {
                if (value != rect5Checked)
                {
                    rect5Checked = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private bool rect10Checked;

        public bool Rect10Checked
        {
            get { return rect10Checked; }
            set
            {
                if (value != rect10Checked)
                {
                    rect10Checked = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private bool rect15Checked;

        public bool Rect15Checked
        {
            get { return rect15Checked; }
            set
            {
                if (value != rect15Checked)
                {
                    rect15Checked = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private bool rect20Checked;

        public bool Rect20Checked
        {
            get { return rect20Checked; }
            set
            {
                if (value != rect20Checked)
                {
                    rect20Checked = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private bool rect25Checked;

        public bool Rect25Checked
        {
            get { return rect25Checked; }
            set
            {
                if (value != rect25Checked)
                {
                    rect25Checked = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private double polarAngle;

        public double PolarAngle
        {
            get { return polarAngle; }
            set
            {
                if (value != polarAngle)
                {
                    if (value < 1)
                    {
                        value = 1;
                    }
                    if (value > 120)
                    {
                        value = 120;
                    }
                    polarAngle = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private AvailableColour lineColour;
        public AvailableColour LineColour
        {
            get
            {
                return lineColour;
            }
            set
            {
                if (value != lineColour)
                {
                    lineColour = value;
                    ColourOfLine.SelectedColour = lineColour;
                    if (Settings != null )
                    {
                        Settings.LineColour = lineColour.Colour;
                    }
                    NotifyPropertyChanged();
                }
            }
        }
        private double lineOpacity;

        public double LineOpacity
        {
            get { return lineOpacity; }
            set
            {
                if (lineOpacity != value)
                {
                    lineOpacity = value;
                    if (Settings != null)
                    {
                        Settings.LineOpacity = lineOpacity;
                    }

                    NotifyPropertyChanged();
                }
            }
        }


        private double lineThickness;

        public double LineThickness
        {
            get { return lineThickness; }
            set
            {
                if (lineThickness != value)
                {
                    lineThickness = value;
                    if ( Settings != null)
                    {
                        Settings.LineThickness = lineThickness;
                    }
                    NotifyPropertyChanged();
                }
            }
        }

        private void ColourOfNewObject_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            LineColour = ColourOfLine.SelectedColour;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           
            
            ColourOfLine.PropertyChanged += ColourOfNewObject_PropertyChanged;
            ColourOfLine.NotifyPropertyChanged("AvailableColours");
            
            if (Settings == null)
            {
                Rect10Checked = true;
                PolarAngle = 10;
                Polar10Checked = true;
                LineColour = ColourPicker.FindAvailableColour(Colors.Yellow);
                LineOpacity =0.5;
                LineThickness = 4;
                ColourOfLine.SelectedColour = LineColour;

            }
            else
            {
                switch (Settings.RectangularGridSize)
                {
                    case 5:
                        {
                            Rect5Checked = true;
                        }
                        break;

                    case 10:
                        {
                            Rect10Checked = true;
                        }
                        break;

                    case 15:
                        {
                            Rect15Checked = true;
                        }
                        break;

                    case 20:
                        {
                            Rect20Checked = true;
                        }
                        break;
                }

                switch (Settings.PolarGridRadius)
                {
                    case 5:
                        {
                            Polar5Checked = true;
                        }
                        break;

                    case 10:
                        {
                            Polar10Checked = true;
                        }
                        break;

                    case 15:
                        {
                            Polar15Checked = true;
                        }
                        break;

                    case 20:
                        {
                            Polar20Checked = true;
                        }
                        break;
                }
                PolarAngle = Settings.PolarGridAngle;
                LineColour = ColourPicker.FindAvailableColour(Settings.LineColour);
                LineOpacity = Settings.LineOpacity;
                LineThickness = Settings.LineThickness;
                
            }
        }
    }
}