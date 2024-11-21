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
    /// Interaction logic for Barrel.xaml
    /// </summary>
    public partial class BarrelDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private const double maxbarrelHeight = 200;
        private const double maxmiddleRadius = 100;
        private const double maxnumberOfStaves = 30;
        private const double maxshellThickness = 50;
        private const double maxStaveDepth = 50;
        private const double maxtopRadius = 100;
        private const double minbarrelHeight = 1;
        private const double minmiddleRadius = 2;
        private const double minnumberOfStaves = 2;
        private const double minshellThickness = 1;
        private const double minStaveDepth = 0.1;
        private const double mintopRadius = 2;
        private double barrelHeight;
        private bool loaded;
        private double middleRadius;
        private double numberOfStaves;
        private bool shellSet;
        private double shellThickness;
        private Visibility shellVisibility;
        private double staveDepth;
        private double topBottomRadius;
        private string warningText;

        public BarrelDlg()
        {
            InitializeComponent();
            ToolName = "Barrel";
            DataContext = this;
            ModelGroup = MyModelGroup;
            loaded = false;
            shellVisibility = Visibility.Hidden;
        }

        public double BarrelHeight
        {
            get
            {
                return barrelHeight;
            }
            set
            {
                if (barrelHeight != value)
                {
                    if (value >= minbarrelHeight && value <= maxbarrelHeight)
                    {
                        barrelHeight = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String BarrelHeightToolTip
        {
            get
            {
                return $"BarrelHeight must be in the range {minbarrelHeight} to {maxbarrelHeight}";
            }
        }

        public double MiddleRadius
        {
            get
            {
                return middleRadius;
            }
            set
            {
                if (middleRadius != value)
                {
                    if (value >= minmiddleRadius && value <= maxmiddleRadius)
                    {
                        middleRadius = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String MiddleRadiusToolTip
        {
            get
            {
                return $"Middle Radius must be in the range {minmiddleRadius} to {maxmiddleRadius}";
            }
        }

        public double NumberOfStaves
        {
            get
            {
                return numberOfStaves;
            }
            set
            {
                if (numberOfStaves != value)
                {
                    if (value >= minnumberOfStaves && value <= maxnumberOfStaves)
                    {
                        numberOfStaves = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String NumberOfStavesToolTip
        {
            get
            {
                return $"Number Of Staves must be in the range {minnumberOfStaves} to {maxnumberOfStaves}";
            }
        }

        public bool ShellSet
        {
            get
            {
                return shellSet;
            }
            set
            {
                if (shellSet != value)
                {
                    shellSet = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                    if (shellSet)
                    {
                        ShellVisibility = Visibility.Visible;
                    }
                    else
                    {
                        ShellVisibility = Visibility.Hidden;
                    }
                }
            }
        }

        public double ShellThickness
        {
            get
            {
                return shellThickness;
            }
            set
            {
                if (shellThickness != value)
                {
                    if (value >= minshellThickness && value <= maxshellThickness)
                    {
                        shellThickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String ShellThicknessToolTip
        {
            get
            {
                return $"Shell Thickness must be in the range {minshellThickness} to {maxshellThickness}";
            }
        }

        public String ShellToolTip
        {
            get
            {
                return $"If set then only the Staves are created.";
            }
        }

        public Visibility ShellVisibility
        {
            get { return shellVisibility; }
            set
            {
                if (shellVisibility != value)
                {
                    shellVisibility = value;
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

        public double StaveDepth
        {
            get
            {
                return staveDepth;
            }
            set
            {
                if (staveDepth != value)
                {
                    if (value >= minStaveDepth && value <= maxStaveDepth)
                    {
                        staveDepth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String StaveDepthToolTip
        {
            get
            {
                return $"Stave Depth must be in the range {minStaveDepth} to {maxStaveDepth}";
            }
        }

        public double TopBottomRadius
        {
            get
            {
                return topBottomRadius;
            }
            set
            {
                if (topBottomRadius != value)
                {
                    if (value >= mintopRadius && value <= maxtopRadius)
                    {
                        topBottomRadius = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String TopRadiusToolTip
        {
            get
            {
                return $"Top Bottom Radius must be in the range {mintopRadius} to {maxtopRadius}";
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
            if (loaded)
            {
                ClearShape();
                BarrelMaker maker = new BarrelMaker(
                    barrelHeight,
                    topBottomRadius,
                    middleRadius,
                    numberOfStaves,
                    shellSet,
                    shellThickness,
                    staveDepth);
                maker.Generate(Vertices, Faces);
                CentreVertices();
            }
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            BarrelHeight = EditorParameters.GetDouble("BarrelHeight", 20);
            TopBottomRadius = EditorParameters.GetDouble("TopBottomRadius", 6);
            MiddleRadius = EditorParameters.GetDouble("MiddleRadius", 8);
            NumberOfStaves = EditorParameters.GetDouble("NumberOfStaves", 16);
            ShellSet = EditorParameters.GetBoolean("ShellSet", false);
            ShellThickness = EditorParameters.GetDouble("ShellThickness", 1);
            StaveDepth = EditorParameters.GetDouble("StaveDepth", 0.25);
        }

        private void ResetDefaults(object sender, RoutedEventArgs e)
        {
            SetDefaults();
            UpdateDisplay();
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            EditorParameters.Set("BarrelHeight", BarrelHeight.ToString());
            EditorParameters.Set("TopBottomRadius", TopBottomRadius.ToString());
            EditorParameters.Set("MiddleRadius", MiddleRadius.ToString());
            EditorParameters.Set("NumberOfStaves", NumberOfStaves.ToString());
            EditorParameters.Set("ShellSet", ShellSet.ToString());
            EditorParameters.Set("ShellThickness", ShellThickness.ToString());
            EditorParameters.Set("StaveDepth", StaveDepth.ToString());
        }

        private void SetDefaults()
        {
            loaded = false;
            BarrelHeight = 20;
            TopBottomRadius = 6;
            MiddleRadius = 8;
            NumberOfStaves = 16;
            ShellSet = false;
            ShellThickness = 1;
            StaveDepth = 0.25;
            loaded = true;
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