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
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for ShapedTiledRoof.xaml
    /// </summary>
    public partial class ShapedTiledRoofDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private List<Point> displayPoints;
        private bool loaded;
        private double mortarGap;
        private double roofWidth;
        private double tileDepth;
        private double tileHeight;
        private double tileLength;
        private string warningText;

        public ShapedTiledRoofDlg()
        {
            InitializeComponent();
            ToolName = "ShapedTiledRoof";
            DataContext = this;
            ModelGroup = MyModelGroup;
            PathEditor.OnFlexiPathChanged += PathPointsChanged;
            PathEditor.AbsolutePaths = true;
            PathEditor.DefaultImagePath = DefaultImagePath;
            PathEditor.ToolName = ToolName;
            PathEditor.HasPresets = true;
            loaded = false;
            tileLength = 3.486;
            tileHeight = 2.17;
            tileDepth = 0.5;
            mortarGap = 0.25;
            roofWidth = 2;
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
                    if (value >= 0.1 && value <= 50)
                    {
                        mortarGap = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double RoofWidth
        {
            get
            {
                return roofWidth;
            }

            set
            {
                if (roofWidth != value)
                {
                    if (value >= 0.1 && value <= 50)
                    {
                        roofWidth = value;
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

        public double TileDepth
        {
            get
            {
                return tileDepth;
            }

            set
            {
                if (tileDepth != value)
                {
                    if (value >= 0.1 && value <= 50)
                    {
                        tileDepth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double TileHeight
        {
            get
            {
                return tileHeight;
            }

            set
            {
                if (tileHeight != value)
                {
                    if (value >= 0.1 && value <= 50)
                    {
                        tileHeight = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double TileLength
        {
            get
            {
                return tileLength;
            }

            set
            {
                if (tileLength != value)
                {
                    if (value >= 0.1 && value <= 50)
                    {
                        tileLength = value;
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
            if (displayPoints != null)
            {
                string path = PathEditor.GetPath();
                ShapedTiledRoofMaker maker = new ShapedTiledRoofMaker(path, tileLength,
                    tileHeight,
                    tileDepth,
                    mortarGap,
                    roofWidth);

                maker.Generate(Vertices, Faces);
            }
            CentreVertices();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            TileLength = EditorParameters.GetDouble("TileLength", 3.486);
            TileHeight = EditorParameters.GetDouble("TileHeight", 2.17);
            TileDepth = EditorParameters.GetDouble("TileDepth", 0.5);
            MortarGap = EditorParameters.GetDouble("MortarGap", 0.2);
            RoofWidth = EditorParameters.GetDouble("RoofWidth", 2);
            PathEditor.SetPath(EditorParameters.Get("Path"));
            string imageName = EditorParameters.Get("ImagePath");
            if (imageName != "")
            {
                PathEditor.LoadImage(imageName);
            }
        }

        private void PathPointsChanged(List<System.Windows.Point> pnts)
        {
            displayPoints = pnts;
            if (PathEditor.PathClosed)
            {
                GenerateShape();
                Redisplay();
            }
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            EditorParameters.Set("TileLength", TileLength.ToString());
            EditorParameters.Set("TileHeight", TileHeight.ToString());
            EditorParameters.Set("TileDepth", TileDepth.ToString());
            EditorParameters.Set("MortarGap", MortarGap.ToString());
            EditorParameters.Set("RoofWidth", RoofWidth.ToString());
            EditorParameters.Set("Path", PathEditor.GetPath());
            EditorParameters.Set("ImagePath", PathEditor.ImagePath);
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
            PathEditor.DefaultImagePath = DefaultImagePath;
            loaded = true;

            UpdateDisplay();
        }
    }
}