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
using CSGLib;
using FileUtils;
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

namespace Barnacle.UserControls.VoxelGridControl
{
    /// <summary>
    /// Interaction logic for VoxelGrid.xaml
    /// </summary>
    public partial class PointGrid : UserControl, INotifyPropertyChanged
    {
        public UserKeyHandler OnPreviewUserKey;
        private const string cameraRecordFile = "pointcamerapos.txt";

        private Axies axies;

        private Bounds3D bounds;

        private Visibility busyVisible = Visibility.Hidden;

        private CameraModes cameraMode;

        private Point3D cameraPosition;

        private EdMode edMode;

        private double fieldOfView;

        private Floor floor;

        private Grid3D grid;

        private GeometryModel3D lastHitModel;

        private Point3D lastHitPoint;

        private int lastHitV0;

        private int lastHitV1;

        private int lastHitV2;

        private Vector3D lookDirection;

        // used if only one model is to be displayed
        private GeometryModel3D model;

        private Model3DGroup modelContent;

        // used if more that one model is to be displayed
        private Model3DGroup multiModels;

        private Point oldMousePos;

        private PolarCamera polarCamera;

        private int selectedVoxel;

        private bool showAxies;

        private bool showFloor;

        private List<Voxel> theVoxels;

        public PointGrid()
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
            theVoxels = new List<Voxel>();
            edMode = EdMode.None;
        }

        public delegate void PointsUpdated(Point3DCollection points);

        public delegate bool UserKeyHandler(Key key, bool shift, bool ctrl, bool alt);

        public delegate bool ViewMouseDown(object sender, MouseEventArgs e);

        public delegate bool ViewMouseMove(object sender, MouseEventArgs e);

        public delegate bool ViewMouseUp(object sender, MouseEventArgs e);

        public event PointsUpdated OnPointsUpdated;

        public event PropertyChangedEventHandler PropertyChanged;

        private enum CameraModes
        {
            Normal,
            Pan
        }

        private enum EdMode
        {
            None,
            AddVoxel,
            DeleteVoxel
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

        public void Display()
        {
            RegenerateVoxelDisplay();
            Refresh();
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

        public void Home_Click(object sender, RoutedEventArgs e)
        {
            HomeCamera();
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
                    lastHitV0 = rayMeshResult.VertexIndex1;
                    lastHitV1 = rayMeshResult.VertexIndex2;
                    lastHitV2 = rayMeshResult.VertexIndex3;
                    // if (lastHitModel == null)
                    {
                        lastHitModel = hitgeo;
                        lastHitPoint = rayMeshResult.PointHit;
                    }
                    result = HitTestResultBehavior.Stop;
                }
            }

            return result;
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

        internal static GeometryModel3D GetMesh(Object3D obj, bool selected)
        {
            GeometryModel3D gm = new GeometryModel3D();
            gm.Geometry = obj.Mesh;

            DiffuseMaterial mt = new DiffuseMaterial();
            Color c = Colors.Blue;
            if (selected)
            {
                c = Colors.Red;
            }
            mt.Color = c;
            mt.Brush = new SolidColorBrush(c);
            gm.Material = mt;
            return gm;
        }

