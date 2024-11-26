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
    /// Interaction logic for BrickTower.xaml
    /// </summary>
    public partial class BrickTowerDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private const double maxTowerHeight = 500;
        private const double maxTowerRadius = 200;
        private const double minTowerHeight = 5;
        private const double minTowerRadius = 5;
        private double brickHeight;
        private double brickLength;
        private double brickWidth;
        private double gapDepth;
        private double gapLength;
        private bool loaded;
        private double towerHeight;
        private double towerRadius;
        private string warningText;

        public BrickTowerDlg()
        {
            InitializeComponent();
            ToolName = "BrickTower";
            DataContext = this;
            loaded = false;
        }

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

        private void Reset()
        {
            loaded = false;
            BrickLength = 4.5;
            BrickHeight = 2;
            BrickWidth = 2;
            GapLength = 1;
            GapDepth = 0.25;
            TowerRadius = 10;
            TowerHeight = 35;
            loaded = true;
            UpdateDisplay();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Reset();
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
                Viewer.Model = GetModel();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WarningText = "";
            LoadEditorParameters();
            UpdateCameraPos();
            Viewer.Clear();
            loaded = true;
            UpdateDisplay();
        }
    }
}