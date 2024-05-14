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

using MakerLib;
using System;
using System.ComponentModel;
using System.Windows;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for Pie.xaml
    /// </summary>
    public partial class PieDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private const double maxcentreThickness = 100;

        private const double maxradius = 200;
        private const double maxsweep = 359;
        private const double maxthickness = 100;

        private const double mincentreThickness = 0.1;

        private const double minradius = 5;
        private const double minsweep = 1;
        private const double minthickness = 1;

        private double centreThickness;
        private Visibility centreThicknessVisibility;
        private double edgeThickness;
        private bool loaded;

        private double radius;
        private double sweep;
        private string warningText;
        private bool wedgeSelected;

        public PieDlg()
        {
            InitializeComponent();
            ToolName = "Pie";
            DataContext = this;
            ModelGroup = MyModelGroup;
            loaded = false;
            centreThicknessVisibility = Visibility.Hidden;
        }

        public double CentreThickness
        {
            get
            {
                return centreThickness;
            }

            set
            {
                if (centreThickness != value)
                {
                    if (value >= mincentreThickness && value <= maxcentreThickness)
                    {
                        centreThickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String CentreThicknessToolTip
        {
            get
            {
                return $"Centre Thickness must be in the range {mincentreThickness} to {maxcentreThickness}";
            }
        }

        public Visibility CentreThicknessVisibility
        {
            get
            {
                return centreThicknessVisibility;
            }

            set
            {
                if (value != centreThicknessVisibility)
                {
                    centreThicknessVisibility = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double Radius
        {
            get
            {
                return radius;
            }

            set
            {
                if (radius != value)
                {
                    if (value >= minradius && value <= maxradius)
                    {
                        radius = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String RadiusToolTip
        {
            get
            {
                return $"Radius must be in the range {minradius} to {maxradius}";
            }
        }

        public override bool ShowAxies
        {
            get
            {
                return showAxies;
            }

            set
            {
                if (showAxies != value)
                {
                    showAxies = value;
                    NotifyPropertyChanged();
                    Redisplay();
                }
            }
        }

        public override bool ShowFloor
        {
            get
            {
                return showFloor;
            }

            set
            {
                if (showFloor != value)
                {
                    showFloor = value;
                    NotifyPropertyChanged();
                    Redisplay();
                }
            }
        }

        public double Sweep
        {
            get
            {
                return sweep;
            }

            set
            {
                if (sweep != value)
                {
                    if (value >= minsweep && value <= maxsweep)
                    {
                        sweep = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double Thickness
        {
            get
            {
                return edgeThickness;
            }

            set
            {
                if (edgeThickness != value)
                {
                    if (value >= minthickness && value <= maxthickness)
                    {
                        edgeThickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String ThicknessToolTip
        {
            get
            {
                return $"Thickness must be in the range {minthickness} to {maxthickness}";
            }
        }

        public string WarningText
        {
            get
            {
                return warningText;
            }

            set
            {
                if (warningText != value)
                {
                    warningText = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool WedgeSelected
        {
            get { return wedgeSelected; }

            set
            {
                if (value != wedgeSelected)
                {
                    wedgeSelected = value;
                    if (wedgeSelected)
                    {
                        CentreThicknessVisibility = Visibility.Visible;
                    }
                    else
                    {
                        CentreThicknessVisibility = Visibility.Hidden;
                    }
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParmeters();
            DialogResult = true;
            Close();
        }

        private void GenerateShape()
        {
            ClearShape();
            PieMaker maker;
            if (wedgeSelected)
            {
                maker = new PieMaker(radius, centreThickness, edgeThickness, sweep);
            }
            else
            {
                maker = new PieMaker(radius, edgeThickness, edgeThickness, sweep);
            }
            maker.Generate(Vertices, Faces);
            CentreVertices();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            Radius = EditorParameters.GetDouble("Radius", 20);
            Thickness = EditorParameters.GetDouble("Thickness", 5);
            Sweep = EditorParameters.GetDouble("Sweep", 90);

            WedgeSelected = EditorParameters.GetBoolean("WedgeSelected", false);
            CentreThickness = EditorParameters.GetDouble("CentreThickness", 1);
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("Radius", Radius.ToString());
            EditorParameters.Set("Thickness", Thickness.ToString());
            EditorParameters.Set("Sweep", Sweep.ToString());

            EditorParameters.Set("WedgeSelected", WedgeSelected.ToString());
            EditorParameters.Set("CentreThickness", CentreThickness.ToString());
        }

        private void UpdateDisplay()
        {
            if (loaded)
            {
                GenerateShape();
                Redisplay();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WarningText = "";
            LoadEditorParameters();

            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            loaded = true;

            UpdateDisplay();
        }
    }
}