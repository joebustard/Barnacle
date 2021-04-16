using Make3D.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Make3D.Dialogs
{
    /// <summary>
    /// Interaction logic for Blank.xaml
    /// </summary>
    public partial class Wheel : BaseModellerDialog, INotifyPropertyChanged
    {
        private double actualRimOutter;
        private double axelBore;
        private double hubInner;
        private double hubOutter;
        private List<String> hubStyles;
        private double hubThickness;
        private double rimOutter;
        private List<String> rimStyles;

        private double rimThickness;
        private string selectedHubStyle;
        private string selectedRimStyle;

        private string selectedTyreStyle;

        private Double twop = Math.PI * 2.0;

        private double tyreDepth;
        private List<String> tyreStyles;

        public Wheel()
        {
            InitializeComponent();
            ToolName = "Wheel";
            DataContext = this;
            tyreDepth = 10;
            hubInner = 10;
            hubOutter = 10;
            axelBore = 5;
            hubThickness = 10;
            rimThickness = 12;
            rimOutter = 5;
            hubStyles = new List<string>();
            rimStyles = new List<string>();
            tyreStyles = new List<string>();
        }

        public double AxelBore
        {
            get
            {
                return axelBore;
            }
            set
            {
                if (axelBore != value)
                {
                    axelBore = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double HubInner
        {
            get
            {
                return hubInner;
            }
            set
            {
                if (hubInner != value)
                {
                    hubInner = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double HubOutter
        {
            get
            {
                return hubOutter;
            }
            set
            {
                if (hubOutter != value)
                {
                    hubOutter = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public List<String> HubStyles
        {
            get
            {
                return hubStyles;
            }
            set
            {
                if (hubStyles != value)
                {
                    hubStyles = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double HubThickness
        {
            get
            {
                return hubThickness;
            }
            set
            {
                if (value != hubThickness)
                {
                    hubThickness = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double RimOutter
        {
            get
            {
                return rimOutter;
            }
            set
            {
                if (rimOutter != value)
                {
                    rimOutter = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public List<String> RimStyles
        {
            get
            {
                return rimStyles;
            }
            set
            {
                rimStyles = value;
                NotifyPropertyChanged();
                UpdateDisplay();
            }
        }

        public double RimThickness
        {
            get
            {
                return rimThickness;
            }
            set
            {
                if (rimThickness != value)
                {
                    rimThickness = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public string SelectedHubStyle
        {
            get
            {
                return selectedHubStyle;
            }
            set
            {
                if (selectedHubStyle != value)
                {
                    selectedHubStyle = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public string SelectedRimStyle
        {
            get
            {
                return selectedRimStyle;
            }
            set
            {
                if (selectedRimStyle != value)
                {
                    selectedRimStyle = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public string SelectedTyreStyle
        {
            get
            {
                return selectedTyreStyle;
            }
            set
            {
                if (selectedTyreStyle != value)
                {
                    selectedTyreStyle = value;
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

        public double TyreDepth
        {
            get
            {
                return tyreDepth;
            }
            set
            {
                if (tyreDepth != value)
                {
                    tyreDepth = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public List<String> TyreStyles
        {
            get
            {
                return tyreStyles;
            }
            set
            {
                if (tyreStyles != value)
                {
                    tyreStyles = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParmeters();
            DialogResult = true;
            Close();
        }

        private void CreateHubSurface(List<Point> inp, List<Point> oup, double z, bool invert)
        {
            int i = 0;
            int j;
            while (i < inp.Count)
            {
                j = i + 1;
                if (j == inp.Count)
                {
                    j = 0;
                }
                int c0 = AddVertice(inp[i].X, inp[i].Y, z);
                int c1 = AddVertice(oup[i].X, oup[i].Y, z);
                int c2 = AddVertice(oup[j].X, oup[j].Y, z);
                int c3 = AddVertice(inp[j].X, inp[j].Y, z);
                if (invert)
                {
                    Faces.Add(c0);
                    Faces.Add(c2);
                    Faces.Add(c1);

                    Faces.Add(c0);
                    Faces.Add(c3);
                    Faces.Add(c2);
                }
                else
                {
                    Faces.Add(c0);
                    Faces.Add(c1);
                    Faces.Add(c2);

                    Faces.Add(c0);
                    Faces.Add(c2);
                    Faces.Add(c3);
                }
                i++;
            }
        }

        private void GenerateHub()
        {
            List<Point> hubExternalPoints = new List<Point>();
            double numSpoke = 0;
            double gapDTheta = 0;
            double spokeDTheta = 0;
            double spokeTipDTheta = 0;
            double numSubs = 10;
            double twopi = Math.PI * 2;
            GetHubParams(ref numSpoke, ref gapDTheta, ref spokeDTheta, ref spokeTipDTheta);
            double actualInner = hubInner;
            if (axelBore > actualInner)
            {
                actualInner = axelBore + 1;
            }
            double actualOutter = hubOutter + actualInner;
            if (actualOutter < actualInner)
            {
                actualOutter = actualInner;
            }

            double theta = 0;
            bool inSpoke = true;
            while (theta <= twopi)
            {
                if (inSpoke)
                {
                    // create spoke points
                    theta += spokeTipDTheta;
                    // creat gap points
                    // Add a gap
                    double dt = spokeDTheta / numSubs;
                    for (int i = 0; i < numSubs; i++)
                    {
                        theta += dt;
                        if (theta < twopi)
                        {
                            double x = actualOutter * Math.Cos(theta);
                            double y = actualOutter * Math.Sin(theta);
                            hubExternalPoints.Add(new System.Windows.Point(x, y));
                        }
                    }
                }
                else
                {
                    theta += spokeTipDTheta;
                    // creat gap points
                    // Add a gap
                    double dt = gapDTheta / numSubs;
                    for (int i = 0; i < numSubs; i++)
                    {
                        theta += dt;
                        if (theta < twopi)
                        {
                            double x = actualInner * Math.Cos(theta);
                            double y = actualInner * Math.Sin(theta);
                            hubExternalPoints.Add(new System.Windows.Point(x, y));
                        }
                    }
                }
                inSpoke = !inSpoke;
            }
            // generate side triangles so original points are already in list

            CreateSideFaces(hubExternalPoints, hubThickness, true);

            if (hubExternalPoints.Count > 0)
            {
                List<System.Windows.Point> hubInternalPoints = new List<Point>();
                // create set of points for the inside of the bore hole.
                // to make triangulation easier, create exactly the same number of points
                double dt = (Math.PI * 2) / (hubExternalPoints.Count - 1);
                theta = 0;
                while (theta < twopi)
                {
                    double x = axelBore * Math.Cos(theta);
                    double y = axelBore * Math.Sin(theta);
                    hubInternalPoints.Add(new System.Windows.Point(x, y));
                    theta += dt;
                }

                CreateSideFaces(hubInternalPoints, hubThickness, false);

                CreateHubSurface(hubInternalPoints, hubExternalPoints, 0.0, true);
                CreateHubSurface(hubInternalPoints, hubExternalPoints, hubThickness, false);
            }
        }

        private void GenerateRim()
        {
            // the actualoutter of the hub is the inner of the rim
            double actualInner = hubInner;
            if (axelBore > actualInner)
            {
                actualInner = axelBore + 1;
            }
            double actualOutter = hubOutter + actualInner;
            if (actualOutter < actualInner)
            {
                actualOutter = actualInner;
            }
            double rimInnerRadius = actualOutter;
            List<double> ringRadius = new List<double>();
            List<double> ringThickness = new List<double>();
            GetRimParams(ringRadius, ringThickness);

            for (int i = 0; i < ringRadius.Count; i++)

            {
                double ro = ringRadius[i];
                double t = ringThickness[i];
                GenerateRing(0, 0, 0, rimInnerRadius, rimInnerRadius + ro, t);
                rimInnerRadius += ro;
            }
            actualRimOutter = rimInnerRadius;
        }

        private void GenerateRing(double cx, double cy, double cz, double ir, double or, double thicky)
        {
            List<Point> inners = new List<Point>();
            List<Point> outters = new List<Point>();
            double twoPi = Math.PI * 2;
            double theta = 0;
            double dt = twoPi / 36;
            double x;
            double y;
            while (theta < twoPi)
            {
                x = ir * Math.Cos(theta);
                y = ir * Math.Sin(theta);
                inners.Add(new Point(x, y));

                x = or * Math.Cos(theta);
                y = or * Math.Sin(theta);
                outters.Add(new Point(x, y));

                theta += dt;
            }
            CreateSideFaces(outters, thicky, true);
            CreateSideFaces(inners, thicky, false);
            CreateHubSurface(inners, outters, 0.0, true);
            CreateHubSurface(inners, outters, thicky, false);
        }

        private void GenerateShape()
        {
            ClearShape();
            GenerateHub();
            GenerateRim();
            GenerateTyre();
        }

        private void GenerateTyre()
        {
            switch (selectedTyreStyle)
            {
                case "1":
                    {
                        double tyreInner = actualRimOutter;
                        double tyreOutter = tyreInner + tyreDepth;
                        GenerateRing(0, 0, 0, tyreInner, tyreOutter, rimThickness + 1);
                    }
                    break;

                case "2":
                    {
                        double tyreInner = actualRimOutter;
                        double tyreOutter = tyreInner + tyreDepth;
                        GenerateTyreProfile1(tyreInner, tyreOutter, rimThickness + 1);
                    }
                    break;

                case "3":
                    {
                        double tyreInner = actualRimOutter;
                        double tyreOutter = tyreInner + tyreDepth;
                        GenerateTyreProfile2(tyreInner, tyreOutter, rimThickness + 1);
                    }
                    break;
            }
        }

        private void GenerateTyreProfile1(double inner, double outter, double t)
        {
            List<PolarCoordinate> polarProfile = new List<PolarCoordinate>();

            double cx = inner;

            int rotDivisions = 36;
            polarProfile.Add(Polar(cx, 0, t));
            polarProfile.Add(Polar(cx + (0.8 * (outter - inner)), 0, t));
            polarProfile.Add(Polar(outter, 0, 0.9 * t));
            polarProfile.Add(Polar(outter, 0, 0.1 * t));
            polarProfile.Add(Polar(cx + (0.8 * (outter - inner)), 0, 0));
            polarProfile.Add(Polar(cx, 0, 0));

            SweepPolarProfileTheta(polarProfile, cx, 0, 360, rotDivisions, false, true, true);
        }

        private void GenerateTyreProfile2(double inner, double outter, double t)
        {
            List<PolarCoordinate> polarProfile1 = new List<PolarCoordinate>();
            List<PolarCoordinate> polarProfile2 = new List<PolarCoordinate>();
            double cx = inner;

            int rotDivisions = 36;
            polarProfile1.Add(Polar(cx, 0, t));
            polarProfile1.Add(Polar(cx + (0.8 * (outter - inner)), 0, t));
            polarProfile1.Add(Polar(outter, 0, 0.9 * t));
            polarProfile1.Add(Polar(outter, 0, 0.1 * t));
            polarProfile1.Add(Polar(cx + (0.8 * (outter - inner)), 0, 0));
            polarProfile1.Add(Polar(cx, 0, 0));

            polarProfile2.Add(Polar(cx, 0, t));
            polarProfile2.Add(Polar(outter, 0, t));
            polarProfile2.Add(Polar(outter, 0, 0));
            polarProfile2.Add(Polar(cx, 0, 0));

            PartSweep(polarProfile1, polarProfile2, cx, 0, 5, rotDivisions, true, true);
        }

        private void GetHubParams(ref double numSpoke, ref double spokeGapDTheta, ref double spokeDTheta, ref double spokeTipDTheta)
        {
            switch (selectedHubStyle)
            {
                case "1":
                    {
                        numSpoke = 8;
                        spokeGapDTheta = twop / (numSpoke * 2.0);
                        spokeDTheta = spokeGapDTheta;
                        spokeTipDTheta = 0;
                    }
                    break;

                case "2":
                    {
                        numSpoke = 8;
                        spokeGapDTheta = twop / ((numSpoke + 1) * 2.0);
                        spokeDTheta = spokeGapDTheta / 2;
                        spokeTipDTheta = spokeDTheta;
                    }
                    break;

                case "3":
                    {
                        numSpoke = 12;
                        spokeGapDTheta = twop / ((numSpoke + 1) * 2.0);
                        spokeDTheta = 0.9 * spokeGapDTheta;
                        spokeTipDTheta = (spokeGapDTheta - spokeDTheta) / 2.0;
                    }
                    break;

                case "4":
                    {
                        numSpoke = 4;
                        spokeGapDTheta = twop / ((numSpoke + 1) * 2.0);
                        spokeDTheta = 0.75 * spokeGapDTheta;
                        spokeTipDTheta = (spokeGapDTheta - spokeDTheta) / 2.0;
                    }
                    break;
            }
        }

        private void GetRimParams(List<double> ringRadius, List<double> ringThickness)
        {
            switch (SelectedRimStyle)
            {
                case "1":
                    {
                        ringRadius.Add(rimOutter);
                        ringThickness.Add(rimThickness);
                    }
                    break;

                case "2":
                    {
                        ringRadius.Add(rimOutter / 2);
                        ringThickness.Add(rimThickness * .75);
                        ringRadius.Add(rimOutter / 2);
                        ringThickness.Add(rimThickness * 1.25);
                    }
                    break;

                case "3":
                    {
                        ringRadius.Add(rimOutter / 3);
                        ringThickness.Add(rimThickness * .9);
                        ringRadius.Add(rimOutter / 3);
                        ringThickness.Add(rimThickness * 1.1);
                        ringRadius.Add(rimOutter / 3);
                        ringThickness.Add(rimThickness * .9);
                    }
                    break;

                case "4":
                    {
                        ringRadius.Add(rimOutter / 2);
                        ringThickness.Add(rimThickness * 0.5);
                        ringRadius.Add(rimOutter / 2);
                        ringThickness.Add(rimThickness);
                    }
                    break;
            }
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            if (EditorParameters.Get("AxelBore") != "")
            {
                AxelBore = Convert.ToDouble(EditorParameters.Get("AxelBore"));
                HubInner = Convert.ToDouble(EditorParameters.Get("HubInner"));
                HubOutter = Convert.ToDouble(EditorParameters.Get("HubOutter"));
                RimOutter = Convert.ToDouble(EditorParameters.Get("RimOutter"));
                HubThickness = Convert.ToDouble(EditorParameters.Get("HubThickness"));
                RimThickness = Convert.ToDouble(EditorParameters.Get("RimThickness"));
                SelectedHubStyle = EditorParameters.Get("SelectedHubStyle");
                SelectedRimStyle = EditorParameters.Get("SelectedRimStyle");
                SelectedTyreStyle = EditorParameters.Get("SelectedTyreStyle");
                TyreDepth = Convert.ToDouble(EditorParameters.Get("TyreDepth"));
            }
        }

        private void PartSweep(List<PolarCoordinate> polarProfile1, List<PolarCoordinate> polarProfile2, double cx, int cy, double sweepAngle, int rotDivisions, bool flipAxies, bool invert)
        {
            List<PolarCoordinate> profile = polarProfile1;
            double numSegs = 360 / sweepAngle;
            numSegs *= 2;
            bool inv1 = false;

            int segs = 0;
            bool firstOne = true;
            double sweep = (Math.PI * 2.0) / (numSegs - 1);

            for (int i = 0; i < numSegs; i++)
            {
                inv1 = (i % 2 == 1);
                if (firstOne)
                {
                    profile = polarProfile1;
                }
                else
                {
                    profile = polarProfile2;
                }
                double a = sweep * i;
                int j = i + 1;
                if (j == numSegs)
                {
                    j = 0;
                }
                double b = sweep * j;
                // do the side walls and main surfaces
                for (int index = 0; index < profile.Count; index++)
                {
                    int index2 = index + 1;
                    if (index2 == profile.Count)
                    {
                        index2 = 0;
                    }
                    PolarCoordinate pc1 = profile[index].Clone();
                    PolarCoordinate pc2 = profile[index2].Clone();
                    PolarCoordinate pc3 = profile[index2].Clone();
                    PolarCoordinate pc4 = profile[index].Clone();
                    pc1.Theta -= a;
                    pc2.Theta -= a;
                    pc3.Theta -= b;
                    pc4.Theta -= b;

                    Point3D p1 = pc1.GetPoint3D();
                    Point3D p2 = pc2.GetPoint3D();
                    Point3D p3 = pc3.GetPoint3D();
                    Point3D p4 = pc4.GetPoint3D();
                    if (flipAxies)
                    {
                        FlipAxies(ref p1);
                        FlipAxies(ref p2);
                        FlipAxies(ref p3);
                        FlipAxies(ref p4);
                    }
                    int v1 = AddVertice(p1);
                    int v2 = AddVertice(p2);
                    int v3 = AddVertice(p3);
                    int v4 = AddVertice(p4);

                    if (invert)
                    {
                        Faces.Add(v1);
                        Faces.Add(v3);
                        Faces.Add(v2);

                        Faces.Add(v1);
                        Faces.Add(v4);
                        Faces.Add(v3);
                    }
                    else
                    {
                        Faces.Add(v1);
                        Faces.Add(v2);
                        Faces.Add(v3);

                        Faces.Add(v1);
                        Faces.Add(v3);
                        Faces.Add(v4);
                    }
                }
                // do spoke surfaces to close the sticky out bits of tread
                Point3D centreOfProfile = new Point3D(cx, 0, cy);
                for (int index = 0; index < profile.Count; index++)
                {
                    int index2 = index + 1;
                    if (index2 == profile.Count)
                    {
                        index2 = 0;
                    }
                    PolarCoordinate pc1 = profile[index].Clone();
                    PolarCoordinate pc2 = profile[index2].Clone();
                    PolarCoordinate pc3 = new PolarCoordinate(0, 0, 0);
                    pc3.SetPoint3D(centreOfProfile);
                    pc1.Theta -= a;
                    pc2.Theta -= a;
                    pc3.Theta -= a;
                    Point3D p1 = pc1.GetPoint3D();
                    Point3D p2 = pc2.GetPoint3D();
                    Point3D p3 = pc3.GetPoint3D();
                    FlipAxies(ref p1);
                    FlipAxies(ref p2);
                    FlipAxies(ref p3);
                    int v1 = AddVertice(p1);
                    int v2 = AddVertice(p2);
                    int v3 = AddVertice(p3);

                    if (inv1)
                    {
                        Faces.Add(v1);
                        Faces.Add(v3);
                        Faces.Add(v2);
                    }
                    else
                    {
                        Faces.Add(v1);
                        Faces.Add(v2);
                        Faces.Add(v3);
                    }
                }

                for (int index = 0; index < profile.Count; index++)
                {
                    int index2 = index + 1;
                    if (index2 == profile.Count)
                    {
                        index2 = 0;
                    }
                    PolarCoordinate pc1 = profile[index].Clone();
                    PolarCoordinate pc2 = profile[index2].Clone();
                    PolarCoordinate pc3 = new PolarCoordinate(0, 0, 0);
                    pc3.SetPoint3D(centreOfProfile);
                    pc1.Theta -= b;
                    pc2.Theta -= b;
                    pc3.Theta -= b;

                    Point3D p1 = pc1.GetPoint3D();
                    Point3D p2 = pc2.GetPoint3D();
                    Point3D p3 = pc3.GetPoint3D();
                    FlipAxies(ref p1);
                    FlipAxies(ref p2);
                    FlipAxies(ref p3);
                    int v1 = AddVertice(p1);
                    int v2 = AddVertice(p2);
                    int v3 = AddVertice(p3);

                    if (inv1)
                    {
                        Faces.Add(v1);
                        Faces.Add(v3);
                        Faces.Add(v2);
                    }
                    else
                    {
                        Faces.Add(v1);
                        Faces.Add(v2);
                        Faces.Add(v3);
                    }
                }

                segs++;
                if (segs == 4)
                {
                    segs = 0;
                    firstOne = !firstOne;
                }
            }
        }

        private PolarCoordinate Polar(double x, double y, double z)
        {
            Point3D p3d = new Point3D(x, y, z);
            PolarCoordinate pcol = new PolarCoordinate(0, 0, 0);
            pcol.SetPoint3D(p3d);
            return pcol;
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
            EditorParameters.Set("AxelBore", axelBore.ToString());
            EditorParameters.Set("HubInner", hubInner.ToString());
            EditorParameters.Set("HubOutter", hubOutter.ToString());
            EditorParameters.Set("RimOutter", rimOutter.ToString());
            EditorParameters.Set("HubThickness", hubThickness.ToString());
            EditorParameters.Set("RimThickness", rimThickness.ToString());
            EditorParameters.Set("SelectedHubStyle", selectedHubStyle);
            EditorParameters.Set("SelectedRimStyle", selectedRimStyle);
            EditorParameters.Set("SelectedTyreStyle", selectedTyreStyle);
            EditorParameters.Set("TyreDepth", tyreDepth.ToString());
        }

        private void UpdateDisplay()
        {
            GenerateShape();
            CentreVertices();
            Redisplay();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            hubStyles.Add("1");
            hubStyles.Add("2");
            hubStyles.Add("3");
            hubStyles.Add("4");
            selectedHubStyle = "1";

            rimStyles.Add("1");
            rimStyles.Add("2");
            rimStyles.Add("3");
            rimStyles.Add("4");
            SelectedRimStyle = "1";

            tyreStyles.Add("1");
            tyreStyles.Add("2");
            tyreStyles.Add("3");
            SelectedTyreStyle = "1";

            LoadEditorParameters();
            GenerateShape();
            UpdateCameraPos();
            MyModelGroup.Children.Clear();

            Redisplay();
        }
    }
}