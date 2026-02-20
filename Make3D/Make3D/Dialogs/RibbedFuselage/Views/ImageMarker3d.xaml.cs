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
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
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
        public List<LetterMarker> markers;
        private Point3D cameraPosition;

        private bool convertMarkerPositionToScreen;
        private double fieldOfView;

        private Floor floor;
        private GeometryModel3D lastHitModel;

        private Point3D lastHitPoint;
        private Vector3D lookDirection;

        private System.Windows.Media.Color meshColour;

        private Point oldMousePos;
        private PolarCamera polarCamera;

        private List<RibPlateModel> ribPlates;
        private SidePlateModel sidePlateModel;
        private PlateModel topPlateModel;

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

        public Model3DGroup ModelGroup
        {
            get; set;
        }

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

        public void Redisplay()
        {
            if (ImageMarkerModelGroup != null)
            {
                ImageMarkerModelGroup.Children.Clear();
                if (floor != null)
                {
                    ImageMarkerModelGroup.Children.Add(floor.FloorMesh);
                }

                ImageMarkerModelGroup.Children.Add(topPlateModel.GetModel());
                ImageMarkerModelGroup.Children.Add(sidePlateModel.GetModel());
                foreach (RibPlateModel pm in ribPlates)
                {
                    ImageMarkerModelGroup.Children.Add(pm.GetModel());
                }
                NotifyPropertyChanged();
            }
        }

        public void SetTopPoints()
        {
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

        internal void AddRibPath(string pth)
        {
            RibPlateModel rpm = new RibPlateModel();
            FlexiPath flexiPath = new FlexiPath();
            flexiPath.FromString(pth);
            rpm.SetPoints(flexiPath.DisplayPointsF());
            ribPlates.Add(rpm);
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

        internal void SetGeneratingRibs(List<RibImageDetailsModel> ribs, List<double> ribXs, List<Dimension> topDims, List<Dimension> sideDims)
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

            for (int i = 0; i < ribPlates.Count; i++)
            {
                SetRibPosition(i, ribXs[i] - ribXs[0], topDims[i].P1.Y, topDims[i].P2.Y, sideDims[i].P1.Y, sideDims[i].P2.Y);
            }
            MoveRibsUp(sideDims[0].P1.Y);
        }

        internal void SetRibPosition(int i, double x, double topMin, double topMax, double sideMin, double sideMax)
        {
            if (i >= 0 && i < ribPlates.Count)
            {
                ribPlates[i].SetPositionAndScale(x + sidePlateModel.LeftOffset, sidePlateModel.MiddleOffset, topMin, topMax, sideMin, sideMax);
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

        internal void SetSidePath(string pth)
        {
            FlexiPath flexiPath = new FlexiPath();
            flexiPath.FromString(pth);
            sidePlateModel.SetPoints(flexiPath.DisplayPointsF());
            topPlateModel.SetYOffset(sidePlateModel.MiddleOffset);
        }

        internal void SetTopPath(string pth)
        {
            FlexiPath flexiPath = new FlexiPath();
            flexiPath.FromString(pth);
            topPlateModel.SetPoints(flexiPath.DisplayPointsF());
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
        }

        private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateCameraPos();
            Redisplay();
        }
    }
}