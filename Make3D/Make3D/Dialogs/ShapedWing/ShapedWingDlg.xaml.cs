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
            airFoilPath = AppDomain.CurrentDomain.BaseDirectory + "data\\Airfoils.xml";
            airFoilDoc = new XmlDocument();
            airFoilDoc.XmlResolver = null;
            rootairfoilNames = new List<string>();
            airfoilGroups = new List<string>();
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
                    Update();
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
                    NotifyPropertyChanged();
                    Update();
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

        private void GenerateShape()
        {
            ClearShape();

            if (displayPoints != null)
            {
                List<System.Windows.Point> line = new List<System.Windows.Point>();
                double top = 0;
                for (int i = 0; i < displayPoints.Count; i++)
                {
                    if (displayPoints[i].Y > top)
                    {
                        top = displayPoints[i].Y;
                    }
                }
                foreach (Point p in displayPoints)
                {
                    line.Add(new Point(p.X, top - p.Y));
                }
                int numSpokes = numDivisions;
                double deltaTheta = 360.0 / numSpokes; // in degrees
                Point3D[,] spokeVertices = new Point3D[numSpokes, line.Count];
                for (int i = 0; i < line.Count; i++)
                {
                    spokeVertices[0, i] = new Point3D(line[i].X, line[i].Y, 0);
                }

                for (int i = 1; i < numSpokes; i++)
                {
                    double theta = i * deltaTheta;
                    double rad = Math.PI * theta / 180.0;
                    for (int j = 0; j < line.Count; j++)
                    {
                        double x = spokeVertices[0, j].X * Math.Cos(rad);
                        double z = spokeVertices[0, j].X * Math.Sin(rad);
                        spokeVertices[i, j] = new Point3D(x, spokeVertices[0, j].Y, z);
                    }
                }
                Vertices.Clear();
                Vertices.Add(new Point3D(0, 0, 0));
                for (int i = 0; i < numSpokes; i++)
                {
                    for (int j = 0; j < line.Count; j++)
                    {
                        Vertices.Add(spokeVertices[i, j]);
                    }
                }
                int topPoint = Vertices.Count;
                Vertices.Add(new Point3D(0, 20, 0));
                Faces.Clear();
                int spOff;
                int spOff2;
                for (int i = 0; i < numSpokes; i++)
                {
                    spOff = i * line.Count + 1;
                    spOff2 = (i + 1) * line.Count + 1;
                    if (i == numSpokes - 1)
                    {
                        spOff2 = 1;
                    }
                    // base
                    Faces.Add(0);
                    Faces.Add(spOff2);
                    Faces.Add(spOff);

                    for (int j = 0; j < line.Count - 1; j++)
                    {
                        Faces.Add(spOff + j);
                        Faces.Add(spOff2 + j);
                        Faces.Add(spOff2 + j + 1);

                        Faces.Add(spOff + j);
                        Faces.Add(spOff2 + j + 1);
                        Faces.Add(spOff + j + 1);
                    }

                    // Top
                    Faces.Add(spOff + line.Count - 1);
                    Faces.Add(spOff2 + line.Count - 1);
                    Faces.Add(topPoint);
                }

                spOff = (numSpokes - 1) * line.Count + 1;
                spOff2 = 1;
                // base
                Faces.Add(0);
                Faces.Add(spOff2);
                Faces.Add(spOff);
            }
        }

        private void GenerateWing()
        {
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
                GenerateShape();
                Redisplay();
            }
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            EditorParameters.Set("Path", PathEditor.AbsolutePathString);
            EditorParameters.Set("NumDivisions", NumDivisions.ToString());
            EditorParameters.Set("ImagePath", PathEditor.ImagePath);
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

        private void Update()
        {
            // EnableControlsForShape();
            GenerateWing();
            Redisplay();
        }

        private void UpdateDisplay()
        {
            if (loaded)
            {
                GenerateShape();
                Redisplay();
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
    }
}