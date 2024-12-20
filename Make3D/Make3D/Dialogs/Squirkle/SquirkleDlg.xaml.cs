﻿// **************************************************************************
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

using Barnacle.LineLib;
using PolygonTriangulationLib;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using PointF = System.Drawing.PointF;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for squirkle.xaml
    /// </summary>
    public partial class SquirkleDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private double squirkleheight;
        private double squirklelength;

        private double squirklewidth;
        private string warningText;

        public SquirkleDlg()
        {
            InitializeComponent();
            ToolName = "Squirkle";
            DataContext = this;
            squirkleheight = 10;
            squirklelength = 10;
            squirklewidth = 10;
        }

        public double SquirkleHeight
        {
            get
            {
                return squirkleheight;
            }
            set
            {
                if (value != squirkleheight && value >= 1)
                {
                    squirkleheight = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double SquirkleLength
        {
            get
            {
                return squirklelength;
            }
            set
            {
                if (value != squirklelength && value > 1)
                {
                    squirklelength = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double SquirkleWidth
        {
            get
            {
                return squirklewidth;
            }
            set
            {
                if (value != squirklewidth && value > 1)
                {
                    squirklewidth = value;
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

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParmeters();
            DialogResult = true;
            Close();
        }

        private void BLMode(int mode)
        {
            UpdateDisplay();
        }

        private void BRMode(int mode)
        {
            UpdateDisplay();
        }

        private void CreateSideFace(List<System.Windows.Point> pnts, int i, bool autoclose = true)
        {
            int v = i + 1;

            if (v == pnts.Count)
            {
                v = 0;
            }

            int c0 = AddVertice(pnts[i].X, pnts[i].Y, 0.0);
            int c1 = AddVertice(pnts[i].X, pnts[i].Y, squirklewidth);
            int c2 = AddVertice(pnts[v].X, pnts[v].Y, squirklewidth);
            int c3 = AddVertice(pnts[v].X, pnts[v].Y, 0.0);
            Faces.Add(c0);
            Faces.Add(c2);
            Faces.Add(c1);

            Faces.Add(c0);
            Faces.Add(c3);
            Faces.Add(c2);
        }

        private void GenerateFromPath(string pathtext)
        {
            FlexiPath flexiPath = new FlexiPath();
            flexiPath.InterpretTextPath(pathtext);
            List<System.Windows.Point> points = flexiPath.DisplayPoints();
            List<System.Windows.Point> tmp = new List<System.Windows.Point>();
            for (int i = 0; i < points.Count; i++)
            {
                tmp.Add(new Point(points[i].X * squirklelength, points[i].Y * squirkleheight));
            }
            // generate side triangles so original points are already in list
            for (int i = 0; i < tmp.Count; i++)
            {
                CreateSideFace(tmp, i);
            }
            // triangulate the basic polygon
            TriangulationPolygon ply = new TriangulationPolygon();
            List<PointF> pf = new List<PointF>();
            foreach (System.Windows.Point p in tmp)
            {
                pf.Add(new PointF((float)p.X, (float)p.Y));
            }
            ply.Points = pf.ToArray();
            List<Triangle> tris = ply.Triangulate();
            foreach (Triangle t in tris)
            {
                int c0 = AddVertice(t.Points[0].X, t.Points[0].Y, 0.0);
                int c1 = AddVertice(t.Points[1].X, t.Points[1].Y, 0.0);
                int c2 = AddVertice(t.Points[2].X, t.Points[2].Y, 0.0);
                Faces.Add(c0);
                Faces.Add(c2);
                Faces.Add(c1);

                c0 = AddVertice(t.Points[0].X, t.Points[0].Y, squirklewidth);
                c1 = AddVertice(t.Points[1].X, t.Points[1].Y, squirklewidth);
                c2 = AddVertice(t.Points[2].X, t.Points[2].Y, squirklewidth);
                Faces.Add(c0);
                Faces.Add(c1);
                Faces.Add(c2);
            }
            CentreVertices();
        }

        private void GenerateShape()
        {
            ClearShape();

            double h = 0.25;
            double l = 0.25;
            string yseg = h.ToString();
            string xseg = l.ToString();

            string pathtext = "M 0,0 ";

            pathtext += "RV -" + yseg + " ";
            // bl
            if (BottomLeftCornerShape.Mode == 0)
            {
                pathtext += "RV -" + yseg + " ";
                pathtext += "RH " + xseg + " ";
            }
            if (BottomLeftCornerShape.Mode == 1)
            {
                pathtext += "RQ 0,-" + yseg + " " + xseg + ",-" + yseg + " ";
            }
            if (BottomLeftCornerShape.Mode == 2)
            {
                pathtext += "RQ " + xseg + ",0 " + xseg + ",-" + yseg + " ";
            }
            pathtext += "RH " + xseg + " ";

            // br
            pathtext += "RH " + xseg + " ";
            if (BottomRightCornerShape.Mode == 0)
            {
                pathtext += "RH " + xseg + " ";
                pathtext += "RV " + yseg + " ";
            }
            if (BottomRightCornerShape.Mode == 1)
            {
                pathtext += "RQ " + xseg + ",0 " + xseg + "," + yseg + " ";
            }
            if (BottomRightCornerShape.Mode == 2)
            {
                pathtext += "RQ 0," + yseg + " " + xseg + "," + yseg + " ";
            }
            pathtext += "RV " + yseg + " ";

            // tr
            pathtext += "RV " + yseg + " ";
            if (TopRightCornerShape.Mode == 0)
            {
                pathtext += "RV " + yseg + " ";
                pathtext += "RH -" + xseg + " ";
            }
            if (TopRightCornerShape.Mode == 1)
            {
                pathtext += "RQ 0," + yseg + " -" + xseg + "," + yseg + " ";
            }
            if (TopRightCornerShape.Mode == 2)
            {
                pathtext += "RQ -" + xseg + ",0 -" + xseg + "," + yseg + " ";
            }
            pathtext += "RH -" + xseg + " ";

            // tl
            pathtext += "RH -" + xseg + " ";
            if (TopLeftCornerShape.Mode == 0)
            {
                pathtext += "RH -" + xseg + " ";
                pathtext += "RV -" + yseg + " ";
            }
            if (TopLeftCornerShape.Mode == 1)
            {
                pathtext += "RQ -" + yseg + ",0 -" + xseg + ",-" + yseg + " ";
            }
            if (TopLeftCornerShape.Mode == 2)
            {
                pathtext += "RQ 0,-" + xseg + " -" + yseg + ",-" + xseg + " ";
            }
            pathtext += "RV -" + xseg + " ";

            GenerateFromPath(pathtext);
            Viewer.Model = GetModel();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            SquirkleHeight = EditorParameters.GetDouble("SquirkleHeight", 10);
            SquirkleLength = EditorParameters.GetDouble("SquirkleLength", 10);
            SquirkleWidth = EditorParameters.GetDouble("SquirkleWidth", 10);
            BottomLeftCornerShape.Mode = EditorParameters.GetInt("BottomLeftCornerShape", 0);
            TopLeftCornerShape.Mode = EditorParameters.GetInt("TopLeftCornerShape", 0);
            BottomRightCornerShape.Mode = EditorParameters.GetInt("BottomRightCornerShape", 0);
            TopRightCornerShape.Mode = EditorParameters.GetInt("TopRightCornerShape", 0);
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            EditorParameters.Set("SquirkleHeight", squirkleheight.ToString());
            EditorParameters.Set("SquirkleLength", squirklelength.ToString());
            EditorParameters.Set("SquirkleWidth", squirklewidth.ToString());
            EditorParameters.Set("BottomLeftCornerShape", BottomLeftCornerShape.Mode.ToString());
            EditorParameters.Set("TopLeftCornerShape", TopLeftCornerShape.Mode.ToString());
            EditorParameters.Set("BottomRightCornerShape", BottomRightCornerShape.Mode.ToString());
            EditorParameters.Set("TopRightCornerShape", TopRightCornerShape.Mode.ToString());
        }

        private void TLMode(int mode)
        {
            UpdateDisplay();
        }

        private void TRMode(int mode)
        {
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            GenerateShape();
            Viewer.Model = GetModel();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TopLeftCornerShape.Location = CornerShape.CornerLocation.TopLeft;
            TopLeftCornerShape.OnModeChanged += TLMode;

            TopRightCornerShape.Location = CornerShape.CornerLocation.TopRight;
            TopRightCornerShape.OnModeChanged += TRMode;

            BottomLeftCornerShape.Location = CornerShape.CornerLocation.BottomLeft;
            BottomLeftCornerShape.OnModeChanged += BLMode;

            BottomRightCornerShape.Location = CornerShape.CornerLocation.BottomRight;
            BottomRightCornerShape.OnModeChanged += BRMode;

            LoadEditorParameters();
            GenerateShape();
            UpdateCameraPos();
            Viewer.Clear();
            warningText = "";
            UpdateDisplay();
        }
    }
}