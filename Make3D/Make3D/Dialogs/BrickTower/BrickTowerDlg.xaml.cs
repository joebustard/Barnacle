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
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for BrickTower.xaml
    /// </summary>
    public partial class BrickTowerDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private string warningText;
        private bool loaded;

        private double brickLength;

        public double BrickLength
        {
            get
            {
                return brickLength;
            }

            set
            {
                if (brickLength != value)
                {
                    if (value >= 1 && value <= 20)
                    {
                        brickLength = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        private double brickHeight;

        public double BrickHeight
        {
            get
            {
                return brickHeight;
            }

            set
            {
                if (brickHeight != value)
                {
                    if (value >= 1 && value <= 20)
                    {
                        brickHeight = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        private double brickWidth;

        public double BrickWidth
        {
            get
            {
                return brickWidth;
            }

            set
            {
                if (brickWidth != value)
                {
                    if (value >= 1 && value <= 10)
                    {
                        brickWidth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        private double gapLength;

        public double GapLength
        {
            get
            {
                return gapLength;
            }

            set
            {
                if (gapLength != value)
                {
                    if (value >= 0.1 && value <= 10)
                    {
                        gapLength = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        private double gapDepth;

        public double GapDepth
        {
            get
            {
                return gapDepth;
            }

            set
            {
                if (gapDepth != value)
                {
                    if (value >= 0.1 && value <= 10)
                    {
                        gapDepth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        private const double minTowerRadius = 5;
        private const double maxTowerRadius = 200;
        private double towerRadius;

        public double TowerRadius
        {
            get
            {
                return towerRadius;
            }

            set
            {
                if (towerRadius != value)
                {
                    if (value >= minTowerRadius && value <= maxTowerRadius)
                    {
                        towerRadius = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String TowerRadiusToolTip
        {
            get
            {
                return $"Tower Radius must be in the range {minTowerRadius} to {maxTowerRadius}";
            }
        }

        private const double minTowerHeight = 5;
        private const double maxTowerHeight = 500;
        private double towerHeight;

        public double TowerHeight
        {
            get
            {
                return towerHeight;
            }

            set
            {
                if (towerHeight != value)
                {
                    if (value >= minTowerHeight && value <= maxTowerHeight)
                    {
                        towerHeight = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String TowerHeightToolTip
        {
            get
            {
                return $"Tower Height must be in the range {minTowerHeight} to {maxTowerHeight}";
            }
        }

        public BrickTowerDlg()
        {
            InitializeComponent();

            ToolName = "BrickTower";
            DataContext = this;
            ModelGroup = MyModelGroup;
            loaded = false;
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
            base.SaveSizeAndLocation();
            DialogResult = true;
            Close();
        }

        private void GenerateShape()
        {
            ClearShape();
            BrickTowerMaker maker = new BrickTowerMaker(brickLength, brickHeight, brickWidth, gapLength, gapDepth, towerRadius, towerHeight);
            maker.Generate(Vertices, Faces);
            CentreVertices();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters

            BrickLength = EditorParameters.GetDouble("BrickLength", 4.5);

            BrickHeight = EditorParameters.GetDouble("BrickHeight", 2);

            BrickWidth = EditorParameters.GetDouble("BrickWidth", 2);

            GapLength = EditorParameters.GetDouble("GapLength", 1);

            GapDepth = EditorParameters.GetDouble("GapDepth", 0.25);

            TowerRadius = EditorParameters.GetDouble("TowerRadius", 10);

            TowerHeight = EditorParameters.GetDouble("TowerHeight", 35);
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("BrickLength", BrickLength.ToString());
            EditorParameters.Set("BrickHeight", BrickHeight.ToString());
            EditorParameters.Set("BrickWidth", BrickWidth.ToString());
            EditorParameters.Set("GapLength", GapLength.ToString());
            EditorParameters.Set("GapDepth", GapDepth.ToString());
            EditorParameters.Set("TowerRadius", TowerRadius.ToString());
            EditorParameters.Set("TowerHeight", TowerHeight.ToString());
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