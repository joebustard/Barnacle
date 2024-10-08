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
using Barnacle.Object3DLib;
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
        private bool flatShape;

        public bool FlatShape
        {
            get { return flatShape; }

            set
            {
                if (value != flatShape)
                {
                    flatShape = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        private bool squareShape;

        public bool SquareShape
        {
            get { return squareShape; }

            set
            {
                if (value != squareShape)
                {
                    squareShape = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        private bool roundShape;

        public bool RoundShape
        {
            get { return roundShape; }

            set
            {
                if (value != roundShape)
                {
                    roundShape = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

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
                if (flatShape)
                {
                    GenerateFlat();
                }
                else
                if (squareShape)
                {
                    GenerateRound(4);
                }
                else
                {
                    GenerateRound(36);
                }
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

        private void GenerateRound(int numCircumferencePoints)
        {
            BufferedPolyline bl = new BufferedPolyline(pathPoints);
            bl.BufferRadius = loftThickness / 2;
            List<CurvePoint> curvePoints = bl.GenerateBufferCurvePoints();
            bl.BufferRadius = loftThickness / 2;
            if (curvePoints != null && curvePoints.Count > 1)
            {
                List<Point3D> circumference = new List<Point3D>();
                double dt = (2 * Math.PI) / numCircumferencePoints;
                double theta = 0;
                for (int i = 0; i < numCircumferencePoints; i++)
                {
                    Point p = CalcPoint(theta, bl.BufferRadius);
                    circumference.Add(new Point3D(0, p.Y, p.X));
                    theta += dt;
                }
                double lx, rx, ty, by, bz, fz;
                lx = by = bz = double.MaxValue;
                rx = ty = fz = double.MinValue;

                Point3D[,] surface = new Point3D[curvePoints.Count, numCircumferencePoints];
                for (int i = 0; i < curvePoints.Count; i++)
                {
                    double rotation = curvePoints[i].angle;
                    List<Point3D> tmp = RotatePoints(circumference, 0, rotation, 0);
                    for (int j = 0; j < numCircumferencePoints; j++)
                    {
                        Point3D pn = tmp[j];
                        pn.X += curvePoints[i].point.X;
                        pn.Y += curvePoints[i].point.Y;

                        surface[i, j] = pn;
                        if (pn.X < lx) lx = pn.X;
                        if (pn.X > rx) rx = pn.X;
                        if (pn.Y < by) by = pn.Y;
                        if (pn.Y > ty) ty = pn.Y;
                        if (pn.Z < bz) bz = pn.Z;
                        if (pn.Z > fz) fz = pn.Z;
                    }
                }

                octTree = CreateOctree(new Point3D(lx - 10, by - 10, bz - 10),
                                       new Point3D(rx + 10, ty + 10, fz + 10));

                for (int i = 0; i < curvePoints.Count - 1; i++)
                {
                    for (int j = 0; j < numCircumferencePoints; j++)
                    {
                        int k = j + 1;
                        if (k == numCircumferencePoints)
                        {
                            k = 0;
                        }

                        int p0 = AddVerticeOctTree(surface[i, j].X, ty - surface[i, j].Y, surface[i, j].Z);
                        int p1 = AddVerticeOctTree(surface[i + 1, j].X, ty - surface[i + 1, j].Y, surface[i + 1, j].Z);
                        int p2 = AddVerticeOctTree(surface[i, k].X, ty - surface[i, k].Y, surface[i, k].Z);
                        int p3 = AddVerticeOctTree(surface[i + 1, k].X, ty - surface[i + 1, k].Y, surface[i + 1, k].Z);

                        Faces.Add(p0);
                        Faces.Add(p1);
                        Faces.Add(p2);

                        Faces.Add(p1);
                        Faces.Add(p3);
                        Faces.Add(p2);
                    }
                }

                // close start
                int cp = AddVerticeOctTree(curvePoints[0].point.X, ty - curvePoints[0].point.Y, 0);
                for (int j = 0; j < numCircumferencePoints; j++)
                {
                    int k = j + 1;
                    if (k == numCircumferencePoints)
                    {
                        k = 0;
                    }

                    int p1 = AddVerticeOctTree(surface[0, j].X, ty - surface[0, j].Y, surface[0, j].Z);
                    int p2 = AddVerticeOctTree(surface[0, k].X, ty - surface[0, k].Y, surface[0, k].Z);

                    Faces.Add(cp);
                    Faces.Add(p1);
                    Faces.Add(p2);
                }

                // close end
                int last = curvePoints.Count - 1;
                cp = AddVerticeOctTree(curvePoints[last].point.X, ty - curvePoints[last].point.Y, 0);
                for (int j = 0; j < numCircumferencePoints; j++)
                {
                    int k = j + 1;
                    if (k == numCircumferencePoints)
                    {
                        k = 0;
                    }

                    int p1 = AddVerticeOctTree(surface[last, j].X, ty - surface[last, j].Y, surface[last, j].Z);
                    int p2 = AddVerticeOctTree(surface[last, k].X, ty - surface[last, k].Y, surface[last, k].Z);

                    Faces.Add(cp);
                    Faces.Add(p2);
                    Faces.Add(p1);
                }
                CentreVertices();
            }
        }

        private List<Point3D> RotatePoints(List<Point3D> pnts, double r1, double r2, double r3)
        {
            List<Point3D> tmp = new List<Point3D>();
            float cosa = (float)Math.Cos(r2);
            float sina = (float)Math.Sin(r2);

            float cosb = (float)Math.Cos(r1);
            float sinb = (float)Math.Sin(r1);

            float cosc = (float)Math.Cos(r3);
            float sinc = (float)Math.Sin(r3);

            float Axx = cosa * cosb;
            float Axy = cosa * sinb * sinc - sina * cosc;
            float Axz = cosa * sinb * cosc + sina * sinc;

            float Ayx = sina * cosb;
            float Ayy = sina * sinb * sinc + cosa * cosc;
            float Ayz = sina * sinb * cosc - cosa * sinc;

            float Azx = -sinb;
            float Azy = cosb * sinc;
            float Azz = cosb * cosc;
            foreach (Point3D cp in pnts)
            {
                Point3D rp = new Point3D();
                rp.X = Axx * cp.X + Axy * cp.Y + Axz * cp.Z;
                rp.Y = Ayx * cp.X + Ayy * cp.Y + Ayz * cp.Z;
                rp.Z = Azx * cp.X + Azy * cp.Y + Azz * cp.Z;

                tmp.Add(rp);
            }
            return tmp;
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
            FlatShape = EditorParameters.GetBoolean("FlatShape", true);
            RoundShape = EditorParameters.GetBoolean("RoundShape", false);
            SquareShape = EditorParameters.GetBoolean("SquareShape", false);
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
            EditorParameters.Set("FlatShape", FlatShape.ToString());
            EditorParameters.Set("RoundShape", RoundShape.ToString());
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