using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Xml;

namespace Make3D.Dialogs
{
    /// <summary>
    /// Interaction logic for Blank.xaml
    /// </summary>
    public partial class Propeller : BaseModellerDialog, INotifyPropertyChanged
    {
        private XmlDocument airFoilDoc;
        private List<String> airfoilGroups;
        private List<String> airfoilNames;
        private string airFoilPath;
        private double bladeAngle;

        private double bladeLength;

        private double bladeMid;
        private double bladeRoot;
        private double bladeTip;
        private int numberOfBlades;
        private string rootGroup;
        private double rootOffset;
        private string selectedAirfoil;
        private bool loaded;
        private double midOffset;

        public double MidOffset
        {
            get { return midOffset; }
            set
            {
                midOffset = value;
                NotifyPropertyChanged();
                UpdateDisplay();
            }
        }

        public Propeller()
        {
            InitializeComponent();
            ToolName = "Propeller";
            DataContext = this;
            NumberOfBlades = 2;
            BladeLength = 60;
            RootOffset = 8;
            MidOffset = 8;
            BladeRoot = 8;
            BladeMid = 10;
            BladeTip = 6;
            BladeAngle = 45;
            airFoilPath = AppDomain.CurrentDomain.BaseDirectory + "data\\Airfoils.xml";
            airFoilDoc = new XmlDocument();
            airfoilNames = new List<string>();
            airfoilGroups = new List<string>();
            loaded = false;
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

        public List<string> AirfoilNames
        {
            get
            {
                return airfoilNames;
            }
            set
            {
                if (airfoilNames != value)
                {
                    airfoilNames = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double BladeAngle
        {
            get
            {
                return bladeAngle;
            }
            set
            {
                if (bladeAngle != value)
                {
                    bladeAngle = value;

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

        public double BladeMid
        {
            get
            {
                return bladeMid;
            }
            set
            {
                if (bladeMid != value)
                {
                    bladeMid = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double BladeRoot
        {
            get
            {
                return bladeRoot;
            }
            set
            {
                if (bladeRoot != value)
                {
                    bladeRoot = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double BladeTip
        {
            get
            {
                return bladeTip;
            }
            set
            {
                if (bladeTip != value)
                {
                    bladeTip = value;
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
                    if (value >= 2)
                    {
                        numberOfBlades = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
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
                    AirfoilNames = names;
                }
            }
        }

        public double RootOffset
        {
            get
            {
                return rootOffset;
            }
            set
            {
                if (rootOffset != value)
                {
                    rootOffset = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public string SelectedAirfoil
        {
            get
            {
                return selectedAirfoil;
            }
            set
            {
                if (selectedAirfoil != value)
                {
                    selectedAirfoil = value;
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

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParmeters();
            DialogResult = true;
            Close();
        }

        private void GenerateShape()
        {
            ClearShape();
            GenerateBlade();
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
        }

        private void UpdateDisplay()
        {
            if (loaded)
            {
                GenerateShape();
                Redisplay();
            }
        }

        private void Redisplay()
        {
            if (MyModelGroup != null)
            {
                MyModelGroup.Children.Clear();

                if (floor != null && ShowFloor)
                {
                    MyModelGroup.Children.Add(floor.FloorMesh);
                    foreach (GeometryModel3D m in grid.Group.Children)
                    {
                        MyModelGroup.Children.Add(m);
                    }
                }

                if (axies != null && ShowAxies)
                {
                    foreach (GeometryModel3D m in axies.Group.Children)
                    {
                        MyModelGroup.Children.Add(m);
                    }
                }
                GeometryModel3D gm = GetModel();
                MyModelGroup.Children.Add(gm);
            }
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
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

        private void TriangulatePerimiter(List<Point> points, double l, double xo, double yo, double z, bool invert)
        {
            TriangulationPolygon ply = new TriangulationPolygon();
            List<System.Drawing.PointF> pf = new List<System.Drawing.PointF>();
            foreach (Point p in points)
            {
                pf.Add(new System.Drawing.PointF((float)p.X, (float)p.Y));
            }
            ply.Points = pf.ToArray();
            List<Triangle> tris = ply.Triangulate();
            foreach (Triangle t in tris)
            {
                int c0 = AddVertice(xo + t.Points[0].X * l, yo + t.Points[0].Y * l, z);
                int c1 = AddVertice(xo + t.Points[1].X * l, yo + t.Points[1].Y * l, z);
                int c2 = AddVertice(xo + t.Points[2].X * l, yo + t.Points[2].Y * l, z);
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

        private void GenerateBlade()
        {
            if (RootGroup != "" && SelectedAirfoil != "")
            {
                double rootEdgeLength = 0;
                double midEdgeLength = 0;
                double tipEdgeLength = 0;
                double offsetX = 0;
                double offsetY = 0;
                double oz = rootOffset;
                double midoffsetZ = midOffset;
                double tipoffsetZ = bladeLength;

                List<Point> rootProfile = GetProfilePoints(RootGroup, SelectedAirfoil, bladeRoot, ref rootEdgeLength);
                List<Point> midProfile = GetProfilePoints(RootGroup, SelectedAirfoil, bladeMid, ref midEdgeLength);
                List<Point> tipProfile = GetProfilePoints(RootGroup, SelectedAirfoil, bladeTip, ref tipEdgeLength);

                Vertices.Clear();
                Faces.Clear();
                double dtheta = (Math.PI * 2) / NumberOfBlades;
                double theta = 0;
                for (int b = 0; b < numberOfBlades; b++)
                {
                    double dt = 0.01;
                    double startT = 0;
                    double endT = 1;
                    double rtoff = bladeMid - bladeRoot;
                    double tpoff = bladeMid - bladeTip;
                    double xoff = -(bladeMid / 2);
                    List<Point> rootPnts = new List<Point>();
                    List<Point> tipPnts = new List<Point>();
                    for (double t = startT; t < endT; t += dt)
                    {
                        Point p1 = GetProfileAt(rootProfile, rootEdgeLength, t);
                        rootPnts.Add(p1);
                        Point p2 = GetProfileAt(rootProfile, rootEdgeLength, t + dt);
                        Point p3 = GetProfileAt(midProfile, midEdgeLength, t + dt);
                        Point p4 = GetProfileAt(midProfile, midEdgeLength, t);

                        Point3D pd1 = new Point3D((p1.X * bladeRoot) + rtoff + xoff, p1.Y * bladeRoot + offsetY, oz);
                        Point3D pd2 = new Point3D((p2.X * bladeRoot) + rtoff + xoff, p2.Y * bladeRoot + offsetY, oz);
                        Point3D pd3 = new Point3D((p3.X * bladeMid) + xoff, (p3.Y * bladeMid) + offsetY, midoffsetZ + oz);
                        Point3D pd4 = new Point3D((p4.X * bladeMid) + xoff, (p4.Y * bladeMid) + offsetY, midoffsetZ + oz);

                        pd1 = RotatePoint(pd1, theta);
                        pd2 = RotatePoint(pd2, theta);
                        pd3 = RotatePoint(pd3, theta);
                        pd4 = RotatePoint(pd4, theta);

                        /*
                        Point3D pd1 = new Point3D((p1.X * bladeRoot) + rtoff, 0, p1.Y * bladeRoot + offsetY);
                        Point3D pd2 = new Point3D((p2.X * bladeRoot) + rtoff, 0, p2.Y * bladeRoot + offsetY);
                        Point3D pd3 = new Point3D((p3.X * bladeMid), midoffsetZ, (p3.Y * bladeMid) + offsetY);
                        Point3D pd4 = new Point3D((p4.X * bladeMid), midoffsetZ, (p4.Y * bladeMid) + offsetY);
                        */
                        int v1 = AddVertice(pd1);
                        int v2 = AddVertice(pd2);
                        int v3 = AddVertice(pd3);
                        int v4 = AddVertice(pd4);

                        Faces.Add(v1);
                        Faces.Add(v3);
                        Faces.Add(v2);

                        Faces.Add(v1);
                        Faces.Add(v4);
                        Faces.Add(v3);

                        p1 = GetProfileAt(midProfile, rootEdgeLength, t);

                        p2 = GetProfileAt(midProfile, rootEdgeLength, t + dt);
                        p3 = GetProfileAt(tipProfile, midEdgeLength, t + dt);
                        p4 = GetProfileAt(tipProfile, midEdgeLength, t);
                        tipPnts.Add(p4);

                        pd1 = new Point3D(p1.X * bladeMid + xoff, p1.Y * bladeMid + offsetY, midoffsetZ + oz);
                        pd2 = new Point3D(p2.X * bladeMid + xoff, p2.Y * bladeMid + offsetY, midoffsetZ + oz);
                        pd3 = new Point3D((p3.X * bladeTip) + tpoff + xoff, (p3.Y * bladeTip) + offsetY, tipoffsetZ + oz);
                        pd4 = new Point3D((p4.X * bladeTip) + tpoff + xoff, (p4.Y * bladeTip) + offsetY, tipoffsetZ + oz);

                        pd1 = RotatePoint(pd1, theta);
                        pd2 = RotatePoint(pd2, theta);
                        pd3 = RotatePoint(pd3, theta);
                        pd4 = RotatePoint(pd4, theta);

                        v1 = AddVertice(pd1);
                        v2 = AddVertice(pd2);
                        v3 = AddVertice(pd3);
                        v4 = AddVertice(pd4);

                        Faces.Add(v1);
                        Faces.Add(v3);
                        Faces.Add(v2);

                        Faces.Add(v1);
                        Faces.Add(v4);
                        Faces.Add(v3);
                    }

                    double mr = bladeTip / 2;
                    double sr = mr / 10;
                    EllipseTip(tipPnts, mr, sr, offsetX + tpoff, offsetY, tipoffsetZ);
                    TriangulatePerimiter(rootPnts, rootEdgeLength, 0, 0, 0, true);
                    theta += dtheta;
                }

                // dont centre until all blades are in place
                // CentreVertices();
            }
        }

        private Point3D RotatePoint(Point3D p, double theta)
        {
            Point3D res;
            double s = Math.Sin(theta);
            double c = Math.Cos(theta);
            double x = p.X * c - p.Z * s;
            double z = p.X * s + p.Z * c;

            res = new Point3D(x, p.Y, z);

            return res;
        }

        private Point GetEllipsePoint(double a, double b, double t)
        {
            Point res = new Point(0, 0);
            if (t >= 0 && t <= 1)
            {
                double theta = t * Math.PI * 2;
                res.X = a * Math.Cos(theta);
                res.Y = b * Math.Sin(theta);
            }
            return res;
        }

        private void EllipseTip(List<Point> tipPnts, double mainRad, double sideRad, double tX, double tY, double tZ)
        {
            List<Point3D> tipEdge = new List<Point3D>();
            double md = mainRad * 2;
            double stepSize = 1.0 / (tipPnts.Count - 1);

            for (double t = 0; t < 0.5; t += stepSize)
            {
                Point elp = GetEllipsePoint(mainRad, sideRad, t);
                //  Point3D p = new Point3D(tX + elp.X + mainRad, tY, tZ + elp.Y);
                Point3D p = new Point3D(elp.X, tY, elp.Y);
                tipEdge.Add(p);
            }
            for (double t = 0.5; t >= 0; t -= stepSize)
            {
                Point elp = GetEllipsePoint(mainRad, sideRad, t);
                // Point3D p = new Point3D(tX + elp.X + mainRad, tY, tZ + elp.Y);
                Point3D p = new Point3D(elp.X, tY, elp.Y);
                tipEdge.Add(p);
            }

            for (int i = 0; i < tipPnts.Count - 1; i++)
            {
                Point3D pd1 = new Point3D(tX + (tipPnts[i].X * md), tY + (tipPnts[i].Y * md), tZ);
                Point3D pd2 = new Point3D(tX + mainRad - tipEdge[i].X, tipEdge[i].Y, tZ + tipEdge[i].Z);
                Point3D pd3 = new Point3D(tX + mainRad - tipEdge[i + 1].X, tipEdge[i + 1].Y, tZ + tipEdge[i + 1].Z);
                Point3D pd4 = new Point3D(tX + (tipPnts[i + 1].X * md), tY + (tipPnts[i + 1].Y * md), tZ);

                int v1 = AddVertice(pd1);
                int v2 = AddVertice(pd2);
                int v3 = AddVertice(pd3);
                int v4 = AddVertice(pd4);

                Faces.Add(v1);
                Faces.Add(v2);
                Faces.Add(v3);

                Faces.Add(v1);
                Faces.Add(v3);
                Faces.Add(v4);
            }
        }

        private Point GetProfileAt(List<Point> profile, double length, double t)
        {
            Point res = new Point(0, 0);
            if (profile != null && profile.Count > 0)
            {
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
            }
            return res;
        }

        private List<Point> GetProfilePoints(string grpName, string airfoil, double len, ref double dist)
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAirFoils();

            RootGroup = airfoilGroups[0];

            SelectedAirfoil = airfoilNames[0];

            LoadEditorParameters();
            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            GenerateShape();
            Redisplay();
            loaded = true;
        }
    }
}