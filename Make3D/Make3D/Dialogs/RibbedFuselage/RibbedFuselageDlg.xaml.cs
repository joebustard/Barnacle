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
using Barnacle.LineLib;
using Barnacle.RibbedFuselage.Models;

using Barnacle.ViewModels;
using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using Barnacle.Dialogs.RibbedFuselage.Views;
using System.Windows.Threading;

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
        private int numberOfDivisions;
        private int oldSelectedRibIndex = -1;
        private RibImageDetailsModel selectedRib;
        private int selectedRibIndex;
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
                        FlexiPath topViewFlexiPath = new FlexiPath();
                        topViewFlexiPath.FromString(fuselageData.TopPath);
                        topViewFlexiPath.CalculatePathBounds();

                        FlexiPath sideViewFlexiPath = new FlexiPath();
                        sideViewFlexiPath.FromString(fuselageData.SidePath);
                        sideViewFlexiPath.CalculatePathBounds();

                        bool okToGenerate = true;
                        double prevX = 0;
                        List<RibImageDetailsModel> generatingRibs = new List<RibImageDetailsModel>();
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

                            double autofirDx = 0.01;
                            string cpPath = cp.FlexiPathText;

                            if (autoFit && generatingRibs.Count > 0)
                            {
                                string prevPath = generatingRibs[generatingRibs.Count - 1].FlexiPathText;

                                // if the path for the current rib is the same as the last one and
                                // they are reasonable difference apart full the gap with with some
                                // other virtual ribs
                                if (cpPath == prevPath && x - prevX > autofirDx)
                                {
                                    double nx = prevX + autofirDx;
                                    while (nx < x - autofirDx)
                                    {
                                        CreateIntermediateRib(topViewFlexiPath, sideViewFlexiPath, generatingRibs, ribXs, topDims, sideDims, cp, nx);
                                        prevX = nx;
                                        nx += autofirDx;
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
                        }

                        okToGenerate = CheckAllRibsHaveData(generatingRibs);

                        // do we have enough data to construct the model
                        if (generatingRibs.Count > 1 && okToGenerate)
                        {
                            GenerateFaces(generatingRibs, ribXs, topDims, sideDims);
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
            if (dirty && MessageBox.Show("Data has changed. Save it before closing", "Warning", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
            {
                Save();
            }
            SaveEditorParmeters();
            base.Ok_Click(sender, e);
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

        private void CreateIntermediateRib(FlexiPath topViewFlexiPath, FlexiPath sideViewFlexiPath, List<RibImageDetailsModel> generatingRibs, List<double> ribXs, List<Dimension> topDims, List<Dimension> sideDims, RibImageDetailsModel cp, double nx)
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

            Redisplay();
        }

        private void GenerateFaces(List<RibImageDetailsModel> generatingRibs, List<double> ribXs, List<Dimension> topDims, List<Dimension> sideDims)
        {
            int facesPerRib = generatingRibs[0].ProfilePoints.Count;

            // the indices of all the points generated for the shape
            int[,] ribvertices = new int[generatingRibs.Count, facesPerRib];

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
                    start = facesPerRib / 2;
                }

                // we need to record the left and right edge points so we can close them later
                List<PointF> leftEdge = new List<PointF>();
                List<PointF> rightEdge = new List<PointF>();

                // go through all the profile points for all the ribs, converting to a 3d position

                double x = ribXs[0];
                double leftx = x;
                double rightx = x;
                int vert = 0;
                int vindex = 0;
                for (int i = 0; i < generatingRibs.Count; i++)
                {
                    x = ribXs[i];
                    // if this is the last rib count it as the right edge and record its position
                    if (i == generatingRibs.Count - 1)
                    {
                        rightx = x;
                    }

                    vindex = 0;
                    for (int proind = start; proind < end; proind++)
                    {
                        if (proind < generatingRibs[i].ProfilePoints.Count)
                        {
                            PointF pnt = generatingRibs[i].ProfilePoints[proind];

                            double v = (double)pnt.X * topDims[i].Height;

                            double z = v + topDims[i].P1.Y;

                            v = (double)pnt.Y * sideDims[i].Height;

                            double y = -(v + sideDims[i].P1.Y);
                            vert = AddVertice(Vertices, x, y, z);
                            ribvertices[i, vindex] = vert;
                            vindex++;
                            if (i == 0)
                            {
                                leftEdge.Add(new PointF((float)y, (float)z));
                            }
                            if (i == generatingRibs.Count - 1)
                            {
                                rightEdge.Add(new PointF((float)y, (float)z));
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"ERROR proind {proind} ProfilePoints Count");
                        }
                    }
                }
                facesPerRib = leftEdge.Count;

                // Vertices now has all the points. ribvertices has a row for each rib, and its
                // columns are the indices of the 3d points So now add triangle faces for
                // consecutive pairs of points on each rib and the one after it.
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
            }
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
            ThreeDView.SetRibs(fuselageData.Ribs);
            CalculateMainRibSizes();
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
            TopPath = pathText;
            UpdateModel();
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
                Viewer.Model = GetModel();
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