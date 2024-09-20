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

using asdflibrary;
using CSGLib;
using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for DualProfile.xaml
    /// </summary>
    public partial class DualProfileDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private const double flippedScale = 0.9;

        private List<System.Windows.Point> frontpnts;

        private double frontXSize;
        private double frontYSize;
        private bool hiResSdfMethod;
        private bool intersectionMethod;
        private bool loaded;
        private bool midResSdfMethod;
        private OctTreeLib.OctTree octTree;
        private List<System.Windows.Point> oldfrontpnts;
        private List<System.Windows.Point> oldtoppnts;
        private bool sdfMethod;
        private bool shapeDirty;
        private DispatcherTimer timer;

        // scaled and centered on 0
        private List<System.Windows.Point> toppnts;

        // private string topProfile;
        private double topXSize;

        private double topYSize;
        private string warningText;

        public DualProfileDlg()
        {
            InitializeComponent();
            ToolName = "Dual";
            DataContext = this;
            frontXSize = 0;
            frontYSize = 0;
            frontpnts = new List<Point>();
            topXSize = 0;
            topYSize = 0;
            toppnts = new List<Point>();
            FrontPathControl.OnFlexiPathChanged += FrontPointsChanged;
            TopPathControl.OnFlexiPathChanged += TopPointsChanged;
            FrontPathControl.AbsolutePaths = true;
            TopPathControl.AbsolutePaths = true;
            ModelGroup = MyModelGroup;
            loaded = false;
            shapeDirty = true;
            // It looks odd if the user chances a value but the
            // screen doesn't update until after the part generation completes.
            // So when a parameter changes, start a timer which triggers the generation
            // later.
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            timer.Tick += Timer_Tick;
        }

        public bool HiResSdfMethod
        {
            get
            {
                return hiResSdfMethod;
            }
            set
            {
                if (value != hiResSdfMethod)
                {
                    hiResSdfMethod = value;
                    if (hiResSdfMethod == true)
                    {
                        intersectionMethod = false;
                        sdfMethod = false;
                        midResSdfMethod = false;
                        shapeDirty = true;
                        NotifyPropertyChanged();
                        StartUpdateTimer();
                    }
                }
            }
        }

        public bool IntersectionMethod
        {
            get
            {
                return intersectionMethod;
            }
            set
            {
                if (value != intersectionMethod)
                {
                    intersectionMethod = value;

                    if (intersectionMethod)
                    {
                        shapeDirty = true;
                        sdfMethod = false;
                        hiResSdfMethod = false;
                        midResSdfMethod = false;
                        NotifyPropertyChanged();
                        StartUpdateTimer();
                    }
                }
            }
        }

        public bool MidResSdfMethod
        {
            get
            {
                return midResSdfMethod;
            }
            set
            {
                if (value != midResSdfMethod)
                {
                    midResSdfMethod = value;
                    if (midResSdfMethod == true)
                    {
                        intersectionMethod = false;
                        sdfMethod = false;
                        hiResSdfMethod = false;
                        shapeDirty = true;
                        NotifyPropertyChanged();
                        StartUpdateTimer();
                    }
                }
            }
        }

        public bool SdfMethod
        {
            get
            {
                return sdfMethod;
            }
            set
            {
                if (value != sdfMethod)
                {
                    sdfMethod = value;
                    if (sdfMethod == true)
                    {
                        intersectionMethod = false;
                        hiResSdfMethod = false;
                        midResSdfMethod = false;
                        shapeDirty = true;
                        NotifyPropertyChanged();
                        StartUpdateTimer();
                    }
                }
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

        public static bool IsPointInPolygon(float x, float y, List<Point> polygon)
        {
            int polyCorners = polygon.Count;
            int i = 0;
            int j = polyCorners - 1;
            bool oddNodes = false;

            for (i = 0; i < polyCorners; i++)
            {
                if (polygon[i].Y < y && polygon[j].Y >= y
                || polygon[j].Y < y && polygon[i].Y >= y)
                {
                    if (polygon[i].X + (y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < x)
                    {
                        oddNodes = !oddNodes;
                    }
                }
                j = i;
            }

            return oddNodes;
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

        public float FindClosestToLine(float x,
                                       float y,
                                       Point p1,
                                       Point p2,
                                       out Point closest)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            if ((dx == 0) && (dy == 0))
            {
                // It's a point not a line segment.
                closest = p1;
                dx = x - p1.X;
                dy = y - p1.Y;
                return (float)Math.Sqrt(dx * dx + dy * dy);
            }

            // Calculate the t that minimizes the distance.
            double t = ((x - p1.X) * dx + (y - p1.Y) * dy) /
                (dx * dx + dy * dy);

            // See if this represents one of the segment's end points or a point in the middle.
            if (t < 0)
            {
                closest = new Point(p1.X, p1.Y);
                dx = p1.X - x;
                dy = p1.Y - y;
            }
            else if (t > 1)
            {
                closest = new Point(p2.X, p2.Y);
                dx = x - p2.X;
                dy = y - p2.Y;
            }
            else
            {
                closest = new Point(p1.X + t * dx, p1.Y + t * dy);
                dx = x - closest.X;
                dy = y - closest.Y;
            }

            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParmeters();
            DialogResult = true;
            Close();
        }

        private void CalculateExtents(List<Point> tmp, out double lx, out double rx, out double ty, out double by)
        {
            lx = double.MaxValue;
            rx = double.MinValue;
            ty = double.MinValue;
            by = double.MaxValue;
            for (int i = 0; i < tmp.Count; i++)
            {
                lx = Math.Min(tmp[i].X, lx);
                rx = Math.Max(tmp[i].X, rx);
                by = Math.Min(tmp[i].Y, by);
                ty = Math.Max(tmp[i].Y, ty);
            }
        }

        private bool ComparePoints(List<Point> pnts1, List<Point> pnts2)
        {
            bool same = true;
            if (pnts2 == null)
            {
                same = false;
            }
            else
            if (pnts1.Count != pnts2.Count)
            {
                same = false;
            }
            else
            {
                for (int i = 0; i < pnts1.Count && same == true; i++)
                {
                    if (pnts1[i].X != pnts2[i].X || pnts1[i].Y != pnts2[i].Y)
                    {
                        same = false;
                    }
                }
            }
            return same;
        }

        private void CopyPoints(List<Point> pnts1, ref List<Point> pnts2)
        {
            if (pnts2 == null)
            {
                pnts2 = new List<Point>();
            }
            pnts2.Clear();
            foreach (Point p in pnts1)
            {
                pnts2.Add(new Point(p.X, p.Y));
            }
        }

        private OctTreeLib.OctTree CreateOctree(Point3D minPoint, Point3D maxPoint)
        {
            octTree = new OctTreeLib.OctTree(Vertices,
                                  minPoint,
                                  maxPoint,
                                  200);
            return octTree;
        }

        private void CreateSideFace(List<System.Windows.Point> pnts, int i, bool flip, bool autoclose = true)
        {
            int v = i + 1;

            if (v == pnts.Count)
            {
                if (autoclose)
                {
                    v = 0;
                }
                else
                {
                    // dont process the final point if caller doesn't want it
                    return;
                }
            }
            int c0;
            int c1;
            int c2;
            int c3;

            if (!flip)
            {
                c0 = AddVerticeOctTree(pnts[i].X, pnts[i].Y, -0.5);
                c1 = AddVerticeOctTree(pnts[i].X, pnts[i].Y, 0.5);
                c2 = AddVerticeOctTree(pnts[v].X, pnts[v].Y, 0.5);
                c3 = AddVerticeOctTree(pnts[v].X, pnts[v].Y, -0.5);
                Faces.Add(c0);
                Faces.Add(c2);
                Faces.Add(c1);

                Faces.Add(c0);
                Faces.Add(c3);
                Faces.Add(c2);
            }
            else
            {
                c0 = AddVerticeOctTree(flippedScale * pnts[i].X, flippedScale * -0.5, flippedScale * pnts[i].Y);
                c1 = AddVerticeOctTree(flippedScale * pnts[i].X, flippedScale * 0.5, flippedScale * pnts[i].Y);
                c2 = AddVerticeOctTree(flippedScale * pnts[v].X, flippedScale * 0.5, flippedScale * pnts[v].Y);
                c3 = AddVerticeOctTree(flippedScale * pnts[v].X, flippedScale * -0.5, flippedScale * pnts[v].Y);
                Faces.Add(c0);
                Faces.Add(c1);
                Faces.Add(c2);

                Faces.Add(c0);
                Faces.Add(c2);
                Faces.Add(c3);
            }
            Faces.Add(c0);
            Faces.Add(c2);
            Faces.Add(c1);

            Faces.Add(c0);
            Faces.Add(c3);
            Faces.Add(c2);
        }

        private float DistToPoly(List<Point> pnts, float x, float y)
        {
            float v = float.MaxValue;
            float d = 0;

            Point cl2 = new Point(0, 0);
            for (int i = 0; i < pnts.Count - 1; i++)
            {
                d = FindClosestToLine(x, y, pnts[i], pnts[i + 1], out cl2);

                if (d < v)
                {
                    v = d;
                }
            }

            return v;
        }

        private float FrontDist(float x, float y)
        {
            float frontDist = DistToPoly(frontpnts, x, y);

            return frontDist;
        }

        private void FrontPointsChanged(List<System.Windows.Point> pnts)
        {
            double tlx = 0;
            double tly = 0;
            double brx = 0;
            double bry = 0;
            Get2DBounds(pnts, ref tlx, ref tly, ref brx, ref bry);
            if (tlx < double.MaxValue)
            {
                frontXSize = brx - tlx;
                frontYSize = bry - tly;

                double mx = tlx + frontXSize / 2.0;
                double my = tly + frontYSize / 2.0;
                frontpnts.Clear();
                foreach (Point p in pnts)
                {
                    frontpnts.Add(new Point((p.X - mx) / frontXSize, (p.Y - my) / frontYSize));
                }
            }
            if (!ComparePoints(frontpnts, oldfrontpnts))
            {
                shapeDirty = true;
            }

            if (shapeDirty)
            {
                CopyPoints(frontpnts, ref oldfrontpnts);
                StartUpdateTimer();
                // Very peculiarly the vm of the flexipath control
                // is given the original copy of the path everytime
                // the control is reactivated.
                // Meaning switching from top to front view
                // always restores the very first path.
                // Get around this by replacing the inital path by the
                // current one. A dirty hack!
                string pth = FrontPathControl.AbsolutePathString;
                FrontPathControl.SetPath(pth);
            }
        }

        private void GenerateByBoolean()
        {
            Solid front = GenerateSolid(frontpnts, false);

            Solid top = GenerateSolid(toppnts, true);

            BooleanModeller.OperationType op = BooleanModeller.OperationType.Intersection;
            Part Object1 = new Part(front);
            Part Object2 = new Part(top);

            BooleanModeller modeller = new BooleanModeller(front, top);
            Solid result = modeller.GetIntersection();

            ClearShape();
            Vector3D[] vc = result.GetVertices();
            if (vc.GetLength(0) > 0)
            {
                foreach (Vector3D v in vc)
                {
                    Point3D p = new Point3D(v.X * frontXSize, v.Y * frontYSize, v.Z * topYSize);
                    Vertices.Add(p);
                }
                int[] ids = result.GetIndices();
                for (int i = 0; i < ids.Length; i++)
                {
                    Faces.Add(ids[i]);
                }
            }
        }

        private void GenerateBySdf(float dd = 0.025F)
        {
            CubeMarcher cm = new CubeMarcher();
            GridCell gc = new GridCell();
            List<asdflibrary.Triangle> triangles = new List<asdflibrary.Triangle>();
            octTree = CreateOctree(new Point3D(-frontXSize, -frontYSize, -topYSize),
                  new Point3D(frontXSize, frontYSize, topYSize));

            for (float x = -0.6F; x <= 0.6; x += dd)
            {
                for (float y = -0.6F; y <= 0.6F; y += dd)
                {
                    for (float z = -0.6F; z <= 0.6; z += dd)
                    {
                        gc.p[0] = new XYZ(x, y, z);

                        gc.p[1] = new XYZ(x + dd, y, z);
                        gc.p[2] = new XYZ(x + dd, y, z + dd);
                        gc.p[3] = new XYZ(x, y, z + dd);
                        gc.p[4] = new XYZ(x, y + dd, z);
                        gc.p[5] = new XYZ(x + dd, y + dd, z);
                        gc.p[6] = new XYZ(x + dd, y + dd, z + dd);
                        gc.p[7] = new XYZ(x, y + dd, z + dd);

                        for (int i = 0; i < 8; i++)
                        {
                            bool fin = InFront((float)gc.p[i].x, (float)gc.p[i].y);
                            bool tin = InTop((float)gc.p[i].x, (float)gc.p[i].z);
                            float fd = FrontDist((float)gc.p[i].x, (float)gc.p[i].y);
                            float td = TopDist((float)gc.p[i].x, (float)gc.p[i].z);

                            gc.val[i] = GetClosestDist(fd, td);
                            if (fin && tin)
                            {
                                gc.val[i] = -gc.val[i];
                            }
                        }
                        triangles.Clear();

                        cm.Polygonise(gc, 0, triangles);

                        foreach (asdflibrary.Triangle t in triangles)
                        {
                            int p0 = AddVerticeOctTree(t.p[0].x * frontXSize, -(t.p[0].y * frontYSize), t.p[0].z * topYSize);
                            int p1 = AddVerticeOctTree(t.p[1].x * frontXSize, -(t.p[1].y * frontYSize), t.p[1].z * topYSize);
                            int p2 = AddVerticeOctTree(t.p[2].x * frontXSize, -(t.p[2].y * frontYSize), t.p[2].z * topYSize);

                            AddFace(p0, p2, p1);
                        }
                    }
                }
            }
        }

        private void GenerateShape()
        {
            if (shapeDirty)
            {
                ClearShape();
                if (TopPathControl.PathClosed && FrontPathControl.PathClosed)
                {
                    if (frontpnts.Count > 3 && toppnts.Count > 3)
                    {
                        if (intersectionMethod)
                        {
                            GenerateByBoolean();
                        }
                        else
                        if (sdfMethod)
                        {
                            GenerateBySdf(0.025F);
                        }
                        else
                        if (midResSdfMethod)
                        {
                            GenerateBySdf(0.0175F);
                        }
                        else
                        if (hiResSdfMethod)
                        {
                            GenerateBySdf(0.01F);
                        }
                    }
                }
                CentreVertices();
                shapeDirty = false;
            }
        }

        private Solid GenerateSolid(List<System.Windows.Point> points, bool flip)
        {
            ClearShape();
            Solid res = null;
            // points should be a list of 2d points scaled between -0.5 and .5
            if (points != null && points.Count > 3)
            {
                List<System.Windows.Point> tmp = new List<System.Windows.Point>();
                double top = 0;
                for (int i = 0; i < points.Count; i++)
                {
                    if (points[i].Y > top)
                    {
                        top = points[i].Y;
                    }
                }

                for (int i = 0; i < points.Count; i++)
                {
                    // flipping coordinates so have to reverse polygon too
                    tmp.Insert(0, new System.Windows.Point(points[i].X, top - points[i].Y));
                }

                octTree = CreateOctree(new Point3D(-2, -2, -2),
                                  new Point3D(2, 2, 2));

                for (int i = 0; i < tmp.Count; i++)
                {
                    CreateSideFace(tmp, i, flip);
                }

                // triangulate the basic polygon
                TriangulationPolygon ply = new TriangulationPolygon();
                List<System.Drawing.PointF> pf = new List<System.Drawing.PointF>();
                foreach (System.Windows.Point p in tmp)
                {
                    pf.Add(new System.Drawing.PointF((float)p.X, (float)p.Y));
                }
                ply.Points = pf.ToArray();
                List<PolygonTriangulationLib.Triangle> tris = ply.Triangulate();

                int c0;
                int c1;
                int c2;
                foreach (PolygonTriangulationLib.Triangle t in tris)
                {
                    if (!flip)
                    {
                        c0 = AddVerticeOctTree(t.Points[0].X, t.Points[0].Y, -0.5);
                        c1 = AddVerticeOctTree(t.Points[1].X, t.Points[1].Y, -0.5);
                        c2 = AddVerticeOctTree(t.Points[2].X, t.Points[2].Y, -0.5);
                        AddFace(c0, c2, c1);
                    }
                    else
                    {
                        c0 = AddVerticeOctTree(flippedScale * t.Points[0].X, flippedScale * -0.5, flippedScale * t.Points[0].Y);
                        c1 = AddVerticeOctTree(flippedScale * t.Points[1].X, flippedScale * -0.5, flippedScale * t.Points[1].Y);
                        c2 = AddVerticeOctTree(flippedScale * t.Points[2].X, flippedScale * -0.5, flippedScale * t.Points[2].Y);

                        AddFace(c0, c1, c2);
                    }

                    if (!flip)
                    {
                        c0 = AddVerticeOctTree(t.Points[0].X, t.Points[0].Y, 0.5);
                        c1 = AddVerticeOctTree(t.Points[1].X, t.Points[1].Y, 0.5);
                        c2 = AddVerticeOctTree(t.Points[2].X, t.Points[2].Y, 0.5);
                        AddFace(c0, c1, c2);
                    }
                    else
                    {
                        c0 = AddVerticeOctTree(flippedScale * t.Points[0].X, flippedScale * 0.5, flippedScale * t.Points[0].Y);
                        c1 = AddVerticeOctTree(flippedScale * t.Points[1].X, flippedScale * 0.5, flippedScale * t.Points[1].Y);
                        c2 = AddVerticeOctTree(flippedScale * t.Points[2].X, flippedScale * 0.5, flippedScale * t.Points[2].Y);
                        AddFace(c0, c2, c1);
                    }
                }
                CentreVertices();
                /*
                MeshSubdivider subdiv = new MeshSubdivider(Vertices, Faces);

                Point3DCollection tmp2 = new Point3DCollection();
                Int32Collection newTri = new Int32Collection();
                subdiv.Subdivide(tmp2, newTri);

                res = new Solid(tmp2, newTri, false);
                */
                res = new Solid(Vertices, Faces, false);
            }
            return res;
        }

        private double GetClosestDist(float distanceToFront, float distanceToTop)
        {
            double res = distanceToFront;

            if (distanceToTop < distanceToFront)
            {
                res = distanceToTop;
            }

            return res;
        }

        private bool InFront(float x, float y)
        {
            bool inFront = IsPointInPolygon(x, y, frontpnts);
            return inFront;
        }

        private bool InTop(float x, float y)
        {
            bool inTop = IsPointInPolygon(x, y, toppnts);
            return inTop;
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            string s = EditorParameters.Get("TopShape");
            TopPathControl.SetPath(s);
            s = EditorParameters.Get("FrontShape");
            FrontPathControl.SetPath(s);
            SdfMethod = EditorParameters.GetBoolean("SdfMethod", true);
            HiResSdfMethod = EditorParameters.GetBoolean("HiResSdfMethod", false);
            MidResSdfMethod = EditorParameters.GetBoolean("MidResSdfMethod", false);
            IntersectionMethod = EditorParameters.GetBoolean("IntersectionMethod", false);
        }

        private void SaveEditorParmeters()
        {
            EditorParameters.Set("TopShape", TopPathControl.GetPath());
            EditorParameters.Set("FrontShape", FrontPathControl.GetPath());
            EditorParameters.Set("SdfMethod", SdfMethod.ToString());
            EditorParameters.Set("IntersectionMethod", IntersectionMethod.ToString());
            EditorParameters.Set("HiResSdfMethod", HiResSdfMethod.ToString());
            EditorParameters.Set("MidResSdfMethod", MidResSdfMethod.ToString());
        }

        private void StartUpdateTimer()
        {
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            if (shapeDirty)
            {
                GenerateShape();
                Redisplay();
            }
        }

        private float TopDist(float x, float y)
        {
            float topDist = DistToPoly(toppnts, x, y);

            return topDist;
        }

        private void TopPointsChanged(List<System.Windows.Point> pnts)
        {
            double tlx = 0;
            double tly = 0;
            double brx = 0;
            double bry = 0;
            Get2DBounds(pnts, ref tlx, ref tly, ref brx, ref bry);
            if (tlx < double.MaxValue)
            {
                topXSize = brx - tlx;
                topYSize = bry - tly;

                double mx = tlx + topXSize / 2.0;
                double my = tly + topYSize / 2.0;
                toppnts.Clear();
                foreach (Point p in pnts)
                {
                    toppnts.Add(new Point((p.X - mx) / topXSize, (p.Y - my) / topYSize));
                }
            }
            if (!ComparePoints(toppnts, oldtoppnts))
            {
                shapeDirty = true;
            }

            if (shapeDirty)
            {
                CopyPoints(toppnts, ref oldtoppnts);
                StartUpdateTimer();
                // Very peculiarly the vm of the flexipath control
                // is given the original copy of the path everytime
                // the control is reactivated.
                // Meaning switching from top to front view
                // always restores the very first path.
                // Get around this by replacing the inital path by the
                // current one. A dirty hack!
                string pth = TopPathControl.AbsolutePathString;
                TopPathControl.SetPath(pth);
            }
        }

        private void UpdateDisplay()
        {
            if (loaded)
            {
                GenerateShape();
                Redisplay();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WarningText = "";
            LoadEditorParameters();

            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            loaded = true;

            UpdateDisplay();
        }
    }
}