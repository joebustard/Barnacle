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

using Barnacle.UserControls;
using MakerLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for ShapedBrickWall.xaml
    /// </summary>
    public partial class ShapedBrickWallDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private double brickDepth;
        private double brickHeight;
        private double brickLength;
        private List<Point> displayPoints;
        private bool loaded;
        private double mortarGap;
        private DispatcherTimer regenTimer;
        private double wallWidth;
        private string warningText;

        public ShapedBrickWallDlg()
        {
            InitializeComponent();
            ToolName = "ShapedBrickWall";
            DataContext = this;
            loaded = false;
            PathEditor.OnFlexiPathChanged += PathPointsChanged;
            PathEditor.OnFlexiUserActive += UserPerformedAction;
            PathEditor.AbsolutePaths = true;
            PathEditor.DefaultImagePath = DefaultImagePath;
            PathEditor.ToolName = ToolName;
            PathEditor.HasPresets = true;
            brickLength = 3;
            brickHeight = 1.1;
            brickDepth = 0.25;
            mortarGap = 0.25;
            wallWidth = 2;
            Properties.Settings.Default.Reload();
            regenTimer = new DispatcherTimer();
            regenTimer.Interval = new TimeSpan(0, 0, Properties.Settings.Default.RegenerationDelay);
            regenTimer.Tick += RegenTimer_Tick;
        }

        public double BrickDepth
        {
            get
            {
                return brickDepth;
            }

            set
            {
                if (brickDepth != value)
                {
                    if (value >= 0.1 && value <= 50)
                    {
                        brickDepth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
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
                    if (value >= 1 && value <= 50)
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
                    if (value >= 1 && value <= 50)
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
                    if (value >= 0.1 && value <= 50)
                    {
                        mortarGap = value;
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
                    if (value >= 1 && value <= 50)
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
            if (regenTimer.IsEnabled)
            {
                regenTimer.Stop();
                Regenerate();
            }
            else
            {
                base.SaveSizeAndLocation();
                SaveEditorParmeters();
                DialogResult = true;
                Close();
            }
        }

        private void DeferRegen()
        {
            if (regenTimer.IsEnabled)
            {
                regenTimer.Stop();
                regenTimer.Start();
            }
        }

        private AsyncGeneratorResult GenerateAsync()
        {
            Point3DCollection v1 = new Point3DCollection();
            Int32Collection i1 = new Int32Collection();
            string path = PathEditor.GetPath();
            ShapedBrickWallMaker maker = new ShapedBrickWallMaker(path, brickLength, brickHeight, brickDepth, wallWidth, mortarGap);
            maker.Generate(v1, i1);

            AsyncGeneratorResult res = new AsyncGeneratorResult();
            // extract the vertices and indices to thread safe arrays
            // while still in the async function
            res.points = new Point3D[v1.Count];
            for (int i = 0; i < v1.Count; i++)
            {
                res.points[i] = new Point3D(v1[i].X, v1[i].Y, v1[i].Z);
            }
            res.indices = new int[i1.Count];
            for (int i = 0; i < i1.Count; i++)
            {
                res.indices[i] = i1[i];
            }
            v1.Clear();
            i1.Clear();
            return (res);
        }

        private async void GenerateShape()
        {
            ClearShape();
            if (displayPoints != null)
            {
                ClearShape();
                Viewer.Busy();
                EditingEnabled = false;
                PathEditor.Busy();
                AsyncGeneratorResult result;
                result = await Task.Run(() => GenerateAsync());
                GetVerticesFromAsyncResult(result);
                CentreVertices();
                Viewer.NotBusy();
                PathEditor.NotBusy();
                EditingEnabled = true;
            }
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            string imageName = EditorParameters.Get("ImagePath");
            if (imageName != "")
            {
                PathEditor.LoadImage(imageName);
            }
            BrickLength = EditorParameters.GetDouble("BrickLength", 3);
            BrickHeight = EditorParameters.GetDouble("BrickHeight", 1.1);
            BrickDepth = EditorParameters.GetDouble("BrickDepth", 0.25);
            MortarGap = EditorParameters.GetDouble("MortarGap", 0.25);
            WallWidth = EditorParameters.GetDouble("WallWidth", 2);
            PathEditor.SetPath(EditorParameters.Get("Path"));
            int v = EditorParameters.GetInt("ShowGrid", 1);
            PathEditor.ShowGrid = (UserControls.GridSettings.GridStyle)v;
            double zoomLevel = EditorParameters.GetDouble("Zoom", 1);
            PathEditor.ZoomLevel = zoomLevel;
        }

        private void PathPointsChanged(List<System.Windows.Point> pnts)
        {
            displayPoints = pnts;
            UpdateDisplay();
        }

        private void Regenerate()
        {
            if (loaded && PathEditor.PathClosed)
            {
                GenerateShape();
            }
            Viewer.Model = GetModel();
        }

        private void RegenTimer_Tick(object sender, EventArgs e)
        {
            regenTimer.Stop();
            Regenerate();
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            EditorParameters.Set("ImagePath", PathEditor.ImagePath);
            EditorParameters.Set("BrickLength", BrickLength.ToString());
            EditorParameters.Set("BrickHeight", BrickHeight.ToString());
            EditorParameters.Set("BrickDepth", MortarGap.ToString());
            EditorParameters.Set("MortarGap", MortarGap.ToString());
            EditorParameters.Set("WallWidth", WallWidth.ToString());
            EditorParameters.Set("Path", PathEditor.GetPath());
            EditorParameters.Set("ShowGrid", ((int)(PathEditor.ShowGrid)).ToString());
            EditorParameters.Set("Zoom", PathEditor.ZoomLevel);
        }

        private void UpdateDisplay()
        {
            regenTimer.Stop();
            regenTimer.Start();
        }

        private void UserPerformedAction()
        {
            DeferRegen();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WarningText = "";
            LoadEditorParameters();
            UpdateCameraPos();
            Viewer.Clear();
            PathEditor.DefaultImagePath = DefaultImagePath;
            loaded = true;
            //UpdateDisplay();
        }
    }
}