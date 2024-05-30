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

using Barnacle.Models.BufferedPolyline;
using OctTreeLib;
using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;
using static Barnacle.Models.BufferedPolyline.BufferedPolyline;
using Triangle = PolygonTriangulationLib.Triangle;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for PathLoft.xaml
    /// </summary>
    public partial class PathLoftDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private const double maxloftHeight = 200;
        private const double maxloftThickness = 200;
        private const double minloftHeight = 0.1;
        private const double minloftThickness = 0.1;
        private double blx = 0;
        private double bly = 0;
        private bool loaded;
        private double loftHeight;
        private double loftThickness;
        private OctTree octTree;
        private List<Point> pathPoints;
        private double triy = 0;
        private double trx = 0;
        private string warningText;
        private double xExtent;
        private double yExtent;

        public PathLoftDlg()
        {
            InitializeComponent();
            ToolName = "PathLoft";
            DataContext = this;
            ModelGroup = MyModelGroup;
            loaded = false;
            PathEditor.OnFlexiPathChanged += PathPointsChanged;
            pathPoints = new List<Point>();
            PathEditor.ToolName = ToolName;
            PathEditor.HasPresets = false;
            PathEditor.ShowAppend = true;
            PathEditor.ContinuousPointsNotify = true;
        }

        public double LoftHeight
        {
            get
            {
                return loftHeight;
            }

            set
            {
                if (loftHeight != value)
                {
                    if (value >= minloftHeight && value <= maxloftHeight)
                    {
                        loftHeight = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String LoftHeightToolTip
        {
            get
            {
                return $"LoftHeight must be in the range {minloftHeight} to {maxloftHeight}";
            }
        }

        public double LoftThickness

        {
            get
            {
                return loftThickness;
            }

            set
            {
                if (loftThickness != value)
                {
                    if (value >= minloftThickness && value <= maxloftThickness)
                    {
                        loftThickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String LoftThicknessToolTip
        {
            get
            {
                return $"LoftThickness must be in the range {minloftThickness} to {maxloftThickness}";
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

        public int AddVerticeOctTree(double x, double y, double z)
        {
            int res = -1;
            if (octTree != null)
            {
                Point3D v = new Point3D(x, y, z);
                res = octTree.PointPresent(v);

                if (res == -1)
                {
                    res = Vertices.Count;
                    octTree.AddPoint(res, v);
                }
            }
            return res;
        }

        protected OctTree CreateOctree(Point3D minPoint, Point3D maxPoint)
        {
            octTree = new OctTree(Vertices, minPoint, maxPoint, 200);
            return octTree;
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParmeters();
            DialogResult = true;
            Close();
        }

        private void CalculateExtents(List<Point> tmp, out double lx, out double rx, out double ty, out double by)
        {
            lx = double.MaxValue;
            rx = double.MinValue;
            ty = double.MinValue;
            by = double.MaxValue;
            for (int i = 0; i < tmp.Count; i++)
            {
                lx = Math.Min(tmp[i].X, lx);
                rx = Math.Max(tmp[i].X, rx);
                by = Math.Min(tmp[i].Y, by);
                ty = Math.Max(tmp[i].Y, ty);
            }
            xExtent = rx - lx;
            yExtent = ty - by;
        }

        private void CreateSideFace(List<System.Windows.Point> pnts, int i, bool autoclose = true)
        {
            int v = i + 1;

            if (v == pnts.Count)
            {
                if (autoclose)
                {
                    v = 0;
                }
                else
                {
                    // dont process the final point if caller doesn't want it
                    return;
                }
            }

            int c0 = AddVerticeOctTree(pnts[i].X, pnts[i].Y, 0.0);
            int c1 = AddVerticeOctTree(pnts[i].X, pnts[i].Y, loftHeight);
            int c2 = AddVerticeOctTree(pnts[v].X, pnts[v].Y, loftHeight);
            int c3 = AddVerticeOctTree(pnts[v].X, pnts[v].Y, 0.0);
            Faces.Add(c0);
            Faces.Add(c2);
            Faces.Add(c1);

            Faces.Add(c0);
            Faces.Add(c3);
            Faces.Add(c2);
        }

        private void GenerateShape()
        {
            ClearShape();
            if (pathPoints != null && pathPoints.Count > 0)
            {
                GenerateFlat();
            }
        }

        private void GenerateFlat()
        {
            BufferedPolyline bl = new BufferedPolyline(pathPoints);
            bl.BufferRadius = loftThickness / 2;
            List<Point> outline = bl.GenerateBufferOutline();
            if (outline != null && outline.Count > 3)
            {
                List<System.Windows.Point> tmp = new List<System.Windows.Point>();
                double top = 0;
                for (int i = 0; i < outline.Count; i++)
                {
                    if (outline[i].Y > top)
                    {
                        top = outline[i].Y;
                    }
                }
                for (int i = 0; i < outline.Count; i++)
                {
                    tmp.Add(new Point(outline[i].X, top - outline[i].Y));
                }
                double lx, rx, ty, by;
                CalculateExtents(tmp, out lx, out rx, out ty, out by);

                octTree = CreateOctree(new Point3D(-lx, -by, -1),
                                        new Point3D(+rx, +ty, loftHeight + 1));

                for (int i = 0; i < tmp.Count; i++)
                {
                    CreateSideFace(tmp, i);
                }

                // triangulate the basic polygon
                TriangulationPolygon ply = new TriangulationPolygon();
                List<System.Drawing.PointF> pf = new List<System.Drawing.PointF>();
                foreach (System.Windows.Point p in tmp)
                {
                    pf.Add(new System.Drawing.PointF((float)p.X, (float)p.Y));
                }
                ply.Points = pf.ToArray();
                List<Triangle> tris = ply.Triangulate();
                foreach (Triangle t in tris)
                {
                    int c0 = AddVerticeOctTree(t.Points[0].X, t.Points[0].Y, 0.0);
                    int c1 = AddVerticeOctTree(t.Points[1].X, t.Points[1].Y, 0.0);
                    int c2 = AddVerticeOctTree(t.Points[2].X, t.Points[2].Y, 0.0);
                    Faces.Add(c0);
                    Faces.Add(c2);
                    Faces.Add(c1);

                    c0 = AddVerticeOctTree(t.Points[0].X, t.Points[0].Y, loftHeight);
                    c1 = AddVerticeOctTree(t.Points[1].X, t.Points[1].Y, loftHeight);
                    c2 = AddVerticeOctTree(t.Points[2].X, t.Points[2].Y, loftHeight);
                    Faces.Add(c0);
                    Faces.Add(c1);
                    Faces.Add(c2);
                }

                CentreVertices();

               
            }
        }


        private void GenerateRound()
        {
            BufferedPolyline bl = new BufferedPolyline(pathPoints);
            bl.BufferRadius = loftThickness / 2;
            List<Point> outline = bl.GenerateBufferOutline();
            if (outline != null && outline.Count > 3)
            {
                List<System.Windows.Point> tmp = new List<System.Windows.Point>();
                double top = 0;
                for (int i = 0; i < outline.Count; i++)
                {
                    if (outline[i].Y > top)
                    {
                        top = outline[i].Y;
                    }
                }
                for (int i = 0; i < outline.Count; i++)
                {
                    tmp.Add(new Point(outline[i].X, top - outline[i].Y));
                }
                double lx, rx, ty, by;
                CalculateExtents(tmp, out lx, out rx, out ty, out by);

                octTree = CreateOctree(new Point3D(-lx, -by, -1),
                                        new Point3D(+rx, +ty, loftHeight + 1));

                for (int i = 0; i < tmp.Count; i++)
                {
                    CreateSideFace(tmp, i);
                }

                // triangulate the basic polygon
                TriangulationPolygon ply = new TriangulationPolygon();
                List<System.Drawing.PointF> pf = new List<System.Drawing.PointF>();
                foreach (System.Windows.Point p in tmp)
                {
                    pf.Add(new System.Drawing.PointF((float)p.X, (float)p.Y));
                }
                ply.Points = pf.ToArray();
                List<Triangle> tris = ply.Triangulate();
                foreach (Triangle t in tris)
                {
                    int c0 = AddVerticeOctTree(t.Points[0].X, t.Points[0].Y, 0.0);
                    int c1 = AddVerticeOctTree(t.Points[1].X, t.Points[1].Y, 0.0);
                    int c2 = AddVerticeOctTree(t.Points[2].X, t.Points[2].Y, 0.0);
                    Faces.Add(c0);
                    Faces.Add(c2);
                    Faces.Add(c1);

                    c0 = AddVerticeOctTree(t.Points[0].X, t.Points[0].Y, loftHeight);
                    c1 = AddVerticeOctTree(t.Points[1].X, t.Points[1].Y, loftHeight);
                    c2 = AddVerticeOctTree(t.Points[2].X, t.Points[2].Y, loftHeight);
                    Faces.Add(c0);
                    Faces.Add(c1);
                    Faces.Add(c2);
                }

                CentreVertices();

                TestCurve();
            }
        }

        private void TestCurve()
        {
            List<Point> pnts = new List<Point>();
            pnts.Add(new Point(10, 10));
            pnts.Add(new Point(20, 10));
            pnts.Add(new Point(25, 0));
            BufferedPolyline bl = new BufferedPolyline(pnts);
            bl.BufferRadius = loftThickness / 2;
            List<CurvePoint> curvePoints = bl.GenerateBufferCurvePoints();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            string imageName = EditorParameters.Get("ImagePath");
            if (imageName != "")
            {
                PathEditor.LoadImage(imageName);
            }
            LoftHeight = EditorParameters.GetDouble("LoftHeight", 10);
            LoftThickness = EditorParameters.GetDouble("LoftThickness", 5);
            String s = EditorParameters.Get("Path");
            if (s != "")
            {
                PathEditor.FromString(s);
            }
        }

        private void PathPointsChanged(List<Point> points)
        {
            pathPoints.Clear();
            foreach (Point p in points)
            {
                pathPoints.Insert(0, new Point(p.X, p.Y));
            }

            GenerateShape();
            Redisplay();
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("LoftHeight", LoftHeight.ToString());
            EditorParameters.Set("LoftThickness", LoftThickness.ToString());
            EditorParameters.Set("Path", PathEditor.AbsolutePathString);
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
            // should flexi control give us live point updates while lines are dragged. Computing
            // new line costs too much so , no, instead wait until mouse up

            PathEditor.OpenEndedPath = true;
            LoadEditorParameters();

            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            loaded = true;

            UpdateDisplay();
        }
    }
}