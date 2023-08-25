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
        private XmlDocument airFoilDoc;
        private List<String> airfoilGroups;
        private string airFoilPath;
        private double dihedralAngle;
        private double dihedralLimit = 20;
        private List<Point> displayPoints;
        private bool loaded;
        private int numDivisions;
        private List<String> rootairfoilNames;
        private string rootGroup;
        private string selectedRootAirfoil;
        private string warningText;
        private List<Point> selectedWingProfilePoints;
        private double selectedWingProfileLength;
        private readonly string defaultWingShape = "M 0,0 RL 10.000,10.000 RL 100.000,10.000 RQ 10.000,10.000 0.000,20.000 RL -100.000,20.000 RL -10.000,20.000";

        public ShapedWingDlg()
        {
            InitializeComponent();
            ToolName = "ShapedWing";
            DataContext = this;
            ModelGroup = MyModelGroup;
            loaded = false;
            numDivisions = 80;
            PathEditor.OnFlexiPathChanged += PathPointsChanged;
            PathEditor.DefaultImagePath = DefaultImagePath;
            PathEditor.ToolName = ToolName;
            PathEditor.HasPresets = true;
            airFoilPath = AppDomain.CurrentDomain.BaseDirectory + "data\\Airfoils.xml";
            airFoilDoc = new XmlDocument();
            airFoilDoc.XmlResolver = null;
            rootairfoilNames = new List<string>();
            airfoilGroups = new List<string>();
            selectedWingProfilePoints = null;
            dihedralAngle = 0.0;
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

        public int NumDivisions
        {
            get { return numDivisions; }
            set
            {
                if (value < 3 || value > 360)
                {
                    WarningText = "Number of divisions must be >= 3 and <= 360";
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

        public override bool ShowFloor
        {
            get
            {
                return showFloor;
            }
            set
            {
                if (showFloor != value)
                {
                    showFloor = value;
                    NotifyPropertyChanged();
                    Redisplay();
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

        private void GenerateWing()
        {
            ClearShape();
            bool needToCloseRight = false;
            FlexiPath flexipath = new FlexiPath();
            flexipath.FromString(PathEditor.GetPath());
            flexipath.CalculatePathBounds();
            List<double> ribX = new List<double>();
            List<double> dihedralOffset = new List<double>();
            List<Point>[] divisions = new List<Point>[numDivisions];
            double minX = double.MaxValue;
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
                        if (Math.Abs(1 - t) < 0.000001)
                        {
                            if (dp.Upper - dp.Lower > 0.001)
                            {
                                needToCloseRight = true;
                            }
                        }
                        if (dihedralAngle == 0.0)
                        {
                            dihedralOffset.Add(0);
                        }
                        else
                        {
                            double da = Math.Sin(DegToRad(dihedralAngle)) * dp.X;
                            dihedralOffset.Add(da);
                        }
                        var si = dp.Upper - dp.Lower;
                        divisions[currentDivision] = new List<Point>();
                        for (double m = 0.0; m <= 1.0; m += dt)
                        {
                            Point wp = GetProfileAt(selectedWingProfilePoints, selectedWingProfileLength, m);
                            double px = -((1.0 - wp.X) * si + dp.Lower);
                            Point scaledPoint = new Point(px, (wp.Y * si));
                            divisions[currentDivision].Add(scaledPoint);
                            minX = Math.Min(minX, px);
                        }

                        currentDivision++;
                    }
                }

                minX = Math.Abs(minX);

                for (int i = 0; i < numDivisions - 1; i++)
                {
                    for (int j = 0; j < divisions[0].Count; j++)
                    {
                        int k = j + 1;
                        if (k >= divisions[0].Count)
                        {
                            k = 0;
                        }
                        int p0 = AddVertice(ribX[i], divisions[i][j].X + minX, divisions[i][j].Y + dihedralOffset[i]);
                        int p1 = AddVertice(ribX[i], divisions[i][k].X + minX, divisions[i][k].Y + dihedralOffset[i]);
                        int p2 = AddVertice(ribX[i + 1], divisions[i + 1][k].X + minX, divisions[i + 1][k].Y + dihedralOffset[i + 1]);
                        int p3 = AddVertice(ribX[i + 1], divisions[i + 1][j].X + minX, divisions[i + 1][j].Y + dihedralOffset[i + 1]);

                        AddFace(p0, p2, p1);
                        AddFace(p0, p3, p2);
                    }
                }

                // close the root side
                List<System.Drawing.PointF> side = new List<System.Drawing.PointF>();
                for (int j = 0; j < divisions[0].Count; j++)
                {
                    int k = j + 1;
                    if (k >= divisions[0].Count)
                    {
                        k = 0;
                    }
                    System.Drawing.PointF pl = new System.Drawing.PointF((float)(divisions[0][j].X + minX), (float)(divisions[0][j].Y));
                    side.Add(pl);
                }
                TriangulatePerimiter(side, ribX[0], true);

                // do we need to close the right
                // if it was round we might not
                if (needToCloseRight)
                {
                    side.Clear();

                    for (int j = 0; j < divisions[0].Count; j++)
                    {
                        int k = j + 1;
                        if (k >= divisions[0].Count)
                        {
                            k = 0;
                        }
                        System.Drawing.PointF pl = new System.Drawing.PointF((float)(divisions[divisions.Length - 1][j].X + minX), (float)(divisions[divisions.Length - 1][j].Y));
                        side.Add(pl);
                    }
                    TriangulatePerimiter(side, ribX[divisions.Length - 1], false);
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

        private void UpdateDisplay()
        {
            if (loaded)
            {
                GenerateWing();
                Redisplay();
                ProfileDisplayer.Refresh();
            }
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WarningText = "";
            LoadAirFoils();

            RootGroup = airfoilGroups[0];

            SelectedRootAirfoil = rootairfoilNames[0];

            LoadEditorParameters();

            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            PathEditor.DefaultImagePath = DefaultImagePath;
            loaded = true;

            UpdateDisplay();
        }

        private void BaseModellerDialog_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ProfileDisplayer.Refresh();
        }
    }
}