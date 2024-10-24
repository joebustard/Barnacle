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
using System.Runtime.CompilerServices;
using System.Windows;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for RailWheel.xaml
    /// </summary>
    public partial class RailWheelDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private double axleBoreDiameter;
        private double flangeDiameter;
        private double flangeHeight;
        private double hubDiameter;
        private double hubHeight;
        private bool loaded;
        private double lowerRimDiameter;
        private double rimHeight;
        private double rimThickness;
        private double upperRimDiameter;
        private string warningText;
        private RailWheelMaker maker;

        public RailWheelDlg()
        {
            InitializeComponent();
            ToolName = "RailWheel";
            DataContext = this;
            ModelGroup = MyModelGroup;
            maker = new RailWheelMaker();
        }

        public double AxleBoreDiameter
        {
            get
            {
                return axleBoreDiameter;
            }

            set
            {
                if (axleBoreDiameter != value)
                {
                    if (CheckRange(value))
                    {
                        axleBoreDiameter = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        private bool CheckRange(double v, [CallerMemberName] String propertyName = "")
        {
            bool res = false;
            if (maker != null)
            {
                res = maker.CheckLimits(propertyName, v);
            }
            return res;
        }

        public double FlangeDiameter
        {
            get
            {
                return flangeDiameter;
            }

            set
            {
                if (flangeDiameter != value)
                {
                    if (CheckRange(value))
                    {
                        flangeDiameter = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double FlangeHeight
        {
            get
            {
                return flangeHeight;
            }

            set
            {
                if (flangeHeight != value)
                {
                    if (CheckRange(value))
                    {
                        flangeHeight = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double HubDiameter
        {
            get
            {
                return hubDiameter;
            }

            set
            {
                if (hubDiameter != value)
                {
                    if (CheckRange(value))
                    {
                        hubDiameter = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double HubHeight
        {
            get
            {
                return hubHeight;
            }

            set
            {
                if (hubHeight != value)
                {
                    if (CheckRange(value))
                    {
                        hubHeight = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double LowerRimDiameter
        {
            get { return lowerRimDiameter; }
            set
            {
                if (value != lowerRimDiameter)
                {
                    if (CheckRange(value))
                    {
                        lowerRimDiameter = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double RimHeight
        {
            get { return rimHeight; }
            set
            {
                if (value != rimHeight)
                {
                    if (CheckRange(value))
                    {
                        rimHeight = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double RimThickness
        {
            get
            {
                return rimThickness;
            }

            set
            {
                if (rimThickness != value)
                {
                    if (CheckRange(value))
                    {
                        rimThickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
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

        public double UpperRimDiameter
        {
            get { return upperRimDiameter; }
            set
            {
                if (value != upperRimDiameter)
                {
                    if (CheckRange(value))
                    {
                        upperRimDiameter = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
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

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParmeters();
            DialogResult = true;
            Close();
        }

        private void GenerateShape()
        {
            ClearShape();
            maker.SetValues(flangeDiameter,
                            flangeHeight,
                            hubDiameter,
                            hubHeight,
                            upperRimDiameter,
                            lowerRimDiameter,
                            rimThickness,
                            rimHeight,
                            axleBoreDiameter);

            maker.Generate(Vertices, Faces);
            CentreVertices();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            FlangeDiameter = EditorParameters.GetDouble("FlangeDiameter", 15.1);
            FlangeHeight = EditorParameters.GetDouble("FlangeHeight", 1);
            HubDiameter = EditorParameters.GetDouble("HubDiameter", 3.8);
            HubHeight = EditorParameters.GetDouble("HubHeight", 3.71);
            RimHeight = EditorParameters.GetDouble("RimHeight", 3.71);
            RimThickness = EditorParameters.GetDouble("RimThickness", 0.8);
            AxleBoreDiameter = EditorParameters.GetDouble("AxleBoreRadius", 0.5);
            UpperRimDiameter = EditorParameters.GetDouble("UpperRimDiameter", 12);
            LowerRimDiameter = EditorParameters.GetDouble("LowerRimDiameter", 12.8);
        }

        private void Reset()
        {
            FlangeDiameter = 15.1;
            FlangeHeight = 1;
            HubDiameter = 3.8;
            HubHeight = 3.71;
            UpperRimDiameter = 12.0;
            LowerRimDiameter = 12.8;
            RimThickness = 0.8;
            RimHeight = 3.71;
            AxleBoreDiameter = 0.5;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Reset();
            loaded = true;
            UpdateDisplay();
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            EditorParameters.Set("FlangeDiameter", FlangeDiameter);
            EditorParameters.Set("FlangeHeight", FlangeHeight);
            EditorParameters.Set("HubDiameter", HubDiameter);
            EditorParameters.Set("HubHeight", HubHeight);
            EditorParameters.Set("UpperRimDiameter", UpperRimDiameter);
            EditorParameters.Set("LowerRimDiameter", LowerRimDiameter);
            EditorParameters.Set("RimHeight", RimHeight);
            EditorParameters.Set("RimThickness", RimThickness);
            EditorParameters.Set("AxleBoreRadius", AxleBoreDiameter);
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
            loaded = false;
            LoadEditorParameters();
            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            warningText = "";
            loaded = true;
            UpdateDisplay();
        }
    }
}