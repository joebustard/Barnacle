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

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for RibbedFuselageDlg.xaml
    /// </summary>
    public partial class RibbedFuselageDlg : BaseModellerDialog
    {
        private bool autoFit;
        private bool backBody;
        private bool dirty;
        private string filePath;
        private bool frontBody;
        private FuselageModel fuselageData;
        private RibImageDetailsModel selectedRib;
        private int selectedRibIndex;
        private String sideImage;
        private string sidePath;
        private String topImage;
        private string topPath;

        public RibbedFuselageDlg()
        {
            InitializeComponent();
            TopPathEditor.OnFlexiImageChanged = TopImageChanged;
            TopPathEditor.OnFlexiPathTextChanged = TopPathChanged;

            SidePathEditor.OnFlexiImageChanged = SideImageChanged;
            SidePathEditor.OnFlexiPathTextChanged = SidePathChanged;

            ModelGroup = MyModelGroup;

            RibCommand = new RelayCommand(OnRibComand);

            SelectedRib = null;
            SelectedRibIndex = -1;

            fuselageData = new FuselageModel();

            HorizontalResolution = 96;
            VerticalResolution = 96;
            frontBody = false;
            BackBody = false;
            DataContext = this;
        }

        public bool AutoFit
        {
            get { return autoFit; }
            set
            {
                autoFit = value;
                UpdateModel();
            }
        }

        public bool BackBody
        {
            get { return backBody; }
            set
            {
                backBody = value;
                UpdateModel();
            }
        }

        public bool FrontBody
        {
            get { return frontBody; }
            set
            {
                frontBody = value;
                UpdateModel();
            }
        }

        private void UpdateModel()
        {
            if (ModelIsVisible)
            {
                GenerateModel();
                Redisplay();
            }
        }

        public bool ModelIsVisible { get; set; }

        public RelayCommand RibCommand { get; set; }

        public int SelectedRibIndex
        {
            get { return selectedRibIndex; }
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
            get { return sideImage; }
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
            get { return sidePath; }
            set
            {
                if (sidePath != value)
                {
                    sidePath = value;
                    fuselageData.SetSidePath(sidePath);
                    NotifyPropertyChanged();
                    if (!String.IsNullOrEmpty(sidePath))
                    {
                        if (sidePath != SidePathEditor.AbsolutePathString)
                        {
                            SidePathEditor.FromString(sidePath, false);
                            CopyPathToSideView();
                        }
                    }
                }
            }
        }

        public String TopImage
        {
            get { return topImage; }
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
            get { return topPath; }
            set
            {
                if (string.Compare(topPath, value) != 0)
                {
                    topPath = value;
                    fuselageData.SetTopPath(topPath);

                    if (topPath != TopPathEditor.AbsolutePathString)
                    {
                        TopPathEditor.FromString(topPath, false);
                        CopyPathToTopView();
                    }
                }
            }
        }

        internal double HorizontalResolution { get; set; }

        public ObservableCollection<RibImageDetailsModel> Ribs
        {
            get { return fuselageData.Ribs; }
        }

        internal RibImageDetailsModel SelectedRib
        {
            get { return selectedRib; }
            set
            {
                if (selectedRib != value)
                {
                    selectedRib = value;
                    NotifyPropertyChanged();
                }
            }
        }

        internal double VerticalResolution { get; set; }

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
                        // first get the x positions of the ribs and the height and width at that position from the top and side views.
                        // generate the profile points for the ribs at the same time.
                        for (int i = 0; i < fuselageData.Ribs.Count; i++)
                        {
                            double x = fuselageData.Markers[i].Position;

                            RibImageDetailsModel cp;
                            cp = fuselageData.Ribs[i];
                            cp.GenerateProfilePoints();
                            /*
                            if (fuselageData.Ribs[i].Dirty)
                            {
                                cp = fuselageData.Ribs[i].Clone();
                                cp.GenerateProfilePoints();
                            }
                            else
                            {
                                cp = fuselageData.Ribs[i];
                            }
                            */
                            double autofirDx = 0.005;
                            string cpPath = cp.FlexiPathText;
                            
                            if (autoFit && generatingRibs.Count > 0)
                            {
                                string prevPath = generatingRibs[generatingRibs.Count - 1].FlexiPathText;

                                if (cpPath == prevPath)
                                {
                                    if (x - prevX > autofirDx)
                                    {
                                        double nx = prevX + autofirDx;
                                        while (nx < x - autofirDx)
                                        {
                                            RibImageDetailsModel nr = cp.Clone();
                                            nr.GenerateProfilePoints();
                                            generatingRibs.Add(nr);
                                            var dp = topViewFlexiPath.GetUpperAndLowerPoints(nx);
                                            topDims.Add(new Dimension(new System.Windows.Point(dp.X, dp.Lower), new System.Windows.Point(dp.X, dp.Upper)));
                                            dp = sideViewFlexiPath.GetUpperAndLowerPoints(x);
                                            sideDims.Add(new Dimension(new System.Windows.Point(dp.X, dp.Lower), new System.Windows.Point(dp.X, dp.Upper)));
                                            ribXs.Add(dp.X);
                                            prevX = nx;
                                            nx += autofirDx;
                                        }
                                    }
                                }
                            }

                            generatingRibs.Add(cp);

                            var dp1 = topViewFlexiPath.GetUpperAndLowerPoints(x);
                            topDims.Add(new Dimension(new System.Windows.Point(dp1.X, dp1.Lower), new System.Windows.Point(dp1.X, dp1.Upper)));
                            dp1 = sideViewFlexiPath.GetUpperAndLowerPoints(x);
                            sideDims.Add(new Dimension(new System.Windows.Point(dp1.X, dp1.Lower), new System.Windows.Point(dp1.X, dp1.Upper)));
                            ribXs.Add(dp1.X);
                            prevX = x;
                        }
                        // check that all the ribs have profile points. If they don't we can't generate the shape.
                        for (int i = 0; i < generatingRibs.Count; i++)
                        {
                            if (generatingRibs[i].ProfilePoints == null || generatingRibs[i].ProfilePoints.Count == 0)
                            {
                                okToGenerate = false;
                                break;
                            }
                        }

                        // do we have enough data to construct the model
                        if (generatingRibs.Count > 1 && okToGenerate)
                        {
                            // theRibs[0].GenerateProfilePoints();
                            int facesPerRib = generatingRibs[0].ProfilePoints.Count;

                            // the indices of all the points generated for the shape
                            int[,] ribvertices = new int[generatingRibs.Count, facesPerRib];

                            // there should be a marker and hence a dimension for every rib.
                            // If ther isn't then somethins wrong
                            if (generatingRibs.Count != topDims.Count)
                            {
                                System.Diagnostics.Debug.WriteLine($"Ribs {generatingRibs.Count} TopView Dimensions {topDims.Count}");
                            }
                            else
                            {
                                // work out the range of faces we are going to do based upon whether we
                                // are doing the whole model or just fron or back

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

                                // go through all the profile points for all the ribs,
                                // converting to a 3d position
                                double x = GetXmm(ribXs[0]);
                                double leftx = x;
                                double rightx = x;
                                int vert = 0;
                                int vindex = 0;
                                for (int i = 0; i < generatingRibs.Count; i++)
                                {
                                    x = GetXmm(ribXs[i]);
                                    // if this is the last rib cunt it as the right edge
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

                                            double v = (double)pnt.X * (double)topDims[i].Height;
                                            double z = GetYmm(v + (double)topDims[i].P1.Y);

                                            v = (double)pnt.Y * (double)sideDims[i].Height;
                                            double y = -GetYmm(v + sideDims[i].P1.Y);
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

                                // Vertices now has all the points.
                                // ribvertices has a row for each rib, and its columns are the indices of the 3d points
                                // So now add triangle faces for consecutive pairs of points on each rib and the one after it.
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

                                TriangulatePerimiter(leftEdge, leftx, 0, 0, true);
                                TriangulatePerimiter(rightEdge, rightx, 0, 0, false);
                                CentreVertices();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "GenerateSkin");
                throw ex;
            }
        }

        public void Load()
        {
            OpenFileDialog opDlg = new OpenFileDialog();
            opDlg.Filter = "Fusalage spar files (*.spr) | *.spr";
            if (opDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    filePath = opDlg.FileName;
                    if (fuselageData != null)
                    {
                        fuselageData.Load(filePath);
                        NotifyPropertyChanged("Ribs");

                        TopImage = fuselageData.TopImageDetails.ImageFilePath;
                        TopPath = fuselageData.TopImageDetails.FlexiPathText;
                        SideImage = fuselageData.SideImageDetails.ImageFilePath;
                        SidePath = fuselageData.SideImageDetails.FlexiPathText;
                    }
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

        private void Dialog_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateCameraPos();
            MyModelGroup.Children.Clear();

            Redisplay();
        }

        private double InchesToMM(double x)
        {
            return x * 25.4;
        }

        private void NextRib()
        {
            if (selectedRibIndex < fuselageData.Ribs.Count - 1)
            {
                SelectedRibIndex++;
                SelectedRib = fuselageData.Ribs[selectedRibIndex];
            }
            RibbedFuselage.ItemsControlExtensions.ScrollToCenterOfView(RibList, SelectedRib);
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
                            UpdateMarkers();
                            UpdateModel();
                            dirty = true;
                        }
                        break;

                    case "copy":
                        {
                            SelectedRib = fuselageData.CloneRib(SelectedRibIndex);
                            SelectedRibIndex = SelectedRibIndex + 1;
                            UpdateMarkers();
                            UpdateModel();
                            dirty = true;
                        }
                        break;

                    case "insert":
                        {
                            SelectedRib = fuselageData.InsertRib(SelectedRibIndex);
                            SelectedRibIndex = SelectedRibIndex + 1;
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
                        }
                        break;

                    case "save":
                        {
                            Save();
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

        private void UpdateMarkers()
        {
            TopView.Markers = GetMarkers();
            SideView.Markers = GetMarkers();
        }

        private void PreviousRib()
        {
            if (selectedRibIndex > 0)
            {
                SelectedRibIndex--;
                SelectedRib = fuselageData.Ribs[selectedRibIndex];
            }
            RibbedFuselage.ItemsControlExtensions.ScrollToCenterOfView(RibList, SelectedRib);
        }

        private void ResetRibs()
        {
            fuselageData.ResetMarkerPositions();
            TopView.Markers = GetMarkers();
            SideView.Markers = GetMarkers();
        }

        private int oldSelectedRibIndex = -1;

        private void RibList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (SelectedRibIndex != -1)
            {
                // RibList.ScrollIntoView(SelectedRib);
                if (SelectedRibIndex != oldSelectedRibIndex)
                {
                    RibbedFuselage.ItemsControlExtensions.ScrollToCenterOfView(RibList, SelectedRib);
                }
            }
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
            System.Windows.Controls.TabControl tc = sender as System.Windows.Controls.TabControl;
            if (tc != null)
            {
                if (tc.SelectedIndex == 3)
                {
                    ModelIsVisible = true;
                    UpdateModel();
                }
                else
                {
                    ModelIsVisible = false;
                }
            }
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

        private void ViewTabChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            System.Windows.Controls.TabControl tc = sender as System.Windows.Controls.TabControl;
            if (tc != null)
            {
                if (tc.SelectedIndex == 0)
                {
                    CopyPathToTopView();
                }
                else
                {
                    CopyPathToSideView();
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