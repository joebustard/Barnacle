﻿using Make3D.Dialogs.MeshEditor;
using Make3D.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Make3D.Dialogs
{
    /// <summary>
    /// Interaction logic for MeshEditor.xaml
    /// </summary>
    public partial class MeshEditorDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private Mesh mesh;

        // these are the points and indices of the shape we are editing.
        // We can't use the normal Mesh and Faces as these will be be used for the "soper Mesh"
        // of wire frame rects and selectable points.
        private Point3DCollection editingPoints;

        private Int32Collection selectedPointIndices;

        private Int32Collection editingFaceIndices;

        private List<Dialogs.MeshEditor.MeshTriangle> editingTriangles;

        private GeometryModel3D lastHitModel;
        private Point3D lastHitPoint;
        private bool showWireFrame;
        private MeshTriangle lastSelectedTriangle;
        private int lastSelectedPoint;
        private Point lastMousePos;

        public bool ShowWireFrame
        {
            get { return showWireFrame; }
            set
            {
                if (showWireFrame != value)
                {
                    showWireFrame = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private MeshGeometry3D wireFrameFaces;
        private MeshGeometry3D unselectedFaces;

        public MeshEditorDlg()
        {
            InitializeComponent();
            mesh = new Mesh();
            editingPoints = new Point3DCollection();
            selectedPointIndices = new Int32Collection();
            editingFaceIndices = new Int32Collection();

            DataContext = this;

            lastSelectedPoint = -1;
            lastSelectedTriangle = null;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            // we should have been loaded with an editing mesh.
            // If not create base one
            if (editingPoints.Count == 0)
            {
                GenerateCube(ref editingPoints, ref editingFaceIndices, 50);
            }
            CreateInitialMesh();

            Redisplay();
        }

        private void CreateInitialMesh()
        {
            for (int i = 0; i < editingPoints.Count; i++)
            {
                mesh.AddVertex(editingPoints[i]);
            }
            for (int i = 0; i < editingFaceIndices.Count; i += 3)
            {
                mesh.AddFace(editingFaceIndices[i], editingFaceIndices[i + 1], editingFaceIndices[i + 2]);
            }
            mesh.FindNeighbours();
        }

        private void Redisplay()
        {
            if (MyModelGroup != null)
            {
                MyModelGroup.Children.Clear();

                if (floor != null && ShowFloor)
                {
                    MyModelGroup.Children.Add(floor.FloorMesh);
                    foreach (GeometryModel3D m in grid.Group.Children)
                    {
                        MyModelGroup.Children.Add(m);
                    }
                }

                if (axies != null && ShowAxies)
                {
                    foreach (GeometryModel3D m in axies.Group.Children)
                    {
                        MyModelGroup.Children.Add(m);
                    }
                }
                MyModelGroup.Children.Add(mesh.GetModels());
            }
        }

        private void CreateWireFrame()
        {
        }

        internal void SetInitialMesh(Point3DCollection points, Int32Collection triangleIndices)
        {
            editingPoints.Clear();
            editingFaceIndices.Clear();
            foreach (Point3D p in points)
            {
                Point3D np = new Point3D(p.X, p.Y, p.Z);
                editingPoints.Add(np);
            }
            foreach (int i in triangleIndices)
            {
                editingFaceIndices.Add(i);
            }
        }

        internal void GenerateCube(ref Point3DCollection pnts, ref Int32Collection indices, double width)
        {
            // this is not the normal cube.
            // it has a lot of sub triangles to allow editing
            double numDiv = 10;
            double div = width / numDiv;
            List<Point3D> perimeter = new List<Point3D>();
            pnts.Clear();
            indices.Clear();
            width = width / 2;
            double x;
            double y;
            double z;
            // 4 sides of perimeter
            for (int i = 0; i < numDiv; i++)
            {
                x = -width + (i * div);
                y = 0;
                z = width;
                perimeter.Add(new Point3D(x, y, z));
            }
            for (int i = 0; i < numDiv; i++)
            {
                x = width;
                y = 0;
                z = width - (i * div);
                perimeter.Add(new Point3D(x, y, z));
            }
            for (int i = 0; i < numDiv; i++)
            {
                x = width - (i * div);
                y = 0;
                z = -width;
                perimeter.Add(new Point3D(x, y, z));
            }
            for (int i = 0; i < numDiv; i++)
            {
                x = -width;
                y = 0;
                z = -width + (i * div);
                perimeter.Add(new Point3D(x, y, z));
            }

            // loft
            for (int i = 0; i < numDiv; i++)
            {
                for (int j = 0; j < perimeter.Count; j++)
                {
                    int k = j + 1;
                    if (k >= perimeter.Count)
                    {
                        k = 0;
                    }
                    Point3D p0 = new Point3D(perimeter[j].X, i * div, perimeter[j].Z);
                    Point3D p1 = new Point3D(perimeter[j].X, (i + 1) * div, perimeter[j].Z);
                    Point3D p2 = new Point3D(perimeter[k].X, (i + 1) * div, perimeter[k].Z);
                    Point3D p3 = new Point3D(perimeter[k].X, i * div, perimeter[k].Z);

                    int v0 = AddPoint(pnts, p0);
                    int v1 = AddPoint(pnts, p1);
                    int v2 = AddPoint(pnts, p2);
                    int v3 = AddPoint(pnts, p3);

                    indices.Add(v0);
                    indices.Add(v2);
                    indices.Add(v1);

                    indices.Add(v0);
                    indices.Add(v3);
                    indices.Add(v2);
                }
            }

            // bottom
            double x2;
            double z2;
            for (int i = 0; i < numDiv; i++)
            {
                x = -width + (i * div);
                x2 = x + div;
                for (int j = 0; j < numDiv; j++)
                {
                    y = 0;
                    z = -width + (j * div);
                    z2 = z + div;

                    Point3D p0 = new Point3D(x, y, z);
                    Point3D p1 = new Point3D(x, y, z2);
                    Point3D p2 = new Point3D(x2, y, z2);
                    Point3D p3 = new Point3D(x2, y, z);

                    int v0 = AddPoint(pnts, p0);
                    int v1 = AddPoint(pnts, p1);
                    int v2 = AddPoint(pnts, p2);
                    int v3 = AddPoint(pnts, p3);

                    indices.Add(v0);
                    indices.Add(v2);
                    indices.Add(v1);

                    indices.Add(v0);
                    indices.Add(v3);
                    indices.Add(v2);
                }
            }

            // top

            for (int i = 0; i < numDiv; i++)
            {
                x = -width + (i * div);
                x2 = x + div;
                for (int j = 0; j < numDiv; j++)
                {
                    y = 2 * width;
                    z = -width + (j * div);
                    z2 = z + div;

                    Point3D p0 = new Point3D(x, y, z);
                    Point3D p1 = new Point3D(x, y, z2);
                    Point3D p2 = new Point3D(x2, y, z2);
                    Point3D p3 = new Point3D(x2, y, z);

                    int v0 = AddPoint(pnts, p0);
                    int v1 = AddPoint(pnts, p1);
                    int v2 = AddPoint(pnts, p2);
                    int v3 = AddPoint(pnts, p3);

                    indices.Add(v0);
                    indices.Add(v1);
                    indices.Add(v2);

                    indices.Add(v0);
                    indices.Add(v2);
                    indices.Add(v3);
                }
            }
        }

        private int AddPoint(Point3DCollection positions, Point3D v)
        {
            int res = -1;
            for (int i = 0; i < positions.Count; i++)
            {
                if (PointUtils.equals(positions[i], v.X, v.Y, v.Z))
                {
                    res = i;
                    break;
                }
            }

            if (res == -1)
            {
                positions.Add(new Point3D(v.X, v.Y, v.Z));
                res = positions.Count - 1;
            }
            return res;
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            bool leftButton = (e.LeftButton == MouseButtonState.Pressed);
            if (leftButton)
            {
                lastMousePos = e.GetPosition(viewport3D1);
            }
            lastHitModel = null;
            lastHitPoint = new Point3D(0, 0, 0);
            HitTest(sender, e);
            bool shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            if (lastHitModel != null)
            {
                if (mesh.CheckHit(lastHitModel, shift, ref lastSelectedPoint, ref lastSelectedTriangle))
                {
                }
                else
                {
                    base.Viewport_MouseDown(viewport3D1, e);
                }
            }
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            lastSelectedPoint = -1;
            lastSelectedTriangle = null;
            e.Handled = true;
        }

        private void Grid_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point newPos = e.GetPosition(viewport3D1);
                bool ctrlDown = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
                if (lastSelectedPoint != -1)
                {
                    MouseMoveControlPoint(lastSelectedPoint, lastMousePos, newPos, ctrlDown);
                    lastMousePos = newPos;
                    e.Handled = true;
                }
                else if (lastSelectedTriangle != null)
                {
                    // although we are using lastSelectedTriangle to see if a triangle is selected
                    // in practice we move all selected triangles at the same time

                    MouseMoveSelectedTriangle(lastMousePos, newPos, ctrlDown);

                    lastMousePos = newPos;
                    e.Handled = true;
                }
                else
                {
                    base.Viewport_MouseMove(viewport3D1, e);
                    lastMousePos = newPos;
                }
            }
        }

        private void MouseMoveSelectedTriangle(Point lastPos, Point newPos, bool ctrlDown)
        {
            double dr = Math.Sqrt(Camera.Distance);
            double deltaX = (newPos.X - lastPos.X) / dr;

            double deltaY;
            double deltaZ;

            if (!ctrlDown)
            {
                deltaY = -(newPos.Y - lastPos.Y) / dr;
                deltaZ = 0;
            }
            else
            {
                deltaY = 0;
                deltaZ = -(newPos.Y - lastPos.Y) / dr;
            }

            Point3D positionChange = new Point3D(0, 0, 0);
            PolarCamera.Orientations ori = Camera.Orientation;
            switch (ori)
            {
                case PolarCamera.Orientations.Front:
                    {
                        positionChange = new Point3D(1 * deltaX, 1 * deltaY, -1 * deltaZ);
                    }
                    break;

                case PolarCamera.Orientations.Back:
                    {
                        positionChange = new Point3D(-1 * deltaX, 1 * deltaY, 1 * deltaZ);
                    }
                    break;

                case PolarCamera.Orientations.Left:
                    {
                        positionChange = new Point3D(1 * deltaZ, 1 * deltaY, 1 * deltaX);
                    }
                    break;

                case PolarCamera.Orientations.Right:
                    {
                        positionChange = new Point3D(-1 * deltaZ, 1 * deltaY, -1 * deltaX);
                    }
                    break;
            }

            if (positionChange != null)
            {
                mesh.MoveSelectedTriangles(positionChange);
            }
            Redisplay();
        }

        private void MouseMoveTriangle(MeshTriangle tri, Point lastPos, Point newPos, bool ctrlDown)
        {
            Int32Collection movingPnts = new Int32Collection();
            movingPnts.Add(tri.P0);
            movingPnts.Add(tri.P1);
            movingPnts.Add(tri.P2);
            MouseMoveTrianglePoints(movingPnts, lastPos, newPos, ctrlDown);
        }

        private void MouseMoveTrianglePoints(Int32Collection movingPnts, Point lastPos, Point newPos, bool ctrlDown)
        {
            double dr = Math.Sqrt(Camera.Distance);
            double deltaX = (newPos.X - lastPos.X) / dr;

            double deltaY;
            double deltaZ;

            if (!ctrlDown)
            {
                deltaY = -(newPos.Y - lastPos.Y) / dr;
                deltaZ = 0;
            }
            else
            {
                deltaY = 0;
                deltaZ = -(newPos.Y - lastPos.Y) / dr;
            }
            foreach (int pindex in movingPnts)
            {
                MovePoint(pindex, deltaX, deltaY, deltaZ);
            }

            Redisplay();
        }

        private void MouseMoveControlPoint(int pindex, Point lastPos, Point newPos, bool ctrlDown)
        {
            double dr = Math.Sqrt(Camera.Distance);
            double deltaX = (newPos.X - lastPos.X) / dr;

            double deltaY;
            double deltaZ;

            if (!ctrlDown)
            {
                deltaY = -(newPos.Y - lastPos.Y) / dr;
                deltaZ = 0;
            }
            else
            {
                deltaY = 0;
                deltaZ = -(newPos.Y - lastPos.Y) / dr;
            }

            MovePoint(pindex, deltaX, deltaY, deltaZ);

            //Redisplay();
        }

        private void MovePoint(int pindex, double deltaX, double deltaY, double deltaZ)
        {
            Point3D positionChange = new Point3D(0, 0, 0);
            PolarCamera.Orientations ori = Camera.Orientation;
            switch (ori)
            {
                case PolarCamera.Orientations.Front:
                    {
                        positionChange = new Point3D(1 * deltaX, 1 * deltaY, -1 * deltaZ);
                    }
                    break;

                case PolarCamera.Orientations.Back:
                    {
                        positionChange = new Point3D(-1 * deltaX, 1 * deltaY, 1 * deltaZ);
                    }
                    break;

                case PolarCamera.Orientations.Left:
                    {
                        positionChange = new Point3D(1 * deltaZ, 1 * deltaY, 1 * deltaX);
                    }
                    break;

                case PolarCamera.Orientations.Right:
                    {
                        positionChange = new Point3D(-1 * deltaZ, 1 * deltaY, -1 * deltaX);
                    }
                    break;
            }

            if (positionChange != null)
            {
                mesh.MovePoint(pindex, positionChange);
            }
        }

        public void HitTest(object sender, System.Windows.Input.MouseButtonEventArgs args)
        {
            Point mouseposition = args.GetPosition(viewport3D1);
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
                        // UpdateResultInfo(rayMeshResult);
                        lastHitModel = hitgeo;
                        lastHitPoint = rayMeshResult.PointHit;
                    }
                    result = HitTestResultBehavior.Stop;
                }
            }

            return result;
        }

        private void ClearSelection_Click(object sender, RoutedEventArgs e)
        {
            mesh.SelectAll(false);
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            mesh.SelectAll(true);
        }

        private void Divide_Click(object sender, RoutedEventArgs e)
        {
            mesh.DivideSelectedFaces();
            //     GenerateSuperMesh();
            Redisplay();
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            Vertices.Clear();
            Faces.Clear();
            mesh.Export(Vertices, Faces);
            mesh.Clear();
            DialogResult = true;
            Close();
        }
    }
}