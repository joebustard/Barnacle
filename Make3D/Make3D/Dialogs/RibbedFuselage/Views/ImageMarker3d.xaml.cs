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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Barnacle.Dialogs.RibbedFuselage.Models;
using Barnacle.LineLib;
using System.Collections.ObjectModel;

namespace Barnacle.Dialogs.RibbedFuselage.Views
{
    /// <summary>
    /// Interaction logic for ImageMarker3d.xaml
    /// </summary>
    public partial class ImageMarker3d : UserControl, INotifyPropertyChanged
    {
        private Point3D cameraPosition;

        private double fieldOfView;

        private GeometryModel3D lastHitModel;

        private Point3D lastHitPoint;
        private Floor floor;
        private Vector3D lookDirection;

        private System.Windows.Media.Color meshColour;

        private PolarCamera polarCamera;

        private PlateModel topPlateModel;
        private SidePlateModel sidePlateModel;
        private List<RibPlateModel> ribPlates;

        public ImageMarker3d()
        {
            InitializeComponent();
            DataContext = this;
            polarCamera = new PolarCamera(100);
            polarCamera.HomeFront();
            floor = new Floor();
            LookDirection = new Vector3D(-polarCamera.CameraPos.X, -polarCamera.CameraPos.Y, -polarCamera.CameraPos.Z);
            FieldOfView = 45;
            meshColour = Colors.Gainsboro;
            topPlateModel = new PlateModel();
            topPlateModel.PointOrientation = PlateModel.Orientation.Top;
            topPlateModel.MeshColour = Colors.LightBlue;
            sidePlateModel = new SidePlateModel();
            sidePlateModel.PointOrientation = PlateModel.Orientation.Side;
            sidePlateModel.MeshColour = Colors.LightGreen;
            ribPlates = new List<RibPlateModel>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

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

        public System.Windows.Media.Color MeshColour
        {
            get
            {
                return meshColour;
            }

            set
            {
                meshColour = value;
            }
        }

        public void SetTopPoints()
        {
        }

        public Model3DGroup ModelGroup { get; set; }

        public void HitTest(Viewport3D viewport3D1, Point mouseposition)
        {
            Point3D testpoint3D = new Point3D(mouseposition.X, mouseposition.Y, 0);
            Vector3D testdirection = new Vector3D(mouseposition.X, mouseposition.Y, 10);
            PointHitTestParameters pointparams = new PointHitTestParameters(mouseposition);
            RayHitTestParameters rayparams = new RayHitTestParameters(testpoint3D, testdirection);

            //test for a result in the Viewport3D
            VisualTreeHelper.HitTest(viewport3D1, null, HTResult, pointparams);
        }

        public HitTestResultBehavior HTResult(System.Windows.Media.HitTestResult rawresult)
        {
            HitTestResultBehavior result = HitTestResultBehavior.Continue;
            RayHitTestResult rayResult = rawresult as RayHitTestResult;

            if (rayResult != null)
            {
                RayMeshGeometry3DHitTestResult rayMeshResult = rayResult as RayMeshGeometry3DHitTestResult;

                if (rayMeshResult != null)
                {
                    GeometryModel3D hitgeo = rayMeshResult.ModelHit as GeometryModel3D;
                    if (lastHitModel == null)
                    {
                        lastHitModel = hitgeo;
                        lastHitPoint = rayMeshResult.PointHit;
                    }
                    result = HitTestResultBehavior.Stop;
                }
            }

            return result;
        }

        public void Log(string s)
        {
            System.Diagnostics.Debug.WriteLine(s);
        }

        public virtual void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        internal void SetSidePath(string pth)
        {
            FlexiPath flexiPath = new FlexiPath();
            flexiPath.FromString(pth);
            sidePlateModel.SetPoints(flexiPath.DisplayPointsF());
            topPlateModel.SetYOffset(sidePlateModel.MiddleOffset);
        }

        public void UpdateCameraPos()
        {
            lookDirection.X = -polarCamera.CameraPos.X;
            lookDirection.Y = -polarCamera.CameraPos.Y;
            lookDirection.Z = -polarCamera.CameraPos.Z;
            //lookDirection.Normalize();
            CameraPosition = new Point3D(polarCamera.CameraPos.X, polarCamera.CameraPos.Y, polarCamera.CameraPos.Z);
            LookDirection = new Vector3D(lookDirection.X, lookDirection.Y, lookDirection.Z);
            NotifyPropertyChanged("LookDirection");
            NotifyPropertyChanged("CameraPosition");
        }

        public void Redisplay()
        {
            if (ImageMarkerModelGroup != null)
            {
                ImageMarkerModelGroup.Children.Clear();
                if (floor != null)
                {
                    ImageMarkerModelGroup.Children.Add(floor.FloorMesh);
                }

                /*
                                if (floor != null && ShowFloor)
                                {
                                }

                                if (axies != null && ShowAxies)
                                {
                                    foreach (GeometryModel3D m in axies.Group.Children)
                                    {
                                        ModelGroup.Children.Add(m);
                                    }
                                }
                */

                ImageMarkerModelGroup.Children.Add(topPlateModel.GetModel());
                ImageMarkerModelGroup.Children.Add(sidePlateModel.GetModel());
                foreach (RibPlateModel pm in ribPlates)
                {
                    ImageMarkerModelGroup.Children.Add(pm.GetModel());
                }
                NotifyPropertyChanged();
            }
        }

        internal void SetTopPath(string pth)
        {
            FlexiPath flexiPath = new FlexiPath();
            flexiPath.FromString(pth);
            topPlateModel.SetPoints(flexiPath.DisplayPointsF());
        }

        internal void AddRibPath(string pth)
        {
            RibPlateModel rpm = new RibPlateModel();
            FlexiPath flexiPath = new FlexiPath();
            flexiPath.FromString(pth);
            rpm.SetPoints(flexiPath.DisplayPointsF());
            ribPlates.Add(rpm);
        }

        protected void SetCameraDistance(bool sideView = false)
        {
            Point3D min = new Point3D(double.MaxValue, double.MaxValue, double.MaxValue);
            Point3D max = new Point3D(double.MinValue, double.MinValue, double.MinValue);
            PointUtils.MinMax(topPlateModel.Vertices, ref min, ref max);
            double w = max.X - min.X;
            double h = max.Y - min.Y;
            double d = max.Z - min.Z;
            if (sideView)
            {
                Camera.DistanceToFit(d, h, w * 1.5);
            }
            else
            {
                Camera.DistanceToFit(w, h, d * 1.5);
            }
        }

        internal void SetRibPosition(int i, double x, double lower1, double upper1, double lower2, double upper2)
        {
            if (i >= 0 && i < ribPlates.Count)
            {
                ribPlates[i].SetPositionAndScale(x + sidePlateModel.LeftOffset, sidePlateModel.MiddleOffset, lower1, upper1, lower2, upper2);
            }
        }

        internal void MoveRibsUp(double minY)
        {
            Vector3D v = new Vector3D(0, minY + sidePlateModel.MiddleOffset, 0);
            for (int i = 0; i < ribPlates.Count; i++)
            {
                for (int j = 0; j < ribPlates[i].Vertices.Count; j++)
                {
                    ribPlates[i].Vertices[j] += v;
                }
            }
        }

        protected virtual void Viewport_MouseDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Viewport3D vp = sender as Viewport3D;
            if (vp != null)
            {
                if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed && e.Handled == false)
                {
                    oldMousePos = e.GetPosition(vp);
                }
            }
        }

