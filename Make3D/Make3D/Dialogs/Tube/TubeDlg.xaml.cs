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

using MakerLib;
using System;
using System.ComponentModel;
using System.Windows;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for Blank.xaml
    /// </summary>
    public partial class TubeDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private double innerRadius;
        private double lowerBevel;
        private double sweepDegrees;
        private double thickness;
        private double tubeHeight;
        private double upperBevel;

        public TubeDlg()
        {
            InitializeComponent();
            ToolName = "Tube";
            TubeHeight = 20;
            InnerRadius = 20;
            TubeThickness = 5;
            UpperBevel = 0;
            LowerBevel = 0;
            SweepDegrees = 360;
            DataContext = this;
            ModelGroup = MyModelGroup;
        }

        public double InnerRadius
        {
            get
            {
                return innerRadius;
            }
            set
            {
                if (innerRadius != value)
                {
                    innerRadius = value;
                    GenerateRing();
                    Redisplay();
                    NotifyPropertyChanged();
                }
            }
        }

        public double LowerBevel
        {
            get
            {
                return lowerBevel;
            }
            set
            {
                if (lowerBevel != value)
                {
                    lowerBevel = value;
                    GenerateRing();
                    Redisplay();
                    NotifyPropertyChanged();
                }
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

        public double SweepDegrees
        {
            get
            {
                return sweepDegrees;
            }
            set
            {
                sweepDegrees = value;
                NotifyPropertyChanged();
                GenerateRing();
                Redisplay();
            }
        }

        public double TubeHeight
        {
            get
            {
                return tubeHeight;
            }
            set
            {
                if (tubeHeight != value)
                {
                    tubeHeight = value;
                    GenerateRing();
                    Redisplay();
                    NotifyPropertyChanged();
                }
            }
        }

        public double TubeThickness
        {
            get
            {
                return thickness;
            }
            set
            {
                if (thickness != value)
                {
                    thickness = value;
                    GenerateRing();
                    Redisplay();
                    NotifyPropertyChanged();
                }
            }
        }

        public double UpperBevel
        {
            get
            {
                return upperBevel;
            }
            set
            {
                if (upperBevel != value)
                {
                    upperBevel = value;
                    GenerateRing();
                    Redisplay();
                    NotifyPropertyChanged();
                }
            }
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParameters();

            DialogResult = true;
            Close();
        }

        private void GenerateRing()
        {
            TubeMaker tm = new TubeMaker(innerRadius, TubeThickness, lowerBevel, upperBevel, tubeHeight, sweepDegrees);
            tm.Generate(Vertices, Faces);
            CentreVertices();
        }

        private void SaveEditorParameters()
        {
            EditorParameters.Set("InnerRadius", innerRadius.ToString());
            EditorParameters.Set("TubeHeight", tubeHeight.ToString());
            EditorParameters.Set("TubeThickness", thickness.ToString());
            EditorParameters.Set("UpperBevel", upperBevel.ToString());
            EditorParameters.Set("LowerBevel", lowerBevel.ToString());
            EditorParameters.Set("SweepDegrees", sweepDegrees.ToString());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string s = EditorParameters.Get("InnerRadius");
            if (s != "")
            {
                InnerRadius = Convert.ToDouble(s);
                TubeHeight = EditorParameters.GetDouble("TubeHeight");
                TubeThickness = EditorParameters.GetDouble("TubeThickness");
                UpperBevel = EditorParameters.GetDouble("UpperBevel");
                LowerBevel = EditorParameters.GetDouble("LowerBevel");
                SweepDegrees = EditorParameters.GetDouble("SweepDegrees");
            }
            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            GenerateRing();
            Redisplay();
        }
    }
}