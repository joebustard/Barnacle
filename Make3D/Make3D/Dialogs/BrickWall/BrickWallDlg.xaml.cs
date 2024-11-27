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
using System.ComponentModel;
using System.Windows;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for BrickWall.xaml
    /// </summary>
    public partial class BrickWallDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private bool bottomBevel;
        private double brickHeight;
        private double brickLength;
        private bool leftBevel;
        private bool loaded;
        private double mortarGap;
        private bool rightBevel;
        private bool topBevel;
        private bool topBevelled;
        private double wallHeight;
        private double wallLength;
        private double wallWidth;
        private string warningText;

        public BrickWallDlg()
        {
            InitializeComponent();
            ToolName = "BrickWall";
            DataContext = this;
            loaded = false;
            BevelSelector.PropertyChanged += BevelSelector_PropertyChanged;
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
                    if (value >= 1 && value <= 100)
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
                    if (value >= 1 && value <= 100)
                    {
                        brickLength = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double MortarGap
        {
            get
            {
                return mortarGap;
            }

            set
            {
                if (mortarGap != value)
                {
                    if (value >= 0 && value <= 100)
                    {
                        mortarGap = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
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

        public double WallHeight
        {
            get
            {
                return wallHeight;
            }

            set
            {
                if (wallHeight != value)
                {
                    if (value >= 1 && value <= 200)
                    {
                        wallHeight = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double WallLength
        {
            get
            {
                return wallLength;
            }

            set
            {
                if (wallLength != value)
                {
                    if (value >= 1 && value <= 200)
                    {
                        wallLength = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double WallWidth
        {
            get
            {
                return wallWidth;
            }

            set
            {
                if (wallWidth != value)
                {
                    if (value >= 1 && value <= 600)
                    {
                        wallWidth = value;
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
            base.SaveSizeAndLocation();
            DialogResult = true;
            Close();
        }

        private void BevelSelector_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (loaded)
            {
                switch (e.PropertyName)
                {
                    case "TopBevelled":
                        {
                            topBevel = BevelSelector.TopBevelled;
                            UpdateDisplay();
                        }
                        break;

                    case "BottomBevelled":
                        {
                            bottomBevel = BevelSelector.BottomBevelled;
                            UpdateDisplay();
                        }
                        break;

                    case "LeftBevelled":
                        {
                            leftBevel = BevelSelector.LeftBevelled;
                            UpdateDisplay();
                        }
                        break;

                    case "RightBevelled":
                        {
                            rightBevel = BevelSelector.RightBevelled;
                            UpdateDisplay();
                        }
                        break;
                }
            }
        }

        private void GenerateShape()
        {
            ClearShape();
            BrickWallMaker maker = new BrickWallMaker(wallLength, wallHeight, wallWidth, brickLength, brickLength / 2, brickHeight, mortarGap);
            maker.SetBevels(topBevel, bottomBevel, leftBevel, rightBevel);
            maker.Generate(Vertices, Faces);
            CentreVertices();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            WallLength = EditorParameters.GetDouble("WallLength", 50);

            WallHeight = EditorParameters.GetDouble("WallHeight", 50);

            WallWidth = EditorParameters.GetDouble("WallWidth", 2);
            BrickLength = EditorParameters.GetDouble("BrickLength", 3.3);
            BrickHeight = EditorParameters.GetDouble("BrickHeight", 1.1);
            MortarGap = EditorParameters.GetDouble("MortarGap", 0.25);
            topBevel = EditorParameters.GetBoolean("TopBevel", false);
            bottomBevel = EditorParameters.GetBoolean("BottomBevel", false);
            leftBevel = EditorParameters.GetBoolean("LeftBevel", false);
            rightBevel = EditorParameters.GetBoolean("RightBevel", false);
        }

        private void ResetDefaults(object sender, RoutedEventArgs e)
        {
            SetDefaults();
            UpdateDisplay();
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            EditorParameters.Set("WallLength", WallLength.ToString());
            EditorParameters.Set("WallHeight", WallHeight.ToString());
            EditorParameters.Set("WallWidth", WallWidth.ToString());
            EditorParameters.Set("BrickLength", BrickLength.ToString());
            EditorParameters.Set("BrickHeight", BrickHeight.ToString());
            EditorParameters.Set("MortarGap", MortarGap.ToString());
            EditorParameters.Set("TopBevel", topBevel.ToString());
            EditorParameters.Set("BottomBevel", bottomBevel.ToString());
            EditorParameters.Set("LeftBevel", leftBevel.ToString());
            EditorParameters.Set("RightBevel", rightBevel.ToString());
        }

        private void SetDefaults()
        {
            loaded = false;
            WallLength = 50;
            WallHeight = 50;
            WallWidth = 2;
            BrickLength = 3.3;
            BrickHeight = 1.1;
            MortarGap = 0.25;
            topBevel = false;
            bottomBevel = false;
            leftBevel = false;
            rightBevel = false;
            UpdateBevelControl();
            loaded = true;
        }

        private void UpdateBevelControl()
        {
            BevelSelector.SetAll(leftBevel, topBevel, rightBevel, bottomBevel);
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
            Viewer.Clear();
            UpdateBevelControl();
            loaded = true;
            UpdateDisplay();
        }
    }
}