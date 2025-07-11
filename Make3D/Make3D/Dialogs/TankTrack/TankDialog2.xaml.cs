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

using Barnacle.LineLib;
using MakerLib.TextureUtils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
using System.Windows.Shapes;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for TankDialog2.xaml
    /// </summary>
    public partial class TankDialog2 : BaseModellerDialog
    {
        public string selectedTrackType;
        private List<System.Windows.Point> displayPoints;
        private ExternalLinks externalLinks;
        private double guideSize;
        private List<System.Windows.Point> innerPolygon;
        private bool loaded;
        private int noOfLinks;
        private List<System.Windows.Point> outerPolygon;
        private Visibility showGuideSize;
        private double spudSize;
        private double thickness;
        private List<System.Windows.Point> trackPath;
        private ObservableCollection<String> trackTypes;
        private double trackWidth = 10;

        public TankDialog2()
        {
            InitializeComponent();
            DataContext = this;
            loaded = false;
            PathEditor.OnFlexiPathChanged += PathPointsChanged;
            PathEditor.AbsolutePaths = true;
            PathEditor.DefaultImagePath = DefaultImagePath;

            PathEditor.HasPresets = true;
            PathEditor.SupportsHoles = false;
            ToolName = "TankTrack";
            DataContext = this;
            NoOfLinks = 50;
            Thickness = 6;
            TrackWidth = 10;
            SpudSize = 1;
            GuideSize = 1;

            TrackTypes = new ObservableCollection<string>();
            TrackTypes.Add("Simple");

            TrackTypes.Add("M1");
            externalLinks = new ExternalLinks();
            bool containsBasic = false;
            foreach (Link ln in externalLinks.Links)
            {
                if (!TrackTypes.Contains(ln.Name))
                {
                    TrackTypes.Add(ln.Name);
                }
                if (ln.Name == "Basic")
                {
                    containsBasic = true;
                }
            }

            // if there is a link type called basic then use it as the default;
            if (containsBasic)
            {
                SelectedTrackType = "Basic";
            }
            trackPath = new List<Point>();
            innerPolygon = new List<Point>();
            outerPolygon = new List<Point>();
        }

        public double GuideSize
        {
            get
            {
                return guideSize;
            }
            set
            {
                if (guideSize != value)
                {
                    guideSize = value;
                    NotifyPropertyChanged();
                    UpdateTrack();
                    UpdateDisplay();
                }
            }
        }

        public int NoOfLinks
        {
            get
            {
                return noOfLinks;
            }
            set
            {
                if (noOfLinks != value)
                {
                    noOfLinks = value;
                    NotifyPropertyChanged();
                    UpdateTrack();
                    UpdateDisplay();
                }
            }
        }

        public string SelectedTrackType
        {
            get
            {
                return selectedTrackType;
            }
            set
            {
                if (selectedTrackType != value)
                {
                    selectedTrackType = value;
                    if ((selectedTrackType == "Centre Guide") ||
                        (selectedTrackType == "M1"))
                    {
                        ShowGuideSize = Visibility.Visible;
                    }
                    else
                    {
                        ShowGuideSize = Visibility.Hidden;
                    }
                    NotifyPropertyChanged();
                    UpdateTrack();
                    UpdateDisplay();
                }
            }
        }

        public Visibility ShowGuideSize
        {
            get
            {
                return showGuideSize;
            }
            set
            {
                if (value != showGuideSize)
                {
                    showGuideSize = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double SpudSize
        {
            get
            {
                return spudSize;
            }
            set
            {
                if (spudSize != value)
                {
                    spudSize = value;
                    NotifyPropertyChanged();
                    UpdateTrack();
                    UpdateDisplay();
                }
            }
        }

        public double Thickness
        {
            get
            {
                return thickness;
            }
            set
            {
                if (thickness != value)
                {
                    thickness = value;
                    NotifyPropertyChanged();
                    UpdateTrack();
                    UpdateDisplay();
                }
            }
        }

        public ObservableCollection<String> TrackTypes
        {
            get
            {
                return trackTypes;
            }
            set
            {
                if (trackTypes != value)
                {
                    trackTypes = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double TrackWidth
        {
            get
            {
                return trackWidth;
            }
            set
            {
                if (trackWidth != value)
                {
                    trackWidth = value;
                    NotifyPropertyChanged();
                    UpdateTrack();
                    UpdateDisplay();
                }
            }
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParameters();
            DialogResult = true;
            Close();
        }

        private void BaseModellerDialog_Loaded(object sender, RoutedEventArgs e)
        {
            loaded = false;
            LoadEditorParameters();
            loaded = true;
        }

        private void GenerateTrack()
        {
            var result = GenerationTask();
            ClearShape();
            foreach (Point3D p in result.Vertices)
            {
                Vertices.Add(new Point3D(p.X, p.Y, p.Z));
            }
            foreach (int i in result.Faces)
            {
                Faces.Add(i);
            }
        }

        private void GenerateTrackFomLink(Link ln, Point3DCollection verts, Int32Collection facs)
        {
            facs.Clear();
            verts.Clear();
            bool firstCall = true;
            double top = double.MinValue;
            for (int i = 0; i < trackPath.Count; i++)
            {
                if (trackPath[i].Y > top)
                {
                    top = trackPath[i].Y;
                }
            }
            for (int i = 0; i < trackPath.Count; i++)
            {
                int j = i + 1;
                if (j >= trackPath.Count)
                {
                    j = 0;
                }

                System.Windows.Point p1 = new System.Windows.Point(trackPath[i].X, top - trackPath[i].Y);
                System.Windows.Point p2 = new System.Windows.Point(trackPath[j].X, top - trackPath[j].Y);

                GenerateLinkPart(p1, p2, verts, facs, firstCall, trackWidth, thickness, ln);

                firstCall = false;
            }
        }

        /// <summary>
        /// Generates the basic path created by the user defined polygon points
        /// </summary>
        private void GenerateTrackPath()
        {
            if (NoOfLinks > 0 && trackPath != null)
            {
                trackPath.Clear();

                if (displayPoints != null && displayPoints.Count > 2)
                {
                    double totalDist = TankTrackUtils.PolygonLength(displayPoints);
                    double t;
                    double dt = 1.0 / NoOfLinks;
                    for (t = 0; t < 1; t += dt)
                    {
                        System.Windows.Point p = GetPathPoint(displayPoints, t, totalDist);
                        trackPath.Add(p);
                    }
                }
            }
        }

        private Genresult GenerationTask()
        {
            Genresult result;
            Point3DCollection verts = new Point3DCollection();
            Int32Collection facs = new Int32Collection();
            result.Vertices = verts;
            result.Faces = facs;

            if (trackPath != null && SelectedTrackType != null)
            {
                switch (SelectedTrackType)
                {
                    case "Simple":
                        GenerateSimpleTrack(0, verts, facs);
                        GenerateFaces(verts, facs);
                        break;

                    case "Simple 2":
                        GenerateSimpleTrack(1, verts, facs);
                        GenerateFaces(verts, facs);
                        break;

                    case "M1":
                        GenerateM1Track(verts, facs);
                        CentreVertices(verts, facs);
                        break;

                    default:
                        {
                            if (externalLinks != null)
                            {
                                foreach (Link ln in externalLinks.Links)
                                {
                                    if (ln.Name == SelectedTrackType)
                                    {
                                        GenerateTrackFomLink(ln, verts, facs);
                                        CentreVertices(verts, facs);
                                        break;
                                    }
                                }
                            }
                        }
                        break;
                }
            }

            return result;
        }

        private System.Windows.Point GetPathPoint(List<System.Windows.Point> points, double t, double totalDist)
        {
            System.Windows.Point res = points[0];
            bool found = false;
            double targetDistance = t * totalDist;

            double runningDist = 0;
            double dist = 0;
            for (int i = 0; i < points.Count && found == false; i++)
            {
                int j = i + 1;
                if (j == points.Count)
                {
                    j = 0;
                }
                System.Windows.Point p1 = points[i];
                System.Windows.Point p2 = points[j];
                dist = Distance(p1, p2);
                if (runningDist < targetDistance && (runningDist + dist >= targetDistance))
                {
                    double overHang = (targetDistance - runningDist) / dist;
                    res = new System.Windows.Point(p1.X + (p2.X - p1.X) * overHang,
                                                    p1.Y + (p2.Y - p1.Y) * overHang);
                    found = true;
                }
                runningDist += dist;
            }
            return res;
        }

        private void LoadEditorParameters()
        {
            string imageName = EditorParameters.Get("ImagePath");
            if (imageName != "")
            {
                PathEditor.LoadImage(imageName);
            }
            string pth = EditorParameters.Get("Path");
            if (pth != "")
            {
                PathEditor.SetPath(pth, 0);
            }
            NoOfLinks = EditorParameters.GetInt("NoOfLinks", 50);
            SelectedTrackType = EditorParameters.Get("TrackType");
            Thickness = EditorParameters.GetDouble("Thickness", 2);
            SpudSize = EditorParameters.GetDouble("SpudSize", 1);
            GuideSize = EditorParameters.GetDouble("GuideSize", 1);
            TrackWidth = EditorParameters.GetDouble("TrackWidth", 10);
        }

        private void PathPointsChanged(List<System.Windows.Point> pnts)
        {
            displayPoints = pnts;

            if (PathEditor.PathClosed || pnts.Count == 0)
            {
                UpdateTrack();
                UpdateDisplay();
            }
        }

        private System.Windows.Point Perpendicular2(System.Windows.Point p1, System.Windows.Point p2, double t, double distanceFromLine)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            Vector v1 = new Vector(dx, dy);
            v1.Normalize();
            Vector v2 = new Vector(-v1.Y, v1.X);
            v2.Normalize();
            System.Windows.Point tp = new System.Windows.Point(p1.X + t * dx, p1.Y + t * dy);
            double x = tp.X + distanceFromLine * v2.X;
            double y = tp.Y + distanceFromLine * v2.Y;
            System.Windows.Point res = new System.Windows.Point(x, y);
            return res;
        }

        private void SaveEditorParameters()
        {
            EditorParameters.Set("ImagePath", PathEditor.ImagePath);
            String s = PathEditor.GetPath();
            EditorParameters.Set("Path", s);
            EditorParameters.Set("NoOfLinks", NoOfLinks.ToString());
            EditorParameters.Set("TrackType", SelectedTrackType);
            EditorParameters.Set("Thickness", Thickness.ToString());
            EditorParameters.Set("SpudSize", SpudSize.ToString());
            EditorParameters.Set("GuideSize", GuideSize.ToString());
            EditorParameters.Set("TrackWidth", TrackWidth.ToString());
        }

        private double ToMM(double x)
        {
            /*
                double res = x;

                res = 25.4 * x / 96;

                return res;
                */
            return x;
        }

        private void UpdateDisplay()
        {
            if (loaded)
            {
                Viewer.Model = GetModel();
            }
        }

        private void UpdateTrack()
        {
            GenerateTrackPath();
            GenerateTrack();
        }

        private struct Genresult
        {
            public Int32Collection Faces;
            public Point3DCollection Vertices;
        }
    }
}