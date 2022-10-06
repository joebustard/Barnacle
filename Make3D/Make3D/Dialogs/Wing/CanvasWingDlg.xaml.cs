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
    /// Interaction logic for WingDlg.xaml
    /// </summary>
    public partial class CanvasWingDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private XmlDocument airFoilDoc;
        private List<String> airfoilGroups;
        private string airFoilPath;
        private bool bottomModelChecked;
        private double dihedralAngle;
        private double dihedralLimit = 20;
        private double dipPercent;
        private int numStruts;
        private List<String> rootairfoilNames;
        private string rootGroup;
        private double rootLength;
        private string selectedRootAirfoil;
        private string selectedShape;
        private string selectedTipAirfoil;
        private string selectedTipShape;
        private List<String> shapeNames;
        private double span;
        private double sweepAngle;
        private Visibility sweepControlsVisible;
        private double sweepLimit = 60;
        private List<String> tipairfoilNames;
        private Visibility tipControlsVisible;
        private string tipGroup;
        private double tipLength;
        private double tipOffsetX = 0;
        private double tipOffsetY = 0;
        private double tipOffsetZ = 0;
        private List<String> tipShapeNames;
        private bool topModelChecked;
        private bool wholeModelChecked;

        public CanvasWingDlg()
        {
            InitializeComponent();
            ToolName = "CanvasWing";
            DataContext = this;
            shapeNames = new List<string>();
            tipShapeNames = new List<string>();
            airFoilPath = AppDomain.CurrentDomain.BaseDirectory + "data\\Airfoils.xml";
            airFoilDoc = new XmlDocument();
            airFoilDoc.XmlResolver = null;
            rootairfoilNames = new List<string>();
            airfoilGroups = new List<string>();
            rootLength = 30;
            tipLength = 10;
            span = 70;
            sweepAngle = 0;
            dihedralAngle = 0;
            ModelGroup = MyModelGroup;
            numStruts = 8;
            dipPercent = 25;
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

        public bool BottomModelChecked
        {
            get
            {
                return bottomModelChecked;
            }
            set
            {
                if (bottomModelChecked != value)
                {
                    bottomModelChecked = value;
                    NotifyPropertyChanged();
                    Update();
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

                    if (dihedralAngle > sweepLimit)
                    {
                        dihedralAngle = sweepLimit;
                    }

                    NotifyPropertyChanged();
                    Update();
                }
            }
        }

        public double DipPercent
        {
            get
            {
                return dipPercent;
            }
            set
            {
                if (dipPercent != value)
                {
                    if (value >= 0 && value < 100)
                    {
                        dipPercent = value;
                        NotifyPropertyChanged();
                        Update();
                    }
                }
            }
        }

        public int NumStruts
        {
            get
            {
                return numStruts;
            }
            set
            {
                if (numStruts != value)
                {
                    numStruts = value;
                    NotifyPropertyChanged();
                    Update();
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

        public double RootLength
        {
            get
            {
                return rootLength;
            }
            set
            {
                if (rootLength != value)
                {
                    rootLength = value;
                    NotifyPropertyChanged();
                    Update();
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

        public String SelectedShape
        {
            get
            {
                return selectedShape;
            }
            set
            {
                if (selectedShape != value)
                {
                    selectedShape = value;
                    NotifyPropertyChanged();

                    Update();
                }
            }
        }

        public string SelectedTipAirfoil
        {
            get
            {
                return selectedTipAirfoil;
            }
            set
            {
                if (selectedTipAirfoil != value)
                {
                    selectedTipAirfoil = value;
                    NotifyPropertyChanged();
                    Update();
                }
            }
        }

        public String SelectedTipShape
        {
            get
            {
                return selectedTipShape;
            }
            set
            {
                if (selectedTipShape != value)
                {
                    selectedTipShape = value;
                    NotifyPropertyChanged();
                    Update();
                }
            }
        }

        public List<String> ShapeNames
        {
            get
            {
                return shapeNames;
            }
            set
            {
                if (shapeNames != value)
                {
                    shapeNames = value;
                    NotifyPropertyChanged();
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

        public double Span
        {
            get
            {
                return span;
            }
            set
            {
                if (span != value)
                {
                    span = value;
                    NotifyPropertyChanged();
                    Update();
                }
            }
        }

        public double SweepAngle
        {
            get
            {
                return sweepAngle;
            }
            set
            {
                if (sweepAngle != value)
                {
                    sweepAngle = value;
                    if (sweepAngle < -sweepLimit)
                    {
                        sweepAngle = -sweepLimit;
                    }

                    if (sweepAngle > sweepLimit)
                    {
                        sweepAngle = sweepLimit;
                    }

                    NotifyPropertyChanged();
                    Update();
                }
            }
        }

        public Visibility SweepControlsVisible
        {
            get
            {
                return sweepControlsVisible;
            }

            set
            {
                if (sweepControlsVisible != value)
                {
                    sweepControlsVisible = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public List<string> TipAirfoilNames
        {
            get
            {
                return tipairfoilNames;
            }
            set
            {
                if (tipairfoilNames != value)
                {
                    tipairfoilNames = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Visibility TipControlsVisible
        {
            get
            {
                return tipControlsVisible;
            }

            set
            {
                if (tipControlsVisible != value)
                {
                    tipControlsVisible = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String TipGroup
        {
            get
            {
                return tipGroup;
            }
            set
            {
                if (tipGroup != value)
                {
                    tipGroup = value;
                    List<string> names = new List<String>();
                    SetProfiles(tipGroup, names);

                    NotifyPropertyChanged();
                    TipAirfoilNames = names;
                    NotifyPropertyChanged();
                }
            }
        }

        public double TipLength
        {
            get
            {
                return tipLength;
            }
            set
            {
                if (tipLength != value)
                {
                    tipLength = value;
                    NotifyPropertyChanged();
                    Update();
                }
            }
        }

        public List<String> TipShapeNames
        {
            get
            {
                return tipShapeNames;
            }
            set
            {
                if (tipShapeNames != value)
                {
                    tipShapeNames = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool TopModelChecked
        {
            get
            {
                return topModelChecked;
            }
            set
            {
                if (topModelChecked != value)
                {
                    topModelChecked = value;
                    NotifyPropertyChanged();
                    Update();
                }
            }
        }

        public bool WholeModelChecked
        {
            get
            {
                return wholeModelChecked;
            }
            set
            {
                if (wholeModelChecked != value)
                {
                    wholeModelChecked = value;
                    NotifyPropertyChanged();
                    Update();
                }
            }
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParmeters();
            DialogResult = true;
            Close();
        }

        private double CloseMissingHalfOfWing(double innerEdgeLength, double startT, double endT)
        {
            double outerEdgeLength = 0;
            double tl = tipLength;
            if (SelectedShape == "Straight")
            {
                tl = rootLength;
            }
            List<Point> innerProfile = GetProfilePoints(RootGroup, SelectedRootAirfoil, rootLength, ref innerEdgeLength);
            List<Point> outerProfile = GetProfilePoints(RootGroup, SelectedRootAirfoil, tl, ref outerEdgeLength);
            Point p1 = GetProfileAt(innerProfile, innerEdgeLength, startT);

            Point p2 = GetProfileAt(innerProfile, innerEdgeLength, endT);
            Point p3 = GetProfileAt(outerProfile, outerEdgeLength, endT);
            Point p4 = GetProfileAt(outerProfile, outerEdgeLength, startT);

            Point3D pd1 = new Point3D(p1.X * rootLength, p1.Y * rootLength, 0);
            Point3D pd2 = new Point3D(p2.X * rootLength, p2.Y * rootLength, 0);
            Point3D pd3 = new Point3D((p3.X * tl) + tipOffsetX, (p3.Y * tl) + tipOffsetY, tipOffsetZ);
            Point3D pd4 = new Point3D((p4.X * tl) + tipOffsetX, (p4.Y * tl) + tipOffsetY, tipOffsetZ);

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
            return innerEdgeLength;
        }

        private void EllipseTip(List<Point> tipPnts, double mainRad, double sideRad, double tX, double tY, double tZ)
        {
            List<Point3D> tipEdge = new List<Point3D>();
            List<Point> triEdge = new List<Point>();
            double md = mainRad * 2;
            double stepSize = 1.0 / (tipPnts.Count - 1);
            if (!WholeModelChecked)
            {
                stepSize = 1.0 / (2 * tipPnts.Count - 1);
            }
            if (wholeModelChecked || topModelChecked)
            {
                for (double t = 0; t <= 0.5; t += stepSize)
                {
                    if (t > 0.5)
                    {
                        t = 0.5;
                    }
                    Point elp = GetEllipsePoint(mainRad, sideRad, t);
                    triEdge.Add(elp);
                    //  Point3D p = new Point3D(tX + elp.X + mainRad, tY, tZ + elp.Y);
                    Point3D p = new Point3D(elp.X, tY, elp.Y);
                    tipEdge.Add(p);
                }
            }
            if (wholeModelChecked || bottomModelChecked)
            {
                for (double t = 0.5; t >= 0; t -= stepSize)
                {
                    Point elp = GetEllipsePoint(mainRad, sideRad, t);
                    triEdge.Add(elp);
                    // Point3D p = new Point3D(tX + elp.X + mainRad, tY, tZ + elp.Y);
                    Point3D p = new Point3D(elp.X, tY, elp.Y);
                    tipEdge.Add(p);
                }
            }

            for (int i = 0; i < tipPnts.Count - 1 && i < tipEdge.Count - 1; i++)
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
            if (!WholeModelChecked)
            {
                TriangulateWingTip(triEdge, 1, tX + mainRad, tY, tipOffsetZ, bottomModelChecked);
            }
        }

        private void EnableControlsForShape()
        {
            switch (selectedShape)
            {
                case "Straight":
                    {
                        TipControlsVisible = Visibility.Hidden;
                        SweepControlsVisible = Visibility.Hidden;
                        tipOffsetX = 0;
                        double theta = (Math.PI * dihedralAngle) / 180;
                        tipOffsetY = span * Math.Sin(theta);
                        tipOffsetZ = span;
                    }
                    break;

                case "Straight Edge":
                    {
                        TipControlsVisible = Visibility.Visible;
                        SweepControlsVisible = Visibility.Hidden;
                        double theta = (Math.PI * dihedralAngle) / 180;
                        tipOffsetY = span * Math.Sin(theta);
                        tipOffsetX = rootLength - tipLength;
                        tipOffsetZ = span;
                    }
                    break;

                case "Tapered":
                    {
                        TipControlsVisible = Visibility.Visible;
                        SweepControlsVisible = Visibility.Hidden;
                        tipOffsetX = (rootLength - tipLength) / 2;
                        double theta = (Math.PI * dihedralAngle) / 180;
                        tipOffsetY = span * Math.Sin(theta);
                        tipOffsetZ = span;
                    }
                    break;

                case "Delta":
                    {
                        TipControlsVisible = Visibility.Visible;
                        SweepControlsVisible = Visibility.Hidden;
                        tipOffsetX = 0;
                        double theta = (Math.PI * dihedralAngle) / 180;
                        tipOffsetY = span * Math.Sin(theta);
                        tipOffsetZ = span;
                    }
                    break;

                case "Swept":
                    {
                        TipControlsVisible = Visibility.Visible;
                        SweepControlsVisible = Visibility.Visible;
                        double theta = (Math.PI * (-sweepAngle)) / 180;
                        tipOffsetX = span * Math.Sin(theta);
                        theta = (Math.PI * dihedralAngle) / 180;
                        tipOffsetY = span * Math.Sin(theta);
                        tipOffsetZ = span * Math.Cos(theta); ;
                    }
                    break;
            }
        }

        private void GenerateWing()
        {
            if (RootGroup != null && RootGroup != "" && SelectedRootAirfoil != "")
            {
                double innerStrutLength = 1;
                double outerStrutLength = 1;
                List<Point> rootPnts = new List<Point>();
                List<Point> tipPnts = new List<Point>();
                int numSubStrats = (4 * numStruts) - 1;
                double innerEdgeLength = 0;
                double strutGap = span / numSubStrats;
                double taperSize = rootLength - tipLength;
                if (SelectedShape == "Straight")
                {
                    taperSize = 0;
                }
                double strutTaperDiff = taperSize / numSubStrats;
                double strutDx = tipOffsetX / numSubStrats;
                double innerXo;
                double outerXo;
                double innerZ;
                double outerZ;
                double innerDip = 100;
                double outerDip = 100;

                int shrinkle = 0;

                double dt = 0.01;
                double startT = 0;
                double endT = 1;
                bool close = false;

                ClearShape();
                rootPnts.Clear();
                tipPnts.Clear();
                if (TopModelChecked)
                {
                    startT = 0;
                    endT = 0.5;
                    close = true;
                }
                if (BottomModelChecked)
                {
                    startT = 0.5;
                    endT = 1.0;
                    close = true;
                }
                double theta = 0;
                for (int strut = 0; strut < numSubStrats; strut++)
                {

                    switch (shrinkle)
                    {
                        case 0:
                            {
                                innerDip = 100.0;
                                outerDip = 100 - (2.0 * dipPercent / 3.0);
                            }
                            break;

                        case 1:
                            {
                                innerDip = 100 - (2.0 * dipPercent / 3.0); ;
                                outerDip = 100 - dipPercent;
                            }
                            break;

                        case 2:
                            {
                                innerDip = 100 - dipPercent;
                                outerDip = 100 - (2.0 * dipPercent / 3.0);
                            }
                            break;

                        case 3:
                            {
                                innerDip = 100 - (2.0 * dipPercent / 3.0);
                                outerDip = 100;
                            }
                            break;
                    }
                    shrinkle = (shrinkle + 1) % 4;
                    innerStrutLength = rootLength - (strut * strutTaperDiff);
                    outerStrutLength = innerStrutLength - strutTaperDiff;
                    innerXo = strut * strutDx;
                    outerXo = innerXo + strutDx;
                    innerZ = strut * strutGap;
                    outerZ = innerZ + strutGap;
                    theta = (Math.PI * dihedralAngle) / 180;
                    double diy = 0;
                    List<Point> innerProfile = GetProfilePoints(RootGroup, SelectedRootAirfoil, innerStrutLength, ref innerEdgeLength);

                    if (innerProfile.Count > 0)
                    {
                        if (TipGroup != null && TipGroup != "" && SelectedTipAirfoil != null && SelectedTipAirfoil != "")
                        {
                            double outerEdgeLength = 0;

                            List<Point> outerProfile = GetProfilePoints(RootGroup, SelectedRootAirfoil, outerStrutLength, ref outerEdgeLength);

                            if (outerProfile.Count > 0)
                            {
                                for (double t = startT; t < endT; t += dt)
                                {
                                    Point p1 = GetProfileAt(innerProfile, innerEdgeLength, t);
                                    if (strut == 0)
                                    {
                                        rootPnts.Add(p1);
                                    }
                                    Point p2 = GetProfileAt(innerProfile, innerEdgeLength, t + dt);
                                    Point p3 = GetProfileAt(outerProfile, outerEdgeLength, t + dt);
                                    Point p4 = GetProfileAt(outerProfile, outerEdgeLength, t);
                                    if (strut == numSubStrats - 1)
                                    {
                                        if (t == startT)
                                        {
                                            tipPnts.Add(p4);
                                        }
                                        else
                                        {
                                            tipPnts.Add(p3);
                                        }
                                    }

                                    Point3D pd1;
                                    Point3D pd2;
                                    Point3D pd3;
                                    Point3D pd4;

                                    // shrink the upper ys of the inner edge
                                    double y = p1.Y;
                                    if (y > 0)
                                    {
                                        y = (y * innerDip) / 100.0;
                                    }
                                    diy = innerZ * Math.Sin(theta);

                                    pd1 = new Point3D(p1.X * innerStrutLength + innerXo, y * innerStrutLength+diy , innerZ);

                                    y = p2.Y;
                                    if (y > 0)
                                    {
                                        y = (y * innerDip) / 100.0;
                                    }
                                    diy = innerZ * Math.Sin(theta);

                                    pd2 = new Point3D(p2.X * innerStrutLength + innerXo, y * innerStrutLength + diy, innerZ);

                                    y = p3.Y;
                                    if (y > 0)
                                    {
                                        y = (y * outerDip) / 100.0;
                                    }
                                    diy = outerZ * Math.Sin(theta);

                                    pd3 = new Point3D(p3.X * outerStrutLength + outerXo, y * outerStrutLength + diy, outerZ);

                                    y = p4.Y;
                                    if (y > 0)
                                    {
                                        y = (y * outerDip) / 100.0;
                                    }
                                    diy = outerZ * Math.Sin(theta);
                                    pd4 = new Point3D(p4.X * outerStrutLength + outerXo, y * outerStrutLength + diy, outerZ);

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
                                }
                            }
                        }
                    }
                }

                // if we are only doing the top or the bottom of the wing we need to seal it up.
                if (close)
                {
                    innerEdgeLength = CloseMissingHalfOfWing(innerEdgeLength, startT, endT);
                }

                // close the root
                TriangulatePerimiter(rootPnts, rootLength, 0, 0, 0, true);

                // Add the wingtip
                GenerateWingTip(outerStrutLength, tipPnts);
                CentreVertices();
            }
        }

        private void GenerateWingTip(double outerStrutLength, List<Point> tipPnts)
        {
            // add the tip
            if (selectedTipShape == "Cut Off")
            {
                TriangulatePerimiter(tipPnts, outerStrutLength, tipOffsetX, tipOffsetY, tipOffsetZ, false);
            }
            else
            if (selectedTipShape == "Ellipse 1")
            {
                double mr = outerStrutLength / 2;
                double sr = mr / 10;
                EllipseTip(tipPnts, mr, sr, tipOffsetX, tipOffsetY, tipOffsetZ);
            }
            else
            if (selectedTipShape == "Ellipse 2")
            {
                double mr = outerStrutLength / 2;
                double sr = mr / 5;
                EllipseTip(tipPnts, mr, sr, tipOffsetX, tipOffsetY, tipOffsetZ);
            }
            if (selectedTipShape == "Ellipse 3")
            {
                double mr = outerStrutLength / 2;
                double sr = mr / 2;
                EllipseTip(tipPnts, mr, sr, tipOffsetX, tipOffsetY, tipOffsetZ);
            }
            if (selectedTipShape == "Ellipse 4")
            {
                double mr = outerStrutLength / 2;
                double sr = mr;
                EllipseTip(tipPnts, mr, sr, tipOffsetX, tipOffsetY, tipOffsetZ);
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
            string s = EditorParameters.Get("Shape");
            if (s != "")
            {
                SelectedShape = s;
                s = EditorParameters.Get("Whole");
                WholeModelChecked = Convert.ToBoolean(s);

                s = EditorParameters.Get("Top");
                TopModelChecked = Convert.ToBoolean(s);

                s = EditorParameters.Get("Bottom");
                BottomModelChecked = Convert.ToBoolean(s);

                s = EditorParameters.Get("Span");
                Span = Convert.ToDouble(s);

                s = EditorParameters.Get("RootLength");
                RootLength = Convert.ToDouble(s);

                s = EditorParameters.Get("TipLength");
                TipLength = Convert.ToDouble(s);

                s = EditorParameters.Get("Sweep");
                SweepAngle = Convert.ToDouble(s);

                s = EditorParameters.Get("Dihedral");
                dihedralAngle = Convert.ToDouble(s);

                s = EditorParameters.Get("Dip");
                DipPercent = Convert.ToDouble(s);

                s = EditorParameters.Get("Struts");
                NumStruts = Convert.ToInt16(s);

                RootGroup = EditorParameters.Get("RootGroup");
                SelectedRootAirfoil = EditorParameters.Get("RootAirfoil");

                TipGroup = EditorParameters.Get("TipGroup");
                SelectedTipAirfoil = EditorParameters.Get("TipAirfoil");
                SelectedTipShape = EditorParameters.Get("TipShape");
            }
        }

        private void SaveEditorParmeters()
        {
            EditorParameters.Set("Shape", selectedShape);
            EditorParameters.Set("Whole", wholeModelChecked.ToString());
            EditorParameters.Set("Top", topModelChecked.ToString());
            EditorParameters.Set("Bottom", bottomModelChecked.ToString());
            EditorParameters.Set("Span", span.ToString());
            EditorParameters.Set("RootLength", rootLength.ToString());
            EditorParameters.Set("TipLength", tipLength.ToString());
            EditorParameters.Set("Sweep", sweepAngle.ToString());
            EditorParameters.Set("Dihedral", dihedralAngle.ToString());
            EditorParameters.Set("Dip", dipPercent.ToString());
            EditorParameters.Set("Struts", numStruts.ToString());
            EditorParameters.Set("RootGroup", RootGroup);
            EditorParameters.Set("RootAirfoil", SelectedRootAirfoil);
            EditorParameters.Set("TipGroup", TipGroup);
            EditorParameters.Set("TipAirfoil", SelectedTipAirfoil);
            EditorParameters.Set("TipShape", SelectedTipShape);
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

        private void SetShapeNames()
        {
            shapeNames.Add("Straight");
            shapeNames.Add("Straight Edge");
            shapeNames.Add("Tapered");
            shapeNames.Add("Swept");
            NotifyPropertyChanged("ShapeNames");

            tipShapeNames.Add("Cut Off");
            tipShapeNames.Add("Ellipse 1");
            tipShapeNames.Add("Ellipse 2");
            tipShapeNames.Add("Ellipse 3");
            tipShapeNames.Add("Ellipse 4");

            NotifyPropertyChanged("TipShapeNames");
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

        private void TriangulateWingTip(List<Point> points, double l, double xo, double yo, double z, bool invert)
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
                int c0 = AddVertice(xo + t.Points[0].X * l, yo, z + t.Points[0].Y * l);
                int c1 = AddVertice(xo + t.Points[1].X * l, yo, z + t.Points[1].Y * l);
                int c2 = AddVertice(xo + t.Points[2].X * l, yo, z + t.Points[2].Y * l);
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

        private void Update()
        {
            EnableControlsForShape();
            GenerateWing();
            Redisplay();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAirFoils();

            SetShapeNames();
            SelectedShape = shapeNames[0];
            SelectedTipShape = tipShapeNames[0];
            RootGroup = airfoilGroups[0];
            TipGroup = airfoilGroups[0];
            SelectedRootAirfoil = rootairfoilNames[0];
            SelectedTipAirfoil = tipairfoilNames[0];
            WholeModelChecked = true;
            LoadEditorParameters();
            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            GenerateWing();
            Redisplay();
        }
    }
}