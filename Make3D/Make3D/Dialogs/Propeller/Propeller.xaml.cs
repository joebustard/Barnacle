using EarClipperLib;
using Make3D.Models;
using Make3D.Object3DLib;
using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
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
        private bool domedHub;
        private bool flatHub;
        private double hubHeight;
        private double hubOffset;
        private double hubRadius;
        private bool loaded;
        private double midOffset;
        private int numberOfBlades;
        private string rootGroup;
        private double rootOffset;
        private string selectedAirfoil;

        private double spokeRadius;

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
            BladeAngle = 10;
            HubRadius = 5;
            HubHeight = 5;
            HubOffset = 0;
            SpokeRadius = 3;
            FlatHub = true;
            DomedHub = false;
            airFoilPath = AppDomain.CurrentDomain.BaseDirectory + "data\\Airfoils.xml";
            airFoilDoc = new XmlDocument();
            airfoilNames = new List<string>();
            airfoilGroups = new List<string>();
            loaded = false;
            ModelGroup = MyModelGroup;
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

        public bool DomedHub
        {
            get
            {
                return domedHub;
            }
            set
            {
                if (domedHub != value)
                {
                    domedHub = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public bool FlatHub
        {
            get
            {
                return flatHub;
            }
            set
            {
                if (flatHub != value)
                {
                    flatHub = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double HubHeight
        {
            get
            {
                return hubHeight;
            }
            set
            {
                if (hubHeight != value)
                {
                    hubHeight = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double HubOffset
        {
            get
            {
                return hubOffset;
            }
            set
            {
                hubOffset = value;
                NotifyPropertyChanged();
                UpdateDisplay();
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

        public double MidOffset
        {
            get
            {
                return midOffset;
            }
            set
            {
                midOffset = value;
                NotifyPropertyChanged();
                UpdateDisplay();
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
                    if (names.Count > 0)
                    {
                        SelectedAirfoil = names[0];
                    }
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

        public double SpokeRadius
        {
            get
            {
                return spokeRadius;
            }
            set
            {
                spokeRadius = value;
                NotifyPropertyChanged();
                UpdateDisplay();
            }
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParmeters();
            DialogResult = true;
            Close();
        }

        private void EllipseTip(List<Point> tipPnts, double mainRad, double sideRad, double tX, double tY, double tZ, double tilt, double rotate)
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

                pd1 = RotatePointAroundZ(pd1, tilt);
                pd1 = RotatePoint(pd1, rotate);

                pd2 = RotatePointAroundZ(pd2, tilt);
                pd2 = RotatePoint(pd2, rotate);

                pd3 = RotatePointAroundZ(pd3, tilt);
                pd3 = RotatePoint(pd3, rotate);

                pd4 = RotatePointAroundZ(pd4, tilt);
                pd4 = RotatePoint(pd4, rotate);

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

        private void GenerateBlades()
        {
            if (RootGroup != "" && SelectedAirfoil != "")
            {
                double rootEdgeLength = 0;
                double midEdgeLength = 0;
                double tipEdgeLength = 0;
                double offsetY = 0;
                double oz = rootOffset;
                double midoffsetZ = midOffset;
                double tipoffsetZ = bladeLength;
                double tilt = bladeAngle * Math.PI * 2 / 360.0;
                List<Point> rootProfile = GetProfilePoints(RootGroup, SelectedAirfoil, bladeRoot, ref rootEdgeLength);
                List<Point> midProfile = GetProfilePoints(RootGroup, SelectedAirfoil, bladeMid, ref midEdgeLength);
                List<Point> tipProfile = GetProfilePoints(RootGroup, SelectedAirfoil, bladeTip, ref tipEdgeLength);

                Vertices.Clear();
                Faces.Clear();
                double dtheta = (Math.PI * 2) / NumberOfBlades;
                double theta = 0;

                for (int b = 0; b < numberOfBlades; b++)
                {
                    GenerateSingleBlade(rootEdgeLength, midEdgeLength, offsetY, oz, midoffsetZ, tipoffsetZ, tilt, rootProfile, midProfile, tipProfile, theta);

                    theta += dtheta;
                }
            }
        }

        private void GenerateDomedHub()
        {
            Point3DCollection cylPnts = new Point3DCollection();
            Int32Collection cylTris = new Int32Collection();
            Vector3DCollection normals = new Vector3DCollection();
            PrimitiveGenerator.GenerateCap(ref cylPnts, ref cylTris, ref normals);

            int faceOff = Vertices.Count;
            foreach (Point3D p in cylPnts)
            {
                Vertices.Add(new Point3D(p.X * hubRadius, p.Z * hubHeight + (hubHeight / 2) + hubOffset, p.Y * hubRadius));
            }
            foreach (int i in cylTris)
            {
                Faces.Add(i + faceOff);
            }
        }

        private void GenerateFlatHub()
        {
            Point3DCollection cylPnts = new Point3DCollection();
            Int32Collection cylTris = new Int32Collection();
            Vector3DCollection normals = new Vector3DCollection();
            PrimitiveGenerator.GenerateCylinder(ref cylPnts, ref cylTris, ref normals);

            int faceOff = Vertices.Count;
            foreach (Point3D p in cylPnts)
            {
                Vertices.Add(new Point3D(p.X * hubRadius, p.Y * hubHeight + (hubHeight / 2) + hubOffset, p.Z * hubRadius));
            }
            foreach (int i in cylTris)
            {
                Faces.Add(i + faceOff);
            }
        }

        private void GenerateHub()
        {
            // if (rootOffset > hubRadius)
            {
                GenerateSpokes();
            }
            if (flatHub)
            {
                GenerateFlatHub();
            }
            if (domedHub)
            {
                GenerateDomedHub();
            }
        }

        private void GenerateShape()
        {
            ClearShape();
            GenerateBlades();
            GenerateHub();
            FloorVertices();
        }

        private void GenerateSingleBlade(double rootEdgeLength, double midEdgeLength, double offsetY, double oz, double midoffsetZ, double tipoffsetZ, double tilt, List<Point> rootProfile, List<Point> midProfile, List<Point> tipProfile, double theta)
        {
            double dt = 0.01;
            double startT = 0;
            double endT = 1;
            double rtoff = bladeMid - bladeRoot;
            double tpoff = bladeMid - bladeTip;
            double xoff = -(bladeMid / 2);
            List<Vector3m> rootPoints = new List<Vector3m>();
            List<Point> tipPnts = new List<Point>();
            for (double t = startT; t < endT; t += dt)
            {
                Point p1 = GetProfileAt(rootProfile, rootEdgeLength, t);

                Point p2 = GetProfileAt(rootProfile, rootEdgeLength, t + dt);
                Point p3 = GetProfileAt(midProfile, midEdgeLength, t + dt);
                Point p4 = GetProfileAt(midProfile, midEdgeLength, t);

                Point3D pd1 = new Point3D((p1.X * bladeRoot) + rtoff + xoff, p1.Y * bladeRoot + offsetY, oz);
                Point3D pd2 = new Point3D((p2.X * bladeRoot) + rtoff + xoff, p2.Y * bladeRoot + offsetY, oz);
                Point3D pd3 = new Point3D((p3.X * bladeMid) + xoff, (p3.Y * bladeMid) + offsetY, midoffsetZ + oz);
                Point3D pd4 = new Point3D((p4.X * bladeMid) + xoff, (p4.Y * bladeMid) + offsetY, midoffsetZ + oz);

                pd1 = RotatePointAroundZ(pd1, tilt);
                pd1 = RotatePoint(pd1, theta);

                pd2 = RotatePointAroundZ(pd2, tilt);
                pd2 = RotatePoint(pd2, theta);

                pd3 = RotatePointAroundZ(pd3, tilt);
                pd3 = RotatePoint(pd3, theta);

                pd4 = RotatePointAroundZ(pd4, tilt);
                pd4 = RotatePoint(pd4, theta);

                rootPoints.Add(new Vector3m(pd1.X, pd1.Y, pd1.Z));

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

                pd1 = new Point3D(p1.X * bladeMid + xoff, p1.Y * bladeMid + offsetY, midoffsetZ + oz);
                pd2 = new Point3D(p2.X * bladeMid + xoff, p2.Y * bladeMid + offsetY, midoffsetZ + oz);
                pd3 = new Point3D((p3.X * bladeTip) + tpoff + xoff, (p3.Y * bladeTip) + offsetY, tipoffsetZ + oz);
                pd4 = new Point3D((p4.X * bladeTip) + tpoff + xoff, (p4.Y * bladeTip) + offsetY, tipoffsetZ + oz);

                pd1 = RotatePointAroundZ(pd1, tilt);
                pd1 = RotatePoint(pd1, theta);

                pd2 = RotatePointAroundZ(pd2, tilt);
                pd2 = RotatePoint(pd2, theta);

                pd3 = RotatePointAroundZ(pd3, tilt);
                pd3 = RotatePoint(pd3, theta);

                pd4 = RotatePointAroundZ(pd4, tilt);
                pd4 = RotatePoint(pd4, theta);

                tipPnts.Add(p4);

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
            EllipseTip(tipPnts, mr, sr, tpoff + xoff, offsetY, tipoffsetZ + oz, tilt, theta);

            EarClipping earClipping = new EarClipping();
            earClipping.SetPoints(rootPoints);
            earClipping.Triangulate();
            var res = earClipping.Result;
            for (int i = 0; i < res.Count; i += 3)
            {
                int v1 = AddVertice(res[i].X, res[i].Y, res[i].Z);
                int v2 = AddVertice(res[i + 1].X, res[i + 1].Y, res[i + 1].Z);
                int v3 = AddVertice(res[i + 2].X, res[i + 2].Y, res[i + 2].Z);
                Faces.Add(v1);
                Faces.Add(v2);
                Faces.Add(v3);
            }
        }

        private void GenerateSpokes()
        {
            Point3DCollection cylPnts = new Point3DCollection();
            Int32Collection cylTris = new Int32Collection();
            Vector3DCollection normals = new Vector3DCollection();
            PrimitiveGenerator.GenerateCylinder(ref cylPnts, ref cylTris, ref normals);
            double spokeDiameter = spokeRadius * 2;
            double dtheta = (Math.PI * 2) / NumberOfBlades;
            double theta = 0;
            for (int b = 0; b < numberOfBlades; b++)
            {
                int faceOff = Vertices.Count;
                foreach (Point3D p in cylPnts)
                {
                    Point3D np = new Point3D(p.X * spokeDiameter, -p.Z * spokeDiameter, (p.Y + 0.5) * rootOffset);
                    np = RotatePoint(np, theta);
                    Vertices.Add(np);
                }
                foreach (int i in cylTris)
                {
                    Faces.Add(i + faceOff);
                }
                theta += dtheta;
            }
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
            if (EditorParameters.Get("BladeLength") != "")
            {
                BladeLength = Convert.ToDouble(EditorParameters.Get("BladeLength"));
                BladeAngle = Convert.ToDouble(EditorParameters.Get("BladeAngle"));
                BladeRoot = Convert.ToDouble(EditorParameters.Get("BladeRoot"));
                BladeMid = Convert.ToDouble(EditorParameters.Get("BladeMid"));
                BladeTip = Convert.ToDouble(EditorParameters.Get("BladeTip"));
                DomedHub = Convert.ToBoolean(EditorParameters.Get("DomedHub"));
                FlatHub = Convert.ToBoolean(EditorParameters.Get("FlatHub"));
                HubHeight = Convert.ToDouble(EditorParameters.Get("HubHeight"));
                HubRadius = Convert.ToDouble(EditorParameters.Get("HubRadius"));
                HubOffset = Convert.ToDouble(EditorParameters.Get("HubOffset"));
                MidOffset = Convert.ToDouble(EditorParameters.Get("MidOffset"));
                NumberOfBlades = Convert.ToInt32(EditorParameters.Get("NumberOfBlades"));
                RootGroup = EditorParameters.Get("RootGroup");
                RootOffset = Convert.ToDouble(EditorParameters.Get("RootOffset"));
                SelectedAirfoil = EditorParameters.Get("SelectedAirfoil");
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

        private Point3D RotatePointAroundZ(Point3D p, double theta)
        {
            Point3D res;
            double s = Math.Sin(theta);
            double c = Math.Cos(theta);
            double x = p.X * c - p.Y * s;
            double y = p.X * s + p.Y * c;

            res = new Point3D(x, y, p.Z);

            return res;
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("BladeLength", bladeLength.ToString());
            EditorParameters.Set("BladeAngle", bladeAngle.ToString());
            EditorParameters.Set("BladeRoot", bladeRoot.ToString());
            EditorParameters.Set("BladeMid", bladeMid.ToString());
            EditorParameters.Set("BladeTip", bladeTip.ToString());
            EditorParameters.Set("DomedHub", domedHub.ToString());
            EditorParameters.Set("FlatHub", flatHub.ToString());
            EditorParameters.Set("HubHeight", hubHeight.ToString());
            EditorParameters.Set("HubRadius", hubRadius.ToString());
            EditorParameters.Set("HubOffset", hubOffset.ToString());
            EditorParameters.Set("MidOffset", midOffset.ToString());
            EditorParameters.Set("HubOffset", hubOffset.ToString());
            EditorParameters.Set("NumberOfBlades", numberOfBlades.ToString());
            EditorParameters.Set("RootGroup", rootGroup);
            EditorParameters.Set("RootOffset", rootOffset.ToString());
            EditorParameters.Set("SelectedAirfoil", selectedAirfoil);
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

        private void UpdateDisplay()
        {
            if (loaded)
            {
                GenerateShape();
                Redisplay();
            }
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