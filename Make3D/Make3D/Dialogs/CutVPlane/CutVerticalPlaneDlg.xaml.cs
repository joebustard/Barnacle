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
using MakerLib.PlaneCutter;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for CutHorizontalPlane.xaml
    /// </summary>
    public partial class CutVerticalPlaneDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private const double maxplaneLevel = 300;
        private const double minplaneLevel = -300;
        private Bounds3D bounds;
        private DpiScale dpi;
        private bool loaded;
        private OctTree octTree;
        private Bounds3D originalBounds;
        private VerticalPlane plane;
        private double planeLevel;
        private bool planeSelected;
        private string warningText;

        public CutVerticalPlaneDlg()
        {
            InitializeComponent();
            ToolName = "CutVerticalPlane";
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
            return new OctTree(Vertices, minPoint, maxPoint, 200);
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
                        double nx = PlaneLevel + (dx / dpi.PixelsPerInchX * 25.4);
                        if (nx < bounds.Lower.X)
                        {
                            nx = bounds.Lower.X;
                        }
                        else if (nx > bounds.Upper.X)
                        {
                            nx = bounds.Upper.X;
                        }
                        PlaneLevel = nx;
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

        private void CutButton_Click(object sender, RoutedEventArgs e)
        {
            RestoreOriginal();
            PlaneCutter cutter = new PlaneCutter(Vertices, Faces, planeLevel);
            cutter.SetVertical();
            cutter.Cut();
            UpdateDisplay();
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
            PlaneLevel = bounds.Lower.X;

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
            PlaneLevel = bounds.Lower.X;
            plane = new VerticalPlane(planeLevel, bounds.Height, bounds.Depth + 20);
            plane.MoveTo(PlaneLevel);
            UpdateDisplay();
        }
    }
}