        internal void ClearPoints()
        {
            theVoxels.Clear();
            RegenerateVoxelDisplay();
            Redisplay();
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

        internal void SetPoints(List<Point3D> sourcePoints)
        {
            theVoxels.Clear();
            foreach (Point3D point in sourcePoints)
            {
                AddVoxel(point.X, point.Y, point.Z, false);
            }
        }

        internal string VoxelText()
        {
            string pntsstr = "";
            foreach (Voxel vl in theVoxels)
            {
                Point3D p = vl.Position;
                pntsstr += $"{p.X},{p.Y},{p.Z}|";
            }
            return pntsstr;
        }

        private void AddVoxel(double x, double y, double z, bool regen = true)
        {
            Voxel vx = new Voxel();
            vx.Position.X = x;
            vx.Position.Y = y;
            vx.Position.Z = z;
            vx.ob = new Object3D();
            vx.Selected = false;
            vx.ob.BuildPrimitive("cube");
            vx.Geometry = GetMesh(vx.ob, vx.Selected);
            theVoxels.Add(vx);
            if (regen)
            {
                RegenerateVoxelDisplay();
                Refresh();
            }
        }

        private void AddVoxelButton_Click(object sender, RoutedEventArgs e)
        {
            edMode = EdMode.AddVoxel;
            EnableSelectionModeBorder(AddVoxelBorder);
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

        private void CentreVoxelButton_Click(object sender, RoutedEventArgs e)
        {
            double minx = double.MaxValue;
            double minz = double.MaxValue;
            double maxx = double.MinValue;
            double maxz = double.MinValue;
            foreach (Voxel v in theVoxels)
            {
                minx = Math.Min(minx, v.Position.X);
                minz = Math.Min(minz, v.Position.Z);
                maxx = Math.Max(maxx, v.Position.X);
                maxz = Math.Max(maxz, v.Position.Z);
            }
            double cx = (minx + maxx) / 2.0;
            double cz = (minz + maxz) / 2.0;

            for (int i = 0; i < theVoxels.Count; i++)
            {
                MoveVoxel(i, -cx, 0, -cz);
            }

            RegenerateVoxelDisplay();
            Refresh();
        }

        private void DelVoxelButton_Click(object sender, RoutedEventArgs e)
        {
            edMode = EdMode.DeleteVoxel;
            EnableSelectionModeBorder(DelVoxelBorder);
        }

        private void DoButtonBorder(Border src, Border trg)
        {
            if (src == trg)
            {
                trg.BorderBrush = System.Windows.Media.Brushes.CadetBlue;
            }
            else
            {
                trg.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
            }
        }

        private void EnableSelectionModeBorder(Border src)
        {
            DoButtonBorder(src, ViewVoxelBorder);
            DoButtonBorder(src, AddVoxelBorder);
            DoButtonBorder(src, DelVoxelBorder);
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

                case Key.F:
                    {
                        handled = true;
                        if (selectedVoxel != -1)
                        {
                            double y = theVoxels[selectedVoxel].Position.Y;
                            MoveVoxel(selectedVoxel, 0, -y + 0.5, 0);
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

                case Key.Left:
                    {
                        handled = true;
                        if (selectedVoxel != -1)
                        {
                            double d = -1;
                            if (shift)
                            {
                                d = -0.1;
                            }
                            MoveVoxel(selectedVoxel, d, 0, 0);
                        }
                    }
                    break;

                case Key.Right:
                    {
                        handled = true;
                        if (selectedVoxel != -1)
                        {
                            double d = 1;
                            if (shift)
                            {
                                d = 0.1;
                            }
                            MoveVoxel(selectedVoxel, d, 0, 0);
                        }
                    }
                    break;

                case Key.Up:
                    {
                        handled = true;
                        if (selectedVoxel != -1)
                        {
                            double d = 1;
                            if (shift)
                            {
                                d = 0.1;
                            }
                            if (ctrl)
                            {
                                MoveVoxel(selectedVoxel, 0, 0, -d);
                            }
                            else
                            {
                                MoveVoxel(selectedVoxel, 0, d, 0);
                            }
                        }
                    }
                    break;

                case Key.Down:
                    {
                        handled = true;
                        if (selectedVoxel != -1)
                        {
                            double d = -1;
                            if (shift)
                            {
                                d = -0.1;
                            }
                            if (ctrl)
                            {
                                MoveVoxel(selectedVoxel, 0, 0, -d);
                            }
                            else
                            {
                                MoveVoxel(selectedVoxel, 0, d, 0);
                            }
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

        private void MoveVoxel(int v, double v1, double v2, double v3)
        {
            if (v != -1)
            {
                Voxel vl = theVoxels[v];
                vl.Position = new Point3D(vl.Position.X + v1, vl.Position.Y + v2, vl.Position.Z + v3);
                vl.ob.Remesh();
                vl.Geometry = GetMesh(vl.ob, vl.Selected);
                theVoxels[v] = vl;
                RegenerateVoxelDisplay();
                Redisplay();
            }
        }

        private void NotifyPointsUpdated(Point3DCollection points)
        {
            OnPointsUpdated?.Invoke(points);
        }

        private void PanCamera()
        {
            GetBounds(model);
            cameraMode = CameraModes.Pan;
            UpdateCameraPos();
        }

        private void RegenerateVoxelDisplay()
        {
            Point3DCollection updatedPoints = new Point3DCollection();
            multiModels.Children.Clear();
            foreach (Voxel vx in theVoxels)
            {
                vx.ob.Position = vx.Position;
                vx.ob.Remesh();
                if (vx.Geometry != null)
                {
                    multiModels.Children.Add(vx.Geometry);
                    foreach (Point3D p in vx.ob.AbsoluteObjectVertices)
                    {
                        updatedPoints.Add(new Point3D(p.X, p.Y, p.Z));
                    }
                }
            }
            NotifyPointsUpdated(updatedPoints);
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

        private void SelectVoxel()
        {
            bool found = false;
            selectedVoxel = -1;
            for (int i = 0; i < theVoxels.Count; i++)
            {
                Voxel v = theVoxels[i];

                if (lastHitModel == v.Geometry)
                {
                    v.Selected = true;
                    v.Geometry = GetMesh(v.ob, true);
                    selectedVoxel = i;
                    found = true;
                }
                else
                {
                    v.Selected = false;
                    v.Geometry = GetMesh(v.ob, false);
                }
                theVoxels[i] = v;
            }
            if (edMode == EdMode.DeleteVoxel && selectedVoxel != -1)
            {
                theVoxels.RemoveAt(selectedVoxel);
                selectedVoxel = -1;
            }

            RegenerateVoxelDisplay();
            Refresh();
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
            EnableSelectionModeBorder(ViewVoxelBorder);
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
                            lastHitModel = null;
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
            if (!handled)
            {
                lastHitModel = null;
                lastHitPoint = new Point3D(0, 0, 0);
                Viewport3D vp = sender as Viewport3D;
                if (vp != null)
                {
                    HitTest(vp, oldMousePos);
                    if (edMode == EdMode.AddVoxel)
                    {
                        if (floor.Matches(lastHitModel) || grid.Matches(lastHitModel) || axies.Matches(lastHitModel))
                        {
                            AddVoxel(lastHitPoint.X, 0, lastHitPoint.Z);
                        }
                        else
                        {
                            SelectVoxel();
                        }
                    }
                    else
                    {
                        SelectVoxel();
                    }
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

        private void ViewVoxelButton_Click(object sender, RoutedEventArgs e)
        {
            edMode = EdMode.None;
            EnableSelectionModeBorder(ViewVoxelBorder);
        }

        private struct Voxel
        {
            public GeometryModel3D Geometry;
            public Object3D ob;
            public Point3D Position;
            public bool Selected;
        }
    }
}