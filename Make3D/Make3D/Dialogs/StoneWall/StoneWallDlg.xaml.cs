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
using System.ComponentModel;
using System.Windows;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for StoneWall.xaml
    /// </summary>
    public partial class StoneWallDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private bool loaded;
        private int stoneSize;
        private double wallHeight;
        private double wallLength;
        private double wallThickness;
        private string warningText;

        public StoneWallDlg()
        {
            InitializeComponent();
            ToolName = "StoneWall";
            wallLength = 100;
            wallHeight = 80;
            wallThickness = 4;
            stoneSize = 5;
            DataContext = this;

            loaded = false;
        }

        public int StoneSize
        {
            get
            {
                return stoneSize;
            }
            set
            {
                if (stoneSize != value)
                {
                    if (value >= 5 && value <= wallHeight / 2 && value <= wallLength / 2)
                    {
                        stoneSize = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                    else
                    {
                        WarningText = "Stone size myust be in the range 5 to wallLength / 2 or wallHeight / 2";
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

        public double WallThickness
        {
            get
            {
                return wallThickness;
            }
            set
            {
                if (wallThickness != value)
                {
                    if (value >= 1 && value <= 200)
                    {
                        wallThickness = value;
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
            StoneWallMaker maker = new StoneWallMaker(wallLength, wallHeight, wallThickness, stoneSize);
            maker.Generate(Vertices, Faces);
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters

            if (EditorParameters.Get("WallLength") != "")
            {
                WallLength = EditorParameters.GetDouble("WallLength");
            }

            if (EditorParameters.Get("WallHeight") != "")
            {
                WallHeight = EditorParameters.GetDouble("WallHeight");
            }

            if (EditorParameters.Get("WallThickness") != "")
            {
                WallThickness = EditorParameters.GetDouble("WallThickness");
            }

            if (EditorParameters.Get("StoneSize") != "")
            {
                StoneSize = EditorParameters.GetInt("StoneSize");
            }
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("WallLength", WallLength.ToString());
            EditorParameters.Set("WallHeight", WallHeight.ToString());
            EditorParameters.Set("WallThickness", WallThickness.ToString());
            EditorParameters.Set("StoneSize", StoneSize.ToString());
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