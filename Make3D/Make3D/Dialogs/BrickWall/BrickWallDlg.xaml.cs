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
        private string warningText;
        private bool loaded;

        private double wallLength;

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

        public bool TopBevelled
        {
            get { return topBevelled; }
            set
            {
                if (value != topBevelled)
                {
                    topBevelled = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private double wallHeight;

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

        private double wallWidth;

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
                    if (value >= 1 && value <= 100)
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
                    if (value >= 1 && value <= 100)
                    {
                        brickHeight = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        private double mortarGap;
        private bool topBevelled;

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

        private bool topBevel;
        private bool bottomBevel;
        private bool leftBevel;
        private bool rightBevel;

        public BrickWallDlg()
        {
            InitializeComponent();

            ToolName = "BrickWall";
            DataContext = this;
            ModelGroup = MyModelGroup;
            loaded = false;
            BevelSelector.PropertyChanged += BevelSelector_PropertyChanged;
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
            BrickWallMaker maker = new BrickWallMaker(wallLength, wallHeight, wallWidth, brickLength, brickLength / 2, brickHeight, mortarGap);
            maker.SetBevels(topBevel, bottomBevel, leftBevel, rightBevel);
            maker.Generate(Vertices, Faces);
            CentreVertices();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters

            if (EditorParameters.Get("WallLength") != "")
            {
                wallLength = EditorParameters.GetDouble("WallLength");
            }

            if (EditorParameters.Get("WallHeight") != "")
            {
                wallHeight = EditorParameters.GetDouble("WallHeight");
            }

            if (EditorParameters.Get("WallWidth") != "")
            {
                wallWidth = EditorParameters.GetDouble("WallWidth");
            }

            if (EditorParameters.Get("BrickLength") != "")
            {
                brickLength = EditorParameters.GetDouble("BrickLength");
            }

            if (EditorParameters.Get("BrickHeight") != "")
            {
                brickHeight = EditorParameters.GetDouble("BrickHeight");
            }

            if (EditorParameters.Get("MortarGap") != "")
            {
                mortarGap = EditorParameters.GetDouble("MortarGap");
            }
            topBevel = EditorParameters.GetBoolean("TopBevel", false);
            bottomBevel = EditorParameters.GetBoolean("BottomBevel", false);
            leftBevel = EditorParameters.GetBoolean("LeftBevel", false);
            rightBevel = EditorParameters.GetBoolean("RightBevel", false);
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
            wallLength = 50;
            wallHeight = 50;
            wallWidth = 2;
            brickLength = 3.3;

            brickHeight = 1.1;
            mortarGap = 0.25;
            topBevel = false;
            bottomBevel = false;
            leftBevel = false;
            rightBevel = false;
            LoadEditorParameters();

            BevelSelector.TopBevelled = topBevel;
            BevelSelector.BottomBevelled = bottomBevel;
            BevelSelector.RightBevelled = rightBevel;
            BevelSelector.LeftBevelled = leftBevel;
            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            loaded = true;

            NotifyPropertyChanged("WallLength");
            NotifyPropertyChanged("WallHeight");

            NotifyPropertyChanged("WallWidth");
            NotifyPropertyChanged("BrickLength");
            NotifyPropertyChanged("BrickHeight");
            NotifyPropertyChanged("MortarGap");

            UpdateDisplay();
        }
    }
}