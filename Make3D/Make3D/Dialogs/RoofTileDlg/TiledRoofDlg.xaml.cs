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
    /// Interaction logic for TiledRoof.xaml
    /// </summary>
    public partial class TiledRoofDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private bool loaded;
        private double tileGap;
        private double tileHeight;
        private double tileLength;
        private double tileOverlap;
        private double tileWidth;
        private double wallHeight;
        private double wallLength;
        private double wallWidth;
        private string warningText;

        public TiledRoofDlg()
        {
            InitializeComponent();
            ToolName = "TiledRoof";
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

        private bool chamfer;

        public bool Chamfer
        {
            get { return chamfer; }

            set
            {
                if (chamfer != value)
                {
                    chamfer = value;
                    NotifyPropertyChanged();

                    UpdateDisplay();
                }
            }
        }

        public double TileGap
        {
            get { return tileGap; }

            set
            {
                if (tileGap != value)
                {
                    if (value >= 0.1 && value <= 10)
                    {
                        tileGap = value;
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
                    if (value >= 1 && value <= 20)
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
                    if (value >= 1 && value <= 10)
                    {
                        tileLength = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double TileOverlap
        {
            get { return tileOverlap; }

            set
            {
                if (tileOverlap != value)
                {
                    if (value >= 0.1 && value <= 0.9)
                    {
                        tileOverlap = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double TileWidth
        {
            get
            {
                return tileWidth;
            }

            set
            {
                if (tileWidth != value)
                {
                    if (value >= 1 && value <= 20)
                    {
                        tileWidth = value;
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
                    if (value >= 10 && value <= 200)
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
                    if (value >= 1 && value <= 200)
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

        private void GenerateShape()
        {
            ClearShape();
            TiledRoofMaker maker = new TiledRoofMaker(wallLength,
                                                        wallHeight,
                                                        wallWidth,
                                                        tileLength,
                                                        tileHeight,
                                                        tileWidth,
                                                        tileOverlap,
                                                        tileGap,
                                                        chamfer
                                                        );
            maker.Generate(Vertices, Faces);
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters

            if (EditorParameters.Get("Length") != "")
            {
                WallLength = EditorParameters.GetDouble("Length");
            }

            if (EditorParameters.Get("WallHeight") != "")
            {
                WallHeight = EditorParameters.GetDouble("WallHeight");
            }

            if (EditorParameters.Get("Width") != "")
            {
                WallWidth = EditorParameters.GetDouble("Width");
            }

            if (EditorParameters.Get("TileLength") != "")
            {
                TileLength = EditorParameters.GetDouble("TileLength");
            }

            if (EditorParameters.Get("TileHeight") != "")
            {
                TileHeight = EditorParameters.GetDouble("TileHeight");
            }

            if (EditorParameters.Get("TileWidth") != "")
            {
                TileWidth = EditorParameters.GetDouble("TileWidth");
            }

            if (EditorParameters.Get("TileGap") != "")
            {
                TileGap = EditorParameters.GetDouble("TileGap");
            }

            if (EditorParameters.Get("TileOverlap") != "")
            {
                TileOverlap = EditorParameters.GetDouble("TileOverlap");
            }

            if (EditorParameters.Get("Chamfer") != "")
            {
                Chamfer = EditorParameters.GetBoolean("Chamfer");
            }
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("WallLength", WallLength.ToString());
            EditorParameters.Set("WallHeight", WallHeight.ToString());
            EditorParameters.Set("WallWidth", WallWidth.ToString());
            EditorParameters.Set("TileLength", TileLength.ToString());
            EditorParameters.Set("TileHeight", TileHeight.ToString());
            EditorParameters.Set("TileWidth", TileWidth.ToString());
            EditorParameters.Set("TileGap", TileGap.ToString());
            EditorParameters.Set("TileOverlap", TileOverlap.ToString());
            EditorParameters.Set("Chamfer", Chamfer.ToString());
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
            WallHeight = 75;
            WallWidth = 2;
            WallLength = 130;
            TileHeight = 2.17;
            TileLength = 3.486;
            TileWidth = 1;
            TileGap = 1;
            TileOverlap = 0.1;
            LoadEditorParameters();

            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            loaded = true;

            UpdateDisplay();
        }
    }
}