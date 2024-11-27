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
        private const string cameraRecordFile = "viewcamerapos.txt";
        private Axies axies;

        private Visibility busyVisible = Visibility.Hidden;
        private Point3D cameraPosition;

        private double fieldOfView;

        private Floor floor;

        private Grid3D grid;

        private Vector3D lookDirection;

        private GeometryModel3D model;
        private Model3DGroup modelContent;

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

            LookDirection = new Vector3D(-polarCamera.CameraPos.X, -polarCamera.CameraPos.Y, -polarCamera.CameraPos.Z);
            FieldOfView = 45;
            floor = new Floor();
            grid = new Grid3D();
            axies = new Axies();
            showFloor = true;
            showAxies = true;
            modelContent = new Model3DGroup();
        }

        public event PropertyChangedEventHandler PropertyChanged;

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

        public void Back_Click(object sender, RoutedEventArgs e)
        {
            BackCamera();
        }

        public void Busy()
        {
            BusyVisible = Visibility.Visible;
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
            string dataPath = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Barnacle");
            dataPath += "\\" + cameraRecordFile;
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
                NotifyPropertyChanged("ModelContent");
            }
        }

        public void Right_Click(object sender, RoutedEventArgs e)
        {
            RightCamera();
        }

        public void UpdateCameraPos()
        {
            lookDirection.X = -polarCamera.CameraPos.X;
            lookDirection.Y = -polarCamera.CameraPos.Y;
            lookDirection.Z = -polarCamera.CameraPos.Z;

            CameraPosition = new Point3D(polarCamera.CameraPos.X, polarCamera.CameraPos.Y, polarCamera.CameraPos.Z);
            LookDirection = new Vector3D(lookDirection.X, lookDirection.Y, lookDirection.Z);
            NotifyPropertyChanged("LookDirection");
            NotifyPropertyChanged("CameraPosition");
        }

        internal void SaveCamera()
        {
            string dataPath = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Barnacle");
            dataPath += "\\" + cameraRecordFile;
            polarCamera.Save(dataPath);
        }

        private void BackCamera()
        {
            polarCamera.HomeBack();
            UpdateCameraPos();
            NotifyPropertyChanged("CameraPos");
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

            return handled;
        }

        private void HomeCamera()
        {
            Camera.HomeFront();
            UpdateCameraPos();
        }

        private void LeftCamera()
        {
            polarCamera.HomeLeft();
            UpdateCameraPos();
        }

        private void LookToCenter()
        {
            lookDirection.X = -polarCamera.CameraPos.X;
            lookDirection.Y = -polarCamera.CameraPos.Y;
            lookDirection.Z = -polarCamera.CameraPos.Z;
            lookDirection.Normalize();
            NotifyPropertyChanged("LookDirection");
        }

        private void RightCamera()
        {
            polarCamera.HomeRight();
            UpdateCameraPos();
        }

        private void RotateCamera(double dt, double dp)
        {
            polarCamera.RotateDegrees(dt, dp);
            UpdateCameraPos();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            HomeCamera();
            BusyVisible = Visibility.Hidden;
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = HandleKeyDown(e.Key, Keyboard.IsKeyDown(Key.LeftShift) | Keyboard.IsKeyDown(Key.RightShift),
                                  Keyboard.IsKeyDown(Key.LeftCtrl) | Keyboard.IsKeyDown(Key.RightCtrl), Keyboard.IsKeyDown(Key.LeftAlt));
        }

        private void Viewport_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.Focus(this);

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

        private void Viewport_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
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