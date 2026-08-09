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

using Barnacle.Object3DLib;
using HullLibrary;
using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for WingDlg.xaml
    /// </summary>
    public partial class WingDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private XmlDocument airFoilDoc;
        private List<String> airfoilGroups;
        private string airFoilPath;
        private bool bottomModelChecked;
        private double dihedralAngle;
        private double dihedralLimit = 20;
        private bool overrideRootHeight;
        private bool overrideTipHeight;
        private List<String> rootairfoilNames;
        private string rootGroup;
        private double rootHeightForced;
        private double rootLength;
        private string selectedRootAirfoil;
        private string selectedShape;
        private string selectedTipAirfoil;
        private string selectedTipShape;
        private List<String> shapeNames;
        private double span;
        private double sweepAngle;
        private Visibility sweepControlsVisible;
        private double sweepLimit = 60;
        private List<String> tipairfoilNames;
        private Visibility tipControlsVisible;
        private string tipGroup;
        private double tipHeightForced;
        private double tipLength;
        private double tipOffsetX = 0;
        private double tipOffsetY = 0;
        private double tipOffsetZ = 0;
        private List<String> tipShapeNames;
        private bool topModelChecked;
        private bool wholeModelChecked;

        public WingDlg()
        {
            InitializeComponent();
            ToolName = "Wing";
            DataContext = this;
            shapeNames = new List<string>();
            tipShapeNames = new List<string>();
            airFoilPath = AppDomain.CurrentDomain.BaseDirectory + "data\\Airfoils.xml";
            airFoilDoc = new XmlDocument();
            airFoilDoc.XmlResolver = null;
            rootairfoilNames = new List<string>();
            airfoilGroups = new List<string>();
            rootLength = 30;
            tipLength = 10;
            span = 70;
            sweepAngle = 0;
            dihedralAngle = 0;
            rootHeightForced = 0;
            tipHeightForced = 0;
            overrideRootHeight = false;
            overrideTipHeight = false;
            //            rootHeightForced = 6.5;
            //            tipHeightForced = 3.5;
        }

        public List<string> AirfoilGroups
        {
            get
            {
                return airfoilGroups;
            }
            set
            {
                if (airfoilGroups != value)
                {
                    airfoilGroups = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool BottomModelChecked
        {
            get
            {
                return bottomModelChecked;
            }
            set
            {
                if (bottomModelChecked != value)
                {
                    bottomModelChecked = value;
                    NotifyPropertyChanged();
                    Update();
                }
            }
        }

        public double DihedralAngle
        {
            get
            {
                return dihedralAngle;
            }
            set
            {
                if (dihedralAngle != value)
                {
                    dihedralAngle = value;
                    if (dihedralAngle < -dihedralLimit)
                    {
                        dihedralAngle = -dihedralLimit;
                    }

                    if (dihedralAngle > sweepLimit)
                    {
                        dihedralAngle = sweepLimit;
                    }

                    NotifyPropertyChanged();
                    Update();
                }
            }
        }

        public bool OverrideRootHeight
        {
            get
            {
                return overrideRootHeight;
            }
            set
            {
                if (overrideRootHeight != value)
                {
                    overrideRootHeight = value;
                    NotifyPropertyChanged();
                    Update();
                }
            }
        }

        public bool OverrideTipHeight
        {
            get
            {
                return overrideTipHeight;
            }
            set
            {
                if (overrideTipHeight != value)
                {
                    overrideTipHeight = value;
                    NotifyPropertyChanged();
                    Update();
                }
            }
        }

        public List<string> RootAirfoilNames
        {
            get
            {
                return rootairfoilNames;
            }
            set
            {
                if (rootairfoilNames != value)
                {
                    rootairfoilNames = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String RootGroup
        {
            get
            {
                return rootGroup;
            }
            set
            {
                if (rootGroup != value)
                {
                    rootGroup = value;
                    List<string> names = new List<String>();
                    SetProfiles(rootGroup, names);

                    NotifyPropertyChanged();
                    RootAirfoilNames = names;
                }
            }
        }

        public double RootHeight
        {
            get
            {
                return rootHeightForced;
            }
            set
            {
                if (rootHeightForced != value)
                {
                    rootHeightForced = value;
                    NotifyPropertyChanged();
                    Update();
                }
            }
        }

        public double RootLength
        {
            get
            {
                return rootLength;
            }
            set
            {
                if (rootLength != value)
                {
                    rootLength = value;
                    NotifyPropertyChanged();
                    Update();
                }
            }
        }

        public string SelectedRootAirfoil
        {
            get
            {
                return selectedRootAirfoil;
            }
            set
            {
                if (selectedRootAirfoil != value)
                {
                    selectedRootAirfoil = value;
                    NotifyPropertyChanged();
                    Update();
                }
            }
        }

        public String SelectedShape
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
                    NotifyPropertyChanged();

                    Update();
                }
            }
        }

        public string SelectedTipAirfoil
        {
            get
            {
                return selectedTipAirfoil;
            }
            set
            {
                if (selectedTipAirfoil != value)
                {
                    selectedTipAirfoil = value;
                    NotifyPropertyChanged();
                    Update();
                }
            }
        }

        public String SelectedTipShape
        {
            get
            {
                return selectedTipShape;
            }
            set
            {
                if (selectedTipShape != value)
                {
                    selectedTipShape = value;
                    NotifyPropertyChanged();
                    Update();
                }
            }
        }

        public List<String> ShapeNames
        {
            get
            {
                return shapeNames;
            }
            set
            {
                if (shapeNames != value)
                {
                    shapeNames = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double Span
        {
            get
            {
                return span;
            }
            set
            {
                if (span != value)
                {
                    span = value;
                    NotifyPropertyChanged();
                    Update();
                }
            }
        }

        public double SweepAngle
        {
            get
            {
                return sweepAngle;
            }
            set
            {
                if (sweepAngle != value)
                {
                    sweepAngle = value;
                    if (sweepAngle < -sweepLimit)
                    {
                        sweepAngle = -sweepLimit;
                    }

                    if (sweepAngle > sweepLimit)
                    {
                        sweepAngle = sweepLimit;
                    }

                    NotifyPropertyChanged();
                    Update();
                }
            }
        }

        public Visibility SweepControlsVisible
        {
            get
            {
                return sweepControlsVisible;
            }

            set
            {
                if (sweepControlsVisible != value)
                {
                    sweepControlsVisible = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public List<string> TipAirfoilNames
        {
            get
            {
                return tipairfoilNames;
            }
            set
            {
                if (tipairfoilNames != value)
                {
                    tipairfoilNames = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Visibility TipControlsVisible
        {
            get
            {
                return tipControlsVisible;
            }

            set
            {
                if (tipControlsVisible != value)
                {
                    tipControlsVisible = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String TipGroup
        {
            get
            {
                return tipGroup;
            }
            set
            {
                if (tipGroup != value)
                {
                    tipGroup = value;
                    List<string> names = new List<String>();
                    SetProfiles(tipGroup, names);

                    NotifyPropertyChanged();
                    TipAirfoilNames = names;
                    NotifyPropertyChanged();
                }
            }
        }

        public double TipHeight
        {
            get
            {
                return tipHeightForced;
            }
            set
            {
                if (tipHeightForced != value)
                {
                    tipHeightForced = value;
                    NotifyPropertyChanged();
                    Update();
                }
            }
        }

        public double TipLength
        {
            get
            {
                return tipLength;
            }
            set
            {
                if (tipLength != value)
                {
                    tipLength = value;
                    NotifyPropertyChanged();
                    Update();
                }
            }
        }

        public List<String> TipShapeNames
        {
            get
            {
                return tipShapeNames;
            }
            set
            {
                if (tipShapeNames != value)
                {
                    tipShapeNames = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool TopModelChecked
        {
            get
            {
                return topModelChecked;
            }
            set
            {
                if (topModelChecked != value)
                {
                    topModelChecked = value;
                    NotifyPropertyChanged();
                    Update();
                }
            }
        }

        public bool WholeModelChecked
        {
            get
            {
                return wholeModelChecked;
            }
            set
            {
                if (wholeModelChecked != value)
                {
                    wholeModelChecked = value;
                    NotifyPropertyChanged();
                    Update();
                }
            }
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParmeters();
            DialogResult = true;
            Close();
        }

        private void BaseModellerDialog_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RootDisplay.Refresh();
            TipDisplay.Refresh();
        }

        private void CloseMissingHalfOfWing(double rootEdgeLength, List<Point> rootProfile, double tipEdgeLength, double tl, List<Point> tipProfile, double startT, double endT)
        {
            Point p1 = GetProfileAt(rootProfile, rootEdgeLength, startT);

            Point p2 = GetProfileAt(rootProfile, rootEdgeLength, endT);
            Point p3 = GetProfileAt(tipProfile, tipEdgeLength, endT);
            Point p4 = GetProfileAt(tipProfile, tipEdgeLength, startT);

            Point3D pd1 = new Point3D(p1.X * rootLength, p1.Y * rootLength, 0);
            Point3D pd2 = new Point3D(p2.X * rootLength, p2.Y * rootLength, 0);
            Point3D pd3 = new Point3D((p3.X * tl) + tipOffsetX, (p3.Y * tl) + tipOffsetY, tipOffsetZ);
            Point3D pd4 = new Point3D((p4.X * tl) + tipOffsetX, (p4.Y * tl) + tipOffsetY, tipOffsetZ);

            int v1 = AddVertice(pd1);
            int v2 = AddVertice(pd2);
            int v3 = AddVertice(pd3);
            int v4 = AddVertice(pd4);

            Faces.Add(v1);
            Faces.Add(v2);
            Faces.Add(v3);

            Faces.Add(v1);
            Faces.Add(v3);
            Faces.Add(v4);
        }

        private Int32Collection EllipseTip(List<Point> tipProfile, double mainRad, double sideRad, double tX, double tY, double tZ)
        {
            Int32Collection res = new Int32Collection();
            List<Point3D> ellipseEdge3D = new List<Point3D>();
            List<Point> ellipseEdgeForClosingTopOrBottom = new List<Point>();
            double mainDiameter = mainRad * 2;
            double stepSize = 1.0 / (tipProfile.Count - 1);
            double yrot = Math.Cos(dihedralAngle);
            if (!WholeModelChecked)
            {
                stepSize = 1.0 / (2 * tipProfile.Count - 1);
            }
            if (wholeModelChecked || topModelChecked)
            {
                for (double t = 0; t <= 0.5; t += stepSize)
                {
                    if (t > 0.5)
                    {
                        t = 0.5;
                    }
                    Point elp = GetEllipsePoint(mainRad, sideRad, t);
                    ellipseEdgeForClosingTopOrBottom.Add(elp);

                    Point3D p = new Point3D(elp.X, tY, elp.Y);
                    // this point needs to be rotated by the dihedral around tX,tY,tZ
                    p = RotateAroundXAxis(0, tY, 0, p, 90 - dihedralAngle);
                    ellipseEdge3D.Add(p);
                }
            }
            if (wholeModelChecked || bottomModelChecked)
            {
                for (double t = 0.5; t >= 0; t -= stepSize)
                {
                    Point elp = GetEllipsePoint(mainRad, sideRad, t);
                    ellipseEdgeForClosingTopOrBottom.Add(elp);
                    // Point3D p = new Point3D(tX + elp.X + mainRad, tY, tZ + elp.Y);

                    Point3D p = new Point3D(elp.X, tY, elp.Y);
                    // this point needs to be rotated by the dihedral around tX,tY,tZ
                    p = RotateAroundXAxis(0, tY, 0, p, 90 - dihedralAngle);
                    ellipseEdge3D.Add(p);
                }
            }
            // use the tipprofile and the ellipse edge to make the
            // shaped part of the tip
            for (int i = 0; i < tipProfile.Count - 1 && i < ellipseEdge3D.Count - 1; i++)
            {
                Point3D pd1 = new Point3D(tX + (tipProfile[i].X * mainDiameter), tY + (tipProfile[i].Y * mainDiameter), tZ);
                Point3D pd2 = new Point3D(tX + mainRad - ellipseEdge3D[i].X, ellipseEdge3D[i].Y, tZ + ellipseEdge3D[i].Z);
                Point3D pd3 = new Point3D(tX + mainRad - ellipseEdge3D[i + 1].X, ellipseEdge3D[i + 1].Y, tZ + ellipseEdge3D[i + 1].Z);
                Point3D pd4 = new Point3D(tX + (tipProfile[i + 1].X * mainDiameter), tY + (tipProfile[i + 1].Y * mainDiameter), tZ);

                int v1 = AddVertice(pd1);
                int v2 = AddVertice(pd2);
                int v3 = AddVertice(pd3);
                int v4 = AddVertice(pd4);
                if (!res.Contains(v1))
                {
                    res.Add(v1);
                }

                if (!res.Contains(v2))
                {
                    res.Add(v2);
                }

                if (!res.Contains(v3))
                {
                    res.Add(v3);
                }

                if (!res.Contains(v4))
                {
                    res.Add(v4);
                }
                Faces.Add(v1);
                Faces.Add(v2);
                Faces.Add(v3);

                Faces.Add(v1);
                Faces.Add(v3);
                Faces.Add(v4);
            }
            // if we were just making the top or bottom of the wing we will have a whole
            // Use the 2D ellipse edge list to close it
            if (!WholeModelChecked)
            {
                /*
                    Int32Collection res2 = TriangulateWingTip(ellipseEdgeForClosingTopOrBottom, 1, tX + mainRad, tY, tipOffsetZ, bottomModelChecked);
                    foreach (int i in res2)
                    {
                        if (!res.Contains(i))
                        {
                            res.Add(i);
                        }
                    }
                    */
            }

            return res;
        }

        private void EnableControlsForShape()
        {
            switch (selectedShape)
            {
                case "Straight":
                    {
                        TipControlsVisible = Visibility.Hidden;
                        SweepControlsVisible = Visibility.Hidden;
                        tipOffsetX = 0;
                        double theta = (Math.PI * dihedralAngle) / 180;
                        tipOffsetY = span * Math.Sin(theta);
                        tipOffsetZ = span;
                    }
                    break;

                case "Straight Edge":
                    {
                        TipControlsVisible = Visibility.Hidden;
                        SweepControlsVisible = Visibility.Hidden;
                        double theta = (Math.PI * dihedralAngle) / 180;
                        tipOffsetY = span * Math.Sin(theta);
                        tipOffsetX = rootLength - tipLength;
                        tipOffsetZ = span;
                    }
                    break;

                case "Tapered":
                    {
                        TipControlsVisible = Visibility.Visible;
                        SweepControlsVisible = Visibility.Hidden;
                        tipOffsetX = (rootLength - tipLength) / 2;
                        double theta = (Math.PI * dihedralAngle) / 180;
                        tipOffsetY = span * Math.Sin(theta);
                        tipOffsetZ = span;
                    }
                    break;

                case "Delta":
                    {
                        TipControlsVisible = Visibility.Visible;
                        SweepControlsVisible = Visibility.Hidden;
                        tipOffsetX = 0;
                        double theta = (Math.PI * dihedralAngle) / 180;
                        tipOffsetY = span * Math.Sin(theta);
                        tipOffsetZ = span;
                    }
                    break;

                case "Swept":
                    {
                        TipControlsVisible = Visibility.Visible;
                        SweepControlsVisible = Visibility.Visible;
                        double theta = (Math.PI * (-sweepAngle)) / 180;
                        tipOffsetX = span * Math.Sin(theta);
                        theta = (Math.PI * dihedralAngle) / 180;
                        tipOffsetY = span * Math.Sin(theta);
                        tipOffsetZ = span * Math.Cos(theta); ;
                    }
                    break;
            }
        }

        private void GenerateWing()
        {
            if (RootGroup != "" && SelectedRootAirfoil != "")
            {
                double rootEdgeLength = 0;
                double rootEdgeHeightScale = rootLength;
                double tipEdgeHeightScale = tipLength;
                double rootMinY = double.MaxValue;
                double rootMaxY = double.MinValue;
                double tipMinY = double.MaxValue;
                double tipMaxY = double.MinValue;
                Int32Collection rootVertices = new Int32Collection();
                Int32Collection tipVertices = new Int32Collection();
                if (SelectedShape == "Straight")
                {
                    tipEdgeHeightScale = rootLength;
                }

                // the thickness of the wing should come from the profile
                // but does yje user want to force it to a specific value
                if (overrideRootHeight)
                {
                    rootEdgeHeightScale = rootHeightForced;
                }
                if (overrideTipHeight)
                {
                    tipEdgeHeightScale = tipHeightForced;
                }
                List<Point> rootProfile = GetProfilePoints(RootGroup, SelectedRootAirfoil, rootLength, ref rootEdgeLength);
                RootDisplay.ProfilePnts = rootProfile;
                if (rootProfile.Count > 0)
                {
                    if (TipGroup != "" && SelectedTipAirfoil != "")
                    {
                        double tipEdgeLength = 0;
                        double tl = tipLength;
                        if (SelectedShape == "Straight")
                        {
                            tl = rootLength;
                        }
                        List<Point> tipProfile = GetProfilePoints(TipGroup, SelectedTipAirfoil, tl, ref tipEdgeLength);
                        TipDisplay.ProfilePnts = tipProfile;
                        if (tipProfile.Count > 0)
                        {
                            ClearShape();

                            double dt = 0.01;
                            double startT = 0;
                            double endT = 1;
                            bool close = false;
                            if (TopModelChecked)
                            {
                                startT = 0;
                                endT = 0.5;
                                close = true;
                            }
                            if (BottomModelChecked)
                            {
                                startT = 0.5;
                                endT = 1.0;
                                close = true;
                            }
                            List<Point> rootPnts = new List<Point>();
                            List<Point> tipPnts = new List<Point>();
                            for (double t = startT; t < endT; t += dt)
                            {
                                Point p1 = GetProfileAt(rootProfile, rootEdgeLength, t);
                                rootPnts.Add(p1);
                                Point p2 = GetProfileAt(rootProfile, rootEdgeLength, t + dt);
                                Point p3 = GetProfileAt(tipProfile, tipEdgeLength, t + dt);
                                Point p4 = GetProfileAt(tipProfile, tipEdgeLength, t);
                                tipPnts.Add(p4);

                                Point3D pd1 = new Point3D(p1.X * rootLength, p1.Y * rootLength, 0);
                                Point3D pd2 = new Point3D(p2.X * rootLength, p2.Y * rootLength, 0);
                                Point3D pd3 = new Point3D((p3.X * tl) + tipOffsetX, (p3.Y * tl) + tipOffsetY, tipOffsetZ);
                                Point3D pd4 = new Point3D((p4.X * tl) + tipOffsetX, (p4.Y * tl) + tipOffsetY, tipOffsetZ);

                                int v1 = AddVertice(pd1);
                                int v2 = AddVertice(pd2);
                                int v3 = AddVertice(pd3);
                                int v4 = AddVertice(pd4);

                                Faces.Add(v1);
                                Faces.Add(v3);
                                Faces.Add(v2);

                                Faces.Add(v1);
                                Faces.Add(v4);
                                Faces.Add(v3);

                                if (!rootVertices.Contains(v1))
                                {
                                    rootVertices.Add(v1);
                                    rootMinY = Math.Min(rootMinY, pd1.Y);
                                    rootMaxY = Math.Max(rootMaxY, pd1.Y);
                                }
                                if (!rootVertices.Contains(v2))
                                {
                                    rootVertices.Add(v2);
                                    rootMinY = Math.Min(rootMinY, pd2.Y);
                                    rootMaxY = Math.Max(rootMaxY, pd2.Y);
                                }

                                if (!tipVertices.Contains(v3))
                                {
                                    tipVertices.Add(v3);
                                    tipMinY = Math.Min(tipMinY, pd3.Y);
                                    tipMaxY = Math.Max(tipMaxY, pd3.Y);
                                }
                                if (!tipVertices.Contains(v4))
                                {
                                    tipVertices.Add(v4);
                                    tipMinY = Math.Min(tipMinY, pd4.Y);
                                    tipMaxY = Math.Max(tipMaxY, pd4.Y);
                                }
                            }
                            if (close)
                            {
                                //  CloseMissingHalfOfWing(rootEdgeLength, rootProfile, tipEdgeLength, tl, tipProfile, startT, endT);
                            }

                            if (overrideRootHeight)
                            {
                                double rootYScaler = rootHeightForced / (rootMaxY - rootMinY);
                                foreach (int pi in rootVertices)
                                {
                                    double y = Vertices[pi].Y * rootYScaler;
                                    Vertices[pi] = new Point3D(Vertices[pi].X, y, Vertices[pi].Z);
                                }
                                TriangulatePerimiter(rootPnts, rootLength, 0, 0, 0, true, rootYScaler);
                            }
                            else
                            {
                                TriangulatePerimiter(rootPnts, rootLength, 0, 0, 0, true);
                            }

                            if (overrideTipHeight)
                            {
                                double tipYScaler = tipHeightForced / (tipMaxY - tipMinY);
                                foreach (int pi in tipVertices)
                                {
                                    double y = Vertices[pi].Y * tipYScaler;
                                    Vertices[pi] = new Point3D(Vertices[pi].X, y, Vertices[pi].Z);
                                }
                                Int32Collection tipV2 = GenerateWingTip(tl, tipPnts);
                                foreach (int pi in tipV2)
                                {
                                    if (!tipVertices.Contains(pi))
                                    {
                                        double y = Vertices[pi].Y * tipYScaler;
                                        Vertices[pi] = new Point3D(Vertices[pi].X, y, Vertices[pi].Z);
                                    }
                                }
                            }
                            else
                            {
                                GenerateWingTip(tl, tipPnts);
                            }
                            if (!WholeModelChecked)
                            {
                                var hullMaker = new ConvexHullCalculator();
                                hullMaker.GeneratePoint3DHull(Vertices, Faces);
                            }
                            CentreVertices();
                        }
                    }
                }
            }
        }

        private Int32Collection GenerateWingTip(double tl, List<Point> tipPnts)
        {
            Int32Collection res = new Int32Collection();
            if (selectedTipShape == "Cut Off")
            {
                res = TriangulatePerimiter(tipPnts, tl, tipOffsetX, tipOffsetY, tipOffsetZ, false);
            }
            else
                                        if (selectedTipShape == "Ellipse 1")
            {
                double mr = tl / 2;
                double sr = mr / 10;
                res = EllipseTip(tipPnts, mr, sr, tipOffsetX, tipOffsetY, tipOffsetZ);
            }
            else
                                        if (selectedTipShape == "Ellipse 2")
            {
                double mr = tl / 2;
                double sr = mr / 5;
                res = EllipseTip(tipPnts, mr, sr, tipOffsetX, tipOffsetY, tipOffsetZ);
            }
            if (selectedTipShape == "Ellipse 3")
            {
                double mr = tl / 2;
                double sr = mr / 2;
                res = EllipseTip(tipPnts, mr, sr, tipOffsetX, tipOffsetY, tipOffsetZ);
            }
            if (selectedTipShape == "Ellipse 4")
            {
                double mr = tl / 2;
                double sr = mr;
                res = EllipseTip(tipPnts, mr, sr, tipOffsetX, tipOffsetY, tipOffsetZ);
            }
            return res;
        }

        private Point GetEllipsePoint(double a, double b, double t)
        {
            Point res = new Point(0, 0);
            if (t >= 0 && t <= 1)
            {
                double theta = t * Math.PI * 2;
                res.X = a * Math.Cos(theta);
                res.Y = b * Math.Sin(theta);
            }
            return res;
        }

        private Point GetProfileAt(List<Point> profile, double length, double t)
        {
            Point res = new Point(0, 0);
            if (t > 1)
            {
                t = 0;
            }
            double targetDist = length * t;

            int i = 0;
            double running = 0;
            bool done = false;
            while (!done)
            {
                int j = i + 1;
                if (j >= profile.Count)
                {
                    j = 0;
                }
                double diff = Distance(profile[i], profile[j]);

                if (running <= targetDist && running + diff >= targetDist)
                {
                    double hang = targetDist - running;
                    hang = hang / diff;

                    double x = profile[i].X + (hang * (profile[j].X - profile[i].X));
                    double y = profile[i].Y + (hang * (profile[j].Y - profile[i].Y));
                    res = new Point(x, y);
                    done = true;
                }
                else
                {
                    running += diff;
                    i++;
                }
            }

            return res;
        }

        private List<Point> GetProfilePoints(string grpName, string airfoil, double len, ref double dist)
        {
            List<Point> res = new List<Point>();
            String content = "";
            XmlNode root = airFoilDoc.SelectSingleNode("Airfoils");
            XmlNodeList grps = root.SelectNodes("grp");
            foreach (XmlNode gn in grps)
            {
                if ((gn as XmlElement).GetAttribute("Name") == grpName)
                {
                    XmlNodeList afs = gn.SelectNodes("af");
                    foreach (XmlNode af in afs)
                    {
                        if ((af as XmlElement).GetAttribute("Name") == airfoil)
                        {
                            content = af.InnerText;
                            break;
                        }
                    }
                    break;
                }
            }

            dist = 0;

            if (content != "")
            {
                string[] words = content.Split(',');
                for (int i = 0; i < words.GetLength(0); i += 2)
                {
                    words[i] = words[i].Trim();
                    double x = 1 - Convert.ToDouble(words[i]);
                    double y = Convert.ToDouble(words[i + 1]);
                    res.Add(new Point(x, y));
                }

                for (int i = 1; i < res.Count; i++)
                {
                    double dx = res[i].X - res[i - 1].X;
                    double dy = res[i].Y - res[i - 1].Y;
                    double d = Math.Sqrt(dx * dx + dy * dy);
                    dist += d;
                }
            }
            return res;
        }

        private void LoadAirFoils()
        {
            if (File.Exists(airFoilPath))
            {
                airFoilDoc.Load(airFoilPath);
                XmlNode root = airFoilDoc.SelectSingleNode("Airfoils");
                XmlNodeList grps = root.SelectNodes("grp");
                foreach (XmlNode gn in grps)
                {
                    airfoilGroups.Add((gn as XmlElement).GetAttribute("Name"));
                }
                NotifyPropertyChanged("AirfoilGroups");
            }
        }

        private void LoadEditorParameters()
        {
            string s = EditorParameters.Get("Shape");
            if (s != "")
            {
                SelectedShape = s;
                s = EditorParameters.Get("Whole");
                WholeModelChecked = Convert.ToBoolean(s);

                s = EditorParameters.Get("Top");
                TopModelChecked = Convert.ToBoolean(s);

                s = EditorParameters.Get("Bottom");
                BottomModelChecked = Convert.ToBoolean(s);

                s = EditorParameters.Get("Span");
                Span = Convert.ToDouble(s);

                s = EditorParameters.Get("RootLength");
                RootLength = Convert.ToDouble(s);

                s = EditorParameters.Get("TipLength");
                TipLength = Convert.ToDouble(s);

                s = EditorParameters.Get("Sweep");
                SweepAngle = Convert.ToDouble(s);

                s = EditorParameters.Get("Dihedral");
                dihedralAngle = Convert.ToDouble(s);

                RootGroup = EditorParameters.Get("RootGroup");
                SelectedRootAirfoil = EditorParameters.Get("RootAirfoil");

                TipGroup = EditorParameters.Get("TipGroup");
                SelectedTipAirfoil = EditorParameters.Get("TipAirfoil");
                SelectedTipShape = EditorParameters.Get("TipShape");
                RootHeight = EditorParameters.GetDouble("ForcedRootHeight", 5.0);
                OverrideRootHeight = EditorParameters.GetBoolean("OverrideRootHeight", false);

                TipHeight = EditorParameters.GetDouble("ForcedTipHeight", 5.0);
                OverrideTipHeight = EditorParameters.GetBoolean("OverrideTipHeight", false);
            }
        }

        private void SaveEditorParmeters()
        {
            EditorParameters.Set("Shape", selectedShape);
            EditorParameters.Set("Whole", wholeModelChecked.ToString());
            EditorParameters.Set("Top", topModelChecked.ToString());
            EditorParameters.Set("Bottom", bottomModelChecked.ToString());
            EditorParameters.Set("Span", span.ToString());
            EditorParameters.Set("RootLength", rootLength.ToString());
            EditorParameters.Set("TipLength", tipLength.ToString());
            EditorParameters.Set("Sweep", sweepAngle.ToString());
            EditorParameters.Set("Dihedral", dihedralAngle.ToString());

            EditorParameters.Set("RootGroup", RootGroup);
            EditorParameters.Set("RootAirfoil", SelectedRootAirfoil);
            EditorParameters.Set("TipGroup", TipGroup);
            EditorParameters.Set("TipAirfoil", SelectedTipAirfoil);
            EditorParameters.Set("TipShape", SelectedTipShape);
            EditorParameters.Set("OverrideRootHeight", OverrideRootHeight.ToString());
            EditorParameters.Set("ForcedRootHeight", RootHeight.ToString());
            EditorParameters.Set("OverrideTipHeight", OverrideTipHeight.ToString());
            EditorParameters.Set("ForcedTipHeight", TipHeight.ToString());
        }

        private void SetProfiles(string grpName, List<string> names)
        {
            XmlNode root = airFoilDoc.SelectSingleNode("Airfoils");
            names.Clear();
            XmlNodeList grps = root.SelectNodes("grp");
            foreach (XmlNode gn in grps)
            {
                if (grpName == (gn as XmlElement).GetAttribute("Name"))
                {
                    XmlNodeList nodeList = gn.SelectNodes("af");
                    foreach (XmlNode nd in nodeList)
                    {
                        XmlElement el = nd as XmlElement;
                        names.Add(el.GetAttribute("Name"));
                    }
                }
            }
        }

        private void SetShapeNames()
        {
            shapeNames.Add("Straight");
            shapeNames.Add("Straight Edge");
            shapeNames.Add("Tapered");
            shapeNames.Add("Delta");
            shapeNames.Add("Swept");
            NotifyPropertyChanged("ShapeNames");

            tipShapeNames.Add("Cut Off");
            tipShapeNames.Add("Ellipse 1");
            tipShapeNames.Add("Ellipse 2");
            tipShapeNames.Add("Ellipse 3");
            tipShapeNames.Add("Ellipse 4");
            NotifyPropertyChanged("TipShapeNames");
        }

        private Int32Collection TriangulatePerimiter(List<Point> points, double l, double xo, double yo, double z, bool invert, double yScaler = 1)
        {
            Int32Collection res = new Int32Collection();

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
                int c0 = AddVertice(xo + t.Points[0].X * l, yo + t.Points[0].Y * l * yScaler, z);
                int c1 = AddVertice(xo + t.Points[1].X * l, yo + t.Points[1].Y * l * yScaler, z);
                int c2 = AddVertice(xo + t.Points[2].X * l, yo + t.Points[2].Y * l * yScaler, z);
                if (!res.Contains(c0))
                {
                    res.Add(c0);
                }

                if (!res.Contains(c1))
                {
                    res.Add(c1);
                }

                if (!res.Contains(c2))
                {
                    res.Add(c2);
                }

                if (invert)
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
            return res;
        }

        private Int32Collection TriangulateWingTip(List<Point> points, double l, double xo, double yo, double zo, bool invert)
        {
            Int32Collection res = new Int32Collection();
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
                int c0 = AddVertice(xo + t.Points[0].X * l, yo, zo + t.Points[0].Y * l);
                int c1 = AddVertice(xo + t.Points[1].X * l, yo, zo + t.Points[1].Y * l);
                int c2 = AddVertice(xo + t.Points[2].X * l, yo, zo + t.Points[2].Y * l);
                if (!res.Contains(c0))
                {
                    res.Add(c0);
                    Vertices[c0] = RotateAroundXAxis(xo, yo, zo, Vertices[c0], 90 - dihedralAngle);
                }
                else
                {
                    bool dup1 = true;
                }

                if (!res.Contains(c1))
                {
                    res.Add(c1);
                    Vertices[c1] = RotateAroundXAxis(xo, yo, zo, Vertices[c1], 90 - dihedralAngle);
                }
                else
                {
                    bool dup1 = true;
                }

                if (!res.Contains(c2))
                {
                    res.Add(c2);
                    Vertices[c2] = RotateAroundXAxis(xo, yo, zo, Vertices[c2], 90 - dihedralAngle);
                }
                else
                {
                    bool dup1 = true;
                }

                if (invert)
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
            return res;
        }

        private void Update()
        {
            EnableControlsForShape();
            GenerateWing();
            Viewer.Model = GetModel();
            RootDisplay.Refresh();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAirFoils();

            SetShapeNames();
            SelectedShape = shapeNames[0];
            SelectedTipShape = tipShapeNames[0];
            RootGroup = airfoilGroups[0];
            TipGroup = airfoilGroups[0];
            SelectedRootAirfoil = rootairfoilNames[0];
            SelectedTipAirfoil = tipairfoilNames[0];
            WholeModelChecked = true;
            LoadEditorParameters();
            UpdateCameraPos();
            Viewer.Clear();
            GenerateWing();
            Viewer.Model = GetModel();
        }
    }
}