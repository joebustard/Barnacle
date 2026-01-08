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
using MakerLib.TextureUtils;
using OctTreeLib;
using PolygonLibrary;
using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for Devtest.xaml
    /// </summary>
    public partial class Platelet : BaseModellerDialog, INotifyPropertyChanged
    {
        private const byte bottomMask = 0x08;
        private const byte frontMask = 0x10;
        private const byte leftMask = 0x01;
        private const byte rightMask = 0x02;
        private const byte topMask = 0x04;
        private double baseWidth;
        private bool boxShape;
        private bool clippedSingle;
        private bool clippedTile;
        private List<System.Windows.Point> displayPoints;
        private bool fittedSingle;
        private bool fittedTile;
        private bool hollowShape;
        private double hTextureResolution;
        private bool largeTexture;
        private bool loaded;
        private OctTree octTree;
        private Point PathCentroid;
        private double plateWidth;
        private int selectedShape;
        private string selectedTexture;
        private Visibility showTextures;
        private Visibility showWidth;
        private bool smallTexture;
        private bool solidShape;
        private double textureDepth;
        private bool texturedShape;
        private SortedList<string, string> textureFiles;
        private TextureManager textureManager;
        private bool tileTexture;
        private double vTextureResolution;
        private double wallWidth;
        private string warningText;

        public Platelet()
        {
            InitializeComponent();

            ToolName = "Platelet";
            DataContext = this;
            PathEditor.OnFlexiPathChanged += PathPointsChanged;
            PathEditor.AbsolutePaths = true;
            PathEditor.DefaultImagePath = DefaultImagePath;
            PathEditor.ToolName = ToolName;
            PathEditor.HasPresets = true;
            PathEditor.SupportsHoles = true;

            wallWidth = 2;
            textureDepth = 1;
            solidShape = true;
            hollowShape = false;
            texturedShape = false;
            largeTexture = true;
            tileTexture = true;
            showWidth = Visibility.Hidden;
            showTextures = Visibility.Hidden;
            loaded = false;
            textureFiles = new SortedList<string, string>();
            selectedTexture = "";
            textureManager = TextureManager.Instance();
            textureManager.LoadTextureNames();
            textureManager.Mode = TextureManager.MapMode.ClippedTile;
        }

        public enum PixelColour
        {
            Unknown,
            Back,
            Front,
            Mixed
        }

        public double BaseWidth
        {
            get
            {
                return baseWidth;
            }

            set
            {
                if (baseWidth != value)
                {
                    if (value > 0 && value <= 500)
                    {
                        baseWidth = value;
                        if (baseWidth > plateWidth)
                        {
                            baseWidth = plateWidth;
                        }
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public bool BoxShape
        {
            get
            {
                return boxShape;
            }

            set
            {
                if (boxShape != value)
                {
                    boxShape = value;
                    if (boxShape == true)
                    {
                        ShowWidth = Visibility.Visible;
                        ShowTextures = Visibility.Hidden;
                    }
                    else
                    {
                        ShowWidth = Visibility.Hidden;
                    }

                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public bool ClippedSingle
        {
            get
            {
                return clippedSingle;
            }

            set
            {
                if (clippedSingle != value)
                {
                    clippedSingle = value;
                    NotifyPropertyChanged();
                    if (clippedSingle)
                    {
                        if (textureManager != null)
                        {
                            textureManager.Mode = TextureManager.MapMode.ClippedSingle;
                        }
                        UpdateDisplay();
                    }
                }
            }
        }

        public bool ClippedTile
        {
            get
            {
                return clippedTile;
            }

            set
            {
                if (clippedTile != value)
                {
                    clippedTile = value;
                    NotifyPropertyChanged();
                    if (clippedTile)
                    {
                        if (textureManager != null)
                        {
                            textureManager.Mode = TextureManager.MapMode.ClippedTile;
                        }
                        UpdateDisplay();
                    }
                }
            }
        }

        public bool FittedSingle
        {
            get
            {
                return fittedSingle;
            }

            set
            {
                if (fittedSingle != value)
                {
                    fittedSingle = value;
                    NotifyPropertyChanged();
                    if (fittedSingle)
                    {
                        if (textureManager != null)
                        {
                            textureManager.Mode = TextureManager.MapMode.FittedSingle;
                        }
                        UpdateDisplay();
                    }
                }
            }
        }

        public bool FittedTile
        {
            get
            {
                return fittedTile;
            }

            set
            {
                if (fittedTile != value)
                {
                    fittedTile = value;
                    NotifyPropertyChanged();
                    if (fittedTile)
                    {
                        if (textureManager != null)
                        {
                            textureManager.Mode = TextureManager.MapMode.FittedTile;
                        }
                        UpdateDisplay();
                    }
                }
            }
        }

        public bool HollowShape
        {
            get
            {
                return hollowShape;
            }

            set
            {
                if (hollowShape != value)
                {
                    hollowShape = value;
                    if (hollowShape == true)
                    {
                        ShowWidth = Visibility.Visible;
                        ShowTextures = Visibility.Hidden;
                    }
                    else
                    {
                        ShowWidth = Visibility.Hidden;
                    }

                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public bool LargeTexture
        {
            get
            {
                return largeTexture;
            }

            set
            {
                if (value != largeTexture)
                {
                    largeTexture = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double PlateWidth
        {
            get
            {
                return plateWidth;
            }

            set
            {
                if (plateWidth != value)
                {
                    if (value > 0 && value <= 500)
                    {
                        plateWidth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public int SelectedShape
        {
            get
            {
                return selectedShape;
            }

            set
            {
                if (selectedShape != value)
                {
                    selectedShape = value;
                    ShapeChanged();
                    NotifyPropertyChanged();
                }
            }
        }

        public string SelectedTexture
        {
            get
            {
                return selectedTexture;
            }

            set
            {
                if (value != selectedTexture)
                {
                    selectedTexture = value;
                    if (textureManager != null && !String.IsNullOrEmpty(selectedTexture))
                    {
                        textureManager.LoadTextureImage(selectedTexture);
                    }
                    NotifyPropertyChanged();
                    UpdateDisplay();
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

        public Visibility ShowTextures
        {
            get
            {
                return showTextures;
            }

            set
            {
                if (showTextures != value)
                {
                    showTextures = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Visibility ShowWidth
        {
            get
            {
                return showWidth;
            }

            set
            {
                if (showWidth != value)
                {
                    showWidth = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool SmallTexture
        {
            get
            {
                return smallTexture;
            }

            set
            {
                if (value != smallTexture)
                {
                    smallTexture = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public bool SolidShape
        {
            get
            {
                return solidShape;
            }

            set
            {
                if (solidShape != value)
                {
                    solidShape = value;
                    if (solidShape)
                    {
                        ShowWidth = Visibility.Hidden;
                        ShowTextures = Visibility.Hidden;
                    }
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double TextureDepth
        {
            get
            {
                return textureDepth;
            }

            set
            {
                if (textureDepth != value)
                {
                    textureDepth = value;
                    if (textureDepth >= 0.1)
                    {
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public bool TexturedShape
        {
            get
            {
                return texturedShape;
            }

            set
            {
                if (texturedShape != value)
                {
                    texturedShape = value;
                    if (texturedShape == true)
                    {
                        ShowWidth = Visibility.Hidden;
                        ShowTextures = Visibility.Visible;
                    }
                    else
                    {
                        ShowTextures = Visibility.Hidden;
                    }

                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public List<String> TextureItems
        {
            get
            {
                return textureManager.TextureNames;
            }
        }

        public IEnumerable<String> TextureNames
        {
            get
            {
                return textureFiles.Keys;
            }
        }

        public bool TileTexture
        {
            get
            {
                return tileTexture;
            }

            set
            {
                if (tileTexture != value)
                {
                    tileTexture = value;
                    NotifyPropertyChanged();
                }
                UpdateDisplay();
            }
        }

        public double WallWidth
        {
            get
            {
                return wallWidth;
            }

            set
            {
                if (wallWidth != value)
                {
                    if (value > 0 && value <= 500)
                    {
                        wallWidth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
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

        public double AreaOfTriangle(Point3DCollection vt, int p0, int p1, int p2)
        {
            Point pt0 = new Point(vt[p0].X, vt[p0].Y);
            Point pt1 = new Point(vt[p1].X, vt[p1].Y);
            Point pt2 = new Point(vt[p2].X, vt[p2].Y);

            double a = Distance(pt0, pt1);
            double b = Distance(pt1, pt2);
            double c = Distance(pt2, pt0);
            double s = (a + b + c) / 2;
            return Math.Sqrt(s * (s - a) * (s - b) * (s - c));
        }

        public bool PointOnLineSegment(Point pt1, Point pt2, Point pt, double epsilon = 0.00001)
        {
            if (pt.X - Math.Max(pt1.X, pt2.X) > epsilon ||
                Math.Min(pt1.X, pt2.X) - pt.X > epsilon ||
                pt.Y - Math.Max(pt1.Y, pt2.Y) > epsilon ||
                Math.Min(pt1.Y, pt2.Y) - pt.Y > epsilon)
                return false;

            if (Math.Abs(pt2.X - pt1.X) < epsilon)
                return Math.Abs(pt1.X - pt.X) < epsilon || Math.Abs(pt2.X - pt.X) < epsilon;
            if (Math.Abs(pt2.Y - pt1.Y) < epsilon)
                return Math.Abs(pt1.Y - pt.Y) < epsilon || Math.Abs(pt2.Y - pt.Y) < epsilon;

            double x = pt1.X + (pt.Y - pt1.Y) * (pt2.X - pt1.X) / (pt2.Y - pt1.Y);
            double y = pt1.Y + (pt.X - pt1.X) * (pt2.Y - pt1.Y) / (pt2.X - pt1.X);

            return Math.Abs(pt.X - x) < epsilon || Math.Abs(pt.Y - y) < epsilon;
        }

        protected Point Centroid(Point[] crns)
        {
            double x = 0;
            double y = 0;
            if (crns.GetLength(0) > 0)
            {
                foreach (Point p in crns)
                {
                    x += p.X;
                    y += p.Y;
                }
                x /= crns.GetLength(0);
                y /= crns.GetLength(0);
            }
            return new Point(x, y);
        }

        protected OctTree CreateOctree(Point3D minPoint, Point3D maxPoint)
        {
            octTree = new OctTree(Vertices, minPoint, maxPoint, 200);
            return octTree;
        }

        protected bool IsPointInPolygon(Point point, List<Point> polygon)
        {
            int polyCorners = polygon.Count;
            int i = 0;
            int j = polyCorners - 1;
            bool oddNodes = false;

            for (i = 0; i < polyCorners; i++)
            {
                if (polygon[i].Y < point.Y && polygon[j].Y >= point.Y
                || polygon[j].Y < point.Y && polygon[i].Y >= point.Y)
                {
                    if (polygon[i].X + (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < point.X)
                    {
                        oddNodes = !oddNodes;
                    }
                }
                j = i;
            }

            return oddNodes;
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParameters();
            DialogResult = true;
            this.SaveSizeAndLocation(true);
            Close();
        }

        protected ConvexPolygon2D RectPoly(double px, double py, double sx, double sy)
        {
            System.Windows.Point[] rct = new System.Windows.Point[4];
            rct[0].X = px;
            rct[0].Y = py;

            rct[1].X = px;
            rct[1].Y = py + sy;

            rct[2].X = px + sx;
            rct[2].Y = py + sy;

            rct[3].X = px + sx;
            rct[3].Y = py;

            return new ConvexPolygon2D(rct);
        }

        protected void TriangulateSurface(Point[] points, double z, bool reverse = false)
        {
            TriangulationPolygon ply = new TriangulationPolygon();
            List<System.Drawing.PointF> pf = new List<System.Drawing.PointF>();
            foreach (Point p in points)
            {
                pf.Add(new System.Drawing.PointF((float)p.X, (float)p.Y));
            }
            ply.Points = pf.ToArray();
            List<Triangle> tris = ply.Triangulate();
            foreach (Triangle t in tris)
            {
                int c0 = AddVertice(t.Points[0].X, t.Points[0].Y, z);
                int c1 = AddVertice(t.Points[1].X, t.Points[1].Y, z);
                int c2 = AddVertice(t.Points[2].X, t.Points[2].Y, z);
                if (reverse)
                {
                    Faces.Add(c0);
                    Faces.Add(c2);
                    Faces.Add(c1);
                }
                else
                {
                    Faces.Add(c0);
                    Faces.Add(c1);
                    Faces.Add(c2);
                }
            }
        }

        private void BrowseTextures_Click(object sender, RoutedEventArgs e)
        {
            string folder = textureManager.GetTextureFolderName();
            if (Directory.Exists(folder))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo("explorer.exe");
                startInfo.Arguments = folder;
                startInfo.WindowStyle = ProcessWindowStyle.Normal;
                Process.Start(startInfo);
            }
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

        private void CloseEdge(double lx, double by, double rx, double ty, List<Point> tmp, double sz)
        {
            Extents ext = new Extents();
            int texturexX = 0;
            int texturexY = 0;
            for (double px = lx; px < rx; px += sz)
            {
                texturexY = 0;
                for (double py = by; py < ty; py += sz)
                {
                    TextureCell cell = textureManager.GetCell(texturexX, texturexY);
                    if (cell.Width > 0)
                    {
                        double z = (double)(cell.Width * textureDepth) / 255.0;
                        ext.Left = px;
                        ext.Bottom = py;
                        ext.Right = px + sz;
                        ext.Top = py + sz;
                        for (int i = 0; i < tmp.Count; i++)
                        {
                            int j = (i + 1) % tmp.Count;
                            Tuple<Point, Point> seg = CohenSutherland.CohenSutherlandLineClip(ext, tmp[i], tmp[j]);
                            if (seg != null)
                            {
                                // if (seg.Item1.X != seg.Item2.X || seg.Item1.Y != seg.Item2.Y)
                                {
                                    int v0 = AddVerticeOctTree(seg.Item1.X, seg.Item1.Y, 0);
                                    int v1 = AddVerticeOctTree(seg.Item2.X, seg.Item2.Y, 0);
                                    int v2 = AddVerticeOctTree(seg.Item2.X, seg.Item2.Y, textureDepth);
                                    int v3 = AddVerticeOctTree(seg.Item1.X, seg.Item1.Y, textureDepth);

                                    Faces.Add(v0);
                                    Faces.Add(v1);
                                    Faces.Add(v2);

                                    Faces.Add(v0);
                                    Faces.Add(v2);
                                    Faces.Add(v3);
                                }
                            }
                        }
                    }
                    texturexY++;
                }
                texturexX++;
            }
        }

        private void CreateInverseSideFace(List<System.Windows.Point> pnts, int i, bool autoclose = true)
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

            int c0 = AddVerticeOctTree(pnts[i].X, pnts[i].Y, 0.0);
            int c1 = AddVerticeOctTree(pnts[i].X, pnts[i].Y, plateWidth);
            int c2 = AddVerticeOctTree(pnts[v].X, pnts[v].Y, plateWidth);
            int c3 = AddVerticeOctTree(pnts[v].X, pnts[v].Y, 0.0);
            Faces.Add(c0);
            Faces.Add(c1);
            Faces.Add(c2);

            Faces.Add(c0);
            Faces.Add(c2);
            Faces.Add(c3);
        }

        private void CreateInvertedSideFace(List<System.Windows.Point> pnts, int i, double offset = 0, bool autoclose = true)
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

            int c0 = AddVerticeOctTree(pnts[i].X, pnts[i].Y, offset);
            int c1 = AddVerticeOctTree(pnts[i].X, pnts[i].Y, plateWidth);
            int c2 = AddVerticeOctTree(pnts[v].X, pnts[v].Y, plateWidth);
            int c3 = AddVerticeOctTree(pnts[v].X, pnts[v].Y, offset);
            Faces.Add(c0);
            Faces.Add(c1);
            Faces.Add(c2);

            Faces.Add(c0);
            Faces.Add(c2);
            Faces.Add(c3);
        }

        private void CreateReversedSideFace(List<System.Windows.Point> pnts, int i, bool autoclose = true)
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

            int c0 = AddVerticeOctTree(pnts[i].X, pnts[i].Y, 0.0);
            int c1 = AddVerticeOctTree(pnts[i].X, pnts[i].Y, -plateWidth);
            int c2 = AddVerticeOctTree(pnts[v].X, pnts[v].Y, -plateWidth);
            int c3 = AddVerticeOctTree(pnts[v].X, pnts[v].Y, 0.0);
            Faces.Add(c0);
            Faces.Add(c1);
            Faces.Add(c2);

            Faces.Add(c0);
            Faces.Add(c2);
            Faces.Add(c3);
        }

        private void CreateSideFace(List<System.Windows.Point> pnts, int i, bool autoclose = true)
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

            int c0 = AddVerticeOctTree(pnts[i].X, pnts[i].Y, 0.0);
            int c1 = AddVerticeOctTree(pnts[i].X, pnts[i].Y, plateWidth);
            int c2 = AddVerticeOctTree(pnts[v].X, pnts[v].Y, plateWidth);
            int c3 = AddVerticeOctTree(pnts[v].X, pnts[v].Y, 0.0);
            Faces.Add(c0);
            Faces.Add(c2);
            Faces.Add(c1);

            Faces.Add(c0);
            Faces.Add(c3);
            Faces.Add(c2);
        }

        private void CreateTheBack(List<Point> tmp)
        {
            // triangulate the basic polygon
            List<Triangle> tris;
            TriangulationPolygon ply = new TriangulationPolygon();
            List<System.Drawing.PointF> pf = new List<System.Drawing.PointF>();
            foreach (System.Windows.Point p in tmp)
            {
                pf.Add(new System.Drawing.PointF((float)p.X, (float)p.Y));
            }
            ply.Points = pf.ToArray();

            tris = ply.Triangulate();
            foreach (Triangle t in tris)
            {
                int c0 = AddVerticeOctTree(t.Points[0].X, t.Points[0].Y, -plateWidth);
                int c1 = AddVerticeOctTree(t.Points[1].X, t.Points[1].Y, -plateWidth);
                int c2 = AddVerticeOctTree(t.Points[2].X, t.Points[2].Y, -plateWidth);
                Faces.Add(c0);
                Faces.Add(c2);
                Faces.Add(c1);
            }
        }

        private int FindClosestHole(List<Point> outLine, List<List<Point>> allHoles, out int op, out int hp)
        {
            op = -1;
            hp = -1;
            int closestHole = -1;
            double closestDist = double.MaxValue;
            for (int edgeIndex = 0; edgeIndex < outLine.Count; edgeIndex++)
            {
                Point edgePoint = outLine[edgeIndex];
                for (int holeIndex = 0; holeIndex < allHoles.Count; holeIndex++)
                {
                    for (int j = 0; j < allHoles[holeIndex].Count; j++)
                    {
                        double dx = allHoles[holeIndex][j].X - edgePoint.X;
                        double dy = allHoles[holeIndex][j].Y - edgePoint.Y;
                        double dist = Math.Sqrt((dx * dx) + (dy * dy));
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            closestHole = holeIndex;
                            op = edgeIndex;
                            hp = j;
                        }
                    }
                }
            }
            return closestHole;
        }

        private int FindVertexAtZero(List<Point> boundary, int sm)
        {
            Point testPoint = new Point(Vertices[sm].X, Vertices[sm].Y);
            if (PointOnBoundary(testPoint, boundary))
            {
                Point3D pAtZero = new Point3D(Vertices[sm].X, Vertices[sm].Y, 0.0);
                return AddVertice(pAtZero);
            }
            else
            {
                return -1;
            }
        }

        private void GenerateBox()
        {
            List<System.Windows.Point> points = PathEditor.GetOutsidePoints();
            List<System.Drawing.PointF> outerPolygon = new List<System.Drawing.PointF>();
            List<System.Drawing.PointF> innerPolygon = new List<System.Drawing.PointF>();
            ClearShape();
            List<System.Windows.Point> tmp = new List<System.Windows.Point>();
            double top = 0;
            double lx = double.MaxValue;
            double rx = double.MinValue;
            double ty = double.MinValue;
            double by = double.MaxValue;
            if (points != null)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    if (points[i].X < lx)
                    {
                        lx = points[i].X;
                    }
                    if (points[i].X > rx)
                    {
                        rx = points[i].X;
                    }
                    if (points[i].Y > ty)
                    {
                        ty = points[i].Y;
                    }
                    if (points[i].Y < by)
                    {
                        by = points[i].Y;
                    }
                }

                // user may have chosen a wallwidth that means the inside walls overlap DOnt allow that.
                double actualWallWidth = wallWidth;
                double d = ((rx - lx) / 2) - 0.1;
                if (d < actualWallWidth)
                {
                    actualWallWidth = d;
                }
                d = ((ty - by) / 2) - 0.1;
                if (d <= actualWallWidth)
                {
                    actualWallWidth = d;
                }

                top = ty;
                for (int i = 0; i < points.Count; i++)
                {
                    // flipping coordinates so have to reverse polygon too
                    tmp.Insert(0, new System.Windows.Point(points[i].X, top - points[i].Y));
                }

                for (int i = 0; i < tmp.Count - 1; i++)
                {
                    outerPolygon.Add(new System.Drawing.PointF((float)tmp[i].X, (float)tmp[i].Y));
                }
                outerPolygon = LineUtils.RemoveCoplanarSegments(outerPolygon);

                for (int i = 0; i < outerPolygon.Count; i++)
                {
                    innerPolygon.Add(new System.Drawing.PointF((float)outerPolygon[i].X, (float)outerPolygon[i].Y));
                }
                innerPolygon = LineUtils.GetEnlargedPolygon(innerPolygon, /*-*/ (float)actualWallWidth);

                tmp.Clear();
                for (int i = outerPolygon.Count - 1; i >= 0; i--)
                {
                    tmp.Add(new System.Windows.Point(outerPolygon[i].X, outerPolygon[i].Y));
                }

                CalculateExtents(tmp, out lx, out rx, out ty, out by);

                OctTree octTree = CreateOctree(new Point3D(-lx, -by, -1.5 * (plateWidth + textureDepth)),
          new Point3D(+rx, +ty, 1.5 * (plateWidth + textureDepth)));                // generate side triangles so original points are already in list
                                                                                    // Point[] clk =
                                                                                    // OrderClockwise(tmp.ToArray());
                                                                                    // tmp = clk.ToList();
                for (int i = 0; i < tmp.Count; i++)
                {
                    CreateInvertedSideFace(tmp, i);
                }

                tmp.Clear();
                for (int i = 0; i < innerPolygon.Count; i++)
                {
                    tmp.Add(new System.Windows.Point(innerPolygon[i].X, innerPolygon[i].Y));
                }

                // sides of indented area
                for (int i = 0; i < tmp.Count; i++)
                {
                    CreateInvertedSideFace(tmp, i, baseWidth);
                }

                // front of the wall
                for (int i = 0; i < outerPolygon.Count; i++)
                {
                    int j = i + 1;
                    if (j == outerPolygon.Count)
                    {
                        j = 0;
                    }

                    int c0 = AddVerticeOctTree(outerPolygon[i].X, outerPolygon[i].Y, plateWidth);
                    int c1 = AddVerticeOctTree(innerPolygon[i].X, innerPolygon[i].Y, plateWidth);
                    int c2 = AddVerticeOctTree(innerPolygon[j].X, innerPolygon[j].Y, plateWidth);
                    int c3 = AddVerticeOctTree(outerPolygon[j].X, outerPolygon[j].Y, plateWidth);
                    Faces.Add(c0);
                    Faces.Add(c2);
                    Faces.Add(c1);

                    Faces.Add(c0);
                    Faces.Add(c3);
                    Faces.Add(c2);
                }

                // Whole back
                TriangulationPolygon ply = new TriangulationPolygon();

                ply.Points = outerPolygon.ToArray();
                List<Triangle> tris = ply.Triangulate();
                foreach (Triangle t in tris)
                {
                    int c0 = AddVerticeOctTree(t.Points[0].X, t.Points[0].Y, 0.0);
                    int c1 = AddVerticeOctTree(t.Points[1].X, t.Points[1].Y, 0.0);
                    int c2 = AddVerticeOctTree(t.Points[2].X, t.Points[2].Y, 0.0);
                    Faces.Add(c0);
                    Faces.Add(c2);
                    Faces.Add(c1);
                }

                // front of the base
                ply.Points = innerPolygon.ToArray();
                tris = ply.Triangulate();
                foreach (Triangle t in tris)
                {
                    int c0 = AddVerticeOctTree(t.Points[0].X, t.Points[0].Y, baseWidth);
                    int c1 = AddVerticeOctTree(t.Points[1].X, t.Points[1].Y, baseWidth);
                    int c2 = AddVerticeOctTree(t.Points[2].X, t.Points[2].Y, baseWidth);
                    Faces.Add(c0);
                    Faces.Add(c1);
                    Faces.Add(c2);
                }
                CentreVertices();
            }
        }

        private void GenerateHollow()
        {
            List<System.Windows.Point> points = PathEditor.GetOutsidePoints(); ;
            List<System.Drawing.PointF> outerPolygon = new List<System.Drawing.PointF>();
            List<System.Drawing.PointF> innerPolygon = new List<System.Drawing.PointF>();
            ClearShape();
            List<System.Windows.Point> tmp = new List<System.Windows.Point>();

            double top = 0;
            double lx = double.MaxValue;
            double rx = double.MinValue;
            double ty = double.MinValue;
            double by = double.MaxValue;
            if (points != null)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    if (points[i].X < lx)
                    {
                        lx = points[i].X;
                    }
                    if (points[i].X > rx)
                    {
                        rx = points[i].X;
                    }
                    if (points[i].Y > ty)
                    {
                        ty = points[i].Y;
                    }
                    if (points[i].Y < by)
                    {
                        by = points[i].Y;
                    }
                }

                // user may have chosen a wallwidth that means the inside walls overlap DOnt allow that.
                double actualWallWidth = wallWidth;
                double d = ((rx - lx) / 2) - 0.1;
                if (d < actualWallWidth)
                {
                    actualWallWidth = d;
                }
                d = ((ty - by) / 2) - 0.1;
                if (d <= actualWallWidth)
                {
                    actualWallWidth = d;
                }

                top = ty;
                for (int i = 0; i < points.Count; i++)
                {
                    // flipping coordinates so have to reverse polygon too
                    tmp.Insert(0, new System.Windows.Point(points[i].X, top - points[i].Y));
                }
                // Point[] clik = OrderAntiClockwise(tmp.ToArray());
                for (int i = 0; i < tmp.Count - 1; i++)
                {
                    outerPolygon.Add(new System.Drawing.PointF((float)tmp[i].X, (float)tmp[i].Y));
                }
                outerPolygon = LineUtils.RemoveCoplanarSegments(outerPolygon);
                outerPolygon = LineUtils.RemoveDuplicatePoints(outerPolygon);
                for (int i = 0; i < outerPolygon.Count; i++)
                {
                    innerPolygon.Add(new System.Drawing.PointF((float)outerPolygon[i].X, (float)outerPolygon[i].Y));
                }
                innerPolygon = LineUtils.GetEnlargedPolygon(innerPolygon,/* - */(float)actualWallWidth);

                tmp.Clear();
                for (int i = outerPolygon.Count - 1; i >= 0; i--)
                {
                    tmp.Add(new System.Windows.Point(outerPolygon[i].X, outerPolygon[i].Y));
                }

                CalculateExtents(tmp, out lx, out rx, out ty, out by);

                OctTree octTree = CreateOctree(new Point3D(-lx, -by, -1.5 * (plateWidth + textureDepth)),
          new Point3D(+rx, +ty, 1.5 * (plateWidth + textureDepth)));                // generate side triangles so original points are already in list
                                                                                    // Point[] clk =
                                                                                    // OrderClockwise(tmp.ToArray());
                                                                                    // tmp = clk.ToList();
                for (int i = 0; i < tmp.Count; i++)
                {
                    CreateInvertedSideFace(tmp, i);
                }

                tmp.Clear();
                for (int i = 0; i < innerPolygon.Count; i++)
                {
                    tmp.Add(new System.Windows.Point(innerPolygon[i].X, innerPolygon[i].Y));
                }
                // generate side triangles so original points are already in list clk =
                // OrderAntiClockwise(tmp.ToArray()); tmp = clk.ToList();
                for (int i = 0; i < tmp.Count; i++)
                {
                    CreateInvertedSideFace(tmp, i);
                }

                for (int i = 0; i < outerPolygon.Count; i++)
                {
                    int j = i + 1;
                    if (j == outerPolygon.Count)
                    {
                        j = 0;
                    }
                    int c0 = AddVerticeOctTree(outerPolygon[i].X, outerPolygon[i].Y, 0.0);
                    int c1 = AddVerticeOctTree(innerPolygon[i].X, innerPolygon[i].Y, 0.0);
                    int c2 = AddVerticeOctTree(innerPolygon[j].X, innerPolygon[j].Y, 0.0);
                    int c3 = AddVerticeOctTree(outerPolygon[j].X, outerPolygon[j].Y, 0.0);
                    Faces.Add(c0);
                    Faces.Add(c1);
                    Faces.Add(c2);

                    Faces.Add(c0);
                    Faces.Add(c2);
                    Faces.Add(c3);

                    c0 = AddVerticeOctTree(outerPolygon[i].X, outerPolygon[i].Y, plateWidth);
                    c1 = AddVerticeOctTree(innerPolygon[i].X, innerPolygon[i].Y, plateWidth);
                    c2 = AddVerticeOctTree(innerPolygon[j].X, innerPolygon[j].Y, plateWidth);
                    c3 = AddVerticeOctTree(outerPolygon[j].X, outerPolygon[j].Y, plateWidth);
                    Faces.Add(c0);
                    Faces.Add(c2);
                    Faces.Add(c1);

                    Faces.Add(c0);
                    Faces.Add(c3);
                    Faces.Add(c2);
                }

                CentreVertices();
            }
        }

        private void GenerateShape()
        {
            if (loaded)
            {
                if (SolidShape)
                {
                    GenerateSolid();
                }
                else if (HollowShape)
                {
                    GenerateHollow();
                }
                else if (TexturedShape)
                {
                    GenerateTexturedShape();
                }
                else if (BoxShape)
                {
                    GenerateBox();
                }
            }
        }

        private void GenerateSoldWithHoles()
        {
            ClearShape();
            List<System.Windows.Point> points = PathEditor.GetOutsidePoints();
            if (points != null && points.Count > 3)
            {
                List<System.Windows.Point> outLineTmp = new List<System.Windows.Point>();
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
                    outLineTmp.Insert(0, new System.Windows.Point(points[i].X, top - points[i].Y));
                }
                double lx, rx, ty, by;
                CalculateExtents(outLineTmp, out lx, out rx, out ty, out by);

                OctTree octTree = CreateOctree(new Point3D(-lx, -by, -1.5 * (plateWidth + textureDepth)),
          new Point3D(+rx, +ty, 1.5 * (plateWidth + textureDepth)));

                // generate side triangles so original points are already in list
                for (int i = 0; i < outLineTmp.Count; i++)
                {
                    CreateSideFace(outLineTmp, i);
                }
                List<List<Point>> allHoles = new List<List<Point>>();
                // Now create the side faces for the holes
                for (int ip = 1; ip < PathEditor.NumberOfPaths; ip++)
                {
                    List<Point> holeTmp = new List<Point>();
                    List<System.Windows.Point> holePoints = PathEditor.GetPathPoints(ip);

                    for (int i = 0; i < holePoints.Count; i++)
                    {
                        if (holePoints[i].Y > top)
                        {
                            top = holePoints[i].Y;
                        }
                    }
                    for (int i = 0; i < holePoints.Count - 1; i++)
                    {
                        if (PathEditor.LocalImage == null)
                        {
                            // flipping coordinates so have to reverse polygon too
                            //tmp.Insert(0, new System.Windows.Point(holePoints[i].X, top - holePoints[i].Y));
                            holeTmp.Insert(0, new System.Windows.Point(holePoints[i].X, top - holePoints[i].Y));
                        }
                        else
                        {
                            double x = PathEditor.ToMM(holePoints[i].X);
                            double y = PathEditor.ToMM(top - holePoints[i].Y);
                            holeTmp.Insert(0, new System.Windows.Point(x, y));
                        }
                    }

                    for (int i = 0; i < holeTmp.Count; i++)
                    {
                        CreateInverseSideFace(holeTmp, i);
                    }
                    allHoles.Add(holeTmp);
                }
                ReverseDirection(allHoles);
                int outerPointIndex;
                int holePointIndex;

                while (allHoles.Count > 0)
                {
                    outerPointIndex = -1;
                    holePointIndex = -1;
                    int closestHole = FindClosestHole(outLineTmp, allHoles, out outerPointIndex, out holePointIndex);
                    if (closestHole != -1)
                    {
                        outLineTmp = JoinHoleToOutline(outLineTmp, allHoles[closestHole], outerPointIndex, holePointIndex);
                        allHoles.RemoveAt(closestHole);
                    }
                    else
                    {
                        allHoles.RemoveAt(0);
                    }
                }

                for (int i = 0; i < outLineTmp.Count; i++)
                {
                    CreateSideFace(outLineTmp, i);
                }

                // triangulate the basic polygon
                TriangulationPolygon ply = new TriangulationPolygon();
                List<System.Drawing.PointF> pf = new List<System.Drawing.PointF>();
                foreach (System.Windows.Point p in outLineTmp)
                {
                    pf.Add(new System.Drawing.PointF((float)p.X, (float)p.Y));
                }
                ply.Points = pf.ToArray();
                List<Triangle> tris = ply.Triangulate(true);
                //for ( int ti = 0; ti < tris.Count; ti++)
                foreach (Triangle t in tris)
                {
                    // Triangle t = tris[ti];
                    int c0 = AddVerticeOctTree(t.Points[0].X, t.Points[0].Y, 0.0);
                    int c1 = AddVerticeOctTree(t.Points[1].X, t.Points[1].Y, 0.0);
                    int c2 = AddVerticeOctTree(t.Points[2].X, t.Points[2].Y, 0.0);
                    Faces.Add(c0);
                    Faces.Add(c2);
                    Faces.Add(c1);

                    c0 = AddVerticeOctTree(t.Points[0].X, t.Points[0].Y, plateWidth);
                    c1 = AddVerticeOctTree(t.Points[1].X, t.Points[1].Y, plateWidth);
                    c2 = AddVerticeOctTree(t.Points[2].X, t.Points[2].Y, plateWidth);
                    Faces.Add(c0);
                    Faces.Add(c1);
                    Faces.Add(c2);
                }

                CentreVertices();
            }
        }

        private void GenerateSoldWithOutHoles()
        {
            ClearShape();
            List<System.Windows.Point> points = PathEditor.GetOutsidePoints();
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
                double lx, rx, ty, by;
                CalculateExtents(tmp, out lx, out rx, out ty, out by);

                OctTree octTree = CreateOctree(new Point3D(-lx, -by, -1.5 * (plateWidth + textureDepth)),
          new Point3D(+rx, +ty, 1.5 * (plateWidth + textureDepth)));

                for (int i = 0; i < tmp.Count; i++)
                {
                    CreateSideFace(tmp, i);
                }

                // triangulate the basic polygon
                TriangulationPolygon ply = new TriangulationPolygon();
                List<System.Drawing.PointF> pf = new List<System.Drawing.PointF>();
                foreach (System.Windows.Point p in tmp)
                {
                    pf.Add(new System.Drawing.PointF((float)p.X, (float)p.Y));
                }
                ply.Points = pf.ToArray();
                List<Triangle> tris = ply.Triangulate();
                foreach (Triangle t in tris)
                {
                    int c0 = AddVerticeOctTree(t.Points[0].X, t.Points[0].Y, 0.0);
                    int c1 = AddVerticeOctTree(t.Points[1].X, t.Points[1].Y, 0.0);
                    int c2 = AddVerticeOctTree(t.Points[2].X, t.Points[2].Y, 0.0);
                    Faces.Add(c0);
                    Faces.Add(c2);
                    Faces.Add(c1);

                    c0 = AddVerticeOctTree(t.Points[0].X, t.Points[0].Y, plateWidth);
                    c1 = AddVerticeOctTree(t.Points[1].X, t.Points[1].Y, plateWidth);
                    c2 = AddVerticeOctTree(t.Points[2].X, t.Points[2].Y, plateWidth);
                    Faces.Add(c0);
                    Faces.Add(c1);
                    Faces.Add(c2);
                }
                CentreVertices();
            }
        }

        private void GenerateSolid()
        {
            if (PathEditor.HasHoles())
            {
                GenerateSoldWithHoles();
            }
            else
            {
                GenerateSoldWithOutHoles();
            }
        }

        private void GenerateTexturedShape()
        {
            if (!String.IsNullOrEmpty(selectedTexture))
            {
                ClearShape();
                PathCentroid = PathEditor.Centroid();
                if (displayPoints?.Count > 3)
                {
                    List<System.Windows.Point> points = PathEditor.GetOutsidePoints();

                    List<System.Windows.Point> tmp = new List<System.Windows.Point>();
                    InvertVertical(points, tmp);
                    // Point[] clk = OrderClockwise(tmp.ToArray()); tmp = clk.ToList();
                    double lx, rx, ty, by;
                    CalculateExtents(tmp, out lx, out rx, out ty, out by);
                    double shapeHeight = Math.Abs(ty - by);
                    double shapeWidth = rx - lx;
                    vTextureResolution = 0.5;
                    hTextureResolution = 0.5;
                    if (textureManager.Mode == TextureManager.MapMode.FittedTile)
                    {
                        // estimate vertical size in steps
                        double ySteps = shapeHeight / vTextureResolution;
                        double vRepeats = (ySteps / textureManager.PatternHeight);
                        vRepeats = Math.Ceiling(vRepeats);
                        vTextureResolution = shapeHeight / (vRepeats * textureManager.PatternHeight);

                        double xSteps = shapeWidth / hTextureResolution;
                        double hRepeats = (xSteps / textureManager.PatternWidth);
                        hRepeats = Math.Ceiling(hRepeats);
                        hTextureResolution = (shapeWidth - 1) / (hRepeats * textureManager.PatternWidth);
                    }
                    if (textureManager.Mode == TextureManager.MapMode.FittedSingle)
                    {
                        // estimate vertical size in steps

                        vTextureResolution = shapeHeight / textureManager.PatternHeight;

                        hTextureResolution = shapeWidth / (textureManager.PatternWidth);

                        // should check if the original rsolution is smaller, if so add an offset to
                        // shift the pattern up or round
                    }
                    OctTree octTree = CreateOctree(new Point3D(-lx, -by, -1.5 * (plateWidth + textureDepth)),
              new Point3D(+rx, +ty, 1.5 * (plateWidth + textureDepth)));

                    double sz = 0.5;
                    if (!largeTexture)
                    {
                        sz = 0.25;
                    }

                    // generate side triangles so original points are already in list
                    for (int i = 0; i < tmp.Count; i++)
                    {
                        CreateReversedSideFace(tmp, i);
                    }

                    CreateTheBack(tmp);

                    ConvexPolygon2D boundary = new ConvexPolygon2D(tmp.ToArray());
                    ConvexPolygon2DHelper interceptor = new ConvexPolygon2DHelper();
                    // LoadTextureImage();

                    double off = 0;
                    int texturexX = 0;
                    int texturexY = 0;
                    for (double px = lx; px < rx; px += hTextureResolution)
                    {
                        texturexY = 0;
                        for (double py = by; py < ty; py += vTextureResolution)
                        {
                            ConvexPolygon2D rect = RectPoly(px, py, hTextureResolution, vTextureResolution);
                            ConvexPolygon2D interception = interceptor.GetIntersectionOfPolygons(rect, boundary);
                            if (interception.Corners.GetLength(0) > 2)
                            {
                                bool insidepixel = false;

                                TextureCell cell = textureManager.GetCell(texturexX, texturexY);
                                if (interception.Corners.GetLength(0) == 4)
                                {
                                    insidepixel = IsItAnInsidePixel(interception.Corners, px, py, hTextureResolution, vTextureResolution);
                                }
                                if (insidepixel)
                                {
                                    PolygonInside(tmp, hTextureResolution, vTextureResolution, off, px, py, cell);
                                }
                                else
                                {
                                    PolygonOnEdge(tmp, hTextureResolution, vTextureResolution, off, px, py, interception, cell);
                                }
                            }
                            texturexY++;
                        }
                        texturexX++;
                    }

                    CloseEdge(lx, by, rx, ty, tmp, sz);
                    CentreVertices();
                }
            }
        }

        /// <summary>
        /// Invert a list of points vertically
        /// </summary>
        /// <param name="points"></param>
        /// <param name="tmp"></param>
        private void InvertVertical(List<Point> points, List<Point> tmp)
        {
            double top = 0;
            for (int i = 0; i < points.Count - 1; i++)
            {
                if (points[i].Y > top)
                {
                    top = points[i].Y;
                }
            }
            for (int i = 0; i < points.Count - 1; i++)
            {
                tmp.Insert(0, new System.Windows.Point(points[i].X, top - points[i].Y));
            }
        }

        private bool IsItAnInsidePixel(Point[] c, double px, double py, double sx, double sy)
        {
            bool result = false;
            if (c[0].X == px && c[0].Y == py)
            {
                if (c[1].X == px + sx && c[1].Y == py)
                {
                    if (c[2].X == px + sx && c[2].Y == py + sy)
                    {
                        if (c[3].X == px && c[3].Y == py + sy)
                        {
                            result = true;
                        }
                    }
                }
            }
            return result;
        }

        private List<Point> JoinHoleToOutline(List<Point> outLineTmp, List<Point> points, int outerPointIndex, int holePointIndex)
        {
            List<Point> combined = new List<Point>();
            for (int si = 0; si <= outerPointIndex; si++)
            {
                combined.Add(outLineTmp[si]);
            }

            for (int hi = holePointIndex; hi < points.Count; hi++)
            {
                combined.Add(points[hi]);
            }
            for (int hi = 0; hi <= holePointIndex; hi++)
            {
                combined.Add(points[hi]);
            }
            for (int si = outerPointIndex; si < outLineTmp.Count; si++)
            {
                combined.Add(outLineTmp[si]);
            }
            return combined;
        }

        private void LoadEditorParameters()
        {
            string imageName = EditorParameters.Get("ImagePath");
            if (imageName != "")
            {
                PathEditor.LoadImage(imageName);
            }
            int numPaths = EditorParameters.GetInt("NumPath", 0);
            for (int i = 0; i < numPaths; i++)
            {
                String s = EditorParameters.Get("Path_" + i.ToString());
                if (s != "")
                {
                    PathEditor.SetPath(s, i);
                }
            }
            WallWidth = EditorParameters.GetDouble("WallWidth", 2);
            PlateWidth = EditorParameters.GetDouble("PlateWidth", 10);
            TextureDepth = EditorParameters.GetDouble("TextureDepth", 0.5);
            HollowShape = EditorParameters.GetBoolean("Hollow", false);
            TexturedShape = EditorParameters.GetBoolean("Textured", false);
            SolidShape = EditorParameters.GetBoolean("Solid", true);
            BoxShape = EditorParameters.GetBoolean("Box", false);
            LargeTexture = EditorParameters.GetBoolean("LargeTexture", true);
            SmallTexture = EditorParameters.GetBoolean("SmallTexture", false);
            TileTexture = EditorParameters.GetBoolean("CentreTexture", true);
            SelectedTexture = EditorParameters.Get("SelectedTexture");
            textureManager.LoadTextureImage(selectedTexture);
            ClippedTile = EditorParameters.GetBoolean("ClippedTile", true);
            FittedTile = EditorParameters.GetBoolean("FittedTile", false);
            ClippedSingle = EditorParameters.GetBoolean("ClippedSingle", false);
            FittedSingle = EditorParameters.GetBoolean("FittedSingle", false);
            BaseWidth = EditorParameters.GetDouble("BaseWidth", 2);
            int v = EditorParameters.GetInt("ShowGrid", 1);
            PathEditor.ShowGrid = (UserControls.GridSettings.GridStyle)v;
        }

        private void PathPointsChanged(List<System.Windows.Point> pnts)
        {
            displayPoints = pnts;
            if (PathEditor.PathClosed || pnts.Count == 0)
            {
                UpdateDisplay();
            }
        }

        private bool PointInTriangle(Point pt, Point v1, Point v2, Point v3)
        {
            double d1, d2, d3;
            bool has_neg, has_pos;

            d1 = sign(pt, v1, v2);
            d2 = sign(pt, v2, v3);
            d3 = sign(pt, v3, v1);

            has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(has_neg && has_pos);
        }

        private bool PointOnBoundary(Point testPoint, List<Point> boundary)
        {
            bool result = false;
            for (int i = 0; i < boundary.Count; i++)
            {
                int j = (i + 1) % boundary.Count;
                if (PointOnLineSegment(boundary[i], boundary[j], testPoint))
                {
                    return true;
                }
            }
            return result;
        }

        private void PolygonInside(List<Point> tmp, double sx, double sy, double off, double px, double py, TextureCell cell)
        {
            double z = 0;
            if (cell != null)
            {
                z = ((double)cell.Width * textureDepth) / 255.0;
                // main surface
                int v0 = AddVerticeOctTree(px, py, z);
                int v1 = AddVerticeOctTree(px + sx, py, z);
                int v2 = AddVerticeOctTree(px + sx, py + sy, z);
                int v3 = AddVerticeOctTree(px, py + sy, z);
                Faces.Add(v0);
                Faces.Add(v1);
                Faces.Add(v2);

                Faces.Add(v0);
                Faces.Add(v2);
                Faces.Add(v3);

                if (cell.WestWall > 0)
                {
                    double westSideDepth = ((double)(cell.Width - cell.WestWall) * textureDepth) / 255.0;

                    v0 = AddVerticeOctTree(px, py, z);
                    v1 = AddVerticeOctTree(px, py, westSideDepth);
                    v2 = AddVerticeOctTree(px, py + sy, westSideDepth);
                    v3 = AddVerticeOctTree(px, py + sy, z);
                    Faces.Add(v0);
                    Faces.Add(v2);
                    Faces.Add(v1);

                    Faces.Add(v0);
                    Faces.Add(v3);
                    Faces.Add(v2);
                }

                if (cell.EastWall > 0)
                {
                    double eastSideDepth = ((double)(cell.Width - cell.EastWall) * textureDepth) / 255.0;
                    v0 = AddVerticeOctTree(px + sx, py, z);
                    v1 = AddVerticeOctTree(px + sx, py, eastSideDepth);
                    v2 = AddVerticeOctTree(px + sx, py + sy, eastSideDepth);
                    v3 = AddVerticeOctTree(px + sx, py + sy, z);
                    Faces.Add(v0);
                    Faces.Add(v1);
                    Faces.Add(v2);

                    Faces.Add(v0);
                    Faces.Add(v2);
                    Faces.Add(v3);
                }

                if (cell.NorthWall > 0)
                {
                    double sideDepth = ((double)(cell.Width - cell.NorthWall) * textureDepth) / 255.0;
                    v0 = AddVerticeOctTree(px, py, z);
                    v1 = AddVerticeOctTree(px + sx, py, z);
                    v2 = AddVerticeOctTree(px + sx, py, sideDepth);
                    v3 = AddVerticeOctTree(px, py, sideDepth);

                    AddFace(v0, v2, v1);
                    AddFace(v0, v3, v2);
                }
                if (cell.SouthWall > 0)
                {
                    double sideDepth = ((double)(cell.Width - cell.SouthWall) * textureDepth) / 255.0;
                    v0 = AddVerticeOctTree(px, py + sy, z);
                    v1 = AddVerticeOctTree(px + sx, py + sy, z);
                    v2 = AddVerticeOctTree(px + sx, py + sy, sideDepth);
                    v3 = AddVerticeOctTree(px, py + sy, sideDepth);

                    AddFace(v0, v1, v2);
                    AddFace(v0, v2, v3);
                }
            }
        }

        private void PolygonOnEdge(List<Point> tmp, double sx, double sy, double off, double px, double py, ConvexPolygon2D interception, TextureCell cell)
        {
            TriangulationPolygon ply;
            List<Triangle> tris;
            double z;
            if (interception.Corners.GetLength(0) == 3)
            {
                z = ((double)cell.Width * textureDepth) / 255.0;
                int v0 = AddVerticeOctTree(interception.Corners[0].X, interception.Corners[0].Y, z);
                int v1 = AddVerticeOctTree(interception.Corners[1].X, interception.Corners[1].Y, z);
                int v2 = AddVerticeOctTree(interception.Corners[2].X, interception.Corners[2].Y, z);

                Faces.Add(v0);
                Faces.Add(v1);
                Faces.Add(v2);
                if (cell.Width > 0)
                {
                    if (cell.WestWall > 0)
                    {
                        if (IsPointInPolygon(new Point(px - off, py + off), tmp))
                        {
                            int s0 = AddVerticeOctTree(px, py, 0);
                            int s1 = AddVerticeOctTree(px, py, z);
                            int s2 = AddVerticeOctTree(px, py + sy, z);
                            int s3 = AddVerticeOctTree(px, py + sy, 0);
                            Faces.Add(s0);
                            Faces.Add(s2);
                            Faces.Add(s1);

                            Faces.Add(s0);
                            Faces.Add(s3);
                            Faces.Add(s2);
                        }
                    }
                }
                if (cell.EastWall != 0)
                {
                    if (IsPointInPolygon(new Point(px + sx + off, py + off), tmp))
                    {
                        int s0 = AddVerticeOctTree(px + sx, py, 0);
                        int s1 = AddVerticeOctTree(px + sx, py, z);
                        int s2 = AddVerticeOctTree(px + sx, py + sy, z);
                        int s3 = AddVerticeOctTree(px + sx, py + sy, 0);
                        Faces.Add(s0);
                        Faces.Add(s1);
                        Faces.Add(s2);

                        Faces.Add(s0);
                        Faces.Add(s2);
                        Faces.Add(s3);
                    }
                }
                if (cell.SouthWall != 0)
                {
                    if (IsPointInPolygon(new Point(px + off, py + sy + off), tmp))
                    {
                        int s0 = AddVerticeOctTree(px, py + sy, 0);
                        int s1 = AddVerticeOctTree(px + sx, py + sy, 0);
                        int s2 = AddVerticeOctTree(px + sx, py + sy, z);
                        int s3 = AddVerticeOctTree(px, py + sy, z
                        );
                        Faces.Add(s0);
                        Faces.Add(s1);
                        Faces.Add(s2);

                        Faces.Add(s0);
                        Faces.Add(s2);
                        Faces.Add(s3);
                    }
                }

                if (cell.NorthWall != 0)
                {
                    if (IsPointInPolygon(new Point(px + off, py - off), tmp))
                    {
                        int s0 = AddVerticeOctTree(px, py, 0);
                        int s1 = AddVerticeOctTree(px + sx, py, 0);
                        int s2 = AddVerticeOctTree(px + sx, py, z);
                        int s3 = AddVerticeOctTree(px, py, z);
                        Faces.Add(s0);
                        Faces.Add(s2);
                        Faces.Add(s1);

                        Faces.Add(s0);
                        Faces.Add(s3);
                        Faces.Add(s2);
                    }
                }
            }
            else
            {
                z = ((double)cell.Width * textureDepth) / 255.0;

                ply = new TriangulationPolygon();
                List<System.Drawing.PointF> pfr = new List<System.Drawing.PointF>();
                foreach (System.Windows.Point p in interception.Corners)
                {
                    pfr.Add(new System.Drawing.PointF((float)p.X, (float)p.Y));
                }
                ply.Points = pfr.ToArray();
                tris = ply.Triangulate();
                foreach (Triangle t in tris)
                {
                    int v0 = AddVerticeOctTree(t.Points[0].X, t.Points[0].Y, z);
                    int v1 = AddVerticeOctTree(t.Points[1].X, t.Points[1].Y, z);
                    int v2 = AddVerticeOctTree(t.Points[2].X, t.Points[2].Y, z);

                    Faces.Add(v0);
                    Faces.Add(v1);
                    Faces.Add(v2);
                }
            }
        }

        private bool PtInTriangle(double px, double py, Point3DCollection vt, int p0, int p1, int p2)
        {
            bool res = PointInTriangle(new Point(px, py),
            new Point(vt[p0].X, vt[p0].Y),
            new Point(vt[p1].X, vt[p1].Y),
            new Point(vt[p2].X, vt[p2].Y));
            return res;
        }

        private void ReverseDirection(List<List<Point>> allHoles)
        {
            for (int i = 0; i < allHoles.Count; i++)
            {
                List<Point> tmp = new List<Point>();
                foreach (Point p in allHoles[i])
                {
                    tmp.Insert(0, p);
                }
                allHoles[i] = tmp;
            }
        }

        private void SaveEditorParameters()
        {
            EditorParameters.Set("ImagePath", PathEditor.ImagePath);
            EditorParameters.Set("NumPath", PathEditor.NumberOfPaths);

            for (int i = 0; i < PathEditor.NumberOfPaths; i++)
            {
                EditorParameters.Set("Path_" + i.ToString(), PathEditor.GetPathText(i));
            }
            EditorParameters.Set("WallWidth", WallWidth.ToString());
            EditorParameters.Set("PlateWidth", PlateWidth.ToString());
            EditorParameters.Set("TextureDepth", TextureDepth.ToString());
            EditorParameters.Set("Hollow", HollowShape.ToString());
            EditorParameters.Set("Solid", SolidShape.ToString());
            EditorParameters.Set("Box", BoxShape.ToString());
            EditorParameters.Set("Textured", TexturedShape.ToString());
            EditorParameters.Set("LargeTexture", LargeTexture.ToString());
            EditorParameters.Set("SmallTexture", SmallTexture.ToString());
            EditorParameters.Set("SelectedTexture", SelectedTexture);
            EditorParameters.Set("CentreTexture", TileTexture.ToString());
            EditorParameters.Set("ClippedTile", ClippedTile.ToString());
            EditorParameters.Set("FittedTile", FittedTile.ToString());
            EditorParameters.Set("ClippedSingle", ClippedSingle.ToString());
            EditorParameters.Set("FittedSingle", FittedSingle.ToString());
            EditorParameters.Set("BaseWidth", BaseWidth.ToString());
            EditorParameters.Set("ShowGrid", ((int)(PathEditor.ShowGrid)).ToString());
        }

        private void SetShapeTab()
        {
            if (SolidShape)
            {
                SelectedShape = 0;
            }
            if (HollowShape)
            {
                SelectedShape = 1;
            }
            if (TexturedShape)
            {
                SelectedShape = 2;
            }
            if (BoxShape)
            {
                SelectedShape = 3;
            }
        }

        private void ShapeChanged()
        {
            switch (selectedShape)
            {
                case 0:
                    {
                        HollowShape = false;
                        TexturedShape = false;
                        BoxShape = false;
                        SolidShape = true;
                    }
                    break;

                case 1:
                    {
                        SolidShape = false;
                        TexturedShape = false;
                        BoxShape = false;
                        HollowShape = true;
                    }
                    break;

                case 2:
                    {
                        SolidShape = false;
                        HollowShape = false;
                        BoxShape = false;
                        TexturedShape = true;
                    }
                    break;

                case 3:
                    {
                        SolidShape = false;
                        HollowShape = false;
                        TexturedShape = false;
                        BoxShape = true;
                    }
                    break;
            }
        }

        private List<System.Drawing.PointF> ShrinkPolygon(List<System.Drawing.PointF> ply, double offset)
        {
            List<System.Drawing.PointF> res = new List<System.Drawing.PointF>();
            float cX = 0.0F;
            float cY = 0.0F;
            foreach (System.Drawing.PointF pf in ply)
            {
                cX += pf.X;
                cY += pf.Y;
            }
            cX /= ply.Count;
            cY /= ply.Count;

            float dx;
            float dy;
            foreach (System.Drawing.PointF pf in ply)
            {
                dx = pf.X - cX;
                dy = pf.Y - cY;
                double dist = Math.Sqrt((dx * dx) + (dy * dy));
                double theta = Math.Atan2(dy, dx);
                if (dist > offset)
                {
                    dist -= offset;
                }
                System.Drawing.PointF np = new System.Drawing.PointF((float)(dist * Math.Cos(theta)) + cX, (float)(dist * Math.Sin(theta)) + cY);
                res.Add(np);
            }
            return res;
        }

        private double sign(Point p1, Point p2, Point p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
        }

        private void UpdateDisplay()
        {
            if (loaded)
            {
                GenerateShape();
                Viewer.Model = GetModel();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            loaded = false;

            LoadEditorParameters();
            PathEditor.DefaultImagePath = DefaultImagePath;
            PathEditor.OpenEndedPath = false;
            SetShapeTab();
            loaded = true;
            UpdateCameraPos();
            warningText = "";
            UpdateDisplay();
        }
    }

    public class TextureTriangle
    {
        public bool inside;
        public int p0;
        public int p1;
        public int p2;

        public TextureTriangle(int c0, int c1, int c2)
        {
            p0 = c0;
            p1 = c1;
            p2 = c2;
        }
    }
}