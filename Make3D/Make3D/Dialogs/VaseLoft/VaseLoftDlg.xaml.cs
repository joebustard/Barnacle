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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for DevTest.xaml
    /// </summary>
    public partial class VaseLoftDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private List<Point> displayPoints;
        private bool isHollow;
        private bool loaded;
        private int numDivisions;
        private Visibility showSurfaceParameters;
        private double surfaceThickness;
        private string warningText;

        public VaseLoftDlg()
        {
            InitializeComponent();
            ToolName = "VaseLoft";
            DataContext = this;

            loaded = false;
            numDivisions = 80;
            surfaceThickness = 1;
            PathEditor.OnFlexiPathChanged += PathPointsChanged;
            PathEditor.DefaultImagePath = DefaultImagePath;
            PathEditor.ToolName = ToolName;
            PathEditor.HasPresets = true;
            PathEditor.IncludeCommonPresets = false;
        }

        public bool IsHollow
        {
            get
            {
                return isHollow;
            }
            set
            {
                if (value != isHollow)
                {
                    isHollow = value;
                    NotifyPropertyChanged();
                    if (isHollow)
                    {
                        ShowHollowParameters = Visibility.Visible;
                    }
                    else
                    {
                        ShowHollowParameters = Visibility.Hidden;
                    }
                    UpdateDisplay();
                }
            }
        }

        public int NumDivisions
        {
            get
            {
                return numDivisions;
            }
            set
            {
                if (value < 3 || value > 360)
                {
                    WarningText = "Number of divisions must be >= 3 and <= 360";
                }
                else
                if (value != numDivisions)
                {
                    WarningText = "";
                    numDivisions = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double SurfaceThickness
        {
            get
            {
                return surfaceThickness;
            }

            set
            {
                if (surfaceThickness != value)
                {
                    surfaceThickness = value;

                    NotifyPropertyChanged();
                    UpdateDisplay();
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

        private Visibility ShowHollowParameters
        {
            get
            {
                return showSurfaceParameters;
            }
            set
            {
                if (showSurfaceParameters != value)
                {
                    showSurfaceParameters = value;
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

        private void GenerateHollow()
        {
            List<System.Windows.Point> line = new List<System.Windows.Point>();
            double top = 0;
            for (int i = 0; i < displayPoints.Count; i++)
            {
                if (displayPoints[i].Y > top)
                {
                    top = displayPoints[i].Y;
                }
            }
            foreach (Point p in displayPoints)
            {
                line.Add(new Point(p.X, top - p.Y));
            }
            // remove the points of the first line segment
            int pointsInFirstSegment = PathEditor.PointsInFirstSegment();
            for (int i = 0; i < pointsInFirstSegment - 1; i++)
            {
                line.RemoveAt(0);
            }

            // dirty hack
            // if there is only two points the shelltosolid goes wrong
            if (line.Count == 2)
            {
                Point mid = Midpoint(line[0], line[1]);
                line.Insert(1, mid);
            }

            int numSpokes = numDivisions;
            double deltaTheta = 360.0 / numSpokes; // in degrees
            Point3D[,] spokeVertices = new Point3D[numSpokes, line.Count];
            for (int i = 0; i < line.Count; i++)
            {
                spokeVertices[0, i] = new Point3D(line[i].X, line[i].Y, 0);
            }

            for (int i = 1; i < numSpokes; i++)
            {
                double theta = i * deltaTheta;
                double rad = Math.PI * theta / 180.0;
                for (int j = 0; j < line.Count; j++)
                {
                    double x = spokeVertices[0, j].X * Math.Cos(rad);
                    double z = spokeVertices[0, j].X * Math.Sin(rad);
                    spokeVertices[i, j] = new Point3D(x, spokeVertices[0, j].Y, z);
                }
            }
            Vertices.Clear();
            Faces.Clear();
            //Vertices.Add(new Point3D(0, 0, 0));
            for (int spokeIndex = 0; spokeIndex < numSpokes; spokeIndex++)
            {
                int spokeIndex2 = spokeIndex + 1;
                if (spokeIndex2 == numSpokes)
                {
                    spokeIndex2 = 0;
                }

                for (int j = 0; j < line.Count - 1; j++)
                {
                    int v0 = AddVertice(spokeVertices[spokeIndex, j]);
                    int v1 = AddVertice(spokeVertices[spokeIndex, j + 1]);
                    int v2 = AddVertice(spokeVertices[spokeIndex2, j]);
                    int v3 = AddVertice(spokeVertices[spokeIndex2, j + 1]);
                    Faces.Add(v0);
                    Faces.Add(v2);
                    Faces.Add(v1);

                    if (j < line.Count - 2)
                    {
                        Faces.Add(v1);
                        Faces.Add(v2);
                        Faces.Add(v3);
                    }
                }
            }
            SurfaceToSolid(Vertices, Faces, SurfaceThickness);
        }

        private void GenerateShape()
        {
            ClearShape();

            if (displayPoints != null)
            {
                if (IsHollow)
                {
                    GenerateHollow();
                }
                else
                {
                    GenerateSolid();
                }
            }
        }

        private void GenerateSolid()
        {
            List<System.Windows.Point> line = new List<System.Windows.Point>();
            double top = 0;
            for (int i = 0; i < displayPoints.Count; i++)
            {
                if (displayPoints[i].Y > top)
                {
                    top = displayPoints[i].Y;
                }
            }
            foreach (Point p in displayPoints)
            {
                line.Add(new Point(p.X, top - p.Y));
            }
            int numSpokes = numDivisions;
            double deltaTheta = 360.0 / numSpokes; // in degrees
            Point3D[,] spokeVertices = new Point3D[numSpokes, line.Count];
            for (int i = 0; i < line.Count; i++)
            {
                spokeVertices[0, i] = new Point3D(line[i].X, line[i].Y, 0);
            }

            for (int i = 1; i < numSpokes; i++)
            {
                double theta = i * deltaTheta;
                double rad = Math.PI * theta / 180.0;
                for (int j = 0; j < line.Count; j++)
                {
                    double x = spokeVertices[0, j].X * Math.Cos(rad);
                    double z = spokeVertices[0, j].X * Math.Sin(rad);
                    spokeVertices[i, j] = new Point3D(x, spokeVertices[0, j].Y, z);
                }
            }
            Vertices.Clear();
            Vertices.Add(new Point3D(0, 0, 0));
            for (int i = 0; i < numSpokes; i++)
            {
                for (int j = 0; j < line.Count; j++)
                {
                    Vertices.Add(spokeVertices[i, j]);
                }
            }
            int topPoint = Vertices.Count;
            Vertices.Add(new Point3D(0, 20, 0));
            Faces.Clear();
            int spOff;
            int spOff2;
            for (int i = 0; i < numSpokes; i++)
            {
                spOff = i * line.Count + 1;
                spOff2 = (i + 1) * line.Count + 1;
                if (i == numSpokes - 1)
                {
                    spOff2 = 1;
                }
                // base
                Faces.Add(0);
                Faces.Add(spOff2);
                Faces.Add(spOff);

                for (int j = 0; j < line.Count - 1; j++)
                {
                    Faces.Add(spOff + j);
                    Faces.Add(spOff2 + j);
                    Faces.Add(spOff2 + j + 1);

                    Faces.Add(spOff + j);
                    Faces.Add(spOff2 + j + 1);
                    Faces.Add(spOff + j + 1);
                }

                // Top
                Faces.Add(spOff + line.Count - 1);
                Faces.Add(spOff2 + line.Count - 1);
                Faces.Add(topPoint);
            }

            spOff = (numSpokes - 1) * line.Count + 1;
            spOff2 = 1;
            // base
            Faces.Add(0);
            Faces.Add(spOff2);
            Faces.Add(spOff);
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            String s = EditorParameters.Get("Path");
            if (s != "")
            {
                PathEditor.FromString(s);
            }
            NumDivisions = EditorParameters.GetInt("NumDivisions", 80);
            IsHollow = EditorParameters.GetBoolean("IsHollow", false);
            SurfaceThickness = EditorParameters.GetDouble("SurfaceThickness", 1);
            string imageName = EditorParameters.Get("ImagePath");
            if (imageName != "")
            {
                PathEditor.LoadImage(imageName);
            }
        }

        private Point Midpoint(Point point1, Point point2)
        {
            Point p = new Point(0, 0);
            p.X = (point1.X + point2.X) / 2;
            p.Y = (point1.Y + point2.Y) / 2;
            return p;
        }

        private void PathPointsChanged(List<System.Windows.Point> pnts)
        {
            displayPoints = pnts;
            if (PathEditor.PathClosed)
            {
                GenerateShape();
                Viewer.Model = GetModel();
            }
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            EditorParameters.Set("Path", PathEditor.AbsolutePathString);
            EditorParameters.Set("NumDivisions", NumDivisions.ToString());
            EditorParameters.Set("ImagePath", PathEditor.ImagePath);
            EditorParameters.Set("IsHollow", IsHollow.ToString());
            EditorParameters.Set("SurfaceThickness", surfaceThickness.ToString());
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
            PathEditor.DefaultImagePath = DefaultImagePath;
            loaded = true;

            UpdateDisplay();
        }
    }
}