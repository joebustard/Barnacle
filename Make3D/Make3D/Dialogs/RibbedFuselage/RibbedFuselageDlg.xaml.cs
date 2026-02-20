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

using Barnacle.Dialogs.RibbedFuselage.Models;
using Barnacle.Dialogs.RibbedFuselage.Views;
using Barnacle.LineLib;
using Barnacle.Object3DLib;
using Barnacle.RibbedFuselage.Models;
using Barnacle.ViewModels;
using ManifoldLib;
using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Reflection;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using MessageBox = System.Windows.MessageBox;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for RibbedFuselageDlg.xaml
    /// </summary>
    public partial class RibbedFuselageDlg : BaseModellerDialog
    {
        private bool autoFit;
        private bool backBody;
        private DispatcherTimer despatcherTimer;
        private bool dirty;
        private string filePath;
        private bool frontBody;
        private FuselageModel fuselageData;
        private bool loaded;
        private bool modelUpToDate;
        private int numberOfDivisions;
        private int oldSelectedRibIndex = -1;
        private bool originToCentroid;
        private RibImageDetailsModel selectedRib;
        private int selectedRibIndex;
        private bool shell;
        private double shellSize;
        private String sideImage;
        private string sidePath;
        private String topImage;
        private string topPath;
        private bool wholeBody;

        public RibbedFuselageDlg()
        {
            InitializeComponent();
            loaded = false;
            TopPathEditor.OnFlexiImageChanged = TopImageChanged;
            TopPathEditor.OnFlexiPathTextChanged = TopPathChanged;

            SidePathEditor.OnFlexiImageChanged = SideImageChanged;
            SidePathEditor.OnFlexiPathTextChanged = SidePathChanged;

            ToolName = "ProfileFuselage";
            RibCommand = new RelayCommand(OnRibComand);

            SelectedRib = null;
            SelectedRibIndex = -1;

            fuselageData = new FuselageModel();

            HorizontalResolution = 96;
            VerticalResolution = 96;
            frontBody = false;
            BackBody = false;
            wholeBody = true;
            shell = false;
            NumberOfDivisions = 100;
            DataContext = this;
            despatcherTimer = new DispatcherTimer();
            despatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            despatcherTimer.Tick += DespatcherTimer_Tick;
        }

        public bool AutoFit
        {
            get
            {
                return autoFit;
            }

            set
            {
                autoFit = value;
                dirty = true;
                NotifyPropertyChanged();
                UpdateModel();
            }
        }

        public bool BackBody
        {
            get
            {
                return backBody;
            }

            set
            {
                backBody = value;
                dirty = true;
                if (backBody)
                {
                    wholeBody = false;
                    frontBody = false;
                    UpdateModel();
                }
                NotifyPropertyChanged();
            }
        }

        public bool FrontBody
        {
            get
            {
                return frontBody;
            }

            set
            {
                frontBody = value;
                dirty = true;
                if (frontBody)
                {
                    backBody = false;
                    wholeBody = false;
                    UpdateModel();
                }
                NotifyPropertyChanged();
            }
        }

        public bool ModelIsVisible
        {
            get; set;
        }

        public int NumberOfDivisions
        {
            get
            {
                return numberOfDivisions;
            }

            set
            {
                if (numberOfDivisions != value && value >= 3 && value <= 360)
                {
                    numberOfDivisions = value;
                    NotifyPropertyChanged();
                    UpdateModel();
                }
            }
        }

        public bool OriginToCentroid
        {
            get
            {
                return originToCentroid;
            }
            set
            {
                if (value != originToCentroid)
                {
                    originToCentroid = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public RelayCommand RibCommand
        {
            get; set;
        }

        public ObservableCollection<RibImageDetailsModel> Ribs
        {
            get
            {
                return fuselageData.Ribs;
            }
        }

        public int SelectedRibIndex
        {
            get
            {
                return selectedRibIndex;
            }

            set
            {
                if (value != selectedRibIndex)
                {
                    oldSelectedRibIndex = selectedRibIndex;
                    selectedRibIndex = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool Shell
        {
            get
            {
                return shell;
            }

            set
            {
                if (shell != value)
                {
                    shell = value;
                    dirty = true;
                    UpdateModel();
                    NotifyPropertyChanged();
                }
            }
        }

        public double ShellSize
        {
            get
            {
                return shellSize;
            }
            set
            {
                if (shellSize != value)
                {
                    shellSize = value;
                    NotifyPropertyChanged();
                    UpdateModel();
                }
            }
        }

        public String SideImage
        {
            get
            {
                return sideImage;
            }

            set
            {
                if (sideImage != value)
                {
                    sideImage = value;
                    fuselageData?.SetSideImage(sideImage);
                    NotifyPropertyChanged();
                    SidePathEditor.LoadImage(sideImage);
                }
            }
        }

        public string SidePath
        {
            get
            {
                return sidePath;
            }

            set
            {
                if (sidePath != value)
                {
                    sidePath = value;
                    fuselageData.SetSidePath(sidePath);
                    NotifyPropertyChanged();
                    ThreeDView.SetSidePath(sidePath);
                    if (!String.IsNullOrEmpty(sidePath) && sidePath != SidePathEditor.AbsolutePathString)
                    {
                        SidePathEditor.FromString(sidePath, false);

                        CopyPathToSideView();
                    }
                }
            }
        }

        public String TopImage
        {
            get
            {
                return topImage;
            }

            set
            {
                if (topImage != value)
                {
                    topImage = value;
                    fuselageData?.SetTopImage(topImage);
                    NotifyPropertyChanged();
                    TopPathEditor.LoadImage(topImage);
                }
            }
        }

        public string TopPath
        {
            get
            {
                return topPath;
            }

            set
            {
                if (string.Compare(topPath, value) != 0)
                {
                    topPath = value;
                    fuselageData.SetTopPath(topPath);
                    ThreeDView.SetTopPath(topPath);
                    if (topPath != TopPathEditor.AbsolutePathString)
                    {
                        TopPathEditor.FromString(topPath, false);
                        CopyPathToTopView();
                    }
                }
            }
        }

        public bool WholeBody
        {
            get
            {
                return wholeBody;
            }

            set
            {
                wholeBody = value;
                dirty = true;
                if (wholeBody)
                {
                    backBody = false;
                    frontBody = false;
                    UpdateModel();
                }
                NotifyPropertyChanged();
            }
        }

        internal double HorizontalResolution
        {
            get; set;
        }

        internal RibImageDetailsModel SelectedRib
        {
            get
            {
                return selectedRib;
            }

            set
            {
                if (selectedRib != value)
                {
                    selectedRib = value;
                    NotifyPropertyChanged();
                }
            }
        }

        internal double VerticalResolution
        {
            get; set;
        }

        public void GenerateModel()
        {
            try
            {
                // clear out existing 3d model
                Vertices.Clear();
                Faces.Clear();
                if (fuselageData.Ribs != null && fuselageData.Ribs.Count > 1)
                {
                    if (!String.IsNullOrEmpty(fuselageData.SidePath) && !String.IsNullOrEmpty(fuselageData.TopPath))
                    {
                        List<double> ribXs = new List<double>();
                        List<Dimension> topDims = new List<Dimension>();
                        List<Dimension> sideDims = new List<Dimension>();
                        List<RibImageDetailsModel> generatingRibs = CreateGeneratingRibs(ribXs, topDims, sideDims);

                        // do we have enough data to construct the model
                        if (generatingRibs.Count > 1 &&
                            CheckAllRibsHaveData(generatingRibs))
                        {
                            GenerateFacesFromRibs(generatingRibs, ribXs, topDims, sideDims);
                            ThreeDView.SetGeneratingRibs(generatingRibs, ribXs, topDims, sideDims);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "GenerateSkin");
            }
        }

        public void Load()
        {
            OpenFileDialog opDlg = new OpenFileDialog();
            opDlg.Filter = "Fuselage spar files (*.spr) | *.spr";
            if (opDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    filePath = opDlg.FileName;
                    Read(filePath);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
            }
        }

        public void RemoveUnreferencedVertices()
        {
            Point3DCollection tmpV = new Point3DCollection();
            Int32Collection tmpF = new Int32Collection();
            foreach (int f in Faces)
            {
                Point3D p = Vertices[f];
                int v = AddVertice(tmpV, p.X, p.Y, p.Z);
                tmpF.Add(v);
            }
            Faces = tmpF;
            Vertices = tmpV;
        }

        public void Save()
        {
            if (String.IsNullOrEmpty(filePath))
            {
                SaveAs();
            }
            else
            {
                Write(filePath);
                dirty = false;
            }
        }

        internal List<LetterMarker> GetMarkers()
        {
            List<LetterMarker> res = new List<LetterMarker>();
            int nextY = 10;
            foreach (MarkerModel m in fuselageData.Markers)
            {
                LetterMarker lm = new LetterMarker(m.Name, new System.Windows.Point(m.Position, nextY));
                res.Add(lm);
                nextY = 40 - nextY;
            }
            return res;
        }

        internal double GetXmm(double position)
        {
            double dpi = HorizontalResolution;
            double posInches = position / dpi;

            return InchesToMM(posInches);
        }

        internal double GetYmm(double position)
        {
            double dpi = VerticalResolution;
            double posInches = position / dpi;

            return InchesToMM(posInches);
        }

        internal void MoveMarker(string s, double x, bool finishedMove)
        {
            if (finishedMove)
            {
                foreach (MarkerModel m in fuselageData.Markers)
                {
                    if (m.Name == s)
                    {
                        m.Position = (double)x;
                        break;
                    }
                }
                if (finishedMove)
                {
                    // regenerate the 3d fuselage
                    UpdateModel();
                    Redisplay();
                    dirty = true;
                }
            }
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (!modelUpToDate && MessageBox.Show("Ribs have changed but the fuselage model has not been regemerated. Regenerate before closing?", "Warning", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
            {
                GenerateModel();
            }

            if (dirty && MessageBox.Show("Data has changed. Save it before closing", "Warning", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
            {
                Save();
            }
            SaveEditorParmeters();
            if (originToCentroid)
            {
                MoveOriginToCentroid();
            }
            base.Ok_Click(sender, e);
        }

        private static void AdjustPointForShell(double shellThickness, ref Point3D p, double sgn = 1)
        {
            // calcuate angle that point makes with origin
            if (p.Y != 0)
            {
                double theta = Math.Atan2(p.Y, p.Z);
                double d = Math.Sqrt((p.Y * p.Y) + (p.Z * p.Z));
                double r = (d - shellThickness);
                p.Y = r * Math.Sin(theta);
                p.Z = r * Math.Cos(theta);
            }
            else
            {
                p.Z -= shellThickness;
            }
            //System.Diagnostics.Debug.WriteLine($" p.Y {p.Y}  p.Z {p.Z}");
        }

        private void CalculateMainRibSizes()
        {
            if (fuselageData.Ribs != null && fuselageData.Ribs.Count > 1)
            {
                if (!String.IsNullOrEmpty(fuselageData.SidePath) && !String.IsNullOrEmpty(fuselageData.TopPath))
                {
                    double minY = double.MaxValue;
                    FlexiPath topViewFlexiPath = new FlexiPath();
                    topViewFlexiPath.FromString(fuselageData.TopPath);
                    topViewFlexiPath.CalculatePathBounds();

                    FlexiPath sideViewFlexiPath = new FlexiPath();
                    sideViewFlexiPath.FromString(fuselageData.SidePath);
                    sideViewFlexiPath.CalculatePathBounds();
                    List<double> ribXs = new List<double>();
                    List<Dimension> topDims = new List<Dimension>();
                    List<Dimension> sideDims = new List<Dimension>();

                    // first get the x positions of the ribs and the height and width at that
                    // position from the top and side views. generate the profile points for the
                    // ribs at the same time.
                    for (int i = 0; i < fuselageData.Ribs.Count; i++)
                    {
                        double x = fuselageData.Markers[i].Position;

                        RibImageDetailsModel cp;
                        cp = fuselageData.Ribs[i];
                        cp.NumDivisions = numberOfDivisions;
                        cp.GenerateProfilePoints();

                        // calculate the top and side dimensions from the images at the ribs given position
                        var dp1 = topViewFlexiPath.GetUpperAndLowerPoints(x);
                        topDims.Add(new Dimension(new System.Windows.Point(dp1.X, dp1.Lower), new System.Windows.Point(dp1.X, dp1.Upper)));
                        var dp2 = sideViewFlexiPath.GetUpperAndLowerPoints(x);
                        sideDims.Add(new Dimension(new System.Windows.Point(dp1.X, dp2.Lower), new System.Windows.Point(dp1.X, dp2.Upper)));
                        ribXs.Add(dp1.X);
                        minY = Math.Min(minY, dp2.Lower);

                        ThreeDView.SetRibPosition(i, dp1.X - ribXs[0], dp1.Lower, dp1.Upper, dp2.Lower, dp2.Upper);
                    }

                    ThreeDView.MoveRibsUp(minY);
                }
            }
        }

        private bool CheckAllRibsHaveData(List<RibImageDetailsModel> generatingRibs)
        {
            bool okToGenerate = true;
            // check that all the ribs have profile points. If they don't we can't generate the shape.
            for (int i = 0; i < generatingRibs.Count; i++)
            {
                if (generatingRibs[i].ProfilePoints == null || generatingRibs[i].ProfilePoints.Count == 0)
                {
                    okToGenerate = false;
                    break;
                }
            }

            return okToGenerate;
        }

        private void CopyPathToSideView()
        {
            SideView.OnMarkerMoved = SideMarkerMoved;
            //get the flexipath from  the side and render the path onto an image
            List<PointF> pnts = SidePathEditor.DisplayPointsF();
            SideView.OutlinePoints = pnts;
            TopView.Markers = GetMarkers();
            SideView.Markers = GetMarkers();
        }

        private void CopyPathToTopView()
        {
            TopView.OnMarkerMoved = TopMarkerMoved;
            //get the flexipath from  the top and render the path onto an image
            List<PointF> pnts = TopPathEditor.DisplayPointsF();
            TopView.OutlinePoints = pnts;
            TopView.Markers = GetMarkers();
            SideView.Markers = GetMarkers();
        }

        private void Create3DSurfaceVerticesFromTheRibs(List<RibImageDetailsModel> generatingRibs, List<double> ribXs, List<Dimension> topDims, List<Dimension> sideDims, int[,] ribvertices, int start, int end, List<PointF> leftEdge, List<PointF> rightEdge, out double leftMostX, out double rightMostX)
        {
            double x = ribXs[0];
            leftMostX = x;
            rightMostX = x;
            int vert = 0;
            int vindex = 0;
            for (int ribId = 0; ribId < generatingRibs.Count; ribId++)
            {
                x = ribXs[ribId];
                // if this is the last rib count it as the right edge and record its position
                if (ribId == generatingRibs.Count - 1)
                {
                    rightMostX = x;
                }

                vindex = 0;
                double z, y;

                for (int proind = start; proind < end; proind++)
                {
                    if (proind < generatingRibs[ribId].ProfilePoints.Count)
                    {
                        PointF pnt = generatingRibs[ribId].ProfilePoints[proind];

                        ribvertices[ribId, vindex] = ScaleProfilePointConvertTo3D(topDims, sideDims, x, ribId, pnt, out z, out y);

                        if (ribId == 0)
                        {
                            leftEdge.Add(new PointF((float)y, (float)z));
                        }
                        if (ribId == generatingRibs.Count - 1)
                        {
                            rightEdge.Add(new PointF((float)y, (float)z));
                        }

                        vindex++;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"ERROR proind {proind} ProfilePoints Count");
                    }
                }
            }
        }

        private List<RibImageDetailsModel> CreateGeneratingRibs(List<double> ribXs, List<Dimension> topDims, List<Dimension> sideDims)
        {
            List<RibImageDetailsModel> generatingRibs = new List<RibImageDetailsModel>();
            FlexiPath topViewFlexiPath = new FlexiPath();
            topViewFlexiPath.FromString(fuselageData.TopPath);
            topViewFlexiPath.CalculatePathBounds();

            FlexiPath sideViewFlexiPath = new FlexiPath();
            sideViewFlexiPath.FromString(fuselageData.SidePath);
            sideViewFlexiPath.CalculatePathBounds();

            double prevX = 0;

            ribXs.Clear();
            topDims.Clear();
            sideDims.Clear();
            // first get the x positions of the ribs and the height and width at that
            // position from the top and side views. generate the profile points for the
            // ribs at the same time.
            RibImageDetailsModel prevcp = null;
            double rib1X = 0;
            for (int i = 0; i < fuselageData.Ribs.Count; i++)
            {
                double x = fuselageData.Markers[i].Position;

                RibImageDetailsModel cp;
                cp = fuselageData.Ribs[i];
                cp.NumDivisions = numberOfDivisions;
                cp.GenerateProfilePoints();

                double autofirDx = 0.01;
                string cpPath = cp.FlexiPathText;

                if (autoFit && generatingRibs.Count > 0)
                {
                    string prevPath = generatingRibs[generatingRibs.Count - 1].FlexiPathText;

                    // if the path for the current rib is the same as the last one and
                    // they are reasonable difference apart full the gap with with some
                    // other virtual ribs
                    if (x - prevX > autofirDx)
                    {
                        double dummyRibX = prevX + autofirDx;
                        while (dummyRibX < x - autofirDx)
                        {
                            if (cpPath == prevPath)
                            {
                                CreateIntermediateRibwWithTheSamePath(topViewFlexiPath, sideViewFlexiPath, generatingRibs, ribXs, topDims, sideDims, cp, dummyRibX);
                            }
                            else
                            {
                                CreateIntermediateRibwWithInterpolatedPath(topViewFlexiPath, sideViewFlexiPath, generatingRibs, ribXs, topDims, sideDims, rib1X, prevcp, x, cp, dummyRibX);
                            }
                            prevX = dummyRibX;
                            dummyRibX += autofirDx;
                        }
                    }
                }

                generatingRibs.Add(cp);

                // calculate the top and side dimensions from the images at the ribs
                // given position
                var dp1 = topViewFlexiPath.GetUpperAndLowerPoints(x);
                topDims.Add(new Dimension(new System.Windows.Point(dp1.X, dp1.Lower), new System.Windows.Point(dp1.X, dp1.Upper)));
                dp1 = sideViewFlexiPath.GetUpperAndLowerPoints(x);
                sideDims.Add(new Dimension(new System.Windows.Point(dp1.X, dp1.Lower), new System.Windows.Point(dp1.X, dp1.Upper)));
                ribXs.Add(dp1.X);

                // move on
                prevX = x;
                rib1X = x;
                prevcp = cp;
            }
            return generatingRibs;
        }

        private void CreateIntermediateRibwWithInterpolatedPath(FlexiPath topViewFlexiPath, FlexiPath sideViewFlexiPath, List<RibImageDetailsModel> generatingRibs, List<double> ribXs, List<Dimension> topDims, List<Dimension> sideDims, double rib1X, RibImageDetailsModel rib1, double rib2X, RibImageDetailsModel rib2, double targetx)
        {
            if (rib1 != null && rib2 != null)
            {
                // copy the rib
                RibImageDetailsModel nr = rib1.Clone();
                nr.ProfilePoints.Clear();
                float distanceBetweenRealRibs = (float)(rib2X - rib1X);
                float t = (float)(targetx - rib1X) / distanceBetweenRealRibs;

                for (int i = 0; i < rib1.ProfilePoints.Count; i++)

                {
                    PointF p1 = rib1.ProfilePoints[i];
                    PointF p2 = rib2.ProfilePoints[i];
                    PointF tp = new PointF();
                    tp.X = p1.X + t * (p2.X - p1.X);
                    tp.Y = p1.Y + t * (p2.Y - p1.Y);
                    nr.ProfilePoints.Add(tp);
                }

                double cx = 0.5;
                // insert specific centre points at midx, top and botomm
                for (int i = 0; i < nr.ProfilePoints.Count; i++)
                {
                    int j = i + 1;
                    if (j == nr.ProfilePoints.Count)
                    {
                        j = 0;
                    }
                    if ((nr.ProfilePoints[i].X < cx) && (nr.ProfilePoints[j].X > cx))
                    {
                        double dx = cx - nr.ProfilePoints[i].X;
                        double nt = dx / (nr.ProfilePoints[j].X - nr.ProfilePoints[i].X);
                        double dy = nt * (nr.ProfilePoints[j].Y - nr.ProfilePoints[i].Y);
                        nr.TopPointIndex = j;
                        nr.TopPoint = new PointF((float)cx, nr.ProfilePoints[i].Y + (float)dy);
                    }
                    else
                    if ((nr.ProfilePoints[i].X > cx) && (nr.ProfilePoints[j].X < cx))
                    {
                        double dx = nr.ProfilePoints[i].X - cx;
                        double nt = dx / (nr.ProfilePoints[i].X - nr.ProfilePoints[j].X);
                        double dy = nt * (nr.ProfilePoints[i].Y - nr.ProfilePoints[j].Y);
                        nr.BottomPointIndex = j;
                        nr.BottomPoint = new PointF((float)cx, nr.ProfilePoints[i].Y - (float)dy);
                    }
                }
                nr.NumDivisions = NumberOfDivisions;

                generatingRibs.Add(nr);

                // get the dimensions from the side and top at the intermediate point
                var dp = topViewFlexiPath.GetUpperAndLowerPoints(targetx);
                topDims.Add(new Dimension(new System.Windows.Point(dp.X, dp.Lower), new System.Windows.Point(dp.X, dp.Upper)));
                dp = sideViewFlexiPath.GetUpperAndLowerPoints(targetx);
                sideDims.Add(new Dimension(new System.Windows.Point(dp.X, dp.Lower), new System.Windows.Point(dp.X, dp.Upper)));
                ribXs.Add(dp.X);
            }
        }

        private void CreateIntermediateRibwWithTheSamePath(FlexiPath topViewFlexiPath, FlexiPath sideViewFlexiPath, List<RibImageDetailsModel> generatingRibs, List<double> ribXs, List<Dimension> topDims, List<Dimension> sideDims, RibImageDetailsModel cp, double nx)
        {
            // copy the rib
            RibImageDetailsModel nr = cp.Clone();
            nr.NumDivisions = NumberOfDivisions;

            generatingRibs.Add(nr);

            // get the dimensions from the side and top at the intermediate point
            var dp = topViewFlexiPath.GetUpperAndLowerPoints(nx);
            topDims.Add(new Dimension(new System.Windows.Point(dp.X, dp.Lower), new System.Windows.Point(dp.X, dp.Upper)));
            dp = sideViewFlexiPath.GetUpperAndLowerPoints(nx);
            sideDims.Add(new Dimension(new System.Windows.Point(dp.X, dp.Lower), new System.Windows.Point(dp.X, dp.Upper)));
            ribXs.Add(dp.X);
        }

        private void DespatcherTimer_Tick(object sender, EventArgs e)
        {
            despatcherTimer.Stop();
            Viewer.LoadCamera();
        }

        private void Dialog_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateCameraPos();
            Viewer.Clear();
            LoadEditorParameters();
            loaded = true;
            NotifyPropertyChanged("Whole");
            NotifyPropertyChanged("Front");
            NotifyPropertyChanged("Back");
            NotifyPropertyChanged("Shell");
            NotifyPropertyChanged("ShellSize");
            NotifyPropertyChanged("Autofit");
            Redisplay();
        }

        private void GenerateFacesFromRibs(List<RibImageDetailsModel> generatingRibs, List<double> ribXs, List<Dimension> topDims, List<Dimension> sideDims)
        {
            int facesPerRib = generatingRibs[0].ProfilePoints.Count;

            // the indices of all the points generated for the shape
            int[,] surfaceVerticeIndices = new int[generatingRibs.Count, facesPerRib];

            // there should be a marker and hence a dimension for every rib. If ther isn't then
            // somethins wrong
            if (generatingRibs.Count != topDims.Count)
            {
                System.Diagnostics.Debug.WriteLine($"Ribs {generatingRibs.Count} TopView Dimensions {topDims.Count}");
            }
            else
            {
                // work out the range of faces we are going to do based upon whether we are doing
                // the whole model or just front or back

                // assume its whole model
                int start = 0;
                int end = facesPerRib;

                // are we only doing the front or back halves
                if (frontBody)
                {
                    end = facesPerRib / 2;
                }
                if (backBody)
                {
                    //start = facesPerRib / 2;
                    start = facesPerRib / 2;
                }

                // we need to record the left and right edge points so we can close them later
                List<PointF> leftEdge2DPoints = new List<PointF>();
                List<PointF> rightEdge2DPoints = new List<PointF>();

                // go through all the profile points for all the ribs, converting to a 3d position
                double leftMostRibX, rightMostX;
                Create3DSurfaceVerticesFromTheRibs(generatingRibs, ribXs, topDims, sideDims, surfaceVerticeIndices, 0, facesPerRib, leftEdge2DPoints, rightEdge2DPoints, out leftMostRibX, out rightMostX);
                facesPerRib = leftEdge2DPoints.Count;

                // Vertices now has all the points. surfaceVerticeIndices has a row for each rib, and its
                // columns are the indices of the 3d points So now add triangle faces for
                // consecutive pairs of points on each rib and the one after it.
                if (shell && frontBody && !backBody)
                {
                    TriangulateFrontShell(generatingRibs, facesPerRib, surfaceVerticeIndices, leftEdge2DPoints, rightEdge2DPoints, leftMostRibX, rightMostX);
                }
                else
                if (shell && backBody && !frontBody)
                {
                    TriangulateBackShell(generatingRibs, facesPerRib, surfaceVerticeIndices, leftEdge2DPoints, rightEdge2DPoints, leftMostRibX, rightMostX);
                }
                else
                if (!shell && frontBody && !backBody)
                {
                    TriangulateFrontNonShell(generatingRibs, facesPerRib, surfaceVerticeIndices, leftEdge2DPoints, rightEdge2DPoints, leftMostRibX, rightMostX);
                }
                else
                if (!shell && !frontBody && backBody)
                {
                    TriangulateBackNonShell(generatingRibs, facesPerRib, surfaceVerticeIndices, leftEdge2DPoints, rightEdge2DPoints, leftMostRibX, rightMostX);
                }
                else
                if (shell && !frontBody && !backBody)
                {
                    TriangulateShell(generatingRibs, facesPerRib, surfaceVerticeIndices, leftEdge2DPoints, rightEdge2DPoints, leftMostRibX, rightMostX);
                }
                else

                {
                    TriangulateNonShell(generatingRibs, facesPerRib, surfaceVerticeIndices, leftEdge2DPoints, rightEdge2DPoints, leftMostRibX, rightMostX);
                }

                // move everything to the centre and floor it
                CentreVertices();
            }
        }

        private double InchesToMM(double x)
        {
            return x * 25.4;
        }

        private void LoadEditorParameters()
        {
            string s = EditorParameters.Get("FilePath");
            if (s != "")
            {
                if (System.IO.File.Exists(s))
                {
                    filePath = s;

                    Read(filePath);

                    dirty = false;
                }
                WholeBody = true;
                FrontBody = false;
                BackBody = false;
                s = EditorParameters.Get("Model");
                if (s == "Front")
                {
                    WholeBody = false;
                    FrontBody = true;
                    BackBody = false;
                }
                else if (s == "Back")
                {
                    BackBody = true;
                    FrontBody = false;
                    WholeBody = false;
                }

                NumberOfDivisions = EditorParameters.GetInt("NumberOfDivisions", 100);

                AutoFit = EditorParameters.GetBoolean("AutoFit");
                Shell = EditorParameters.GetBoolean("Shell");
                ShellSize = EditorParameters.GetDouble("Shell", 1.0);
                OriginToCentroid = EditorParameters.GetBoolean("OriginToCentroid");
            }
        }

        private void MoveOriginToCentroid()
        {
            Point3D min = new Point3D(double.MaxValue, double.MaxValue, double.MaxValue);
            Point3D max = new Point3D(double.MinValue, double.MinValue, double.MinValue);
            PointUtils.MinMax(Vertices, ref min, ref max);
            double dx = -(min.X + (max.X - min.X) / 2.0);
            double dy = -(min.Y + (max.Y - min.Y) / 2.0);
            double dz = -(min.Z + (max.Z - min.Z) / 2.0);

            Point3DCollection pn = new Point3DCollection();
            for (int i = 0; i < Vertices.Count; i++)
            {
                pn.Add(new Point3D(Vertices[i].X + dx,
                                    Vertices[i].Y + dy,
                                    Vertices[i].Z + dz)
                       );
            }
            Vertices = pn;
        }

        private void NextRib()
        {
            if (selectedRibIndex < fuselageData.Ribs.Count - 1)
            {
                SelectedRibIndex++;
                SelectedRib = fuselageData.Ribs[selectedRibIndex];
            }
            if (SelectedRib != null)
            {
                RibbedFuselage.ItemsControlExtensions.ScrollToCenterOfView(RibList, SelectedRib);
            }
        }

        private void OnRibComand(object obj)
        {
            string prm = obj as string;
            if (!string.IsNullOrEmpty(prm))
            {
                switch (prm.ToLower())
                {
                    case "append":
                        {
                            RibImageDetailsModel rib = fuselageData.AddRib();
                            NotifyPropertyChanged("Ribs");
                            SelectedRibIndex = fuselageData.Ribs.Count - 1;
                            SelectedRib = rib;
                            fuselageData.AddMarker(rib);
                            RibsChanged();
                            UpdateMarkers();
                            UpdateModel();
                            dirty = true;
                        }
                        break;

                    case "copy":
                        {
                            SelectedRib = fuselageData.CloneRib(SelectedRibIndex);
                            SelectedRibIndex = SelectedRibIndex + 1;
                            RibsChanged();
                            UpdateMarkers();
                            UpdateModel();
                            dirty = true;
                        }
                        break;

                    case "insert":
                        {
                            SelectedRib = fuselageData.InsertRib(SelectedRibIndex);
                            SelectedRibIndex = SelectedRibIndex + 1;
                            RibsChanged();
                            UpdateMarkers();
                            UpdateModel();
                            dirty = true;
                        }
                        break;

                    case "rename":
                        {
                            fuselageData.RenameAllRibs();
                            UpdateMarkers();
                            UpdateModel();
                            dirty = true;
                        }
                        break;

                    case "delete":
                        {
                            if (fuselageData.DeleteRib(selectedRib))
                            {
                                if (selectedRibIndex > 0)
                                {
                                    SelectedRibIndex--;
                                    SelectedRib = Ribs[selectedRibIndex];
                                    RibsChanged();
                                }
                                UpdateMarkers();
                                UpdateModel();
                            }
                            dirty = true;
                        }
                        break;

                    case "load":
                        {
                            Load();
                            NotifyPropertyChanged("Ribs");
                            if (Ribs.Count > 0)
                            {
                                SelectedRib = Ribs[0];
                                SelectedRibIndex = 0;
                            }
                            RibsChanged();
                        }
                        break;

                    case "save":
                        {
                            Save();
                        }
                        break;

                    case "saveas":
                        {
                            SaveAs();
                        }
                        break;

                    case "previous":
                        {
                            PreviousRib();
                        }
                        break;

                    case "next":
                        {
                            NextRib();
                        }
                        break;

                    case "reset":
                        {
                            ResetRibs();
                            UpdateMarkers();
                            UpdateModel();
                            dirty = true;
                        }
                        break;
                }
            }
        }

        private void PositionTab_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var l = e.RemovedItems;
        }

        private void PreviousRib()
        {
            if (selectedRibIndex > 0)
            {
                SelectedRibIndex--;
                SelectedRib = fuselageData.Ribs[selectedRibIndex];
            }
            if (SelectedRib != null)
            {
                RibbedFuselage.ItemsControlExtensions.ScrollToCenterOfView(RibList, SelectedRib);
            }
        }

        private void Read(string fileName)
        {
            if (fuselageData != null)
            {
                fuselageData.Load(fileName);
                NotifyPropertyChanged("Ribs");

                TopImage = fuselageData.TopImageDetails.ImageFilePath;
                TopPath = fuselageData.TopImageDetails.FlexiPathText;
                SideImage = fuselageData.SideImageDetails.ImageFilePath;
                SidePath = fuselageData.SideImageDetails.FlexiPathText;
                RibsChanged();
            }
        }

        private void ResetRibs()
        {
            fuselageData.ResetMarkerPositions();
            TopView.Markers = GetMarkers();
            SideView.Markers = GetMarkers();
        }

        private void RibList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (SelectedRibIndex != -1)
            {
                // RibList.ScrollIntoView(SelectedRib);
                if (SelectedRibIndex != oldSelectedRibIndex && SelectedRibIndex >= 0 && SelectedRibIndex < Ribs.Count)
                {
                    selectedRib = Ribs[SelectedRibIndex];
                    RibbedFuselage.ItemsControlExtensions.ScrollToCenterOfView(RibList, SelectedRib);
                }
            }
        }

        private void RibsChanged()
        {
          
            CalculateMainRibSizes();
            List<double> ribXs = new List<double>();
            List<Dimension> topDims = new List<Dimension>();
            List<Dimension> sideDims = new List<Dimension>();
            List<RibImageDetailsModel> generatingRibs = CreateGeneratingRibs(ribXs, topDims, sideDims);
            ObservableCollection<RibImageDetailsModel> tmpRibs = new ObservableCollection<RibImageDetailsModel>();
            foreach (RibImageDetailsModel ridm in generatingRibs)
            {
                tmpRibs.Add(ridm);
            }
            ThreeDView.SetRibs(tmpRibs);
            modelUpToDate = false;
        }

        private void SaveAs()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Fuselage spar files (*.spr) | *.spr";
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Write(saveFileDialog.FileName);
                filePath = saveFileDialog.FileName;
                dirty = false;
            }
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            EditorParameters.Set("FilePath", filePath);
            String m = "Whole";
            if (backBody)
            {
                m = "Back";
            }
            if (frontBody)
            {
                m = "Front";
            }
            EditorParameters.Set("Model", m);
            EditorParameters.Set("NumberOfDivisions", numberOfDivisions.ToString());
            EditorParameters.Set("AutoFit", autoFit.ToString());
            EditorParameters.Set("Shell", shell.ToString());
            EditorParameters.Set("ShellSize", shellSize.ToString());
            EditorParameters.Set("OriginToCentroid", originToCentroid.ToString());
        }

        private int ScaleProfilePointConvertTo3D(List<Dimension> topDims, List<Dimension> sideDims, double x, int ribId, PointF pnt, out double z, out double y)
        {
            double v = (double)pnt.X * topDims[ribId].Height;

            z = v + topDims[ribId].P1.Y;
            v = (double)pnt.Y * sideDims[ribId].Height;

            y = -(v + sideDims[ribId].P1.Y);
            int vert = AddVertice(Vertices, x, y, z);
            return vert;
        }

        private void SideImageChanged(string imagePath)
        {
            SideImage = imagePath;
        }

        private void SideMarkerMoved(string s, System.Windows.Point p, bool finishedMove)
        {
            MoveMarker(s, p.X, finishedMove);
            if (finishedMove)
            {
                UpdateModel();
                Redisplay();
                dirty = true;
            }
        }

        private void SidePathChanged(string pathText)
        {
            LoggerLib.Logger.LogLine($"rcved notification from path control -SidePath changed to {pathText}");

            SidePath = pathText;
        }

        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var l = e.RemovedItems;
            if (l != null && l.Count == 1)
            {
                System.Windows.Controls.TabItem oldTc = l[0] as System.Windows.Controls.TabItem;
                if (oldTc != null && oldTc.Tag != null && oldTc.Tag.ToString() == "P")
                {
                    Viewer.SaveCamera();
                }
            }
            System.Windows.Controls.TabControl tc = sender as System.Windows.Controls.TabControl;
            if (tc != null)
            {
                if (tc.SelectedIndex == 3)
                {
                    ModelIsVisible = true;
                    UpdateModel();
                    despatcherTimer.Start();
                }
                else
                {
                    ModelIsVisible = false;
                }
            }
            System.Windows.Controls.TabItem ti = tc.SelectedItem as System.Windows.Controls.TabItem;
            LoggerLib.Logger.LogLine($"{ti.Header}");
        }

        private void TabItem_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
        }

        private void TabItem_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void TopImageChanged(string imagePath)
        {
            TopImage = imagePath;
        }

        private void TopMarkerMoved(string s, System.Windows.Point p, bool finishedMove)
        {
            MoveMarker(s, p.X, finishedMove);
            if (finishedMove)
            {
                UpdateModel();
                Redisplay();
                dirty = true;
            }
        }

        private void TopPathChanged(string pathText)
        {
            LoggerLib.Logger.LogLine($"rcved notification from path control -TopPath changed to {pathText}");
            TopPath = pathText;
            //  UpdateModel();
        }

        private void TriangulateBackNonShell(List<RibImageDetailsModel> generatingRibs, int facesPerRib, int[,] ribvertices, List<PointF> leftEdge, List<PointF> rightEdge, double leftx, double rightx)
        {
            int start = generatingRibs[0].BottomPointIndex;
            int end = generatingRibs[0].TopPointIndex;
            int numP = generatingRibs[0].ProfilePoints.Count;
            List<PointF> localleftEdge = new List<PointF>();
            List<PointF> localrightEdge = new List<PointF>();
            for (int ribIndex = 0; ribIndex < generatingRibs.Count - 1; ribIndex++)
            {
                int pIndex = start;
                while (pIndex != end)
                {
                    int ind2 = pIndex + 1;
                    if (ind2 >= numP)
                    {
                        ind2 = 0;
                    }
                    int v1 = ribvertices[ribIndex, pIndex];
                    int v2 = ribvertices[ribIndex, ind2];
                    int v3 = ribvertices[ribIndex + 1, pIndex];
                    int v4 = ribvertices[ribIndex + 1, ind2];

                    Faces.Add(v1);
                    Faces.Add(v2);
                    Faces.Add(v3);

                    Faces.Add(v3);
                    Faces.Add(v2);
                    Faces.Add(v4);
                    pIndex++;
                    if (pIndex >= numP)
                    {
                        pIndex = 0;
                    }
                }
            }

            int sideIndex = start;
            while (sideIndex != end + 1)
            {
                localleftEdge.Add(leftEdge[sideIndex]);

                localrightEdge.Add(rightEdge[sideIndex]);

                sideIndex++;
                if (sideIndex >= numP)
                {
                    sideIndex = 0;
                }
            }

            // close off the left
            TriangulatePerimiter(localleftEdge, leftx, 0, 0, true);

            // close off the right
            TriangulatePerimiter(localrightEdge, rightx, 0, 0, false);

            // close back
            for (int ribIndex = 0; ribIndex < generatingRibs.Count - 1; ribIndex++)
            {
                int v1 = ribvertices[ribIndex, start];
                int v2 = ribvertices[ribIndex, end];
                int v3 = ribvertices[ribIndex + 1, start];
                int v4 = ribvertices[ribIndex + 1, end];

                Faces.Add(v1);
                Faces.Add(v3);
                Faces.Add(v2);

                Faces.Add(v3);
                Faces.Add(v4);
                Faces.Add(v2);
            }
        }

        private void TriangulateBackShell(List<RibImageDetailsModel> generatingRibs, int facesPerRib, int[,] ribvertices, List<PointF> leftEdge, List<PointF> rightEdge, double leftx, double rightx)
        {
            double miny = double.MaxValue;
            double maxy = double.MinValue;
            double maxz = double.MinValue;
            int start = generatingRibs[0].BottomPointIndex;
            int end = generatingRibs[0].TopPointIndex;
            int numP = generatingRibs[0].ProfilePoints.Count;
            for (int index = 0; index < generatingRibs.Count; index++)
            {
                int j = start;
                while (j != end + 1)
                {
                    int v = ribvertices[index, j];
                    Point3D pm = Vertices[v];
                    miny = Math.Min(miny, pm.Y);
                    maxy = Math.Max(maxy, pm.Y);
                    maxz = Math.Max(maxz, pm.Z);
                    j++;
                }
            }

            // move points to origin 0,0
            double midy = miny + (maxy - miny) / 2.0;
            for (int index = 0; index < Vertices.Count; index++)
            {
                Vertices[index] = new Point3D(Vertices[index].X, Vertices[index].Y - midy, Vertices[index].Z - maxz);
            }

            Int32Collection topOutside = new Int32Collection();
            Int32Collection topInside = new Int32Collection();

            Int32Collection botOutside = new Int32Collection();
            Int32Collection botInside = new Int32Collection();

            for (int ribIndex = 0; ribIndex < generatingRibs.Count - 1; ribIndex++)
            {
                int pIndex = start;
                while (pIndex != end)
                {
                    int ind2 = pIndex + 1;
                    if (ind2 >= facesPerRib)
                    {
                        ind2 = 0;
                    }
                    int v1 = ribvertices[ribIndex, pIndex];
                    int v2 = ribvertices[ribIndex, ind2];
                    int v3 = ribvertices[ribIndex + 1, pIndex];
                    int v4 = ribvertices[ribIndex + 1, ind2];

                    if (pIndex == start)
                    {
                        botOutside.Add(v1);
                        if (ribIndex + 1 == generatingRibs.Count - 1)
                        {
                            botOutside.Add(v3);
                        }
                    }

                    if (ind2 == end)
                    {
                        topOutside.Add(v2);
                        if (ribIndex + 1 == generatingRibs.Count - 1)
                        {
                            topOutside.Add(v4);
                        }
                    }

                    Faces.Add(v1);
                    Faces.Add(v2);
                    Faces.Add(v3);

                    Faces.Add(v3);
                    Faces.Add(v2);
                    Faces.Add(v4);
                    pIndex++;
                }

                while (pIndex != start)
                {
                    int ind2 = pIndex - 1;
                    if (ind2 < 0)
                    {
                        ind2 = numP - 1;
                    }

                    int v1 = ribvertices[ribIndex, pIndex];
                    Point3D p = new Point3D(Vertices[v1].X, Vertices[v1].Y, Vertices[v1].Z);
                    AdjustPointForShell(shellSize, ref p, 1);
                    v1 = AddVertice(Vertices, p.X, p.Y, p.Z);

                    int v2 = ribvertices[ribIndex, ind2];
                    p = new Point3D(Vertices[v2].X, Vertices[v2].Y, Vertices[v2].Z);
                    AdjustPointForShell(shellSize, ref p, 1);
                    v2 = AddVertice(Vertices, p.X, p.Y, p.Z);

                    int v3 = ribvertices[ribIndex + 1, pIndex];
                    p = new Point3D(Vertices[v3].X, Vertices[v3].Y, Vertices[v3].Z);
                    AdjustPointForShell(shellSize, ref p, 1);
                    v3 = AddVertice(Vertices, p.X, p.Y, p.Z);

                    int v4 = ribvertices[ribIndex + 1, ind2];
                    p = new Point3D(Vertices[v4].X, Vertices[v4].Y, Vertices[v4].Z);
                    AdjustPointForShell(shellSize, ref p, 1);
                    v4 = AddVertice(Vertices, p.X, p.Y, p.Z);

                    if (ind2 == start)
                    {
                        botInside.Add(v2);
                        if (ribIndex + 1 == generatingRibs.Count - 1)
                        {
                            botInside.Add(v4);
                        }
                    }

                    if (pIndex == end)
                    {
                        topInside.Add(v1);
                        if (ribIndex + 1 == generatingRibs.Count - 1)
                        {
                            topInside.Add(v3);
                        }
                    }

                    Faces.Add(v1);
                    Faces.Add(v2);
                    Faces.Add(v3);

                    Faces.Add(v3);
                    Faces.Add(v2);
                    Faces.Add(v4);

                    pIndex--;
                    if (pIndex < 0)
                    {
                        pIndex = numP - 1;
                    }
                }
            }

            // close off the top edge
            for (int ribId = 0; ribId < topOutside.Count - 1; ribId++)
            {
                Faces.Add(topOutside[ribId]);
                Faces.Add(topInside[ribId]);
                Faces.Add(topOutside[ribId + 1]);

                Faces.Add(topInside[ribId]);
                Faces.Add(topInside[ribId + 1]);
                Faces.Add(topOutside[ribId + 1]);

                Faces.Add(botOutside[ribId]);
                Faces.Add(botOutside[ribId + 1]);
                Faces.Add(botInside[ribId]);

                Faces.Add(botInside[ribId]);
                Faces.Add(botOutside[ribId + 1]);
                Faces.Add(botInside[ribId + 1]);
            }

            // close left edge
            Int32Collection leftSide = new Int32Collection();

            int pIndex2 = start;
            int co = 0;
            while (pIndex2 != end + 1)
            {
                int v1 = ribvertices[0, pIndex2];
                leftSide.Add(v1);
                co++;
                Point3D p = new Point3D(Vertices[v1].X, Vertices[v1].Y, Vertices[v1].Z);
                AdjustPointForShell(shellSize, ref p, 1);
                v1 = AddVertice(Vertices, p.X, p.Y, p.Z);
                leftSide.Add(v1);
                co++;
                pIndex2++;
                if (pIndex2 == numP)
                {
                    pIndex2 = 0;
                }
            }
            for (int i = 0; i < co - 2; i += 2)
            {
                int v1 = leftSide[i];
                int v2 = leftSide[i + 1];
                int v3 = leftSide[i + 2];
                int v4 = leftSide[i + 3];

                Faces.Add(v1);
                Faces.Add(v2);
                Faces.Add(v3);

                Faces.Add(v3);
                Faces.Add(v2);
                Faces.Add(v4);
            }

            // close right edge
            int gri = generatingRibs.Count - 1;
            Int32Collection rightSide = new Int32Collection();
            pIndex2 = start;
            co = 0;
            while (pIndex2 != end + 1)
            {
                int v1 = ribvertices[gri, pIndex2];
                rightSide.Add(v1);
                co++;
                Point3D p = new Point3D(Vertices[v1].X, Vertices[v1].Y, Vertices[v1].Z);
                AdjustPointForShell(shellSize, ref p, 1);
                v1 = AddVertice(Vertices, p.X, p.Y, p.Z);
                rightSide.Add(v1);
                co++;
                pIndex2++;
            }
            for (int i = 0; i < co - 2; i += 2)
            {
                int v1 = rightSide[i];
                int v2 = rightSide[i + 1];
                int v3 = rightSide[i + 2];
                int v4 = rightSide[i + 3];

                Faces.Add(v1);
                Faces.Add(v3);
                Faces.Add(v2);

                Faces.Add(v3);
                Faces.Add(v4);
                Faces.Add(v2);
            }

            // finally, drop any unreferenced Vertices
            RemoveUnreferencedVertices();
        }

        private void TriangulateFrontNonShell(List<RibImageDetailsModel> generatingRibs, int facesPerRib, int[,] ribvertices, List<PointF> leftEdge, List<PointF> rightEdge, double leftx, double rightx)
        {
            int start = generatingRibs[0].TopPointIndex;
            int end = generatingRibs[0].BottomPointIndex;
            int numP = generatingRibs[0].ProfilePoints.Count;
            List<PointF> localleftEdge = new List<PointF>();
            List<PointF> localrightEdge = new List<PointF>();
            for (int ribIndex = 0; ribIndex < generatingRibs.Count - 1; ribIndex++)
            {
                int pIndex = start;
                while (pIndex != end - 1)
                {
                    int ind2 = pIndex + 1;
                    if (ind2 >= numP)
                    {
                        ind2 = 0;
                    }
                    int v1 = ribvertices[ribIndex, pIndex];
                    int v2 = ribvertices[ribIndex, ind2];
                    int v3 = ribvertices[ribIndex + 1, pIndex];
                    int v4 = ribvertices[ribIndex + 1, ind2];

                    Faces.Add(v1);
                    Faces.Add(v2);
                    Faces.Add(v3);

                    Faces.Add(v3);
                    Faces.Add(v2);
                    Faces.Add(v4);
                    pIndex++;
                    if (pIndex >= numP)
                    {
                        pIndex = 0;
                    }
                }
            }

            int sideIndex = start;
            while (sideIndex != end + 1)
            {
                localleftEdge.Add(leftEdge[sideIndex]);

                localrightEdge.Add(rightEdge[sideIndex]);

                sideIndex++;
                if (sideIndex >= numP)
                {
                    sideIndex = 0;
                }
            }

            // close off the left
            TriangulatePerimiter(localleftEdge, leftx, 0, 0, true);

            // close off the right
            TriangulatePerimiter(localrightEdge, rightx, 0, 0, false);

            // close back
            for (int ribIndex = 0; ribIndex < generatingRibs.Count - 1; ribIndex++)
            {
                int v1 = ribvertices[ribIndex, start];
                int v2 = ribvertices[ribIndex, end];
                int v3 = ribvertices[ribIndex + 1, start];
                int v4 = ribvertices[ribIndex + 1, end];

                Faces.Add(v1);
                Faces.Add(v3);
                Faces.Add(v2);

                Faces.Add(v3);
                Faces.Add(v4);
                Faces.Add(v2);
            }
        }

        private void TriangulateFrontShell(List<RibImageDetailsModel> generatingRibs, int facesPerRib, int[,] ribvertices, List<PointF> leftEdge, List<PointF> rightEdge, double leftx, double rightx)
        {
            double miny = double.MaxValue;
            double maxy = double.MinValue;
            double minz = double.MaxValue;
            int start = generatingRibs[0].TopPointIndex;
            int end = generatingRibs[0].BottomPointIndex + 1;
            int numP = generatingRibs[0].ProfilePoints.Count;
            for (int index = 0; index < generatingRibs.Count; index++)
            {
                int j = start;
                while (j != end)
                {
                    int v = ribvertices[index, j];
                    Point3D pm = Vertices[v];
                    miny = Math.Min(miny, pm.Y);
                    maxy = Math.Max(maxy, pm.Y);
                    minz = Math.Min(minz, pm.Z);
                    j++;
                    if (j == numP)
                    {
                        j = 0;
                    }
                }
            }

            // move points to origin 0,0
            double midy = miny + (maxy - miny) / 2.0;
            for (int index = 0; index < Vertices.Count; index++)
            {
                Vertices[index] = new Point3D(Vertices[index].X, Vertices[index].Y - midy, Vertices[index].Z - minz);
            }

            Int32Collection topPointA = new Int32Collection();
            Int32Collection topPointB = new Int32Collection();

            Int32Collection botPointA = new Int32Collection();
            Int32Collection botPointB = new Int32Collection();

            for (int ribIndex = 0; ribIndex < generatingRibs.Count - 1; ribIndex++)
            {
                int pIndex = start;
                while (pIndex != end - 1)
                {
                    int ind2 = pIndex + 1;
                    if (ind2 >= facesPerRib)
                    {
                        ind2 = 0;
                    }
                    int v1 = ribvertices[ribIndex, pIndex];
                    int v2 = ribvertices[ribIndex, ind2];
                    int v3 = ribvertices[ribIndex + 1, pIndex];
                    int v4 = ribvertices[ribIndex + 1, ind2];

                    if (pIndex == start)
                    {
                        topPointA.Add(v1);
                        if (ribIndex + 1 == generatingRibs.Count - 1)
                        {
                            topPointA.Add(v3);
                        }
                    }

                    if (ind2 == end - 1)
                    {
                        botPointA.Add(v2);
                        if (ribIndex + 1 == generatingRibs.Count - 1)
                        {
                            botPointA.Add(v4);
                        }
                    }

                    Faces.Add(v1);
                    Faces.Add(v2);
                    Faces.Add(v3);

                    Faces.Add(v3);
                    Faces.Add(v2);
                    Faces.Add(v4);
                    pIndex++;
                    if (pIndex == numP)
                    {
                        pIndex = 0;
                    }
                }

                while (pIndex != start)
                {
                    int ind2 = pIndex - 1;
                    if (ind2 < 0)
                    {
                        ind2 = numP - 1;
                    }

                    int v1 = ribvertices[ribIndex, pIndex];
                    Point3D p = new Point3D(Vertices[v1].X, Vertices[v1].Y, Vertices[v1].Z);
                    AdjustPointForShell(shellSize, ref p);
                    v1 = AddVertice(Vertices, p.X, p.Y, p.Z);

                    int v2 = ribvertices[ribIndex, ind2];
                    p = new Point3D(Vertices[v2].X, Vertices[v2].Y, Vertices[v2].Z);
                    AdjustPointForShell(shellSize, ref p);
                    v2 = AddVertice(Vertices, p.X, p.Y, p.Z);

                    int v3 = ribvertices[ribIndex + 1, pIndex];
                    p = new Point3D(Vertices[v3].X, Vertices[v3].Y, Vertices[v3].Z);
                    AdjustPointForShell(shellSize, ref p);
                    v3 = AddVertice(Vertices, p.X, p.Y, p.Z);

                    int v4 = ribvertices[ribIndex + 1, ind2];
                    p = new Point3D(Vertices[v4].X, Vertices[v4].Y, Vertices[v4].Z);
                    AdjustPointForShell(shellSize, ref p);
                    v4 = AddVertice(Vertices, p.X, p.Y, p.Z);

                    if (pIndex == end - 1)
                    {
                        botPointB.Add(v1);
                        if (ribIndex + 1 == generatingRibs.Count - 1)
                        {
                            botPointB.Add(v3);
                        }
                    }

                    if (ind2 == start)
                    {
                        topPointB.Add(v2);
                        if (ribIndex + 1 == generatingRibs.Count - 1)
                        {
                            topPointB.Add(v4);
                        }
                    }

                    Faces.Add(v1);
                    Faces.Add(v2);
                    Faces.Add(v3);

                    Faces.Add(v3);
                    Faces.Add(v2);
                    Faces.Add(v4);

                    pIndex--;
                    if (pIndex < 0)
                    {
                        pIndex = numP - 1;
                    }
                }
            }

            // close off the top edge
            for (int ribId = 0; ribId < topPointA.Count - 1; ribId++)
            {
                Faces.Add(topPointA[ribId]);
                Faces.Add(topPointA[ribId + 1]);
                Faces.Add(topPointB[ribId]);

                Faces.Add(topPointB[ribId]);
                Faces.Add(topPointA[ribId + 1]);
                Faces.Add(topPointB[ribId + 1]);

                Faces.Add(botPointA[ribId]);
                Faces.Add(botPointB[ribId]);
                Faces.Add(botPointA[ribId + 1]);

                Faces.Add(botPointB[ribId]);
                Faces.Add(botPointB[ribId + 1]);
                Faces.Add(botPointA[ribId + 1]);
            }

            // close left edge
            Int32Collection endA = new Int32Collection();

            int pIndex2 = start;
            int co = 0;
            while (pIndex2 != end)
            {
                int v1 = ribvertices[0, pIndex2];
                endA.Add(v1);
                co++;
                Point3D p = new Point3D(Vertices[v1].X, Vertices[v1].Y, Vertices[v1].Z);
                AdjustPointForShell(shellSize, ref p);
                v1 = AddVertice(Vertices, p.X, p.Y, p.Z);
                endA.Add(v1);
                co++;
                pIndex2++;
                if (pIndex2 == numP)
                {
                    pIndex2 = 0;
                }
            }
            for (int i = 0; i < co - 2; i += 2)
            {
                int v1 = endA[i];
                int v2 = endA[i + 1];
                int v3 = endA[i + 2];
                int v4 = endA[i + 3];

                Faces.Add(v1);
                Faces.Add(v2);
                Faces.Add(v3);

                Faces.Add(v3);
                Faces.Add(v2);
                Faces.Add(v4);
            }

            // close right edge
            int gri = generatingRibs.Count - 1;
            Int32Collection endB = new Int32Collection();
            pIndex2 = start;
            co = 0;
            while (pIndex2 != end)
            {
                int v1 = ribvertices[gri, pIndex2];
                endB.Add(v1);
                co++;
                Point3D p = new Point3D(Vertices[v1].X, Vertices[v1].Y, Vertices[v1].Z);
                AdjustPointForShell(shellSize, ref p);
                v1 = AddVertice(Vertices, p.X, p.Y, p.Z);
                endB.Add(v1);
                co++;
                pIndex2++;
                if (pIndex2 == numP)
                {
                    pIndex2 = 0;
                }
            }
            for (int i = 0; i < co - 2; i += 2)
            {
                int v1 = endB[i];
                int v2 = endB[i + 1];
                int v3 = endB[i + 2];
                int v4 = endB[i + 3];

                Faces.Add(v1);
                Faces.Add(v3);
                Faces.Add(v2);

                Faces.Add(v3);
                Faces.Add(v4);
                Faces.Add(v2);
            }

            // finally, drop any unreferenced Vertices

            RemoveUnreferencedVertices();
        }

        private void TriangulateNonShell(List<RibImageDetailsModel> generatingRibs, int facesPerRib, int[,] ribvertices, List<PointF> leftEdge, List<PointF> rightEdge, double leftx, double rightx)
        {
            for (int ribIndex = 0; ribIndex < generatingRibs.Count - 1; ribIndex++)
            {
                for (int pIndex = 0; pIndex < facesPerRib; pIndex++)
                {
                    int ind2 = pIndex + 1;
                    if (ind2 >= facesPerRib)
                    {
                        ind2 = 0;
                    }
                    int v1 = ribvertices[ribIndex, pIndex];
                    int v2 = ribvertices[ribIndex, ind2];
                    int v3 = ribvertices[ribIndex + 1, pIndex];
                    int v4 = ribvertices[ribIndex + 1, ind2];

                    Faces.Add(v1);
                    Faces.Add(v2);
                    Faces.Add(v3);

                    Faces.Add(v3);
                    Faces.Add(v2);
                    Faces.Add(v4);
                }
            }

            // close off the left
            TriangulatePerimiter(leftEdge, leftx, 0, 0, true);

            // close off the right
            TriangulatePerimiter(rightEdge, rightx, 0, 0, false);
        }

        private void TriangulatePerimiter(List<PointF> points, double xo, double yo, double z, bool invert)
        {
            TriangulationPolygon ply = new TriangulationPolygon();

            ply.Points = points.ToArray();
            List<Triangle> tris = ply.Triangulate();
            foreach (Triangle t in tris)
            {
                int c0 = AddVertice(Vertices, xo, yo + t.Points[0].X, z + t.Points[0].Y);
                int c1 = AddVertice(Vertices, xo, yo + t.Points[1].X, z + t.Points[1].Y);
                int c2 = AddVertice(Vertices, xo, yo + t.Points[2].X, z + t.Points[2].Y);
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

        private void TriangulateShell(List<RibImageDetailsModel> generatingRibs, int facesPerRib, int[,] surfaceVerticeIndices, List<PointF> leftEdge2DPoints, List<PointF> rightEdge2DPoints, double leftMostRibX, double rightMostRibX)
        {
            for (int ribIndex = 0; ribIndex < generatingRibs.Count - 1; ribIndex++)
            {
                for (int pIndex = 0; pIndex < facesPerRib; pIndex++)
                {
                    int ind2 = pIndex + 1;
                    if (ind2 >= facesPerRib)
                    {
                        ind2 = 0;
                    }
                    int v1 = surfaceVerticeIndices[ribIndex, pIndex];
                    int v2 = surfaceVerticeIndices[ribIndex, ind2];
                    int v3 = surfaceVerticeIndices[ribIndex + 1, pIndex];
                    int v4 = surfaceVerticeIndices[ribIndex + 1, ind2];

                    Faces.Add(v1);
                    Faces.Add(v2);
                    Faces.Add(v3);

                    Faces.Add(v3);
                    Faces.Add(v2);
                    Faces.Add(v4);
                }
            }

            List<Point3D> ribCentres = new List<Point3D>();
            for (int ribIndex = 0; ribIndex < generatingRibs.Count; ribIndex++)
            {
                double sumX = 0;
                double sumY = 0;
                double sumZ = 0;
                for (int pIndex = 0; pIndex < facesPerRib; pIndex++)
                {
                    int v1 = surfaceVerticeIndices[ribIndex, pIndex];
                    Point3D p = new Point3D(Vertices[v1].X, Vertices[v1].Y, Vertices[v1].Z);
                    sumX += p.X;
                    sumY += p.Y;
                    sumZ += p.Z;
                }
                Point3D ap = new Point3D(sumX / facesPerRib, sumY / facesPerRib, sumZ / facesPerRib);
                ribCentres.Add(ap);
            }
            for (int ribIndex = 0; ribIndex < generatingRibs.Count - 1; ribIndex++)
            {
                for (int pIndex = 0; pIndex < facesPerRib; pIndex++)
                {
                    int ind2 = pIndex + 1;
                    if (ind2 >= facesPerRib)
                    {
                        ind2 = 0;
                    }

                    int v1 = surfaceVerticeIndices[ribIndex, pIndex];
                    Point3D p = new Point3D(Vertices[v1].X, Vertices[v1].Y - ribCentres[ribIndex].Y, Vertices[v1].Z - ribCentres[ribIndex].Z);
                    AdjustPointForShell(shellSize, ref p, 1);
                    v1 = AddVertice(Vertices, p.X, p.Y + ribCentres[ribIndex].Y, p.Z + ribCentres[ribIndex].Z);

                    int v2 = surfaceVerticeIndices[ribIndex, ind2];
                    p = new Point3D(Vertices[v2].X, Vertices[v2].Y - ribCentres[ribIndex].Y, Vertices[v2].Z - ribCentres[ribIndex].Z);
                    AdjustPointForShell(shellSize, ref p, 1);
                    v2 = AddVertice(Vertices, p.X, p.Y + ribCentres[ribIndex].Y, p.Z + ribCentres[ribIndex].Z);

                    int v3 = surfaceVerticeIndices[ribIndex + 1, pIndex];
                    p = new Point3D(Vertices[v3].X, Vertices[v3].Y - ribCentres[ribIndex + 1].Y, Vertices[v3].Z - ribCentres[ribIndex + 1].Z);
                    AdjustPointForShell(shellSize, ref p, 1);
                    v3 = AddVertice(Vertices, p.X, p.Y + ribCentres[ribIndex + 1].Y, p.Z + ribCentres[ribIndex + 1].Z);

                    int v4 = surfaceVerticeIndices[ribIndex + 1, ind2];
                    p = new Point3D(Vertices[v4].X, Vertices[v4].Y - ribCentres[ribIndex + 1].Y, Vertices[v4].Z - ribCentres[ribIndex + 1].Z);
                    AdjustPointForShell(shellSize, ref p, 1);
                    v4 = AddVertice(Vertices, p.X, p.Y + ribCentres[ribIndex + 1].Y, p.Z + ribCentres[ribIndex + 1].Z);

                    Faces.Add(v1);
                    Faces.Add(v3);
                    Faces.Add(v2);

                    Faces.Add(v2);
                    Faces.Add(v3);
                    Faces.Add(v4);
                }
            }

            // Close left edge
            double cx = 0;
            double cy = 0;
            List<PointF> leftEdgeOutter = new List<PointF>();
            foreach (PointF pf in leftEdge2DPoints)
            {
                leftEdgeOutter.Add(new PointF(pf.X, pf.Y));
                cx += pf.X;
                cy += pf.Y;
            }
            cx = cx / leftEdge2DPoints.Count;
            cy = cy / leftEdge2DPoints.Count;

            List<PointF> leftEdgeInner = new List<PointF>();
            foreach (PointF pf in leftEdge2DPoints)
            {
                System.Windows.Point p = new System.Windows.Point(pf.X - cx, pf.Y - cy);
                // calcuate angle that point makes with origin
                if (p.Y != 0)
                {
                    if (p.X == 0)
                    {
                        if (p.Y > 0)
                        {
                            p.Y -= shellSize;
                            if (p.Y < 0)
                            {
                                p.Y = 0;
                            }
                        }
                        else
                        {
                            p.Y += shellSize;
                            if (p.Y > 0)
                            {
                                p.Y = 0;
                            }
                        }
                    }
                    else
                    {
                        double theta = Math.Atan2(p.Y, p.X);
                        double d = Math.Sqrt((p.Y * p.Y) + (p.X * p.X));
                        double r = (d - shellSize);
                        p.Y = r * Math.Sin(theta);
                        p.X = r * Math.Cos(theta);
                    }
                }
                else
                {
                    if (p.X > 0)
                    {
                        p.X -= shellSize;
                    }
                    else
                    {
                        p.X += shellSize;
                    }
                }

                leftEdgeInner.Add(new PointF((float)(p.X + cx), (float)(p.Y + cy)));
            }

            for (int i = 0; i < leftEdgeOutter.Count; i++)
            {
                int j = i + 1;
                if (j >= leftEdgeOutter.Count)
                {
                    j = 0;
                }

                int v1 = AddVertice(Vertices, leftMostRibX, leftEdgeOutter[i].X, leftEdgeOutter[i].Y);
                int v2 = AddVertice(Vertices, leftMostRibX, leftEdgeInner[i].X, leftEdgeInner[i].Y);
                int v3 = AddVertice(Vertices, leftMostRibX, leftEdgeOutter[j].X, leftEdgeOutter[j].Y);
                int v4 = AddVertice(Vertices, leftMostRibX, leftEdgeInner[j].X, leftEdgeInner[j].Y);
                Faces.Add(v1);
                Faces.Add(v2);
                Faces.Add(v3);

                Faces.Add(v3);
                Faces.Add(v2);
                Faces.Add(v4);
            }

            // Close right edge
            int last = 0;
            cx = 0;
            cy = 0;
            List<PointF> rightEdgeOutter = new List<PointF>();
            foreach (PointF pf in rightEdge2DPoints)
            {
                rightEdgeOutter.Add(new PointF(pf.X, pf.Y));
                cx += pf.X;
                cy += pf.Y;
            }
            cx = cx / rightEdge2DPoints.Count;
            cy = cy / rightEdge2DPoints.Count;

            List<PointF> rightEdgeInner = new List<PointF>();

            foreach (PointF pf in rightEdge2DPoints)
            {
                System.Windows.Point p = new System.Windows.Point(pf.X - cx, pf.Y - cy);
                // calculate angle that point makes with origin
                if (p.Y != 0)
                {
                    if (p.X == 0)
                    {
                        if (p.Y > 0)
                        {
                            p.Y -= shellSize;
                            if (p.Y < 0)
                            {
                                p.Y = 0;
                            }
                        }
                        else
                        {
                            p.Y += shellSize;
                            if (p.Y > 0)
                            {
                                p.Y = 0;
                            }
                        }
                    }
                    else
                    {
                        double theta = Math.Atan2(p.Y, p.X);
                        double d = Math.Sqrt((p.Y * p.Y) + (p.X * p.X));
                        double r = (d - shellSize);
                        p.Y = r * Math.Sin(theta);
                        p.X = r * Math.Cos(theta);
                    }
                }
                else
                {
                    if (p.X > 0)
                    {
                        p.X -= shellSize;
                    }
                    else
                    {
                        p.X += shellSize;
                    }
                }

                rightEdgeInner.Add(new PointF((float)(p.X + cx), (float)(p.Y + cy)));
            }

            for (int i = 0; i < rightEdgeOutter.Count; i++)
            {
                int j = i + 1;
                if (j >= rightEdgeOutter.Count)
                {
                    j = 0;
                }

                int v1 = AddVertice(Vertices, rightMostRibX, rightEdgeOutter[i].X, rightEdgeOutter[i].Y);
                int v2 = AddVertice(Vertices, rightMostRibX, rightEdgeInner[i].X, rightEdgeInner[i].Y);
                int v3 = AddVertice(Vertices, rightMostRibX, rightEdgeOutter[j].X, rightEdgeOutter[j].Y);
                int v4 = AddVertice(Vertices, rightMostRibX, rightEdgeInner[j].X, rightEdgeInner[j].Y);
                Faces.Add(v1);
                Faces.Add(v3);
                Faces.Add(v2);

                Faces.Add(v3);
                Faces.Add(v4);
                Faces.Add(v2);
            }
        }

        private void UpdateMarkers()
        {
            TopView.Markers = GetMarkers();
            SideView.Markers = GetMarkers();
            ThreeDView.Markers = GetMarkers();
        }

        private void UpdateModel()
        {
            if (ModelIsVisible && loaded)
            {
                GenerateModel();
                modelUpToDate = true;
                Viewer.Model = GetModel();
                ThreeDView.Redisplay();
            }
        }

        private void ViewTabChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            System.Windows.Controls.TabControl tc = sender as System.Windows.Controls.TabControl;
            if (tc != null)
            {
                switch (tc.SelectedIndex)
                {
                    case 0:
                        {
                            CopyPathToTopView();
                        }
                        break;

                    case 1:
                        {
                            CopyPathToSideView();
                        }
                        break;

                    case 2:
                        {
                            ThreeDView.Redisplay();
                        }
                        break;
                }
            }
        }

        private void Write(string filePath)
        {
            if (fuselageData != null)
            {
                fuselageData.Save(filePath);
            }
        }
    }
}