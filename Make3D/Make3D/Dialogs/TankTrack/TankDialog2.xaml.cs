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
        private double trackOverlap;
        private List<System.Windows.Point> trackPath;
        private double trackThickness;
        private ObservableCollection<String> trackTypes;
        private double trackWidth = 10;

        public TankDialog2()
        {
            InitializeComponent();
            DataContext = this;
            loaded = false;
            ToolName = "TankTrack";
            PathEditor.OnFlexiPathChanged += PathPointsChanged;
            PathEditor.AbsolutePaths = true;
            PathEditor.DefaultImagePath = DefaultImagePath;
            PathEditor.ToolName = ToolName;
            PathEditor.HasPresets = true;
            PathEditor.IncludeCommonPresets = false;
            PathEditor.SupportsHoles = false;

            DataContext = this;
            NoOfLinks = 50;
            TrackThickness = 6;
            TrackWidth = 10;
            LinkOverlap = 0;
            GuideSize = 1;

            TrackTypes = new ObservableCollection<string>();
            TrackTypes.Add("None");
            //TrackTypes.Add("Simple");

            //TrackTypes.Add("M1");
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

        public double LinkOverlap
        {
            get
            {
                return trackOverlap;
            }
            set
            {
                if (trackOverlap != value)
                {
                    if (value >= 0 && value <= 100)
                    {
                        trackOverlap = value;
                        NotifyPropertyChanged();
                        UpdateTrack();
                        UpdateDisplay();
                    }
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

        public double TrackThickness
        {
            get
            {
                return trackThickness;
            }
            set
            {
                if (trackThickness != value)
                {
                    trackThickness = value;
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
            if (SelectedTrackType != null && SelectedTrackType == "")
            {
                SelectedTrackType = "Basic";
            }
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
            if (trackPath.Count > 0)
            {
                double top = double.MinValue;
                for (int i = 0; i < trackPath.Count; i++)
                {
                    if (trackPath[i].Y > top)
                    {
                        top = trackPath[i].Y;
                    }
                }
                for (int i = 0; i < trackPath.Count - 1; i++)
                {
                    int j = i + 1;
                    if (j >= trackPath.Count)
                    {
                        j = 0;
                    }

                    System.Windows.Point p1 = new System.Windows.Point(trackPath[i].X, top - trackPath[i].Y);
                    System.Windows.Point p2 = new System.Windows.Point(trackPath[j].X, top - trackPath[j].Y);

                    GenerateLinkPart(p1, p2, verts, facs, trackWidth, trackThickness, ln);
                }
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

            if (trackPath != null && trackPath.Count > 2 && SelectedTrackType != null && SelectedTrackType != "None")
            {
                if (externalLinks != null)
                {
                    double linkLength = Math.Sqrt((trackPath[1].X - trackPath[2].X) * (trackPath[1].X - trackPath[2].X) +
                                                  (trackPath[1].Y - trackPath[2].Y) * (trackPath[1].Y - trackPath[2].Y));

                    linkLength += (linkLength / 100.0 * LinkOverlap);
                    foreach (Link ln in externalLinks.Links)
                    {
                        if (ln.Name == SelectedTrackType)
                        {
                            ln.SetLinkSize(linkLength, TrackThickness, TrackWidth);
                            GenerateTrackFomLink(ln, verts, facs);
                            CentreVertices(verts, facs);
                            break;
                        }
                    }
                }
                /*
                switch (SelectedTrackType)
                {
                        case "Simple":
                            GenerateSimpleTrack(0, verts, facs);
                            GenerateFacesFromRibs(verts, facs);
                            break;

                        case "Simple 2":
                            GenerateSimpleTrack(1, verts, facs);
                            GenerateFacesFromRibs(verts, facs);
                            break;

                        case "M1":
                            GenerateM1Track(verts, facs);
                            CentreVertices(verts, facs);
                            break;

                    default:
                        {
                        }
                        break;
                }
                */
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
            TrackThickness = EditorParameters.GetDouble("TrackThickness", 2);
            LinkOverlap = EditorParameters.GetDouble("LinkOverlap", 1);

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
            EditorParameters.Set("TrackThickness", TrackThickness.ToString());
            EditorParameters.Set("LinkOverlap", LinkOverlap.ToString());
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
            if (loaded)
            {
                GenerateTrackPath();
                GenerateTrack();
            }
        }

        private struct Genresult
        {
            public Int32Collection Faces;
            public Point3DCollection Vertices;
        }
    }
}