        private Point oldMousePos;

        protected virtual void Viewport_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Viewport3D vp = sender as Viewport3D;
            if (vp != null)
            {
                if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed && e.Handled == false)
                {
                    Point pn = e.GetPosition(vp);
                    double dx = pn.X - oldMousePos.X;
                    double dy = pn.Y - oldMousePos.Y;
                    polarCamera.Move(dx, -dy);
                    UpdateCameraPos();
                    oldMousePos = pn;
                    e.Handled = true;
                }
            }
        }

        protected virtual void Viewport_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            Viewport3D vp = sender as Viewport3D;
            if (vp != null)
            {
                double diff = Math.Sign(e.Delta) * 1;
                polarCamera.Zoom(diff);
                UpdateCameraPos();
            }
        }

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            /*
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (selectedMarker != null)
                {
                    Br_MouseMove(sender, e);
                }
                else
                if (pinSelected)
                {
                    Ply_MouseMove(sender, e);
                }
            }
            */
        }

        private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            /*
            Br_MouseUp(sender, e);

            Ply_MouseUp(sender, e);
            */
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateCameraPos();
            Redisplay();
        }

        public List<LetterMarker> markers;
        private bool convertMarkerPositionToScreen;

        public List<LetterMarker> Markers

        {
            get
            {
                return markers;
            }

            set
            {
                if (markers != value)
                {
                    markers = value;
                    convertMarkerPositionToScreen = true;
                    if (markers != null)
                    {
                    }
                }
            }
        }

        internal void SetRibs(ObservableCollection<RibImageDetailsModel> ribs)
        {
            ribPlates.Clear();
            foreach (RibImageDetailsModel rdm in ribs)
            {
                if (rdm.ProfilePoints == null)
                {
                    rdm.GenerateProfilePoints();
                }
                RibPlateModel plateModel = new RibPlateModel();
                plateModel.MeshColour = Colors.OrangeRed;
                plateModel.SetPoints(rdm.ProfilePoints);
                ribPlates.Add(plateModel);
            }
        }
    }
}