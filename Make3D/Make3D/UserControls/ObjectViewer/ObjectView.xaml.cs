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

using Barnacle.Models;
using Barnacle.Object3DLib;
using FileUtils;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace Barnacle.UserControls.ObjectViewer
{
    /// <summary>
    /// Interaction logic for ObjectView.xaml
    /// </summary>
    public partial class ObjectView : UserControl, INotifyPropertyChanged
    {
        public UserKeyHandler OnPreviewUserKey;

        private const string cameraRecordFile = "viewcamerapos.txt";

        private Axies axies;

        private Bounds3D bounds;

        private Visibility busyVisible = Visibility.Hidden;

        private CameraModes cameraMode;

        private Point3D cameraPosition;

        private double fieldOfView;

        private Floor floor;

        private Grid3D grid;

        private Vector3D lookDirection;

        // used if only one model is to be displayed
        private GeometryModel3D model;

        private Model3DGroup modelContent;

        // used if more that one model is to be displayed
        private Model3DGroup multiModels;

        private Point oldMousePos;

        private PolarCamera polarCamera;

        private bool showAxies;

        private bool showFloor;

        public ObjectView()
        {
            InitializeComponent();
            DataContext = this;
            polarCamera = new PolarCamera(100);
            polarCamera.HomeFront();
            cameraMode = CameraModes.Normal;

            LookDirection = new Vector3D(-polarCamera.CameraPos.X, -polarCamera.CameraPos.Y, -polarCamera.CameraPos.Z);
            FieldOfView = 45;
            floor = new Floor();
            grid = new Grid3D();
            axies = new Axies();
            showFloor = true;
            showAxies = true;
            modelContent = new Model3DGroup();
            multiModels = new Model3DGroup();
            OnViewMouseDown = null;
            OnViewMouseUp = null;
            OnViewMouseMove = null;
        }

        public delegate bool UserKeyHandler(Key key, bool shift, bool ctrl, bool alt);

        public delegate bool ViewMouseDown(object sender, MouseEventArgs e);

        public delegate bool ViewMouseMove(object sender, MouseEventArgs e);

        public delegate bool ViewMouseUp(object sender, MouseEventArgs e);

        public event PropertyChangedEventHandler PropertyChanged;

        private enum CameraModes
        {
            Normal,
            Pan
        }

        public Visibility BusyVisible
        {
            get
            {
                return busyVisible;
            }
            set
            {
                if (value != busyVisible)
                {
                    busyVisible = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public PolarCamera Camera
        {
            get
            {
                return polarCamera;
            }

            set
            {
                polarCamera = value;
            }
        }

        public Point3D CameraPosition
        {
            get
            {
                return cameraPosition;
            }

            set
            {
                if (cameraPosition != value)
                {
                    cameraPosition = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double FieldOfView
        {
            get
            {
                return fieldOfView;
            }

            set
            {
                if (fieldOfView != value)
                {
                    fieldOfView = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Vector3D LookDirection
        {
            get
            {
                return lookDirection;
            }

            set
            {
                if (lookDirection != value)
                {
                    lookDirection = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public GeometryModel3D Model
        {
            get
            {
                return model;
            }

            set
            {
                if (value != model)
                {
                    model = value;

                    Redisplay();
                }
            }
        }

        public Model3DGroup ModelContent
        {
            get
            {
                return modelContent;
            }

            set
            {
                if (value != modelContent)
                {
                    modelContent = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Model3DGroup MultiModels
        {
            get
            {
                return multiModels;
            }
        }

        public ViewMouseDown OnViewMouseDown
        {
            get;
            set;
        }

        public ViewMouseMove OnViewMouseMove
        {
            get;
            set;
        }

        public ViewMouseUp OnViewMouseUp
        {
            get;
            set;
        }

        public bool ShowAxies
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

        public bool ShowFloor
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

        public FrameworkElement Visual
        {
            get
            {
                return viewport3D1;
            }
        }

        public void Back_Click(object sender, RoutedEventArgs e)
        {
            BackCamera();
        }

        public void Bottom_Click(object sender, RoutedEventArgs e)
        {
            BottomCamera();
        }

        public void Busy()
        {
            BusyVisible = Visibility.Visible;
        }

        public void Center_Clicked(object sender, RoutedEventArgs e)
        {
            LookToCenter();
        }

        public void Clear()
        {
            Model = null;
        }

        public void Home_Click(object sender, RoutedEventArgs e)
        {
            HomeCamera();
        }

        public void Left_Click(object sender, RoutedEventArgs e)
        {
            LeftCamera();
        }

        public void LoadCamera()
        {
            string dataPath = PathManager.CommonAppDataFolder() + "\\" + cameraRecordFile;
            polarCamera.Read(dataPath);
            UpdateCameraPos();
            NotifyPropertyChanged("CameraPos");
        }

        public void NotBusy()
        {
            BusyVisible = Visibility.Hidden;
        }

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void Pan_Clicked(object sender, RoutedEventArgs e)
        {
            PanCamera();
        }

        public void Redisplay()
        {
            if (modelContent != null)
            {
                modelContent.Children.Clear();

                if (floor != null && ShowFloor)
                {
                    modelContent.Children.Add(floor.FloorMesh);
                    foreach (GeometryModel3D m in grid.Group.Children)
                    {
                        modelContent.Children.Add(m);
                    }
                }

                if (axies != null && ShowAxies)
                {
                    foreach (GeometryModel3D m in axies.Group.Children)
                    {
                        modelContent.Children.Add(m);
                    }
                }

                if (model != null)
                {
                    modelContent.Children.Add(model);
                }
                foreach (GeometryModel3D gmm in multiModels.Children)
                {
                    modelContent.Children.Add(gmm);
                }
                NotifyPropertyChanged("ModelContent");
            }
        }

        public void Right_Click(object sender, RoutedEventArgs e)
        {
            RightCamera();
        }

        public void Top_Click(object sender, RoutedEventArgs e)
        {
            TopCamera();
        }

        public void UpdateCameraPos()
        {
            switch (cameraMode)
            {
                case CameraModes.Normal:
                    {
                        lookDirection.X = -polarCamera.CameraPos.X;
                        lookDirection.Y = -polarCamera.CameraPos.Y;
                        lookDirection.Z = -polarCamera.CameraPos.Z;

                        CameraPosition = new Point3D(polarCamera.CameraPos.X, polarCamera.CameraPos.Y, polarCamera.CameraPos.Z);
                        LookDirection = new Vector3D(lookDirection.X, lookDirection.Y, lookDirection.Z);
                        NotifyPropertyChanged("LookDirection");
                        NotifyPropertyChanged("CameraPosition");
                    }
                    break;

                case CameraModes.Pan:
                    {
                        CameraPosition = new Point3D(polarCamera.CameraPos.X, polarCamera.CameraPos.Y, polarCamera.CameraPos.Z);
                        NotifyPropertyChanged("CameraPosition");
                    }
                    break;
            }
        }

        internal void Refresh()
        {
            Redisplay();
        }

        internal void SaveCamera()
        {
            string dataPath = PathManager.CommonAppDataFolder() + "\\" + cameraRecordFile;
            polarCamera.Save(dataPath);
        }

        private void BackCamera()
        {
            GetBounds(model);
            polarCamera.HomeBack();
            cameraMode = CameraModes.Normal;
            SetDistanceToFit();
            UpdateCameraPos();
        }

        private void BottomCamera()
        {
            GetBounds(model);
            polarCamera.HomeBottom();
            cameraMode = CameraModes.Normal;
            SetDistanceToFit();
            UpdateCameraPos();
        }

        private void GetBounds(GeometryModel3D model)
        {
            bounds = new Bounds3D();

            if (model != null)
            {
                MeshGeometry3D mesh = model.Geometry as MeshGeometry3D;
                if (mesh != null)
                {
                    foreach (Point3D p in mesh.Positions)
                    {
                        bounds.Adjust(p);
                    }
                }
            }
        }

        private bool HandleKeyDown(Key key, bool shift, bool ctrl, bool alt)
        {
            bool handled = false;

            switch (key)
            {
                case Key.Insert:
                    {
                        handled = true;
                        LeftCamera();
                    }
                    break;

                case Key.PageUp:
                    {
                        handled = true;
                        RightCamera();
                    }
                    break;

                case Key.Home:
                    {
                        handled = true;
                        if (shift)
                        {
                            BackCamera();
                        }
                        else
                        {
                            HomeCamera();
                        }
                    }
                    break;

                case Key.End:
                    {
                        handled = true;
                        BackCamera();
                    }
                    break;

                case Key.F5:
                    {
                        handled = true;
                        RotateCamera(-0.5, 0.0);
                    }
                    break;

                case Key.F6:
                    {
                        handled = true;
                        RotateCamera(0.5, 0.0);
                    }
                    break;

                case Key.F7:
                    {
                        handled = true;
                        RotateCamera(0.0, -0.5);
                    }
                    break;

                case Key.F8:
                    {
                        handled = true;
                        RotateCamera(0.0, 0.5);
                    }
                    break;

                case Key.F12:
                    {
                        handled = true;
                        if (shift)
                        {
                            LoadCamera();
                        }
                        else
                        {
                            SaveCamera();
                        }
                    }
                    break;
            }
            if (!handled)
            {
                if (OnPreviewUserKey != null)
                {
                    handled = OnPreviewUserKey(key, shift, ctrl, alt);
                }
            }
            return handled;
        }

        private void HomeCamera()
        {
            GetBounds(model);
            Camera.HomeFront();
            cameraMode = CameraModes.Normal;
            SetDistanceToFit();
            UpdateCameraPos();
        }

        private void LeftCamera()
        {
            GetBounds(model);
            polarCamera.HomeLeft();
            cameraMode = CameraModes.Normal;
            SetDistanceToFit(true);
            UpdateCameraPos();
        }

        private void LookToCenter()
        {
            lookDirection.X = -polarCamera.CameraPos.X;
            lookDirection.Y = -polarCamera.CameraPos.Y;
            lookDirection.Z = -polarCamera.CameraPos.Z;
            lookDirection.Normalize();
            NotifyPropertyChanged("LookDirection");
            cameraMode = CameraModes.Normal;
        }

        private void PanCamera()
        {
            GetBounds(model);
            cameraMode = CameraModes.Pan;
            UpdateCameraPos();
        }

        private void RightCamera()
        {
            GetBounds(model);
            polarCamera.HomeRight();
            cameraMode = CameraModes.Normal;
            SetDistanceToFit(true);
            UpdateCameraPos();
        }

        private void RotateCamera(double dt, double dp)
        {
            polarCamera.RotateDegrees(dt, dp);
            UpdateCameraPos();
        }

        private void SetDistanceToFit(bool sideView = false)
        {
            double l = bounds.Width;
            double h = bounds.Height;
            double d = bounds.Depth;

            if (sideView)
            {
                polarCamera.DistanceToFit(l, h, 30);
            }
            else
            {
                polarCamera.DistanceToFit(l, h, 30);
            }
        }

        private void TopCamera()
        {
            GetBounds(model);
            polarCamera.HomeTop();
            cameraMode = CameraModes.Normal;
            SetDistanceToFit();
            UpdateCameraPos();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            HomeCamera();
            BusyVisible = Visibility.Hidden;
            Redisplay();
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = HandleKeyDown(e.Key, Keyboard.IsKeyDown(Key.LeftShift) | Keyboard.IsKeyDown(Key.RightShift),
                                  Keyboard.IsKeyDown(Key.LeftCtrl) | Keyboard.IsKeyDown(Key.RightCtrl), Keyboard.IsKeyDown(Key.LeftAlt));
        }

        private void Viewport_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.Focus(this);
            bool handled = false;
            if (OnViewMouseDown != null)
            {
                handled = OnViewMouseDown(sender, e);
            }
            if (!handled)
            {
                Viewport3D vp = sender as Viewport3D;
                if (vp != null)
                {
                    if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed && e.Handled == false)
                    {
                        oldMousePos = e.GetPosition(vp);
                    }
                }
                else
                {
                    oldMousePos = new Point(double.NaN, double.NaN);
                }
            }
        }

        private void Viewport_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            bool handled = false;
            if (OnViewMouseMove != null)
            {
                handled = OnViewMouseMove(sender, e);
            }
            if (!handled)
            {
                Viewport3D vp = sender as Viewport3D;

                if (vp != null)
                {
                    if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed && e.Handled == false)
                    {
                        Point pn = e.GetPosition(vp);
                        if (!double.IsNaN(oldMousePos.X))
                        {
                            double dx = pn.X - oldMousePos.X;
                            double dy = pn.Y - oldMousePos.Y;
                            polarCamera.Move(dx, -dy);
                            UpdateCameraPos();
                        }
                        oldMousePos = pn;
                        e.Handled = true;
                    }
                }
            }
        }

        private void Viewport_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Keyboard.Focus(this);
            bool handled = false;
            if (OnViewMouseUp != null)
            {
                handled = OnViewMouseUp(sender, e);
            }
        }

        private void Viewport_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            Viewport3D vp = sender as Viewport3D;
            if (vp != null)
            {
                double diff = Math.Sign(e.Delta) * 1;
                polarCamera.Zoom(diff);
                UpdateCameraPos();
            }
        }
    }
}