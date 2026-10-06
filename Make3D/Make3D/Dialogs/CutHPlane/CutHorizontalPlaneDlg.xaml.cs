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
using Barnacle.UserControls;
using MakerLib.PlaneCutter;
using MathsLib;
using OctTreeLib;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

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
        private PlaneControl plane;
        private PlaneEquation planeEquation;
        private Point3D planeOrigin;
        private double planeOriginX;
        private double planeOriginY;
        private double planeOriginZ;
        private bool planeSelected;
        private Vector3D planeVector;
        private string warningText;

        public CutHorizontalPlaneDlg()
        {
            InitializeComponent();
            ToolName = "CutHorizontalPlane";
            DataContext = this;
            loaded = false;
            planeSelected = false;
            planeVector = new Vector3D(0, 1, 0);
            planeOrigin = new Point3D(0, 0, 0);
            dpi = VisualTreeHelper.GetDpi(this);
            PlaneDirection.OnUpdated += PlaneDirectionUpdated;
        }

        private void PlaneDirectionUpdated(PlaneDirectionControl.Direction direction)
        {
            PolarCoordinate pc = new PolarCoordinate(0, 0, 1);
            TestPolarCoordinate(10, 10, 10);
            TestPolarCoordinate(-4, -3, -2);
            TestPolarCoordinate(17, 18, -1.10);
            double dp = 0.02;
            pc.SetPoint3D(new Point3D(planeVector.X, planeVector.Y, planeVector.Z));
            switch (direction)
            {
                case PlaneDirectionControl.Direction.Up:
                    {
                        pc.Phi += dp;
                    }
                    break;

                case PlaneDirectionControl.Direction.Down:
                    {
                        pc.Phi -= dp;
                    }
                    break;

                case PlaneDirectionControl.Direction.Right:
                    {
                        pc.Theta += dp;
                    }
                    break;

                case PlaneDirectionControl.Direction.Left:
                    {
                        pc.Theta -= dp;
                    }
                    break;
            }
            Point3D np = pc.GetPoint3D();
            planeVector.X = np.X;
            planeVector.Y = np.Y;
            planeVector.Z = np.Z;
            planeEquation = new PlaneEquation(planeVector.X, planeVector.Y, planeVector.Z, planeOrigin.X, planeOrigin.Y, planeOrigin.Z);
            plane = new PlaneControl(planeEquation, plane.Radius);
            UpdatePlaneDisplay();
        }

        private double Degs(double v)
        {
            return v * 180.0 / Math.PI;
        }

        private void TestPolarCoordinate(int v1, int v2, double v3)
        {
            double lim = 1.0e-7;
            PolarCoordinate pc = new PolarCoordinate(0, 0, 1);
            pc.SetPoint3D(new Point3D(v1, v2, v3));
            System.Diagnostics.Debug.WriteLine($"radius {pc.Rho},  phi {Degs(pc.Phi)} theta {Degs(pc.Theta)}");
            Point3D p = pc.GetPoint3D();
            if (Math.Abs(p.X - v1) > lim)
            {
                System.Diagnostics.Debug.WriteLine($"v1 error {Math.Abs(p.X - v1)}");
            }
            if (Math.Abs(p.Y - v2) > lim)
            {
                System.Diagnostics.Debug.WriteLine($"v2 error {Math.Abs(p.Y - v2)}");
            }
            if (Math.Abs(p.Z - v3) > lim)
            {
                System.Diagnostics.Debug.WriteLine($"v3 error {Math.Abs(p.Z - v3)}");
            }
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

        public double PlaneOriginX
        {
            get
            {
                return planeOriginX;
            }
            set
            {
                if (planeOriginX != value)
                {
                    planeOriginX = value;
                    planeOrigin.X = planeOriginX;
                    NotifyPropertyChanged();
                    UpdatePlaneDisplay();
                }
            }
        }

        public double PlaneOriginY
        {
            get
            {
                return planeOriginY;
            }
            set
            {
                if (planeOriginY != value)
                {
                    planeOriginY = value;
                    planeOrigin.Y = planeOriginY;
                    NotifyPropertyChanged();
                    UpdatePlaneDisplay();
                }
            }
        }

        public double PlaneOriginZ
        {
            get
            {
                return planeOriginZ;
            }
            set
            {
                if (planeOriginZ != value)
                {
                    planeOriginZ = value;
                    planeOrigin.Z = PlaneOriginZ;
                    NotifyPropertyChanged();
                    UpdatePlaneDisplay();
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
            Viewer.MultiModels.Children.Clear();
            Viewer.MultiModels.Children.Add(GetModel());
            if (plane != null && plane.PlaneMesh != null)
            {
                Viewer.MultiModels.Children.Add(plane.PlaneMesh);
            }
            Viewer.Redisplay();
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
                        double ny = planeOrigin.Y - (dy / dpi.PixelsPerInchY * 25.4);
                        if (ny < 0)
                        {
                            ny = 0;
                        }
                        else if (ny > bounds.Height)
                        {
                            ny = Height;
                        }
                        planeOrigin.Y = ny;
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

        private void HorizontalButton_Click(object sender, RoutedEventArgs e)
        {
            CreateHorizontalPlaneControl();
        }

        private void CreateHorizontalPlaneControl()
        {
            planeEquation = new PlaneEquation(0, 1, 0, planeOrigin.X, planeOrigin.Y, planeOrigin.Z);
            double radius = Math.Max(bounds.Width, bounds.Depth) + 20;
            radius /= 2;
            plane = new HorizontalPlane(planeEquation, radius);
            planeVector = new Vector3D(planeEquation.A, planeEquation.B, planeEquation.C);
            plane.MoveTo(planeOrigin.X, planeOrigin.Y, planeOrigin.Z);
            RestoreOriginal();
            UpdateDisplay();
        }

        private void VerticalButton_Click(object sender, RoutedEventArgs e)
        {
            planeEquation = new PlaneEquation(1, 0, 0, planeOrigin.X, planeOrigin.Y, planeOrigin.Z);
            double radius = Math.Max(bounds.Height, bounds.Depth) + 20;
            radius /= 2;
            plane = new HorizontalPlane(planeEquation, radius);
            planeVector = new Vector3D(planeEquation.A, planeEquation.B, planeEquation.C);
            plane.MoveTo(planeOrigin.X, planeOrigin.Y, planeOrigin.Z);
            RestoreOriginal();
            UpdateDisplay();
        }

        private void DistalButton_Click(object sender, RoutedEventArgs e)
        {
            planeEquation = new PlaneEquation(0, 0, 1, planeOrigin.X, planeOrigin.Y, planeOrigin.Z);
            double radius = Math.Max(bounds.Width, bounds.Height) + 20;
            radius /= 2;
            plane = new DistalPlane(planeEquation, radius);
            planeVector = new Vector3D(planeEquation.A, planeEquation.B, planeEquation.C);
            plane.MoveTo(planeOrigin.X, planeOrigin.Y, planeOrigin.Z);
            RestoreOriginal();
            UpdateDisplay();
        }

        private void CutButton_Click(object sender, RoutedEventArgs e)
        {
            RestoreOriginal();
            PlaneCutter cutter = new PlaneCutter(Vertices, Faces, planeEquation);
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
            PlaneOriginX = 0;
            PlaneOriginY = 0;
            PlaneOriginZ = 0;
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

        private void UpdatePlaneDisplay()
        {
            if (plane != null)
            {
                plane.MoveTo(planeOrigin.X, planeOrigin.Y, planeOrigin.Z);
                UpdateDisplay();
            }
        }

        private void Viewer_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    {
                        if (Keyboard.Modifiers == ModifierKeys.Control)
                        {
                            PlaneOriginZ -= 1.0;
                        }
                        else
                        {
                            PlaneOriginY += 1.0;
                        }

                        e.Handled = true;
                    }
                    break;

                case Key.Down:
                    {
                        if (Keyboard.Modifiers == ModifierKeys.Control)
                        {
                            PlaneOriginZ += 1.0;
                        }
                        else
                        {
                            PlaneOriginY -= 1.0;
                        }

                        e.Handled = true;
                    }
                    break;

                case Key.Left:
                    {
                        PlaneOriginX -= 1.0;
                        e.Handled = true;
                    }
                    break;

                case Key.Right:
                    {
                        PlaneOriginX += 1.0;
                        e.Handled = true;
                    }
                    break;
            }
            if (e.Handled)
            {
                Viewer.Focus();
            }
        }

        private void Viewport_MouseUp(object sender, System.Windows.Input.MouseEventArgs e)
        {
            planeSelected = true;
        }

        private void viewport3D1_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
        }

        private void viewport3D1_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WarningText = "";

            UpdateCameraPos();
            Viewer.Clear();
            Viewer.KeyUp += Viewer_KeyUp;
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
            CreateHorizontalPlaneControl();
        }
    }
}