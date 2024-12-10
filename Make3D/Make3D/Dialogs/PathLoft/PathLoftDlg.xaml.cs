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

using Barnacle.Models.BufferedPolyline;
using OctTreeLib;
using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Media3D;
using static Barnacle.Models.BufferedPolyline.BufferedPolyline;
using Triangle = PolygonTriangulationLib.Triangle;

namespace Barnacle.Dialogs
{
    public class NameOfProfile : INotifyPropertyChanged
    {
        private bool isSelected;

        public NameOfProfile(string n, string u)
        {
            Name = n;
            ImageUri = new Uri(@"pack://application:,,,/Images/Buttons/" + u);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Uri ImageUri
        {
            get; set;
        }

        public bool IsSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                if (value != isSelected)
                {
                    isSelected = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string Name
        {
            get; set;
        }

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    /// <summary>
    /// Interaction logic for PathLoft.xaml
    /// </summary>
    public partial class PathLoftDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private const double maxloftHeight = 200;
        private const double maxloftThickness = 200;
        private const double minBaseThickness = 0.1;
        private const double minloftHeight = 0.1;
        private const double minloftThickness = 0.1;
        private double baseThickness;

        private bool flatShape;
        private bool loaded;
        private double loftHeight;
        private double loftThickness;
        private OctTree octTree;
        private List<Point> pathPoints;
        private int profileIndex;
        private List<NameOfProfile> profileNames;
        private Visibility ubeamVisibility;
        private string warningText;
        private double xExtent;
        private double yExtent;

        public PathLoftDlg()
        {
            InitializeComponent();
            ToolName = "PathLoft";
            DataContext = this;

            loaded = false;
            PathEditor.OnFlexiPathChanged += PathPointsChanged;
            pathPoints = new List<Point>();
            PathEditor.ToolName = ToolName;
            PathEditor.HasPresets = false;
            PathEditor.ShowAppend = true;
            PathEditor.ContinuousPointsNotify = true;
            profileNames = new List<NameOfProfile>();
            profileNames.Add(new NameOfProfile("Flat", "FlatProfile.png"));
            profileNames.Add(new NameOfProfile("Round", "RoundProfile.png"));
            profileNames.Add(new NameOfProfile("Square", "SquareProfile.png"));
            profileNames.Add(new NameOfProfile("UBeam", "UBeamProfile.png"));
            profileNames.Add(new NameOfProfile("Wedge", "WedgeProfile.png"));
            profileNames.Add(new NameOfProfile("Wedge2", "Wedge2Profile.png"));
            profileNames[0].IsSelected = true;
        }

        private enum ProfileIndices
        {
            FlatProfile,
            RoundProfile,
            SquareProfile,
            UBeamProfile,
            WedgeProfile,
            WedgeProfile2
        }

        public double BaseThickness
        {
            get
            {
                return baseThickness;
            }
            set
            {
                if (value != baseThickness)
                {
                    if (value > minBaseThickness)
                    {
                        baseThickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String BaseThicknessToolTip
        {
            get
            {
                return $"BaseThickness must be in the range {minBaseThickness} to Loft Thickness";
            }
        }

        public double LoftHeight
        {
            get
            {
                return loftHeight;
            }

            set
            {
                if (loftHeight != value)
                {
                    if (value >= minloftHeight && value <= maxloftHeight)
                    {
                        loftHeight = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String LoftHeightToolTip
        {
            get
            {
                return $"LoftHeight must be in the range {minloftHeight} to {maxloftHeight}";
            }
        }

        public double LoftThickness

        {
            get
            {
                return loftThickness;
            }

            set
            {
                if (loftThickness != value)
                {
                    if (value >= minloftThickness && value <= maxloftThickness)
                    {
                        loftThickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String LoftThicknessToolTip
        {
            get
            {
                return $"LoftThickness must be in the range {minloftThickness} to {maxloftThickness}";
            }
        }

        public int ProfileIndex
        {
            get
            {
                return profileIndex;
            }
            set
            {
                if (value != profileIndex)
                {
                    profileIndex = value;
                    NotifyPropertyChanged();
                    if (profileIndex == (int)ProfileIndices.UBeamProfile)
                    {
                        UBeamVisibility = Visibility.Visible;
                    }
                    else
                    {
                        UBeamVisibility = Visibility.Hidden;
                    }
                    UpdateDisplay();
                }
            }
        }

        public List<NameOfProfile> ProfileNames
        {
            get
            {
                return profileNames;
            }
        }

        public Visibility UBeamVisibility
        {
            get
            {
                return ubeamVisibility;
            }
            set
            {
                if (value != ubeamVisibility)
                {
                    ubeamVisibility = value;
                    NotifyPropertyChanged();
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
            octTree = new OctTree(Vertices, minPoint, maxPoint, 200);
            return octTree;
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
            xExtent = rx - lx;
            yExtent = ty - by;
        }

        private void CreateSideFace(List<System.Windows.Point> pnts, int i)
        {
            int v = i + 1;

            if (v == pnts.Count)
            {
                v = 0;
            }

            int c0 = AddVerticeOctTree(pnts[i].X, pnts[i].Y, 0.0);
            int c1 = AddVerticeOctTree(pnts[i].X, pnts[i].Y, loftHeight);
            int c2 = AddVerticeOctTree(pnts[v].X, pnts[v].Y, loftHeight);
            int c3 = AddVerticeOctTree(pnts[v].X, pnts[v].Y, 0.0);
            Faces.Add(c0);
            Faces.Add(c2);
            Faces.Add(c1);

            Faces.Add(c0);
            Faces.Add(c3);
            Faces.Add(c2);
        }

        private void CreateSolidFromProfile(int numProfilePoints, double lx, double rx, double ty, double by, double bz, double fz, List<CurvePoint> curvePoints, Point3D[,] surface)
        {
            octTree = CreateOctree(new Point3D(lx - 10, by - 10, bz - 10),
                                   new Point3D(rx + 10, ty + 10, fz + 10));

            for (int i = 0; i < curvePoints.Count - 1; i++)
            {
                for (int j = 0; j < numProfilePoints; j++)
                {
                    int k = j + 1;
                    if (k == numProfilePoints)
                    {
                        k = 0;
                    }

                    int p0 = AddVerticeOctTree(surface[i, j].X, ty - surface[i, j].Y, surface[i, j].Z);
                    int p1 = AddVerticeOctTree(surface[i + 1, j].X, ty - surface[i + 1, j].Y, surface[i + 1, j].Z);
                    int p2 = AddVerticeOctTree(surface[i, k].X, ty - surface[i, k].Y, surface[i, k].Z);
                    int p3 = AddVerticeOctTree(surface[i + 1, k].X, ty - surface[i + 1, k].Y, surface[i + 1, k].Z);

                    Faces.Add(p0);
                    Faces.Add(p1);
                    Faces.Add(p2);

                    Faces.Add(p1);
                    Faces.Add(p3);
                    Faces.Add(p2);
                }
            }

            // close start
            int cp = AddVerticeOctTree(curvePoints[0].point.X, ty - curvePoints[0].point.Y, 0);
            for (int j = 0; j < numProfilePoints; j++)
            {
                int k = j + 1;
                if (k == numProfilePoints)
                {
                    k = 0;
                }

                int p1 = AddVerticeOctTree(surface[0, j].X, ty - surface[0, j].Y, surface[0, j].Z);
                int p2 = AddVerticeOctTree(surface[0, k].X, ty - surface[0, k].Y, surface[0, k].Z);

                Faces.Add(cp);
                Faces.Add(p1);
                Faces.Add(p2);
            }

            // close end
            int last = curvePoints.Count - 1;
            cp = AddVerticeOctTree(curvePoints[last].point.X, ty - curvePoints[last].point.Y, 0);
            for (int j = 0; j < numProfilePoints; j++)
            {
                int k = j + 1;
                if (k == numProfilePoints)
                {
                    k = 0;
                }

                int p1 = AddVerticeOctTree(surface[last, j].X, ty - surface[last, j].Y, surface[last, j].Z);
                int p2 = AddVerticeOctTree(surface[last, k].X, ty - surface[last, k].Y, surface[last, k].Z);

                Faces.Add(cp);
                Faces.Add(p2);
                Faces.Add(p1);
            }
        }

        private Point3D[,] CreateSurfaceFromProfile(List<CurvePoint> curvePoints, List<Point3D> profile, ref double lx, ref double rx, ref double ty, ref double by, ref double bz, ref double fz)
        {
            int numProfilePoints = profile.Count;
            Point3D[,] surface = new Point3D[curvePoints.Count, numProfilePoints];
            for (int i = 0; i < curvePoints.Count; i++)
            {
                double rotation = curvePoints[i].angle;
                List<Point3D> tmp = RotatePoints(profile, 0, rotation, 0);
                for (int j = 0; j < numProfilePoints; j++)
                {
                    Point3D pn = tmp[j];
                    pn.X += curvePoints[i].point.X;
                    pn.Y += curvePoints[i].point.Y;

                    surface[i, j] = pn;
                    if (pn.X < lx) lx = pn.X;
                    if (pn.X > rx) rx = pn.X;
                    if (pn.Y < by) by = pn.Y;
                    if (pn.Y > ty) ty = pn.Y;
                    if (pn.Z < bz) bz = pn.Z;
                    if (pn.Z > fz) fz = pn.Z;
                }
            }

            return surface;
        }

        private void CreateTubeInnerSideFace(List<System.Windows.Point> pnts, int i, double baseHeight)
        {
            int v = i + 1;

            if (v == pnts.Count)
            {
                v = 0;
            }

            int c0 = AddVerticeOctTree(pnts[i].X, pnts[i].Y, baseHeight);
            int c1 = AddVerticeOctTree(pnts[i].X, pnts[i].Y, loftHeight);
            int c2 = AddVerticeOctTree(pnts[v].X, pnts[v].Y, loftHeight);
            int c3 = AddVerticeOctTree(pnts[v].X, pnts[v].Y, baseHeight);
            Faces.Add(c0);
            Faces.Add(c1);
            Faces.Add(c2);

            Faces.Add(c0);
            Faces.Add(c2);
            Faces.Add(c3);
        }

        private List<Point3D> GenerateCircularProfile(int numProfilePoints, BufferedPolyline bl)
        {
            List<Point3D> profile = new List<Point3D>();
            double dt = (2 * Math.PI) / numProfilePoints;
            double theta = 0;
            for (int i = 0; i < numProfilePoints; i++)
            {
                Point p = CalcPoint(theta, bl.BufferRadius);
                profile.Add(new Point3D(0, p.Y, p.X));
                theta += dt;
            }

            return profile;
        }

        private void GenerateFlat()
        {
            BufferedPolyline bl = new BufferedPolyline(pathPoints);
            bl.BufferRadius = loftThickness / 2;
            List<Point> outline = bl.GenerateBufferOutline();
            if (outline != null && outline.Count > 3)
            {
                List<System.Windows.Point> tmp = new List<System.Windows.Point>();
                double top = 0;
                for (int i = 0; i < outline.Count; i++)
                {
                    if (outline[i].Y > top)
                    {
                        top = outline[i].Y;
                    }
                }
                for (int i = 0; i < outline.Count; i++)
                {
                    tmp.Add(new Point(outline[i].X, top - outline[i].Y));
                }
                double lx, rx, ty, by;
                CalculateExtents(tmp, out lx, out rx, out ty, out by);

                octTree = CreateOctree(new Point3D(-lx, -by, -1),
                                        new Point3D(+rx, +ty, loftHeight + 1));

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

                    c0 = AddVerticeOctTree(t.Points[0].X, t.Points[0].Y, loftHeight);
                    c1 = AddVerticeOctTree(t.Points[1].X, t.Points[1].Y, loftHeight);
                    c2 = AddVerticeOctTree(t.Points[2].X, t.Points[2].Y, loftHeight);
                    Faces.Add(c0);
                    Faces.Add(c1);
                    Faces.Add(c2);
                }

                CentreVertices();
            }
        }

        private void GenerateRound(int numProfilePoints)
        {
            double lx, rx, ty, by, bz, fz;
            lx = by = bz = double.MaxValue;
            rx = ty = fz = double.MinValue;
            BufferedPolyline bl = null;
            List<CurvePoint> curvePoints = null;
            GetBufferedPolyLine(out bl, out curvePoints);
            if (curvePoints != null && curvePoints.Count > 1)
            {
                List<Point3D> profile = GenerateCircularProfile(numProfilePoints, bl);

                Point3D[,] surface = CreateSurfaceFromProfile(curvePoints, profile, ref lx, ref rx, ref ty, ref by, ref bz, ref fz);
                if (surface != null)
                {
                    CreateSolidFromProfile(numProfilePoints, lx, rx, ty, by, bz, fz, curvePoints, surface);
                    CentreVertices();
                }
            }
        }

        private void GenerateShape()
        {
            ClearShape();
            if (pathPoints != null && pathPoints.Count > 0)
            {
                switch ((ProfileIndices)profileIndex)
                {
                    case ProfileIndices.FlatProfile:
                        {
                            GenerateFlat();
                        }
                        break;

                    case ProfileIndices.RoundProfile:
                        {
                            GenerateRound(36);
                        }
                        break;

                    case ProfileIndices.SquareProfile:
                        {
                            GenerateRound(4);
                        }
                        break;

                    case ProfileIndices.UBeamProfile:
                        {
                            GenerateU();
                        }
                        break;

                    case ProfileIndices.WedgeProfile:
                        {
                            GenerateWedge();
                        }
                        break;

                    case ProfileIndices.WedgeProfile2:
                        {
                            GenerateWedge(true);
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        private void GenerateU()
        {
            double backThickness = loftThickness / 2;
            double frontThickness = backThickness * 0.5;
            double baseHeight = baseThickness;
            if (baseHeight > loftHeight)
            {
                baseHeight = loftHeight;
            }
            double top = 0;
            Point startEdgeP0 = new Point(0, 0);
            Point startEdgeP1 = new Point(0, 0);
            Point startEdgeP2 = new Point(0, 0);
            Point startEdgeP3 = new Point(0, 0);

            Point endEdgeP0 = new Point(0, 0);
            Point endEdgeP1 = new Point(0, 0);
            Point endEdgeP2 = new Point(0, 0);
            Point endEdgeP3 = new Point(0, 0);

            int c0;
            int c1;
            int c2;
            int c3;

            int endOfOutbound = -1;
            List<System.Windows.Point> tmp;
            tmp = new List<System.Windows.Point>();
            List<System.Windows.Point> tmp2;
            tmp2 = new List<System.Windows.Point>();
            // generate back
            BufferedPolyline bl = new BufferedPolyline(pathPoints);
            bl.BufferRadius = backThickness;
            List<Point> outline = bl.GenerateBufferOutline();
            endOfOutbound = bl.LastOutBoundIndex;
            if (outline != null && outline.Count > 3)
            {
                for (int i = 0; i < outline.Count; i++)
                {
                    if (outline[i].Y > top)
                    {
                        top = outline[i].Y;
                    }
                }
                for (int i = 0; i < outline.Count; i++)
                {
                    tmp.Add(new Point(outline[i].X, top - outline[i].Y));
                }
                startEdgeP0 = tmp[0];
                startEdgeP1 = tmp[tmp.Count - 1];
                endEdgeP0 = tmp[endOfOutbound];
                endEdgeP1 = tmp[endOfOutbound + 1];
                double lx, rx, ty, by;
                CalculateExtents(tmp, out lx, out rx, out ty, out by);

                octTree = CreateOctree(new Point3D(-lx, -by, -1),
                                        new Point3D(+rx, +ty, loftHeight + 1));

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
                    c0 = AddVerticeOctTree(t.Points[0].X, t.Points[0].Y, 0.0);
                    c1 = AddVerticeOctTree(t.Points[1].X, t.Points[1].Y, 0.0);
                    c2 = AddVerticeOctTree(t.Points[2].X, t.Points[2].Y, 0.0);

                    Faces.Add(c0);
                    Faces.Add(c2);
                    Faces.Add(c1);
                }

                for (int i = 0; i < endOfOutbound /*- 1*/; i++)
                {
                    CreateSideFace(tmp, i);
                }

                for (int i = endOfOutbound + 1; i < tmp.Count - 1; i++)
                {
                    CreateSideFace(tmp, i);
                }
            }

            // generate front of base
            bl = new BufferedPolyline(pathPoints);
            bl.BufferRadius = frontThickness;
            outline = new List<Point>();
            outline = bl.GenerateBufferOutline();
            if (outline != null && outline.Count > 3)
            {
                for (int i = 0; i < outline.Count; i++)
                {
                    tmp2.Add(new Point(outline[i].X, top - outline[i].Y));
                }
                startEdgeP2 = tmp2[0];
                startEdgeP3 = tmp2[tmp2.Count - 1];
                endEdgeP2 = tmp2[endOfOutbound];
                endEdgeP3 = tmp2[endOfOutbound + 1];
                // triangulate the basic polygon
                TriangulationPolygon ply = new TriangulationPolygon();
                List<System.Drawing.PointF> pf = new List<System.Drawing.PointF>();
                foreach (System.Windows.Point p in tmp2)
                {
                    pf.Add(new System.Drawing.PointF((float)p.X, (float)p.Y));
                }
                ply.Points = pf.ToArray();
                List<Triangle> tris = ply.Triangulate();
                foreach (Triangle t in tris)
                {
                    c0 = AddVerticeOctTree(t.Points[0].X, t.Points[0].Y, baseHeight);
                    c1 = AddVerticeOctTree(t.Points[1].X, t.Points[1].Y, baseHeight);
                    c2 = AddVerticeOctTree(t.Points[2].X, t.Points[2].Y, baseHeight);
                    AddFace(c0, c1, c2);
                    /*
                    Faces.Add(c0);
                    Faces.Add(c1);
                    Faces.Add(c2);
                    */
                }

                for (int i = 0; i < endOfOutbound /*- 1*/; i++)
                {
                    CreateTubeInnerSideFace(tmp2, i, baseHeight);
                }

                for (int i = endOfOutbound + 1; i < tmp2.Count - 1; i++)
                {
                    CreateTubeInnerSideFace(tmp2, i, baseHeight);
                }

                for (int i = 0; i < (tmp2.Count / 2) - 1; i++)
                {
                    c0 = AddVerticeOctTree(tmp[i].X, tmp[i].Y, loftHeight);
                    c1 = AddVerticeOctTree(tmp[i + 1].X, tmp[i + 1].Y, loftHeight);
                    c2 = AddVerticeOctTree(tmp2[i].X, tmp2[i].Y, loftHeight);
                    c3 = AddVerticeOctTree(tmp2[i + 1].X, tmp2[i + 1].Y, loftHeight);
                    AddFace(c0, c1, c2);
                    AddFace(c1, c3, c2);

                    /*Faces.Add(c0);
                    Faces.Add(c1);
                    Faces.Add(c2);

                    Faces.Add(c1);
                    Faces.Add(c3);
                    Faces.Add(c2);
                    */
                }

                for (int i = (tmp2.Count / 2); i < tmp2.Count - 1; i++)
                {
                    c0 = AddVerticeOctTree(tmp[i].X, tmp[i].Y, loftHeight);
                    c1 = AddVerticeOctTree(tmp[i + 1].X, tmp[i + 1].Y, loftHeight);
                    c2 = AddVerticeOctTree(tmp2[i].X, tmp2[i].Y, loftHeight);
                    c3 = AddVerticeOctTree(tmp2[i + 1].X, tmp2[i + 1].Y, loftHeight);
                    /*
                    Faces.Add(c0);
                    Faces.Add(c1);
                    Faces.Add(c2);

                    Faces.Add(c1);
                    Faces.Add(c3);
                    Faces.Add(c2);
                    */
                    AddFace(c0, c1, c2);
                    AddFace(c1, c3, c2);
                }

                // close start end
                // central area
                c0 = AddVerticeOctTree(startEdgeP2.X, startEdgeP2.Y, 0);
                c1 = AddVerticeOctTree(startEdgeP2.X, startEdgeP2.Y, baseHeight);
                c2 = AddVerticeOctTree(startEdgeP3.X, startEdgeP3.Y, 0);
                c3 = AddVerticeOctTree(startEdgeP3.X, startEdgeP3.Y, baseHeight);

                AddFace(c0, c1, c2);
                AddFace(c1, c3, c2);
                /*
                                Faces.Add(c0);
                                Faces.Add(c1);
                                Faces.Add(c2);

                                Faces.Add(c1);
                                Faces.Add(c3);
                                Faces.Add(c2);
                                */
                // raised edge
                c0 = AddVerticeOctTree(startEdgeP0.X, startEdgeP0.Y, 0);
                c1 = AddVerticeOctTree(startEdgeP0.X, startEdgeP0.Y, loftHeight);
                c2 = AddVerticeOctTree(startEdgeP2.X, startEdgeP2.Y, 0);
                c3 = AddVerticeOctTree(startEdgeP2.X, startEdgeP2.Y, loftHeight);
                AddFace(c0, c1, c2);
                AddFace(c1, c3, c2);
                /*
                                Faces.Add(c0);
                                Faces.Add(c1);
                                Faces.Add(c2);

                                Faces.Add(c1);
                                Faces.Add(c3);
                                Faces.Add(c2);
                                */
                c0 = AddVerticeOctTree(startEdgeP1.X, startEdgeP1.Y, 0);
                c1 = AddVerticeOctTree(startEdgeP1.X, startEdgeP1.Y, loftHeight);
                c2 = AddVerticeOctTree(startEdgeP3.X, startEdgeP3.Y, 0);
                c3 = AddVerticeOctTree(startEdgeP3.X, startEdgeP3.Y, loftHeight);

                AddFace(c0, c2, c1);
                AddFace(c1, c2, c3);
                /*
                                Faces.Add(c0);
                                Faces.Add(c2);
                                Faces.Add(c1);

                                Faces.Add(c1);
                                Faces.Add(c2);
                                Faces.Add(c3);
                */
                // close start end
                // central area
                c0 = AddVerticeOctTree(endEdgeP2.X, endEdgeP2.Y, 0);
                c1 = AddVerticeOctTree(endEdgeP2.X, endEdgeP2.Y, baseHeight);
                c2 = AddVerticeOctTree(endEdgeP3.X, endEdgeP3.Y, 0);
                c3 = AddVerticeOctTree(endEdgeP3.X, endEdgeP3.Y, baseHeight);

                AddFace(c0, c2, c1);
                AddFace(c1, c2, c3);

                /*
                                Faces.Add(c0);
                                Faces.Add(c2);
                                Faces.Add(c1);

                                Faces.Add(c1);
                                Faces.Add(c2);
                                Faces.Add(c3);
                                */

                // raised edge
                c0 = AddVerticeOctTree(endEdgeP0.X, endEdgeP0.Y, 0);
                c1 = AddVerticeOctTree(endEdgeP0.X, endEdgeP0.Y, loftHeight);
                c2 = AddVerticeOctTree(endEdgeP2.X, endEdgeP2.Y, 0);
                c3 = AddVerticeOctTree(endEdgeP2.X, endEdgeP2.Y, loftHeight);
                AddFace(c0, c2, c1);
                AddFace(c1, c2, c3);
                /*
                Faces.Add(c0);
                Faces.Add(c2);
                Faces.Add(c1);

                Faces.Add(c1);
                Faces.Add(c2);
                Faces.Add(c3);
                */
                c0 = AddVerticeOctTree(endEdgeP1.X, endEdgeP1.Y, 0);
                c1 = AddVerticeOctTree(endEdgeP1.X, endEdgeP1.Y, loftHeight);
                c2 = AddVerticeOctTree(endEdgeP3.X, endEdgeP3.Y, 0);
                c3 = AddVerticeOctTree(endEdgeP3.X, endEdgeP3.Y, loftHeight);

                AddFace(c0, c1, c2);
                AddFace(c1, c3, c2);
                /*
                                Faces.Add(c0);
                                Faces.Add(c1);
                                Faces.Add(c2);

                                Faces.Add(c1);
                                Faces.Add(c3);
                                Faces.Add(c2);
                                */
            }
            CentreVertices();
        }

        private void GenerateWedge(bool thinFront = false)
        {
            double lx, rx, ty, by, bz, fz;
            lx = by = bz = double.MaxValue;
            rx = ty = fz = double.MinValue;
            BufferedPolyline bl = null;
            List<CurvePoint> curvePoints = null;
            GetBufferedPolyLine(out bl, out curvePoints);
            if (curvePoints != null && curvePoints.Count > 1)
            {
                List<Point3D> profile = GenerateWedgeProfile(loftHeight, loftThickness, thinFront);

                Point3D[,] surface = CreateSurfaceFromProfile(curvePoints, profile, ref lx, ref rx, ref ty, ref by, ref bz, ref fz);
                if (surface != null)
                {
                    CreateSolidFromProfile(profile.Count, lx, rx, ty, by, bz, fz, curvePoints, surface);
                    CentreVertices();
                }
            }
        }

        private List<Point3D> GenerateWedgeProfile(double loftHeight, double loftThickness, bool thinFront)
        {
            List<Point3D> prof = new List<Point3D>();
            double h2 = loftThickness / 2;
            double w2 = loftHeight / 2;
            prof.Add(new Point3D(0, h2, -w2)); //1
            prof.Add(new Point3D(0, h2, w2)); // 2
            if (thinFront)
            {
                prof.Add(new Point3D(0, 0.8 * h2, w2)); // 3
            }
            else
            {
                prof.Add(new Point3D(0, 0, w2)); // 3
            }
            prof.Add(new Point3D(0, -h2, -w2)); // 4

            return prof;
        }

        private void GetBufferedPolyLine(out BufferedPolyline bl, out List<CurvePoint> curvePoints)
        {
            bl = new BufferedPolyline(pathPoints);
            bl.BufferRadius = loftThickness / 2;
            curvePoints = bl.GenerateBufferCurvePoints();
            bl.BufferRadius = loftThickness / 2;
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            string imageName = EditorParameters.Get("ImagePath");
            if (imageName != "")
            {
                PathEditor.LoadImage(imageName);
            }
            LoftHeight = EditorParameters.GetDouble("LoftHeight", 10);
            LoftThickness = EditorParameters.GetDouble("LoftThickness", 5);
            String s = EditorParameters.Get("Path");
            if (s != "")
            {
                PathEditor.FromString(s);
            }
            ProfileIndex = EditorParameters.GetInt("ProfileIndex", 0);
            BaseThickness = EditorParameters.GetDouble("BaseThickness", 1);
        }

        private void PathPointsChanged(List<Point> points)
        {
            pathPoints.Clear();
            foreach (Point p in points)
            {
                pathPoints.Insert(0, new Point(p.X, p.Y));
            }

            GenerateShape();
            Redisplay();
        }

        private List<Point3D> RotatePoints(List<Point3D> pnts, double r1, double r2, double r3)
        {
            List<Point3D> tmp = new List<Point3D>();
            float cosa = (float)Math.Cos(r2);
            float sina = (float)Math.Sin(r2);

            float cosb = (float)Math.Cos(r1);
            float sinb = (float)Math.Sin(r1);

            float cosc = (float)Math.Cos(r3);
            float sinc = (float)Math.Sin(r3);

            float Axx = cosa * cosb;
            float Axy = cosa * sinb * sinc - sina * cosc;
            float Axz = cosa * sinb * cosc + sina * sinc;

            float Ayx = sina * cosb;
            float Ayy = sina * sinb * sinc + cosa * cosc;
            float Ayz = sina * sinb * cosc - cosa * sinc;

            float Azx = -sinb;
            float Azy = cosb * sinc;
            float Azz = cosb * cosc;
            foreach (Point3D cp in pnts)
            {
                Point3D rp = new Point3D();
                rp.X = Axx * cp.X + Axy * cp.Y + Axz * cp.Z;
                rp.Y = Ayx * cp.X + Ayy * cp.Y + Ayz * cp.Z;
                rp.Z = Azx * cp.X + Azy * cp.Y + Azz * cp.Z;

                tmp.Add(rp);
            }
            return tmp;
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("LoftHeight", LoftHeight.ToString());
            EditorParameters.Set("LoftThickness", LoftThickness.ToString());
            EditorParameters.Set("Path", PathEditor.AbsolutePathString);
            EditorParameters.Set("ImagePath", PathEditor.ImagePath);
            EditorParameters.Set("ProfileIndex", ProfileIndex.ToString());
            EditorParameters.Set("BaseThickness", BaseThickness.ToString());
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
            WarningText = "";
            // should flexi control give us live point updates while lines are dragged. Computing
            // new line costs too much so , no, instead wait until mouse up
            PathEditor.OpenEndedPath = true;
            LoadEditorParameters();

            UpdateCameraPos();
            Viewer.Clear();
            loaded = true;
            UpdateDisplay();
        }
    }
}