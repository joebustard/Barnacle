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
        private double tyreDepth;

        public Wheel()
        {
            InitializeComponent();
            ToolName = "Wheel";
            DataContext = this;
            tyreDepth = 50;
            hubInner = 10;
            hubOutter = 10;
            axelBore = 5;
            hubThickness = 10;
            rimThickness = 12;
            rimOutter = 5;
            hubStyles = new List<string>();
            rimStyles = new List<string>();
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

        private void CreateSideFace(List<Point> points, int i, double thickness, bool rev)
        {
            int v = i + 1;
            if (v == points.Count)
            {
                v = 0;
            }

            int c0 = AddVertice(points[i].X, points[i].Y, 0.0);
            int c1 = AddVertice(points[i].X, points[i].Y, thickness);
            int c2 = AddVertice(points[v].X, points[v].Y, thickness);
            int c3 = AddVertice(points[v].X, points[v].Y, 0.0);
            if (rev)
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
            for (int i = 0; i < hubExternalPoints.Count; i++)
            {
                CreateSideFace(hubExternalPoints, i, hubThickness, true);
            }
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
                for (int i = 0; i < hubInternalPoints.Count; i++)
                {
                    CreateSideFace(hubInternalPoints, i, hubThickness, false);
                }

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
                GenerateRimRing(rimInnerRadius, rimInnerRadius + ro, t);
                rimInnerRadius += ro;
            }
        }

        private void GenerateRimRing(double rimInnerRadius, double v, double t)
        {
        }

        private void GenerateShape()
        {
            ClearShape();
            GenerateHub();
            GenerateRim();
        }

        private void GetHubParams(ref double numSpoke, ref double spokeGapDTheta, ref double spokeDTheta, ref double spokeTipDTheta)
        {
            Double twop = Math.PI * 2.0;
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
            }
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
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
            selectedHubStyle = "1";

            rimStyles.Add("1");
            rimStyles.Add("2");
            rimStyles.Add("3");
            SelectedRimStyle = "1";

            LoadEditorParameters();
            GenerateShape();
            UpdateCameraPos();
            MyModelGroup.Children.Clear();

            Redisplay();
        }
    }
}