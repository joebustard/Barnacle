using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Make3D.Dialogs
{
    /// <summary>
    /// Interaction logic for ThreadDlg.xaml
    /// </summary>
    public partial class ThreadDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private double angle;
        private double crest;
        private Point3DCollection helix1;
        private Point3DCollection helix2;
        private Point3DCollection helix3;
        private Point3DCollection helix4;
        private Point3DCollection helix5;
        private double length;
        private double majorRadius;

        private double minorRadius;

        private double pitch;
        private double root;
        private string warningText;

        public ThreadDlg()
        {
            InitializeComponent();
            ToolName = "ThreadDlg";
            DataContext = this;
            ModelGroup = MyModelGroup;
            minorRadius = 12.5;
            majorRadius = 15;
            pitch = 3.5;
            root = 0.1;
            crest = 0.1;
            length = 10;
            warningText = "";
            helix1 = new Point3DCollection();
            helix2 = new Point3DCollection();
            helix3 = new Point3DCollection();
            helix4 = new Point3DCollection();
            helix5 = new Point3DCollection();
        }

        public double Angle
        {
            get
            {
                return angle;
            }
            set
            {
                if (angle != value)
                {
                    angle = value;
                    NotifyPropertyChanged();

                    UpdateDisplay();
                }
            }
        }

        public double Crest
        {
            get
            {
                return crest;
            }
            set
            {
                if (crest != value)
                {
                    crest = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double Length
        {
            get
            {
                return length;
            }
            set
            {
                if (length != value)
                {
                    length = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double MajorRadius
        {
            get
            {
                return majorRadius;
            }
            set
            {
                if (majorRadius != value)
                {
                    majorRadius = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double MinorRadius
        {
            get
            {
                return minorRadius;
            }
            set
            {
                if (minorRadius != value)
                {
                    minorRadius = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double Pitch
        {
            get
            {
                return pitch;
            }
            set
            {
                if (pitch != value)
                {
                    pitch = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double Root
        {
            get
            {
                return root;
            }
            set
            {
                if (root != value)
                {
                    root = value;
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

        private int CreateHelix(Point3DCollection helix, double radius, double angle, double length, double zOff = 0)
        {
            helix.Clear();
            int pointsInFirstRotation = 0;
            double t = 0;
            double dt = 0.01;
            double ao = (Math.PI * 2.0) / pitch;
            while (t < length)
            {
                if (t < Math.PI * 2.0)
                {
                    pointsInFirstRotation++;
                }
                double theta = t * ao;
                double x = radius * Math.Cos(theta);
                double y = radius * Math.Sin(theta);
                double z = t + zOff;
                helix.Add(new Point3D(x, y, z));
                t += dt;
            }
            return pointsInFirstRotation;
        }

        private void GenerateShape()
        {
            ClearShape();
            WarningText = "";
            if (minorRadius >= majorRadius)
            {
                WarningText = "Minor Radius must be less than Major Radius";
            }
            else
            {
                if (crest + root >= pitch)
                {
                    WarningText = "Pitch must be more than Root + Crest";
                }
                else
                {
                    // Helix1 is the spiral for the main diameter
                    CreateHelix(helix1, majorRadius, angle, length);

                    // helix2 is helix1 offset by the crest
                    helix2.Clear();
                    foreach (Point3D p in helix1)
                    {
                        helix2.Add(new Point3D(p.X, p.Y, p.Z + crest));
                    }

                    // add the crest
                    TriangulateHelixPair(helix1, helix2);

                    // Helix3 is the spiral for the minor diameter
                    CreateHelix(helix3, minorRadius, angle, length, pitch / 2);

                    // helix4 is helix1 offset by the crest
                    helix4.Clear();
                    foreach (Point3D p in helix3)
                    {
                        helix4.Add(new Point3D(p.X, p.Y, p.Z + root));
                    }

                    // add the crest
                    TriangulateHelixPair(helix3, helix4);

                    TriangulateHelixPair(helix2, helix3);
                    CreateHelix(helix5, minorRadius, angle, length, (-pitch / 2.0) + root);
                    TriangulateHelixPair(helix5, helix1);
                }
            }
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
        }

        private void TriangulateHelixPair(Point3DCollection hx1, Point3DCollection hx2)
        {
            for (int i = 0; i < hx1.Count - 1; i++)
            {
                int v1 = AddVertice(hx1[i]);
                int v2 = AddVertice(hx2[i]);
                int v3 = AddVertice(hx2[i + 1]);
                int v4 = AddVertice(hx1[i + 1]);

                Faces.Add(v1);
                Faces.Add(v3);
                Faces.Add(v2);

                Faces.Add(v1);
                Faces.Add(v4);
                Faces.Add(v3);
            }
        }

        private void UpdateDisplay()
        {
            GenerateShape();
            Redisplay();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadEditorParameters();
            GenerateShape();
            UpdateCameraPos();
            MyModelGroup.Children.Clear();

            Redisplay();
        }
    }
}