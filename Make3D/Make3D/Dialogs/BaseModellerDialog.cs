﻿// **************************************************************************
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

using Barnacle.EditorParameterLib;
using Barnacle.Models;
using Barnacle.Object3DLib;
using HalfEdgeLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace Barnacle.Dialogs
{
    public partial class BaseModellerDialog : Window, INotifyPropertyChanged
    {
        protected Axies axies;
        protected string defaultImagePath;
        protected bool editingEnabled;
        protected Floor floor;
        protected Grid3D grid;
        protected GeometryModel3D lastHitModel;
        protected Point3D lastHitPoint;
        protected int lastHitV0;
        protected int lastHitV1;
        protected int lastHitV2;
        protected System.Windows.Media.Color meshColour;
        protected Point oldMousePos;
        protected bool showAxies;
        protected bool showFloor;
        private Bounds3D bounds;
        private Visibility busyVisible;
        private Point3D cameraPosition;
        private EditorParameters editorParameters;

        private double fieldOfView;

        private Vector3D lookDirection;

        private PolarCamera polarCamera;

        private SpaceTreeNode spaceTreeRoot;
        private Int32Collection tris;

        private Point3DCollection vertices;

        public BaseModellerDialog()
        {
            vertices = new Point3DCollection();
            tris = new Int32Collection();
            polarCamera = new PolarCamera(100);
            polarCamera.HomeFront();

            LookDirection = new Vector3D(-polarCamera.CameraPos.X, -polarCamera.CameraPos.Y, -polarCamera.CameraPos.Z);
            FieldOfView = 45;
            meshColour = Colors.Gainsboro;
            editorParameters = new EditorParameters();
            floor = new Floor();
            grid = new Grid3D();
            axies = new Axies();
            showFloor = true;
            showAxies = true;
            bounds = new Bounds3D();
            spaceTreeRoot = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Bounds3D Bounds
        {
            get
            {
                return bounds;
            }

            set
            {
                if (bounds != value)
                {
                    bounds = value;
                }
            }
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

        public string DefaultImagePath
        {
            get
            {
                return defaultImagePath;
            }

            set
            {
                defaultImagePath = value;
            }
        }

        public bool EditingEnabled
        {
            get
            {
                return editingEnabled;
            }
            set
            {
                if (editingEnabled != value)
                {
                    editingEnabled = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public EditorParameters EditorParameters
        {
            get
            {
                return editorParameters;
            }

            set
            {
                editorParameters = value;
            }
        }

        public Int32Collection Faces
        {
            get
            {
                return tris;
            }

            set
            {
                tris = value;
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

        public Model3DGroup ModelGroup
        {
            get; set;
        }

        public virtual bool ShowAxies
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
                }
            }
        }

        public virtual bool ShowFloor
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
                }
            }
        }

        public string ToolName
        {
            get
            {
                if (editorParameters != null)
                {
                    return editorParameters.ToolName;
                }
                else
                {
                    return "";
                }
            }

            set
            {
                if (editorParameters != null)
                {
                    editorParameters.ToolName = value;
                }
            }
        }

        public Point3DCollection Vertices
        {
            get
            {
                return vertices;
            }

            set
            {
                vertices = value;
            }
        }

        public static void CreateCanvasGrid(Canvas cnv, out double gridX, out double gridY, double gridSizeMM, List<Shape> markers)
        {
            DpiScale sc = VisualTreeHelper.GetDpi(cnv);

            double aw = cnv.ActualWidth;
            double ah = cnv.ActualHeight;
            CreateGrid(sc, aw, ah, out gridX, out gridY, gridSizeMM, markers);
        }

        public static void CreateGrid(DpiScale sc, double aw, double ah, out double gridX, out double gridY, double gridSizeMM, List<Shape> markers)
        {
            double x = 0;
            double y = 0;
            gridX = (sc.PixelsPerInchX / 25.4) * gridSizeMM;

            gridY = (sc.PixelsPerInchY / 25.4) * gridSizeMM;

            while (x < aw)
            {
                y = 0;
                while (y < ah)
                {
                    Ellipse el = new Ellipse();
                    Canvas.SetLeft(el, x - 1);
                    Canvas.SetTop(el, y - 1);
                    el.Width = 3;
                    el.Height = 3;
                    el.Fill = Brushes.AliceBlue;
                    el.Stroke = Brushes.CadetBlue;
                    if (markers != null)
                    {
                        markers.Add(el);
                    }
                    y += gridY;
                }
                x += gridX;
            }
        }

        public static Point3D SpherePoint(Point3D center, double r, double theta, double phi)
        {
            double y = r * Math.Cos(phi);
            double h = r * Math.Sin(phi);
            double x = h * Math.Sin(theta);
            double z = h * Math.Cos(theta);
            return center + new Vector3D(x, y, z);
        }

        public void AddFace(int c0, int c1, int c2)
        {
            Faces.Add(c0);
            Faces.Add(c1);
            Faces.Add(c2);
        }

        public int AddVertice(double x, double y, double z)
        {
            int res = AddVertice(new Point3D(x, y, z));
            return res;
        }

        /// <summary>
        /// Turn on the twirlywoo and stop any controls being changed
        /// </summary>
        public virtual void Busy()
        {
            EditingEnabled = false;
            BusyVisible = Visibility.Visible;
        }

        public void ClearShape()
        {
            spaceTreeRoot = null;
            Vertices.Clear();
            Faces.Clear();
        }

        public void Debug(string txt, [CallerMemberName] string caller = "")
        {
            System.Diagnostics.Debug.WriteLine($" {caller}: {txt}");
        }

        public void GenerateSphere(Point3DCollection verts, Int32Collection faces, Point3D center, double radius, int numTheta, int numPhi)
        {
            // Generate the points.
            double dtheta = 2 * Math.PI / numTheta;
            double dphi = Math.PI / numPhi;
            double theta = 0;
            for (int t = 0; t < numTheta; t++)
            {
                double phi = 0;
                for (int p = 0; p < numPhi; p++)
                {
                    // Find this piece's points.
                    Point3D[] points =
                    {
                        SpherePoint(center, radius, theta, phi),
                        SpherePoint(center, radius, theta, phi + dphi),
                        SpherePoint(center, radius, theta + dtheta, phi + dphi),
                        SpherePoint(center, radius, theta + dtheta, phi),
                    };

                    int c0 = AddVertice(verts, points[0].X, points[0].Y, points[0].Z);
                    int c1 = AddVertice(verts, points[1].X, points[1].Y, points[1].Z);
                    int c2 = AddVertice(verts, points[2].X, points[2].Y, points[2].Z);
                    int c3 = AddVertice(verts, points[3].X, points[3].Y, points[3].Z);

                    faces.Add(c0);
                    faces.Add(c1);
                    faces.Add(c2);

                    faces.Add(c0);
                    faces.Add(c2);
                    faces.Add(c3);

                    phi += dphi;
                }
                theta += dtheta;
            }
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

        public void Log(string s)
        {
            System.Diagnostics.Debug.WriteLine(s);
        }

        /// <summary>
        /// Turn off the twirlywoo and allow any controls being changed
        /// </summary>
        public virtual void NotBusy()
        {
            BusyVisible = Visibility.Hidden;
            EditingEnabled = true;
        }

        public virtual void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public Point[] OrderAntiClockwise(Point[] points)
        {
            double mX = 0;
            double my = 0;
            foreach (Point p in points)
            {
                mX += p.X;
                my += p.Y;
            }
            mX /= points.Length;
            my /= points.Length;
            return points.OrderByDescending(v => Math.Atan2(v.Y - my, v.X - mX)).ToArray();
        }

        public Point[] OrderClockwise(Point[] points)
        {
            double mX = 0;
            double my = 0;
            foreach (Point p in points)
            {
                mX += p.X;
                my += p.Y;
            }
            mX /= points.Length;
            my /= points.Length;
            return points.OrderBy(v => Math.Atan2(v.Y - my, v.X - mX)).ToArray();
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

        internal static double Distance3D(Point3D p1, Point3D p2)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double dz = p2.Z - p1.Z;
            return Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
        }

        internal int AddPoint(Point3DCollection positions, Point3D v)
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

        internal void GenerateCube(ref Point3DCollection pnts, ref Int32Collection indices, double width)
        {
            // this is not the normal cube. it has a lot of sub triangles to allow editing
            double numDiv = 20;
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
                    // indices.Add(v1);
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

        internal void SaveSizeAndLocation()
        {
            this.SaveSizeAndLocation(true);
        }

        internal void SweepPolarProfilePhi(List<PolarCoordinate> polarProfile, double cx, double cy, double sweepRange, int numSegs, bool clear = true)
        {
            // now we have a lovely copy of the profile in polar coordinates.
            if (clear)
            {
                ClearShape();
            }

            double sweep = sweepRange * (Math.PI * 2.0) / 360.0;
            double da = sweep / (numSegs - 1);
            for (int i = 0; i < numSegs; i++)
            {
                double a = da * i;
                int j = i + 1;
                if (j == numSegs)
                {
                    if (sweepRange == 360)
                    {
                        j = 0;
                    }
                    else
                    {
                        // dont connect end to start if the sweep doesn't go all the way round
                        break;
                    }
                }
                double b = da * j;

                for (int index = 0; index < polarProfile.Count; index++)
                {
                    int index2 = index + 1;
                    if (index2 == polarProfile.Count)
                    {
                        index2 = 0;
                    }
                    PolarCoordinate pc1 = polarProfile[index].Clone();
                    PolarCoordinate pc2 = polarProfile[index2].Clone();
                    PolarCoordinate pc3 = polarProfile[index2].Clone();
                    PolarCoordinate pc4 = polarProfile[index].Clone();
                    pc1.Phi -= a;
                    pc2.Phi -= a;
                    pc3.Phi -= b;
                    pc4.Phi -= b;

                    Point3D p1 = pc1.GetPoint3D();
                    Point3D p2 = pc2.GetPoint3D();
                    Point3D p3 = pc3.GetPoint3D();
                    Point3D p4 = pc4.GetPoint3D();

                    int v1 = AddVertice(p1);
                    int v2 = AddVertice(p2);
                    int v3 = AddVertice(p3);
                    int v4 = AddVertice(p4);

                    Faces.Add(v1);
                    Faces.Add(v2);
                    Faces.Add(v3);

                    Faces.Add(v1);
                    Faces.Add(v3);
                    Faces.Add(v4);
                }
            }
            return;
        }

        internal void SweepPolarProfileTheta(List<PolarCoordinate> polarProfile, double cx, double cy, double sweepRange, int numSegs, bool clear = true, bool flipAxies = false, bool invert = false)
        {
            // now we have a lovely copy of the profile in polar coordinates.
            if (clear)
            {
                ClearShape();
            }

            double sweep = sweepRange * (Math.PI * 2.0) / 360.0;
            double da = sweep / (numSegs - 1);
            for (int i = 0; i < numSegs; i++)
            {
                double a = da * i;
                int j = i + 1;
                if (j == numSegs)
                {
                    if (sweepRange == 360)
                    {
                        j = 0;
                    }
                    else
                    {
                        // dont connect end to start if the sweep doesn't go all the way round
                        break;
                    }
                }
                double b = da * j;

                for (int index = 0; index < polarProfile.Count; index++)
                {
                    int index2 = index + 1;
                    if (index2 == polarProfile.Count)
                    {
                        index2 = 0;
                    }
                    PolarCoordinate pc1 = polarProfile[index].Clone();
                    PolarCoordinate pc2 = polarProfile[index2].Clone();
                    PolarCoordinate pc3 = polarProfile[index2].Clone();
                    PolarCoordinate pc4 = polarProfile[index].Clone();
                    pc1.Theta -= a;
                    pc2.Theta -= a;
                    pc3.Theta -= b;
                    pc4.Theta -= b;

                    Point3D p1 = pc1.GetPoint3D();
                    Point3D p2 = pc2.GetPoint3D();
                    Point3D p3 = pc3.GetPoint3D();
                    Point3D p4 = pc4.GetPoint3D();
                    if (flipAxies)
                    {
                        FlipAxies(ref p1);
                        FlipAxies(ref p2);
                        FlipAxies(ref p3);
                        FlipAxies(ref p4);
                    }
                    int v1 = AddVertice(p1);
                    int v2 = AddVertice(p2);
                    int v3 = AddVertice(p3);
                    int v4 = AddVertice(p4);
                    if (invert)
                    {
                        Faces.Add(v1);
                        Faces.Add(v3);
                        Faces.Add(v2);

                        Faces.Add(v1);
                        Faces.Add(v4);
                        Faces.Add(v3);
                    }
                    else
                    {
                        Faces.Add(v1);
                        Faces.Add(v2);
                        Faces.Add(v3);

                        Faces.Add(v1);
                        Faces.Add(v3);
                        Faces.Add(v4);
                    }
                }
            }
            if (sweepRange != 360.0)
            {
                // both ends will be open.
                Point3D centreOfProfile = new Point3D(cx, 0, cy);
                for (int index = 0; index < polarProfile.Count; index++)
                {
                    int index2 = index + 1;
                    if (index2 == polarProfile.Count)
                    {
                        index2 = 0;
                    }
                    PolarCoordinate pc1 = polarProfile[index].Clone();
                    PolarCoordinate pc2 = polarProfile[index2].Clone();
                    PolarCoordinate pc3 = new PolarCoordinate(0, 0, 0);
                    pc3.SetPoint3D(centreOfProfile);

                    Point3D p1 = pc1.GetPoint3D();
                    Point3D p2 = pc2.GetPoint3D();
                    Point3D p3 = pc3.GetPoint3D();

                    int v1 = AddVertice(p1);
                    int v2 = AddVertice(p2);
                    int v3 = AddVertice(p3);
                    if (invert)
                    {
                        Faces.Add(v1);
                        Faces.Add(v2);
                        Faces.Add(v3);
                    }
                    else
                    {
                        Faces.Add(v1);
                        Faces.Add(v3);
                        Faces.Add(v2);
                    }
                }

                for (int index = 0; index < polarProfile.Count; index++)
                {
                    int index2 = index + 1;
                    if (index2 == polarProfile.Count)
                    {
                        index2 = 0;
                    }
                    PolarCoordinate pc1 = polarProfile[index].Clone();
                    PolarCoordinate pc2 = polarProfile[index2].Clone();
                    PolarCoordinate pc3 = new PolarCoordinate(0, 0, 0);
                    pc3.SetPoint3D(centreOfProfile);
                    pc1.Theta -= sweep;
                    pc2.Theta -= sweep;
                    pc3.Theta -= sweep;

                    Point3D p1 = pc1.GetPoint3D();
                    Point3D p2 = pc2.GetPoint3D();
                    Point3D p3 = pc3.GetPoint3D();

                    int v1 = AddVertice(p1);
                    int v2 = AddVertice(p2);
                    int v3 = AddVertice(p3);

                    if (invert)
                    {
                        Faces.Add(v1);
                        Faces.Add(v3);
                        Faces.Add(v2);
                    }
                    else
                    {
                        Faces.Add(v1);
                        Faces.Add(v2);
                        Faces.Add(v3);
                    }
                }
            }
        }

        protected int AddVertice(Point3DCollection verts, double x, double y, double z)
        {
            int res = -1;
            for (int i = 0; i < verts.Count; i++)
            {
                if (PointUtils.equals(verts[i], x, y, z))
                {
                    res = i;
                    break;
                }
            }

            if (res == -1)
            {
                verts.Add(new Point3D(x, y, z));
                res = verts.Count - 1;
            }
            return res;
        }

        protected int AddVertice(Point3D v)
        {
            int res = -1;
            if (spaceTreeRoot != null)
            {
                res = spaceTreeRoot.Present(v);
            }

            if (res == -1)
            {
                vertices.Add(new Point3D(v.X, v.Y, v.Z));
                res = vertices.Count - 1;
                if (spaceTreeRoot == null)
                {
                    spaceTreeRoot = SpaceTreeNode.Create(v, res);
                }
                else
                {
                    spaceTreeRoot.Add(v, spaceTreeRoot, res);
                }
            }
            return res;
        }

        protected Point CalcPoint(double theta, double r)
        {
            Point p = new Point();
            p.X = r * Math.Sin(theta);
            p.Y = r * Math.Cos(theta);
            return p;
        }

        protected virtual void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        protected void CentreVertices(bool floor = true)
        {
            Point3D min = new Point3D(double.MaxValue, double.MaxValue, double.MaxValue);
            Point3D max = new Point3D(double.MinValue, double.MinValue, double.MinValue);
            PointUtils.MinMax(Vertices, ref min, ref max);

            double scaleX = max.X - min.X;
            double scaleY = max.Y - min.Y;
            double scaleZ = max.Z - min.Z;

            double midx = min.X + (scaleX / 2);
            double midy = min.Y + (scaleY / 2);
            double midz = min.Z + (scaleZ / 2);
            Vector3D offset;
            if (floor)
            {
                offset = new Vector3D(-midx, -min.Y, -midz);
            }
            else
            {
                offset = new Vector3D(-midx, 0, -midz);
            }
            bounds.Zero();
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertices[i] += offset;
                bounds.Adjust(Vertices[i]);
            }
        }

        protected void CentreVertices(Point3DCollection verts, Int32Collection facs)
        {
            Point3D min = new Point3D(double.MaxValue, double.MaxValue, double.MaxValue);
            Point3D max = new Point3D(double.MinValue, double.MinValue, double.MinValue);
            PointUtils.MinMax(verts, ref min, ref max);

            double scaleX = max.X - min.X;
            double scaleY = max.Y - min.Y;
            double scaleZ = max.Z - min.Z;

            double midx = min.X + (scaleX / 2);
            double midy = min.Y + (scaleY / 2);
            double midz = min.Z + (scaleZ / 2);
            Vector3D offset = new Vector3D(-midx, -min.Y, -midz);
            // bounds.Zero();
            for (int i = 0; i < verts.Count; i++)
            {
                verts[i] += offset;
                // bounds.Adjust(Vertices[i]);
            }
        }

        // run around around a list of points assume the start and end are linked.
        protected void CreateSideFaces(List<Point> points, double thickness, bool rev, double zOff = 0)
        {
            for (int i = 0; i < points.Count; i++)
            {
                int v = i + 1;
                if (v == points.Count)
                {
                    v = 0;
                }

                int c0 = AddVertice(points[i].X, points[i].Y, zOff);
                int c1 = AddVertice(points[i].X, points[i].Y, zOff + thickness);
                int c2 = AddVertice(points[v].X, points[v].Y, zOff + thickness);
                int c3 = AddVertice(points[v].X, points[v].Y, zOff);
                if (rev)
                {
                    Faces.Add(c0);
                    Faces.Add(c2);
                    Faces.Add(c1);

                    Faces.Add(c0);
                    Faces.Add(c3);
                    Faces.Add(c2);
                }
                else
                {
                    Faces.Add(c0);
                    Faces.Add(c1);
                    Faces.Add(c2);

                    Faces.Add(c0);
                    Faces.Add(c2);
                    Faces.Add(c3);
                }
            }
        }

        protected double DegToRad(double deg)
        {
            return deg * Math.PI / 180;
        }

        protected double Distance(Point p1, Point p2)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            return Math.Sqrt((dx * dx) + (dy * dy));
        }

        protected void FlipAxies(ref Point3D p1)
        {
            p1 = new Point3D(p1.X, p1.Z, p1.Y);
        }

        protected void FloorVertices()
        {
            Point3D min = new Point3D(double.MaxValue, double.MaxValue, double.MaxValue);
            Point3D max = new Point3D(double.MinValue, double.MinValue, double.MinValue);
            PointUtils.MinMax(Vertices, ref min, ref max);

            Vector3D offset = new Vector3D(0, -min.Y, 0);
            bounds.Zero();
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertices[i] += offset;
                bounds.Adjust(Vertices[i]);
            }
        }

        protected void Get2DBounds(List<Point> pnts, ref double tlx, ref double tly, ref double brx, ref double bry)
        {
            tlx = double.MaxValue;
            tly = double.MaxValue;
            brx = double.MinValue;
            bry = double.MinValue;
            foreach (Point p in pnts)
            {
                if (p.X < tlx) tlx = p.X;
                if (p.X > brx) brx = p.X;
                if (p.Y < tly) tly = p.Y;
                if (p.Y > bry) bry = p.Y;
            }
        }

        protected Bounds3D GetBounds3D(Point3DCollection pnts)
        {
            Bounds3D bnds = new Bounds3D();
            foreach (Point3D p in pnts)
            {
                bnds.Adjust(p);
            }
            return bnds;
        }

        protected virtual GeometryModel3D GetModel()
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.Positions = Vertices;
            mesh.TriangleIndices = Faces;
            mesh.Normals = null;
            GeometryModel3D gm = new GeometryModel3D();
            gm.Geometry = mesh;

            DiffuseMaterial mt = new DiffuseMaterial();
            mt.Color = meshColour;
            mt.Brush = new SolidColorBrush(meshColour);
            gm.Material = mt;
            DiffuseMaterial mtb = new DiffuseMaterial();
            mtb.Color = Colors.CornflowerBlue;
            mtb.Brush = new SolidColorBrush(Colors.Green);
            gm.BackMaterial = mtb;

            return gm;
        }

        /// <summary>
        /// Purely used to get over a threading issue when
        /// using async.
        /// We can't get Point3DCollections back from an async function
        /// so we pass points as an array and convert type afterwards
        /// </summary>
        /// <param name="result"></param>
        protected void GetVerticesFromAsyncResult(AsyncGeneratorResult result)
        {
            foreach (Point3D p in result.points)
            {
                Vertices.Add(new Point3D(p.X, p.Y, p.Z));
            }
            foreach (int f in result.indices)
            {
                Faces.Add(f);
            }
        }

        protected virtual void Home_Click(object sender, RoutedEventArgs e)
        {
            Camera.HomeFront();
            // SetCameraDistance();
            UpdateCameraPos();
        }

        protected virtual void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.SaveSizeAndLocation(true);
            base.OnClosing(e);
        }

        protected virtual void Redisplay()
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
            }
        }

        protected void RestoreSizeAndLocation()
        {
            this.RestoreSizeAndLocation(true);
        }

        protected void ScaleVertices(double x, double y, double z)
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                Point3D p = Vertices[i];
                Vertices[i] = new Point3D(p.X * x, p.Y * y, p.Z * z);
            }
        }

        protected void SetCameraDistance(bool sideView = false)
        {
            Point3D min = new Point3D(double.MaxValue, double.MaxValue, double.MaxValue);
            Point3D max = new Point3D(double.MinValue, double.MinValue, double.MinValue);
            PointUtils.MinMax(Vertices, ref min, ref max);
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

        protected void SurfaceToSolid(Point3DCollection vertices, Int32Collection tris, double thickness)
        {
            Mesh hemesh = new HalfEdgeLib.Mesh(vertices, tris);
            Vector3D[] normals = new Vector3D[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                normals[i] = hemesh.GetVertexNormal(i);
            }

            Point3DCollection innerVerts = new Point3DCollection();
            for (int i = 0; i < vertices.Count; i++)
            {
                Point3D p = new Point3D(vertices[i].X + (normals[i].X * thickness),
                                        vertices[i].Y + (normals[i].Y * thickness),
                                        vertices[i].Z + (normals[i].Z * thickness));
                innerVerts.Add(p);
            }

            int faceOffset = tris.Count;
            for (int findex = 0; findex < faceOffset; findex += 3)
            {
                int f0 = tris[findex];
                int f1 = tris[findex + 1];
                int f2 = tris[findex + 2];

                int v0 = AddPoint(vertices, innerVerts[f0]);
                int v1 = AddPoint(vertices, innerVerts[f1]);
                int v2 = AddPoint(vertices, innerVerts[f2]);

                tris.Add(v0);
                tris.Add(v2);
                tris.Add(v1);
            }

            // close sides, The Fake face used to close the boundary of the inner surface has
            // vertices which correspond to there outer counter parts. i.e. Vertex 0 in the inner is
            // vertex 0 of the outer but moved along the normal so in effect we triangulate the
            // rectangle formed by two outer and and the corresponding two inner vertices
            foreach (List<HalfEdge> lhe in hemesh.Boundaries)
            {
                foreach (HalfEdge he in lhe)
                {
                    // outer indices
                    int f0 = he.StartVertex;
                    int f1 = he.EndVertex;

                    // inner indices
                    int v0 = AddPoint(vertices, innerVerts[f0]);
                    int v1 = AddPoint(vertices, innerVerts[f1]);

                    // make a triangle
                    tris.Add(f0);
                    tris.Add(f1);
                    tris.Add(v0);

                    // make the other triangle
                    tris.Add(f1);
                    tris.Add(v1);
                    tris.Add(v0);
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

        private double InchesToMM(double x)
        {
            return x * 25.4;
        }

        public struct AsyncGeneratorResult
        {
            public int[] indices;
            public Point3D[] points;
        }
    }
}