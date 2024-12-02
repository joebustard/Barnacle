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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for Blank.xaml
    /// </summary>
    public partial class TurboFan : BaseModellerDialog, INotifyPropertyChanged
    {
        private bool anticlockwise;
        private double bladeLength;
        private double bladeOverlap;
        private double bladePitch;
        private double bladeThickness;
        private bool clockwise;

        private double diskOffset;
        private double diskThickness;
        private double hubRadius;
        private bool loaded;
        private int numberOfBlades;

        private bool supportDisk;

        public TurboFan()
        {
            InitializeComponent();
            ToolName = "TurboFan";
            DataContext = this;
            loaded = false;
        }

        public bool Anticlockwise
        {
            get
            {
                return anticlockwise;
            }
            set
            {
                if (anticlockwise != value)
                {
                    anticlockwise = value;
                    clockwise = !anticlockwise;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double BladeLength
        {
            get
            {
                return bladeLength;
            }
            set
            {
                if (bladeLength != value)
                {
                    bladeLength = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double BladeOverlap
        {
            get
            {
                return bladeOverlap;
            }
            set
            {
                if (bladeOverlap != value)
                {
                    bladeOverlap = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double BladePitch
        {
            get
            {
                return bladePitch;
            }
            set
            {
                if (bladePitch != value)
                {
                    bladePitch = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double BladeThickness
        {
            get
            {
                return bladeThickness;
            }
            set
            {
                if (bladeThickness != value)
                {
                    bladeThickness = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public bool Clockwise
        {
            get
            {
                return clockwise;
            }
            set
            {
                if (clockwise != value)
                {
                    clockwise = value;
                    anticlockwise = !clockwise;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double DiskOffset
        {
            get
            {
                return diskOffset;
            }
            set
            {
                if (diskOffset != value)
                {
                    diskOffset = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double DiskThickness
        {
            get
            {
                return diskThickness;
            }
            set
            {
                if (diskThickness != value)
                {
                    diskThickness = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double HubRadius
        {
            get
            {
                return hubRadius;
            }
            set
            {
                if (hubRadius != value)
                {
                    hubRadius = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public int NumberOfBlades
        {
            get
            {
                return numberOfBlades;
            }
            set
            {
                if (numberOfBlades != value)
                {
                    numberOfBlades = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public bool SupportDisk
        {
            get
            {
                return supportDisk;
            }
            set
            {
                if (supportDisk != value)
                {
                    supportDisk = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public Point3D[] MakePolygonPoints(int numSides, double radius, double z)
        {
            //double radius = 0.5;
            // Generate the points.
            Point3D[] points = new Point3D[numSides];
            double dtheta = 2 * Math.PI / numSides;
            double theta = 0;
            for (int i = 0; i < numSides; i++)
            {
                points[i] = new Point3D(radius * Math.Cos(theta), radius * Math.Sin(theta), z);
                theta += dtheta;
            }
            return points;
        }

        internal void GenerateCylinder(double radius, double thickness, double offset)
        {
            int numSides = 180;
            Point3D[] bottom = MakePolygonPoints(numSides, radius, (-thickness / 2) + offset);
            // Top is the bottom reversed and moved up to 1
            Point3D[] top = new Point3D[numSides];
            int ind = numSides - 1;
            foreach (Point3D p in bottom)
            {
                top[ind] = new Point3D(p.X, p.Y, (thickness / 2) + offset);
                ind--;
            }
            Point3D bottomCentre = new Point3D(0, 0, (-thickness / 2) + offset);
            Point3D topCentre = new Point3D(0, 0, (thickness / 2) + offset);
            for (int i = 0; i < numSides; i++)
            {
                int j = i + 1;
                if (j == numSides)
                {
                    j = 0;
                }

                int k = numSides - i - 1;
                int l = k - 1;
                if (l < 0)
                {
                    l = numSides - 1;
                }
                // bottom cap
                AddTriangle(bottomCentre, bottom[i], bottom[j]);
                // top cap
                AddTriangle(topCentre, top[i], top[j]);

                // vertical 1
                AddTriangle(bottom[i], top[k], top[l]);

                // vertical 2
                AddTriangle(bottom[i], top[l], bottom[j]);
            }
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParmeters();
            DialogResult = true;
            Close();
        }

        private void AddTriangle(Point3D p0, Point3D p1, Point3D p2)
        {
            int i0 = AddVertice(p0);
            int i1 = AddVertice(p1);
            int i2 = AddVertice(p2);
            Faces.Add(i0);
            Faces.Add(i2);
            Faces.Add(i1);
        }

        private void GenerateBlades()
        {
            int numSteps = 30;

            // angle between the start of each blade.
            double bladeStartDTheta = (Math.PI * 2) / numberOfBlades;

            // angle covered by the sweep of an individual blade
            double bladeSweepDTheta = bladeStartDTheta + (bladeOverlap * bladeStartDTheta / 100.0);
            double dtheta = bladeSweepDTheta / numSteps;
            // blade pitch in radians
            double pitch = (90 - bladePitch) * Math.PI / 180.0;

            // inside of blade is outside of hub
            double innerBladeRadius = hubRadius;
            double outterBladeRadius = innerBladeRadius + bladeLength;
            double x, y;
            List<double> innerz = new List<double>();
            List<double> outterz = new List<double>();
            for (int i = 0; i < numberOfBlades; i++)
            {
                double st = i * bladeStartDTheta;
                List<Point> inner = new List<Point>();
                for (int j = 0; j < numSteps; j++)
                {
                    x = innerBladeRadius * Math.Cos(st + (j * dtheta));
                    y = innerBladeRadius * Math.Sin(st + (j * dtheta));
                    inner.Add(new Point(x, y));
                }
                double mid = st + (bladeStartDTheta / 2);
                double midx = innerBladeRadius * Math.Cos(mid);
                double midy = innerBladeRadius * Math.Sin(mid);
                if (innerz.Count == 0)
                {
                    foreach (Point pi in inner)
                    {
                        double dist = (pi.X - midx) * (pi.X - midx) + (pi.Y - midy) * (pi.Y - midy);
                        int sn = Math.Sign(pi.X - midx);
                        if (anticlockwise)
                        {
                            sn = -sn;
                        }
                        if (dist != 0)
                        {
                            dist = Math.Sqrt(dist);
                        }
                        double z = sn * dist * Math.Cos(pitch);
                        innerz.Add(z);
                    }
                }
                List<Point> outter = new List<Point>();
                for (int j = 0; j < numSteps; j++)
                {
                    x = outterBladeRadius * Math.Cos(st + (j * dtheta));
                    y = outterBladeRadius * Math.Sin(st + (j * dtheta));
                    outter.Add(new Point(x, y));
                }

                if (outterz.Count == 0)
                {
                    midx = outterBladeRadius * Math.Cos(mid);
                    midy = outterBladeRadius * Math.Sin(mid);

                    foreach (Point pi in outter)
                    {
                        double dist = (pi.X - midx) * (pi.X - midx) + (pi.Y - midy) * (pi.Y - midy);
                        int sn = Math.Sign(pi.X - midx);
                        if (anticlockwise)
                        {
                            sn = -sn;
                        }
                        if (dist != 0)
                        {
                            dist = Math.Sqrt(dist);
                        }
                        double z = sn * dist * Math.Cos(pitch);
                        outterz.Add(z);
                    }
                }
                int backP1 = -1;
                int backP2 = -1;

                int backP3 = -1;
                int backP4 = -1;
                // back
                for (int j = 0; j < inner.Count - 1; j++)
                {
                    int p1 = AddVertice(new Point3D(inner[j].X, inner[j].Y, innerz[j]));
                    int p2 = AddVertice(new Point3D(inner[j + 1].X, inner[j + 1].Y, innerz[j + 1]));
                    int p3 = AddVertice(new Point3D(outter[j].X, outter[j].Y, outterz[j]));
                    int p4 = AddVertice(new Point3D(outter[j + 1].X, outter[j + 1].Y, outterz[j + 1]));
                    if (j == 0)
                    {
                        backP1 = p1;
                        backP2 = p3;
                    }

                    if (j == inner.Count - 2)
                    {
                        backP3 = p2;
                        backP4 = p4;
                    }
                    Faces.Add(p1);
                    Faces.Add(p2);
                    Faces.Add(p3);

                    Faces.Add(p3);
                    Faces.Add(p2);
                    Faces.Add(p4);
                }

                int frontP1 = -1;
                int frontP2 = -1;
                int frontP3 = -1;
                int frontP4 = -1;
                // front
                for (int j = 0; j < inner.Count - 1; j++)
                {
                    int p1 = AddVertice(new Point3D(inner[j].X, inner[j].Y, innerz[j] + bladeThickness));
                    int p2 = AddVertice(new Point3D(inner[j + 1].X, inner[j + 1].Y, innerz[j + 1] + bladeThickness));
                    int p3 = AddVertice(new Point3D(outter[j].X, outter[j].Y, outterz[j] + bladeThickness));
                    int p4 = AddVertice(new Point3D(outter[j + 1].X, outter[j + 1].Y, outterz[j + 1] + bladeThickness));
                    if (j == 0)
                    {
                        frontP1 = p1;
                        frontP2 = p3;
                    }
                    if (j == inner.Count - 2)
                    {
                        frontP3 = p2;
                        frontP4 = p4;
                    }
                    Faces.Add(p1);
                    Faces.Add(p3);
                    Faces.Add(p2);

                    Faces.Add(p3);
                    Faces.Add(p4);
                    Faces.Add(p2);
                }

                // inner
                for (int j = 0; j < inner.Count - 1; j++)
                {
                    int p1 = AddVertice(new Point3D(inner[j].X, inner[j].Y, innerz[j]));
                    int p2 = AddVertice(new Point3D(inner[j + 1].X, inner[j + 1].Y, innerz[j + 1]));
                    int p3 = AddVertice(new Point3D(inner[j].X, inner[j].Y, innerz[j] + bladeThickness));
                    int p4 = AddVertice(new Point3D(inner[j + 1].X, inner[j + 1].Y, innerz[j + 1] + bladeThickness));
                    Faces.Add(p1);
                    Faces.Add(p3);
                    Faces.Add(p2);

                    Faces.Add(p3);
                    Faces.Add(p4);
                    Faces.Add(p2);
                }

                // outter
                for (int j = 0; j < inner.Count - 1; j++)
                {
                    int p1 = AddVertice(new Point3D(outter[j].X, outter[j].Y, outterz[j]));
                    int p2 = AddVertice(new Point3D(outter[j + 1].X, outter[j + 1].Y, outterz[j + 1]));
                    int p3 = AddVertice(new Point3D(outter[j].X, outter[j].Y, outterz[j] + bladeThickness));
                    int p4 = AddVertice(new Point3D(outter[j + 1].X, outter[j + 1].Y, outterz[j + 1] + bladeThickness));
                    Faces.Add(p1);
                    Faces.Add(p2);
                    Faces.Add(p3);

                    Faces.Add(p3);
                    Faces.Add(p2);
                    Faces.Add(p4);
                }

                if (backP1 != -1 && frontP1 != -1)
                {
                    Faces.Add(backP1);
                    Faces.Add(backP2);
                    Faces.Add(frontP1);

                    Faces.Add(frontP1);
                    Faces.Add(backP2);
                    Faces.Add(frontP2);

                    Faces.Add(backP3);

                    Faces.Add(frontP3);
                    Faces.Add(backP4);

                    Faces.Add(frontP3);
                    Faces.Add(frontP4);
                    Faces.Add(backP4);
                }
            }
        }

        private void GenerateHub()
        {
            GenerateCylinder(hubRadius, hubRadius, 0);
        }

        private void GenerateShape()
        {
            ClearShape();
            GenerateBlades();
            GenerateHub();
            if (supportDisk)
            {
                GenerateSupportDisk();
            }
            CentreVertices();
        }

        private void GenerateSupportDisk()
        {
            GenerateCylinder(hubRadius + bladeLength, diskThickness, -(diskOffset * diskThickness));
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            NumberOfBlades = EditorParameters.GetInt("NumberOfBlades", 8);
            BladeLength = EditorParameters.GetDouble("BladeLength", 20);
            BladePitch = EditorParameters.GetDouble("BladePitch", 45); ;
            BladeThickness = EditorParameters.GetDouble("BladeThickness", 1);
            HubRadius = EditorParameters.GetDouble("HubRadius", 10);
            DiskThickness = EditorParameters.GetDouble("DiskThickness", 10);
            DiskOffset = EditorParameters.GetDouble("DiskOffset", 0.5);
            Clockwise = EditorParameters.GetBoolean("Clockwise", true);
            Anticlockwise = EditorParameters.GetBoolean("Anticlockwise", false);
            SupportDisk = EditorParameters.GetBoolean("SupportDisk", false);
        }

        private void ResetDefaults(object sender, RoutedEventArgs e)
        {
            loaded = false;
            SetDefaults();
            loaded = true;
            UpdateDisplay();
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            EditorParameters.Set("NumberOfBlades", NumberOfBlades.ToString());
            EditorParameters.Set("BladeLength", BladeLength.ToString());
            EditorParameters.Set("BladePitch", BladePitch.ToString()); ;
            EditorParameters.Set("BladeThickness", BladeThickness.ToString());
            EditorParameters.Set("HubRadius", HubRadius.ToString());
            EditorParameters.Set("DiskThickness", DiskThickness.ToString());
            EditorParameters.Set("DiskOffset", DiskOffset.ToString());
            EditorParameters.Set("Clockwise", Clockwise.ToString());
            EditorParameters.Set("Anticlockwise", Anticlockwise.ToString());
            EditorParameters.Set("SupportDisk", SupportDisk.ToString());
        }

        private void SetDefaults()
        {
            NumberOfBlades = 8;
            BladeLength = 20;
            BladePitch = 45;
            BladeThickness = 1;
            HubRadius = 10;
            DiskThickness = 10;
            DiskOffset = 0.5;
            Clockwise = true;
            Anticlockwise = false;
            SupportDisk = false;
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
            SetDefaults();
            LoadEditorParameters();
            Viewer.Clear();
            UpdateCameraPos();
            loaded = true;
            UpdateDisplay();
        }
    }
}