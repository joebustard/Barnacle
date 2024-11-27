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
    /// Interaction logic for PlankWall.xaml
    /// </summary>
    public partial class PlankWallDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private bool bottomBevel;
        private double gap;
        private double gapDepth;
        private bool leftBevel;
        private bool loaded;
        private double plankWidth;
        private bool rightBevel;
        private bool topBevel;
        private double wallHeight;
        private double wallLength;
        private double wallWidth;
        private string warningText;

        public PlankWallDlg()
        {
            InitializeComponent();
            ToolName = "PlankWall";
            DataContext = this;
            BevelSelector.PropertyChanged += BevelSelector_PropertyChanged;
        }

        public double Gap
        {
            get
            {
                return gap;
            }

            set
            {
                if (gap != value)
                {
                    if (value >= 0.1 && value <= 300)
                    {
                        gap = value;
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
                    if (value >= 0.11 && value <= 300)
                    {
                        gapDepth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double PlankWidth
        {
            get
            {
                return plankWidth;
            }

            set
            {
                if (plankWidth != value)
                {
                    if (value >= 1 && value <= 300)
                    {
                        plankWidth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
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
                    if (value >= 1 && value <= 300)
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
                    if (value >= 1 && value <= 300)
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
                    if (value >= 1 && value <= 300)
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
            PlankWallMaker maker = new PlankWallMaker(
                wallLength, wallHeight, wallWidth, plankWidth, gap, gapDepth
                );
            maker.SetBevels(topBevel, bottomBevel, leftBevel, rightBevel);
            maker.Generate(Vertices, Faces);
            CentreVertices();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            WallLength = EditorParameters.GetDouble("WallLength", 50);
            WallHeight = EditorParameters.GetDouble("WallHeight", 25);
            WallWidth = EditorParameters.GetDouble("WallWidth", 2);
            PlankWidth = EditorParameters.GetDouble("PlankWidth", 5);
            Gap = EditorParameters.GetDouble("Gap", 1);
            GapDepth = EditorParameters.GetDouble("GapDepth", 1);
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
            EditorParameters.Set("PlankWidth", PlankWidth.ToString());
            EditorParameters.Set("Gap", Gap.ToString());
            EditorParameters.Set("GapDepth", GapDepth.ToString());
            EditorParameters.Set("TopBevel", topBevel.ToString());
            EditorParameters.Set("BottomBevel", bottomBevel.ToString());
            EditorParameters.Set("LeftBevel", leftBevel.ToString());
            EditorParameters.Set("RightBevel", rightBevel.ToString());
        }

        private void SetDefaults()
        {
            loaded = false;
            WallLength = 50;
            WallHeight = 25;
            WallWidth = 2;
            PlankWidth = 5;
            Gap = 1;
            GapDepth = 1;
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
            UpdateBevelControl();
            UpdateCameraPos();
            Viewer.Clear();
            loaded = true;
            UpdateDisplay();
        }
    }
}