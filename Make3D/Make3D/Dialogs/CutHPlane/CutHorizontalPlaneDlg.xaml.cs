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

using Barnacle.Models;
using Barnacle.Object3DLib;
using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using OctTreeLib;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for CutHorizontalPlane.xaml
    /// </summary>
    public partial class CutHorizontalPlaneDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private const double maxplaneLevel = 300;
        private const double minplaneLevel = -300;
        private Bounds3D bounds;
        private DpiScale dpi;
        private bool loaded;
        private OctTree octTree;
        private Bounds3D originalBounds;
        private HorizontalPlane plane;
        private double planeLevel;
        private bool planeSelected;
        private string warningText;

        public CutHorizontalPlaneDlg()
        {
            InitializeComponent();
            ToolName = "CutHorizontalPlane";
            DataContext = this;
            ModelGroup = MyModelGroup;
            loaded = false;
            planeLevel = 5;
            planeSelected = false;
            dpi = VisualTreeHelper.GetDpi(this);
        }

        public Int32Collection OriginalFaces
        {
            get; internal set;
        }

        public Point3DCollection OriginalVertices
        {
            get;
            set;
        }

        public double PlaneLevel
        {
            get
            {
                return planeLevel;
            }

            set
            {
                if (planeLevel != value)
                {
                    if (value >= minplaneLevel && value <= maxplaneLevel)
                    {
                        planeLevel = value;
                        if (plane != null)

                        {
                            plane.MoveTo(PlaneLevel);
                        }
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String PlaneLevelToolTip
        {
            get
            {
                return $"Plane Level must be in the range {minplaneLevel} to {maxplaneLevel}";
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
            DialogResult = true;
            Close();
        }

        protected override void Redisplay()
        {
            if (ModelGroup != null)
            {
                ModelGroup.Children.Clear();

                if (floor != null && ShowFloor)
                {
                    ModelGroup.Children.Add(floor.FloorMesh);
                    foreach (GeometryModel3D m in grid.Group.Children)
                    {
                        ModelGroup.Children.Add(m);
                    }
                }

                if (axies != null && ShowAxies)
                {
                    foreach (GeometryModel3D m in axies.Group.Children)
                    {
                        ModelGroup.Children.Add(m);
                    }
                }

                GeometryModel3D gm = GetModel();
                ModelGroup.Children.Add(gm);

                if (plane != null && plane.PlaneMesh != null)
                {
                    ModelGroup.Children.Add(plane.PlaneMesh);
                }
            }
        }

        protected override void Viewport_MouseDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Viewport3D vp = sender as Viewport3D;
            if (vp != null)
            {
                if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed && e.Handled == false)
                {
                    lastHitModel = null;

                    oldMousePos = e.GetPosition(vp);
                    HitTest(vp, oldMousePos);
                    if (plane.Matches(lastHitModel))
                    {
                        planeSelected = true;
                    }

                    if (floor.Matches(lastHitModel) || grid.Matches(lastHitModel))
                    {
                        planeSelected = false;
                    }
                }
            }
        }

        protected override void Viewport_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Viewport3D vp = sender as Viewport3D;
            if (vp != null)
            {
                if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed && e.Handled == false)
                {
                    Point pn = e.GetPosition(vp);
                    double dx = pn.X - oldMousePos.X;
                    double dy = pn.Y - oldMousePos.Y;
                    if (planeSelected)
                    {
                        double ny = PlaneLevel - (dy / dpi.PixelsPerInchY * 25.4);
                        if (ny < 0)
                        {
                            ny = 0;
                        }
                        else if (ny > bounds.Height)
                        {
                            ny = Height;
                        }
                        PlaneLevel = ny;
                    }
                    else
                    {
                        Camera.Move(dx, -dy);
                        UpdateCameraPos();
                    }
                    oldMousePos = pn;
                    e.Handled = true;
                }
            }
        }

        private int AddVerticeOctTree(Point3D p)
        {
            return AddVerticeOctTree(p.X, p.Y, p.Z);
        }

        private int CrossingPoint(int a, int b)
        {
            double t;
            double x0 = Vertices[a].X;
            double y0 = Vertices[a].Y;
            double z0 = Vertices[a].Z;
            double x1 = Vertices[b].X;
            double y1 = Vertices[b].Y;
            double z1 = Vertices[b].Z;
            t = (planeLevel - y0) / (y1 - y0);

            double x = x0 + t * (x1 - x0);
            double z = z0 + t * (z1 - z0);
            int res = AddVerticeOctTree(x, planeLevel, z);
            return res;
        }

        private void CutButton_Click(object sender, RoutedEventArgs e)
        {
            RestoreOriginal();
            EdgeProcessor edgeProc = new EdgeProcessor();

            Int32Collection newFaces = new Int32Collection();
            for (int i = 0; i < Faces.Count; i += 3)
            {
                int a = Faces[i];
                int b = Faces[i + 1];
                int c = Faces[i + 2];

                int upCount = 0;
                bool aUp = false;
                bool bUp = false;
                bool cUp = false;
                if (Vertices[a].Y > planeLevel)
                {
                    upCount++;
                    aUp = true;
                }
                if (Vertices[b].Y > planeLevel)
                {
                    upCount++;
                    bUp = true;
                }
                if (Vertices[c].Y > planeLevel)
                {
                    upCount++;
                    cUp = true;
                }

                switch (upCount)
                {
                    case 0:
                        {
                            //all three points of trinagle are on or below the cut plane
                        }
                        break;

                    case 1:
                        {
                            //one point of triangle is above the cut plane
                            // clip it against the plane
                            if (aUp)
                            {
                                MakeTri1(a, ref b, ref c);
                                newFaces.Add(a);
                                newFaces.Add(b);
                                newFaces.Add(c);
                                edgeProc.Add(b, c);
                            }
                            else if (bUp)
                            {
                                MakeTri1(b, ref c, ref a);
                                newFaces.Add(b);
                                newFaces.Add(c);
                                newFaces.Add(a);
                                edgeProc.Add(c, a);
                            }
                            else if (cUp)
                            {
                                MakeTri1(c, ref a, ref b);
                                newFaces.Add(c);
                                newFaces.Add(a);
                                newFaces.Add(b);
                                edgeProc.Add(a, b);
                            }
                        }
                        break;

                    case 2:
                        {
                            // two points are above the cut plane
                            if (aUp && bUp)
                            {
                                int dp = CrossingPoint(b, c);
                                int ep = CrossingPoint(a, c);
                                newFaces.Add(a);
                                newFaces.Add(b);
                                newFaces.Add(dp);

                                newFaces.Add(a);
                                newFaces.Add(dp);
                                newFaces.Add(ep);
                                edgeProc.Add(dp, ep);
                            }
                            else if (bUp && cUp)
                            {
                                int dp = CrossingPoint(c, a);
                                int ep = CrossingPoint(a, b);
                                newFaces.Add(b);
                                newFaces.Add(c);
                                newFaces.Add(dp);

                                newFaces.Add(b);
                                newFaces.Add(dp);
                                newFaces.Add(ep);
                                edgeProc.Add(dp, ep);
                            }
                            else if (cUp && aUp)
                            {
                                int dp = CrossingPoint(a, b);
                                int ep = CrossingPoint(b, c);
                                newFaces.Add(a);
                                newFaces.Add(dp);
                                newFaces.Add(c);

                                newFaces.Add(c);
                                newFaces.Add(dp);
                                newFaces.Add(ep);
                                edgeProc.Add(dp, ep);
                            }
                        }
                        break;

                    case 3:
                        {
                            //all three points of triangle are above the cut plane
                            // entire triangle should be taken as is
                            newFaces.Add(a);
                            newFaces.Add(b);
                            newFaces.Add(c);
                        }
                        break;
                }
            }
            bool moreLoops = true;
            while (moreLoops)
            {
                moreLoops = false;
                List<EdgeRecord> loop = edgeProc.MakeLoop();
                if (loop.Count > 3)
                {
                    TriangulationPolygon ply = new TriangulationPolygon();
                    List<System.Drawing.PointF> pf = new List<System.Drawing.PointF>();
                    foreach (EdgeRecord er in loop)
                    {
                        Point3D p = Vertices[er.Start];
                        pf.Add(new System.Drawing.PointF((float)p.X, (float)p.Z));
                    }
                    ply.Points = pf.ToArray();
                    List<Triangle> tris = ply.Triangulate();
                    foreach (Triangle t in tris)
                    {
                        int c0 = AddVerticeOctTree(t.Points[0].X, planeLevel, t.Points[0].Y);
                        int c1 = AddVerticeOctTree(t.Points[1].X, planeLevel, t.Points[1].Y);
                        int c2 = AddVerticeOctTree(t.Points[2].X, planeLevel, t.Points[2].Y);
                        newFaces.Add(c0);
                        newFaces.Add(c1);
                        newFaces.Add(c2);
                    }
                }
                if (loop.Count != 0 && edgeProc.EdgeRecords.Count > 0)
                {
                    moreLoops = true;
                }
            }

            Point3DCollection allPoints = Vertices;
            Vertices = new Point3DCollection();
            ClearShape();
            octTree = CreateOctree(originalBounds.Lower, originalBounds.Upper);
            foreach (int j in newFaces)
            {
                int v = AddVerticeOctTree(allPoints[j]);
                Faces.Add(v);
            }

            UpdateDisplay();
        }

        private void GenerateShape()
        {
        }

        private void MakeTri1(int a, ref int b, ref int c)
        {
            double t;
            double x0 = Vertices[a].X;
            double y0 = Vertices[a].Y;
            double z0 = Vertices[a].Z;
            double x1 = Vertices[b].X;
            double y1 = Vertices[b].Y;
            double z1 = Vertices[b].Z;
            t = (planeLevel - y0) / (y1 - y0);

            double x = x0 + t * (x1 - x0);
            double z = z0 + t * (z1 - z0);

            b = AddVerticeOctTree(x, planeLevel, z);

            x1 = Vertices[c].X;
            y1 = Vertices[c].Y;
            z1 = Vertices[c].Z;
            t = (planeLevel - y0) / (y1 - y0);

            x = x0 + t * (x1 - x0);
            z = z0 + t * (z1 - z0);

            c = AddVerticeOctTree(x, planeLevel, z);
        }

        private void ResetDefaults(object sender, RoutedEventArgs e)
        {
            SetDefaults();
            UpdateDisplay();
        }

        private void RestoreOriginal()
        {
            bounds = new Bounds3D();
            bounds.Zero();
            ClearShape();
            octTree = CreateOctree(originalBounds.Lower, originalBounds.Upper);

            if (OriginalFaces != null)
            {
                foreach (int i in OriginalFaces)
                {
                    Point3D p = OriginalVertices[i];
                    bounds.Adjust(p);
                    int k = AddVerticeOctTree(p.X, p.Y, p.Z);
                    Faces.Add(k);
                }
            }

            CentreVertices();
        }

        private void SetDefaults()
        {
            loaded = false;
            PlaneLevel = 0;

            loaded = true;
        }

        private void UncutButton_Click(object sender, RoutedEventArgs e)
        {
            RestoreOriginal();
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (loaded)
            {
                GenerateShape();
                Redisplay();
            }
        }

        private void Viewport_MouseUp(object sender, System.Windows.Input.MouseEventArgs e)
        {
            planeSelected = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WarningText = "";

            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            loaded = true;
            originalBounds = new Bounds3D();
            originalBounds.Zero();
            bounds = new Bounds3D();
            bounds.Zero();
            ClearShape();
            if (OriginalVertices != null)
            {
                foreach (Point3D p in OriginalVertices)
                {
                    originalBounds.Adjust(p);
                }
            }
            octTree = CreateOctree(originalBounds.Lower, originalBounds.Upper);

            RestoreOriginal();

            plane = new HorizontalPlane(planeLevel, bounds.Width + 20, bounds.Depth + 20);
            plane.MoveTo(PlaneLevel);
            UpdateDisplay();
        }
    }
}