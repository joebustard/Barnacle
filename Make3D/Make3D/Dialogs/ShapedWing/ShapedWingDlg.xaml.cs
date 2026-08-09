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

using Barnacle.LineLib;
using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Xml;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for DevTest.xaml
    /// </summary>
    public partial class ShapedWingDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private readonly string defaultWingShape = "M 0,0 RL 10.000,10.000 RL 100.000,10.000 RQ 10.000,10.000 0.000,20.000 RL -100.000,20.000 RL -10.000,20.000";
        private XmlDocument airFoilDoc;
        private List<String> airfoilGroups;
        private string airFoilPath;
        private double dihedralAngle;
        private double dihedralLimit = 20;
        private List<Point> displayPoints;
        private double forcedRootHeight;
        private bool loaded;
        private int numDivisions;
        private bool overrideRootHeight;
        private List<String> rootairfoilNames;
        private string rootGroup;
        private string selectedRootAirfoil;
        private double selectedWingProfileLength;
        private List<Point> selectedWingProfilePoints;
        private string warningText;

        public ShapedWingDlg()
        {
            InitializeComponent();
            ToolName = "ShapedWing";
            DataContext = this;
            loaded = false;
            numDivisions = 80;
            PathEditor.OnFlexiPathChanged += PathPointsChanged;
            PathEditor.DefaultImagePath = DefaultImagePath;
            PathEditor.FixedEndPath = true;
            PathEditor.ToolName = ToolName;
            PathEditor.HasPresets = true;
            PathEditor.IncludeCommonPresets = false;
            PathEditor.SupportsHoles = false;
            airFoilPath = AppDomain.CurrentDomain.BaseDirectory + "data\\Airfoils.xml";
            airFoilDoc = new XmlDocument();
            airFoilDoc.XmlResolver = null;
            rootairfoilNames = new List<string>();
            airfoilGroups = new List<string>();
            selectedWingProfilePoints = null;
            dihedralAngle = 0.0;
            overrideRootHeight = false;
            forcedRootHeight = 20.0;
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

                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double ForcedRootHeight
        {
            get
            {
                return forcedRootHeight;
            }
            set
            {
                if (forcedRootHeight != value)
                {
                    forcedRootHeight = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public int NumDivisions
        {
            get
            {
                return numDivisions;
            }

            set
            {
                if (value < 3 || value > 360)
                {
                    WarningText = "Number of ribProfiles must be >= 3 and <= 360";
                }
                else
                if (value != numDivisions)
                {
                    WarningText = "";
                    numDivisions = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
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
                if (value != overrideRootHeight)
                {
                    overrideRootHeight = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
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
                    if (!String.IsNullOrEmpty(selectedRootAirfoil) && !String.IsNullOrEmpty(rootGroup))
                    {
                        selectedWingProfilePoints = GetProfilePoints(rootGroup, selectedRootAirfoil, ref selectedWingProfileLength);
                        ProfileDisplayer.ProfilePnts = selectedWingProfilePoints;
                    }
                    NotifyPropertyChanged();
                    UpdateDisplay();
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

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParmeters();
            DialogResult = true;
            Close();
        }

        private void BaseModellerDialog_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ProfileDisplayer.Refresh();
        }

        private void GenerateWing()
        {
            ClearShape();
            bool needToCloseRight = false;
            // make a seperate flexipath to work with so we
            // dont mess up the users one.
            FlexiPath flexipath = new FlexiPath();
            flexipath.FromString(PathEditor.GetPath());
            flexipath.CalculatePathBounds();

            // the X coordinate for each of the ribs
            List<double> ribX = new List<double>();

            // if there is a dehedral then each of the ribs (except the first one) will have to moved up.
            List<double> yOffsetDueToDihedral = new List<double>();

            // the selected profile is scaled to the correct size at each rib position
            // Store these scaled profiles in ribProfiles
            List<Point>[] ribProfiles = new List<Point>[numDivisions];

            double minX = double.MaxValue;

            // These may be used if we are forcing the root to have a user defined height
            // rather than just getting it from the profile

            if (displayPoints != null)
            {
                if (numDivisions > 0)
                {
                    int currentDivision = 0;
                    double dt = 1.0 / (numDivisions - 1);
                    for (double t = 0; t <= 1; t += dt)
                    {
                        // get the basic size of the wing rib
                        var dp = flexipath.GetUpperAndLowerPoints(t, false);

                        ribX.Add(dp.X);
                        LoggerLib.Logger.Log($"t {t} dp.x {dp.X} dp.Lower {dp.Lower} dp.Upper {dp.Upper}\n");
                        if (Math.Abs(1 - t) < 0.000001)
                        {
                            if (dp.Upper - dp.Lower > 0.001)
                            {
                                needToCloseRight = true;
                            }
                        }

                        // if no dihedral then just set the y offset for the current rib to 0
                        if (dihedralAngle == 0.0)
                        {
                            yOffsetDueToDihedral.Add(0);
                        }
                        else
                        {
                            // set the y offset based on how far away it is from the root
                            double da = Math.Sin(DegToRad(dihedralAngle)) * dp.X;
                            yOffsetDueToDihedral.Add(da);
                        }
                        // use the length of the gap between the top edge and bottom edge of
                        // the flexipath to calculate how long the wing is at t
                        var si = dp.Upper - dp.Lower;
                        // create the outline profile  of the current rib (i.e. at point t)
                        ribProfiles[currentDivision] = new List<Point>();
                        for (double m = 0.0; m <= 1.0; m += dt)
                        {
                            Point wp = GetProfileAt(selectedWingProfilePoints, selectedWingProfileLength, m);
                            double px = -((1.0 - wp.X) * si + dp.Lower);
                            Point scaledPoint = new Point(px, (wp.Y * si));
                            ribProfiles[currentDivision].Add(scaledPoint);
                            minX = Math.Min(minX, px);
                        }

                        currentDivision++;
                    }
                }

                minX = Math.Abs(minX);
                // if we are overriding the calculated root height we need to calculate the scale factor
                // to apply to the Y values
                // start be finding just how high the current root is going to be
                if (overrideRootHeight)
                {
                    double minRootY = double.MaxValue;
                    double maxRootY = double.MinValue;
                    foreach (Point point in ribProfiles[0])
                    {
                        minRootY = Math.Min(minRootY, point.Y);
                        maxRootY = Math.Max(maxRootY, point.Y);
                    }
                    double generatedRootHeight = maxRootY - minRootY;
                    double heightScaleFactor = forcedRootHeight / generatedRootHeight;

                    // Apply scale to all ribs
                    for (int currentRib = 0; currentRib < ribProfiles.GetLength(0); currentRib++)
                    {
                        if (ribProfiles[currentRib] != null)
                        {
                            for (int pointIndex = 0; pointIndex < ribProfiles[currentRib].Count; pointIndex++)
                            {
                                Point p = ribProfiles[currentRib][pointIndex];
                                ribProfiles[currentRib][pointIndex] = new Point(p.X, p.Y * heightScaleFactor);
                            }
                        }
                    }
                }

                for (int i = 0; i < numDivisions - 1; i++)
                {
                    if (i + 1 < ribX.Count)
                    {
                        for (int j = 0; j < ribProfiles[0].Count; j++)
                        {
                            int k = j + 1;
                            if (k >= ribProfiles[0].Count)
                            {
                                k = 0;
                            }
                            if (i < ribProfiles.GetLength(0) &&
                                 j < ribProfiles[0].Count &&
                                 i < yOffsetDueToDihedral.Count &&
                                 k < ribProfiles[0].Count)
                            {
                                int p0 = AddVertice(ribX[i], ribProfiles[i][j].X + minX, ribProfiles[i][j].Y + yOffsetDueToDihedral[i]);
                                int p1 = AddVertice(ribX[i], ribProfiles[i][k].X + minX, ribProfiles[i][k].Y + yOffsetDueToDihedral[i]);
                                int p2 = AddVertice(ribX[i + 1], ribProfiles[i + 1][k].X + minX, ribProfiles[i + 1][k].Y + yOffsetDueToDihedral[i + 1]);
                                int p3 = AddVertice(ribX[i + 1], ribProfiles[i + 1][j].X + minX, ribProfiles[i + 1][j].Y + yOffsetDueToDihedral[i + 1]);

                                AddFace(p0, p2, p1);
                                AddFace(p0, p3, p2);
                            }
                        }
                    }
                }

                // close the root side
                List<System.Drawing.PointF> side = new List<System.Drawing.PointF>();
                for (int j = 0; j < ribProfiles[0].Count; j++)
                {
                    int k = j + 1;
                    if (k >= ribProfiles[0].Count)
                    {
                        k = 0;
                    }
                    System.Drawing.PointF pl = new System.Drawing.PointF((float)(ribProfiles[0][j].X + minX), (float)(ribProfiles[0][j].Y));
                    side.Add(pl);
                }
                TriangulatePerimiter(side, ribX[0], true);

                // do we need to close the right if it was round we might not
                if (needToCloseRight)
                {
                    side.Clear();

                    for (int j = 0; j < ribProfiles[0].Count; j++)
                    {
                        int k = j + 1;
                        if (k >= ribProfiles[0].Count)
                        {
                            k = 0;
                        }
                        System.Drawing.PointF pl = new System.Drawing.PointF((float)(ribProfiles[ribProfiles.Length - 1][j].X + minX), (float)(ribProfiles[ribProfiles.Length - 1][j].Y));
                        side.Add(pl);
                    }
                    TriangulatePerimiter(side, ribX[ribProfiles.Length - 1], false);
                }
            }
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

        /// <summary>
        /// Gets the points defining te profile of an aerofoil from the database file
        /// </summary>
        /// <param name="grpName"></param>
        /// <param name="airfoil"></param>
        /// <param name="dist"></param>
        /// <returns></returns>
        private List<Point> GetProfilePoints(string grpName, string airfoil, ref double dist)
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
            // load back the tool specific parameters
            String s = EditorParameters.Get("Path");
            if (s != "")
            {
                PathEditor.FromString(s);
            }
            else
            {
                PathEditor.FromString(defaultWingShape);
            }
            NumDivisions = EditorParameters.GetInt("NumDivisions", 80);
            DihedralAngle = EditorParameters.GetDouble("Dihedral", 0);
            string imageName = EditorParameters.Get("ImagePath");
            if (imageName != "")
            {
                PathEditor.LoadImage(imageName);
            }
            int v = EditorParameters.GetInt("ShowGrid", 1);
            PathEditor.ShowGrid = (UserControls.GridSettings.GridStyle)v;
            PathEditor.ZoomLevel = EditorParameters.GetDouble("Zoom", 1.0);
            ForcedRootHeight = EditorParameters.GetDouble("ForcedRootHeight", 5.0);
            OverrideRootHeight = EditorParameters.GetBoolean("OverrideRootHeight", false);
        }

        private void PathPointsChanged(List<System.Windows.Point> pnts)
        {
            displayPoints = pnts;
            if (PathEditor.PathClosed)
            {
                GenerateWing();
                Redisplay();
            }
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            EditorParameters.Set("Path", PathEditor.AbsolutePathString);
            EditorParameters.Set("NumDivisions", NumDivisions.ToString());
            EditorParameters.Set("ImagePath", PathEditor.ImagePath);
            EditorParameters.Set("Dihedral", dihedralAngle.ToString());
            EditorParameters.Set("ShowGrid", ((int)(PathEditor.ShowGrid)).ToString());
            EditorParameters.Set("Zoom", (PathEditor.ZoomLevel).ToString());
            EditorParameters.Set("OverrideRootHeight", OverrideRootHeight.ToString());
            EditorParameters.Set("ForcedRootHeight", ForcedRootHeight.ToString());
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

        private void TriangulatePerimiter(List<System.Drawing.PointF> points, double xo, bool invert)
        {
            TriangulationPolygon ply = new TriangulationPolygon();

            ply.Points = points.ToArray();
            List<Triangle> tris = ply.Triangulate();
            foreach (Triangle t in tris)
            {
                int c0 = AddVertice(Vertices, xo, t.Points[0].X, t.Points[0].Y);
                int c1 = AddVertice(Vertices, xo, t.Points[1].X, t.Points[1].Y);
                int c2 = AddVertice(Vertices, xo, t.Points[2].X, t.Points[2].Y);
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
        }

        private void UpdateDisplay()
        {
            if (loaded)
            {
                GenerateWing();
                Viewer.Model = GetModel();
                ProfileDisplayer.Refresh();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WarningText = "";
            LoadAirFoils();

            RootGroup = airfoilGroups[0];

            SelectedRootAirfoil = rootairfoilNames[0];

            LoadEditorParameters();

            UpdateCameraPos();
            Viewer.Clear();
            PathEditor.DefaultImagePath = DefaultImagePath;
            loaded = true;

            UpdateDisplay();
        }
    }
}