using Barnacle.Dialogs;
using Barnacle.Dialogs.RibbedFuselage.Models;
using Barnacle.LineLib;
using Barnacle.RibbedFuselage.Models;
using Barnacle.ViewModels;
using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.RibbedFuselage.ViewModels
{
    internal class FuselageViewModel : BaseModellerDialog, INotifyPropertyChanged
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

        public FuselageViewModel()
        {
            FuselageData = new FuselageModel();
            RibCommand = new RelayCommand(OnRibComand);
            SelectedRib = null;
            SelectedRibIndex = -1;
            HorizontalResolution = 96;
            VerticalResolution = 96;
            frontBody = false;
            BackBody = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool Autofit
        {
            get { return autoFit; }
            set
            {
                autoFit = value;
                if (ModelIsVisible)
                {
                    GenerateModel();
                }
            }
        }

        public bool BackBody
        {
            get { return backBody; }
            set
            {
                backBody = value;
                if (ModelIsVisible)
                {
                    GenerateModel();
                }
            }
        }

        public bool FrontBody
        {
            get { return frontBody; }
            set
            {
                frontBody = value;
                if (ModelIsVisible)
                {
                    GenerateModel();
                }
            }
        }

        public FuselageModel FuselageData
        {
            get { return fuselageData; }
            set
            {
                if (fuselageData != value)
                {
                    fuselageData = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool ModelIsVisible { get; set; }

        public RelayCommand RibCommand { get; set; }

        public ObservableCollection<RibImageDetailsModel> Ribs
        {
            get { return fuselageData.Ribs; }
        }

        public RibImageDetailsModel SelectedRib
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

        public int SelectedRibIndex
        {
            get { return selectedRibIndex; }
            set
            {
                if (value != selectedRibIndex)
                {
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
                    FuselageData?.SetSideImage(sideImage);
                    NotifyPropertyChanged();
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
                    FuselageData.SetSidePath(sidePath);
                    NotifyPropertyChanged();
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
                    FuselageData?.SetTopImage(topImage);
                    NotifyPropertyChanged();
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
                    FuselageData.SetTopPath(topPath);
                    NotifyPropertyChanged();
                }
            }
        }

        internal double HorizontalResolution { get; set; }

        internal double VerticalResolution { get; set; }

        public void GenerateModel()
        {
            try
            {
                // clear out existing 3d model
                ClearShape();
                if (fuselageData.Ribs != null && fuselageData.Ribs.Count > 1)
                {
                    if (!String.IsNullOrEmpty(fuselageData.SidePath) && !String.IsNullOrEmpty(fuselageData.TopPath))
                    {
                        FlexiPath topViewFlexiPath = new FlexiPath();
                        topViewFlexiPath.FromString(topPath);
                        topViewFlexiPath.CalculatePathBounds();

                        FlexiPath sideViewFlexiPath = new FlexiPath();
                        sideViewFlexiPath.FromString(sidePath);
                        sideViewFlexiPath.CalculatePathBounds();

                        bool okToGenerate = true;
                        double prevX = 0;
                        List<RibImageDetailsModel> theRibs = new List<RibImageDetailsModel>();
                        List<double> ribXs = new List<double>();
                        List<Dimension> topDims = new List<Dimension>();
                        List<Dimension> sideDims = new List<Dimension>();
                        // first get the x positins of the ribs and the height and width at that position from the top and side views.
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
                            /*
                             *string cpPath = cp.GenPath();
                              if (autoFit && theRibs.Count > 0)
                              {
                                  string prevPath = theRibs[theRibs.Count - 1].PathText;

                                  if (cpPath == prevPath)
                                  {
                                      if (x - prevX > 4)
                                      {
                                          double nx = prevX + 1;
                                          while (nx < x)
                                          {
                                              RibAndPlanEditControl nr = RibManager.Ribs[i].Clone(false);
                                              // nr.GenerateProfilePoints();
                                              theRibs.Add(nr);
                                              ribXs.Add(nx);
                                              Dimension dp1 = TopView.GetUpperAndLowerPoints((int)nx);
                                              topDims.Add(dp1);
                                              dp1 = SideView.GetUpperAndLowerPoints((int)nx);
                                              sideDims.Add(dp1);
                                              nx += 4;
                                          }
                                      }
                                  }
                              }
                              */
                            theRibs.Add(cp);

                            var dp = topViewFlexiPath.GetUpperAndLowerPoints(x);
                            topDims.Add(new Dimension(new System.Windows.Point(dp.X, dp.Lower), new System.Windows.Point(dp.X, dp.Upper)));
                            dp = sideViewFlexiPath.GetUpperAndLowerPoints(x);
                            sideDims.Add(new Dimension(new System.Windows.Point(dp.X, dp.Lower), new System.Windows.Point(dp.X, dp.Upper)));
                            ribXs.Add(dp.X);
                            prevX = x;
                        }
                        // check that all the ribs have profile points. If they don't we can't generate the shape.
                        for (int i = 0; i < theRibs.Count; i++)
                        {
                            if (theRibs[i].ProfilePoints == null || theRibs[i].ProfilePoints.Count == 0)
                            {
                                okToGenerate = false;
                                break;
                            }
                        }

                        // do we have enough data to construct the model
                        if (theRibs.Count > 1 && okToGenerate)
                        {
                            // theRibs[0].GenerateProfilePoints();
                            int facesPerRib = theRibs[0].ProfilePoints.Count;

                            // the indices of all the points generated for the shape
                            int[,] ribvertices = new int[theRibs.Count, facesPerRib];

                            // there should be a marker and hence a dimension for every rib.
                            // If ther isn't then somethins wrong
                            if (theRibs.Count != topDims.Count)
                            {
                                System.Diagnostics.Debug.WriteLine($"Ribs {theRibs.Count} TopView Dimensions {topDims.Count}");
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
                                for (int i = 0; i < theRibs.Count; i++)
                                {
                                    x = GetXmm(ribXs[i]);
                                    // if this is the last rib cunt it as the right edge
                                    if (i == theRibs.Count - 1)
                                    {
                                        rightx = x;
                                    }

                                    vindex = 0;
                                    for (int proind = start; proind < end; proind++)
                                    {
                                        if (proind < theRibs[i].ProfilePoints.Count)
                                        {
                                            PointF pnt = theRibs[i].ProfilePoints[proind];

                                            double v = (double)pnt.X * (double)topDims[i].Height;
                                            double z = GetYmm(v + (double)topDims[i].P1.Y);

                                            v = (double)pnt.Y * (double)sideDims[i].Height;
                                            double y = -GetYmm(v + sideDims[i].P1.Y);

                                            vert = AddVertice(x, y, z);
                                            ribvertices[i, vindex] = vert;
                                            vindex++;
                                            if (i == 0)
                                            {
                                                leftEdge.Add(new PointF((float)y, (float)z));
                                            }
                                            if (i == theRibs.Count - 1)
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
                                for (int ribIndex = 0; ribIndex < theRibs.Count - 1; ribIndex++)
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

        internal void HomeClick(object sender, RoutedEventArgs e)
        {
            base.Home_Click(sender, e);
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
                        if (Ribs.Count > 0)
                        {
                            SelectedRib = Ribs[0];
                            selectedRibIndex = 0;
                        }
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

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
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
                // regenerate the 3d fuselage
                if (ModelIsVisible)
                {
                    GenerateModel();
                    UpdateDisplay();
                }
            }
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
                            SelectedRib = rib;
                            fuselageData.AddMarker(rib.Name);
                        }
                        break;

                    case "copy":
                        {
                            SelectedRib = fuselageData.CloneRib(SelectedRibIndex);
                            NotifyPropertyChanged("Markers");
                        }
                        break;

                    case "rename":
                        {
                            fuselageData.RenameAllRibs();
                        }
                        break;

                    case "delete":
                        {
                            if (fuselageData.DeleteRib(selectedRib))
                            {
                                NotifyPropertyChanged("Ribs");
                            }
                        }
                        break;

                    case "load":
                        {
                            Load();
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
                }
            }
        }

        private void PreviousRib()
        {
            if (selectedRibIndex > 0)
            {
                SelectedRibIndex--;
                SelectedRib = fuselageData.Ribs[selectedRibIndex];
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

        private void TriangulatePerimiter(List<PointF> points, double xo, double yo, double z, bool invert)
        {
            TriangulationPolygon ply = new TriangulationPolygon();

            ply.Points = points.ToArray();
            List<Triangle> tris = ply.Triangulate();
            foreach (Triangle t in tris)
            {
                int c0 = AddVertice(xo, yo + t.Points[0].X, z + t.Points[0].Y);
                int c1 = AddVertice(xo, yo + t.Points[1].X, z + t.Points[1].Y);
                int c2 = AddVertice(xo, yo + t.Points[2].X, z + t.Points[2].Y);
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

        public void UpdateDisplay()
        {
            Redisplay();
            ModelGroup.Children.Add(GetFuselageModel());
            NotifyPropertyChanged("CameraPos");
        }

        protected GeometryModel3D GetFuselageModel()
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.Positions = Vertices;
            mesh.TriangleIndices = Faces;
            mesh.Normals = null;
            GeometryModel3D gm = new GeometryModel3D();
            gm.Geometry = mesh;

            DiffuseMaterial mt = new DiffuseMaterial();
            mt.Color = meshColour;
            mt.Brush = new SolidColorBrush(meshColour);
            gm.Material = mt;
            DiffuseMaterial mtb = new DiffuseMaterial();
            mtb.Color = Colors.CornflowerBlue;
            mtb.Brush = new SolidColorBrush(Colors.Green);
            gm.BackMaterial = mtb;

            return gm;
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