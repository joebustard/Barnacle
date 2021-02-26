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
    public partial class TurboFan : BaseModellerDialog, INotifyPropertyChanged
    {
        private bool anticlockwise;
        private int bladeLength;
        private double bladeOverlap;
        private double bladePitch;
        private double bladeThickness;
        private bool clockwise;

        private double diskOffset;
        private double diskThickness;
        private double hubRadius;
        private int numberOfBlades;

        private bool supportDisk;

        public TurboFan()
        {
            InitializeComponent();
            ToolName = "TurboFan";
            DataContext = this;
            numberOfBlades = 8;
            bladeLength = 20;
            bladePitch = 45;
            bladeThickness = 1;
            hubRadius = 10;
            diskThickness = 10;
            diskOffset = 0.5;
            clockwise = true;
            anticlockwise = false;
        }

        public bool Anticlockwise
        {
            get
            {
                return anticlockwise;
            }
            set
            {
                if (anticlockwise != value)
                {
                    anticlockwise = value;
                    clockwise = !anticlockwise;
                    NotifyPropertyChanged();
                    Regenerate();
                }
            }
        }

        public int BladeLength
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
                    Regenerate();
                }
            }
        }

        public double BladeOverlap
        {
            get
            {
                return bladeOverlap;
            }
            set
            {
                if (bladeOverlap != value)
                {
                    bladeOverlap = value;
                    NotifyPropertyChanged();
                    Regenerate();
                }
            }
        }

        public double BladePitch
        {
            get
            {
                return bladePitch;
            }
            set
            {
                if (bladePitch != value)
                {
                    bladePitch = value;
                    NotifyPropertyChanged();
                    Regenerate();
                }
            }
        }

        public double BladeThickness
        {
            get
            {
                return bladeThickness;
            }
            set
            {
                if (bladeThickness != value)
                {
                    bladeThickness = value;
                    NotifyPropertyChanged();
                    Regenerate();
                }
            }
        }

        public bool Clockwise
        {
            get
            {
                return clockwise;
            }
            set
            {
                if (clockwise != value)
                {
                    clockwise = value;
                    anticlockwise = !clockwise;
                    NotifyPropertyChanged();
                    Regenerate();
                }
            }
        }

        public double DiskOffset
        {
            get
            {
                return diskOffset;
            }
            set
            {
                if (diskOffset != value)
                {
                    diskOffset = value;
                    NotifyPropertyChanged();
                    Regenerate();
                }
            }
        }

        public double DiskThickness
        {
            get
            {
                return diskThickness;
            }
            set
            {
                if (diskThickness != value)
                {
                    diskThickness = value;
                    NotifyPropertyChanged();
                    Regenerate();
                }
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
                    Regenerate();
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
                    numberOfBlades = value;
                    NotifyPropertyChanged();
                    Regenerate();
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
                    Regenerate();
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

        public bool SupportDisk
        {
            get
            {
                return supportDisk;
            }
            set
            {
                if (supportDisk != value)
                {
                    supportDisk = value;
                    NotifyPropertyChanged();
                    Regenerate();
                }
            }
        }

        public Point3D[] MakePolygonPoints(int numSides, double radius, double z)
        {
            //double radius = 0.5;
            // Generate the points.
            Point3D[] points = new Point3D[numSides];
            double dtheta = 2 * Math.PI / numSides;
            double theta = 0;
            for (int i = 0; i < numSides; i++)
            {
                points[i] = new Point3D(radius * Math.Cos(theta), radius * Math.Sin(theta), z);
                theta += dtheta;
            }
            return points;
        }

        internal void GenerateCylinder(double radius, double thickness, double offset)
        {
            int numSides = 180;
            Point3D[] bottom = MakePolygonPoints(numSides, radius, (-thickness / 2) + offset);
            // Top is the bottom reversed and moved up to 1
            Point3D[] top = new Point3D[numSides];
            int ind = numSides - 1;
            foreach (Point3D p in bottom)
            {
                top[ind] = new Point3D(p.X, p.Y, (thickness / 2) + offset);
                ind--;
            }
            Point3D bottomCentre = new Point3D(0, 0, (-thickness / 2) + offset);
            Point3D topCentre = new Point3D(0, 0, (thickness / 2) + offset);
            for (int i = 0; i < numSides; i++)
            {
                int j = i + 1;
                if (j == numSides)
                {
                    j = 0;
                }

                int k = numSides - i - 1;
                int l = k - 1;
                if (l < 0)
                {
                    l = numSides - 1;
                }
                // bottom cap
                AddTriangle(bottomCentre, bottom[i], bottom[j]);
                // top cap
                AddTriangle(topCentre, top[i], top[j]);

                // vertical 1
                AddTriangle(bottom[i], top[k], top[l]);

                // vertical 2
                AddTriangle(bottom[i], top[l], bottom[j]);
            }
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParmeters();
            DialogResult = true;
            Close();
        }

        private void AddTriangle(Point3D p0, Point3D p1, Point3D p2)
        {
            int i0 = AddVertice(p0);
            int i1 = AddVertice(p1);
            int i2 = AddVertice(p2);
            Faces.Add(i0);
            Faces.Add(i2);
            Faces.Add(i1);
        }

        private void GenerateBlades()
        {
            int numSteps = 30;

            // angle between the start of each blade.
            double bladeStartDTheta = (Math.PI * 2) / numberOfBlades;

            // angle covered by the sweep of an individual blade
            double bladeSweepDTheta = bladeStartDTheta + (bladeOverlap * bladeStartDTheta / 100.0);
            double dtheta = bladeSweepDTheta / numSteps;
            // blade pitch in radians
            double pitch = (90 - bladePitch) * Math.PI / 180.0;

            // inside of blade is outside of hub
            double innerBladeRadius = hubRadius;
            double outterBladeRadius = innerBladeRadius + bladeLength;
            double x, y;
            List<double> innerz = new List<double>();
            List<double> outterz = new List<double>();
            for (int i = 0; i < numberOfBlades; i++)
            {
                double st = i * bladeStartDTheta;
                List<Point> inner = new List<Point>();
                for (int j = 0; j < numSteps; j++)
                {
                    x = innerBladeRadius * Math.Cos(st + (j * dtheta));
                    y = innerBladeRadius * Math.Sin(st + (j * dtheta));
                    inner.Add(new Point(x, y));
                }
                double mid = st + (bladeStartDTheta / 2);
                double midx = innerBladeRadius * Math.Cos(mid);
                double midy = innerBladeRadius * Math.Sin(mid);
                if (innerz.Count == 0)
                {
                    foreach (Point pi in inner)
                    {
                        double dist = (pi.X - midx) * (pi.X - midx) + (pi.Y - midy) * (pi.Y - midy);
                        int sn = Math.Sign(pi.X - midx);
                        if (anticlockwise)
                        {
                            sn = -sn;
                        }
                        if (dist != 0)
                        {
                            dist = Math.Sqrt(dist);
                        }
                        double z = sn * dist * Math.Cos(pitch);
                        innerz.Add(z);
                    }
                }
                List<Point> outter = new List<Point>();
                for (int j = 0; j < numSteps; j++)
                {
                    x = outterBladeRadius * Math.Cos(st + (j * dtheta));
                    y = outterBladeRadius * Math.Sin(st + (j * dtheta));
                    outter.Add(new Point(x, y));
                }

                if (outterz.Count == 0)
                {
                    midx = outterBladeRadius * Math.Cos(mid);
                    midy = outterBladeRadius * Math.Sin(mid);

                    foreach (Point pi in outter)
                    {
                        double dist = (pi.X - midx) * (pi.X - midx) + (pi.Y - midy) * (pi.Y - midy);
                        int sn = Math.Sign(pi.X - midx);
                        if (anticlockwise)
                        {
                            sn = -sn;
                        }
                        if (dist != 0)
                        {
                            dist = Math.Sqrt(dist);
                        }
                        double z = sn * dist * Math.Cos(pitch);
                        outterz.Add(z);
                    }
                }
                int backP1 = -1;
                int backP2 = -1;

                int backP3 = -1;
                int backP4 = -1;
                // back
                for (int j = 0; j < inner.Count - 1; j++)
                {
                    int p1 = AddVertice(new Point3D(inner[j].X, inner[j].Y, innerz[j]));
                    int p2 = AddVertice(new Point3D(inner[j + 1].X, inner[j + 1].Y, innerz[j + 1]));
                    int p3 = AddVertice(new Point3D(outter[j].X, outter[j].Y, outterz[j]));
                    int p4 = AddVertice(new Point3D(outter[j + 1].X, outter[j + 1].Y, outterz[j + 1]));
                    if (j == 0)
                    {
                        backP1 = p1;
                        backP2 = p3;
                    }

                    if (j == inner.Count - 2)
                    {
                        backP3 = p2;
                        backP4 = p4;
                    }
                    Faces.Add(p1);
                    Faces.Add(p2);
                    Faces.Add(p3);

                    Faces.Add(p3);
                    Faces.Add(p2);
                    Faces.Add(p4);
                }

                int frontP1 = -1;
                int frontP2 = -1;
                int frontP3 = -1;
                int frontP4 = -1;
                // front
                for (int j = 0; j < inner.Count - 1; j++)
                {
                    int p1 = AddVertice(new Point3D(inner[j].X, inner[j].Y, innerz[j] + bladeThickness));
                    int p2 = AddVertice(new Point3D(inner[j + 1].X, inner[j + 1].Y, innerz[j + 1] + bladeThickness));
                    int p3 = AddVertice(new Point3D(outter[j].X, outter[j].Y, outterz[j] + bladeThickness));
                    int p4 = AddVertice(new Point3D(outter[j + 1].X, outter[j + 1].Y, outterz[j + 1] + bladeThickness));
                    if (j == 0)
                    {
                        frontP1 = p1;
                        frontP2 = p3;
                    }
                    if (j == inner.Count - 2)
                    {
                        frontP3 = p2;
                        frontP4 = p4;
                    }
                    Faces.Add(p1);
                    Faces.Add(p3);
                    Faces.Add(p2);

                    Faces.Add(p3);
                    Faces.Add(p4);
                    Faces.Add(p2);
                }

                // inner
                for (int j = 0; j < inner.Count - 1; j++)
                {
                    int p1 = AddVertice(new Point3D(inner[j].X, inner[j].Y, innerz[j]));
                    int p2 = AddVertice(new Point3D(inner[j + 1].X, inner[j + 1].Y, innerz[j + 1]));
                    int p3 = AddVertice(new Point3D(inner[j].X, inner[j].Y, innerz[j] + bladeThickness));
                    int p4 = AddVertice(new Point3D(inner[j + 1].X, inner[j + 1].Y, innerz[j + 1] + bladeThickness));
                    Faces.Add(p1);
                    Faces.Add(p3);
                    Faces.Add(p2);

                    Faces.Add(p3);
                    Faces.Add(p4);
                    Faces.Add(p2);
                }

                // outter
                for (int j = 0; j < inner.Count - 1; j++)
                {
                    int p1 = AddVertice(new Point3D(outter[j].X, outter[j].Y, outterz[j]));
                    int p2 = AddVertice(new Point3D(outter[j + 1].X, outter[j + 1].Y, outterz[j + 1]));
                    int p3 = AddVertice(new Point3D(outter[j].X, outter[j].Y, outterz[j] + bladeThickness));
                    int p4 = AddVertice(new Point3D(outter[j + 1].X, outter[j + 1].Y, outterz[j + 1] + bladeThickness));
                    Faces.Add(p1);
                    Faces.Add(p2);
                    Faces.Add(p3);

                    Faces.Add(p3);
                    Faces.Add(p2);
                    Faces.Add(p4);
                }

                if (backP1 != -1 && frontP1 != -1)
                {
                    Faces.Add(backP1);
                    Faces.Add(backP2);
                    Faces.Add(frontP1);

                    Faces.Add(frontP1);
                    Faces.Add(backP2);
                    Faces.Add(frontP2);

                    Faces.Add(backP3);

                    Faces.Add(frontP3);
                    Faces.Add(backP4);

                    Faces.Add(frontP3);
                    Faces.Add(frontP4);
                    Faces.Add(backP4);
                }
            }
        }

        private void GenerateHub()
        {
            GenerateCylinder(hubRadius, hubRadius, 0);
        }

        private void GenerateShape()
        {
            ClearShape();
            GenerateBlades();
            GenerateHub();
            if (supportDisk)
            {
                GenerateSupportDisk();
            }
            CentreVertices();
        }

        private void GenerateSupportDisk()
        {
            GenerateCylinder(hubRadius + bladeLength, diskThickness, -(diskOffset * diskThickness));
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

        private void Regenerate()
        {
            GenerateShape();
            Redisplay();
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadEditorParameters();
            MyModelGroup.Children.Clear();
            GenerateShape();
            UpdateCameraPos();
            Redisplay();
        }
    }
}