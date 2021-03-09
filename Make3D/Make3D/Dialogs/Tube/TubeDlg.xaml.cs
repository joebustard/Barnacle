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
    public partial class TubeDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private double innerRadius;
        private double lowerBevel;
        private double sweepDegrees;
        private double thickness;
        private double tubeHeight;
        private double upperBevel;

        public TubeDlg()
        {
            InitializeComponent();
            ToolName = "Tube";
            TubeHeight = 20;
            InnerRadius = 20;
            TubeThickness = 5;
            UpperBevel = 0;
            LowerBevel = 0;
            SweepDegrees = 360;
            DataContext = this;
        }

        public double InnerRadius
        {
            get
            {
                return innerRadius;
            }
            set
            {
                if (innerRadius != value)
                {
                    innerRadius = value;
                    GenerateRing();
                    Redisplay();
                    NotifyPropertyChanged();
                }
            }
        }

        public double LowerBevel
        {
            get
            {
                return lowerBevel;
            }
            set
            {
                if (lowerBevel != value)
                {
                    lowerBevel = value;
                    GenerateRing();
                    Redisplay();
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

        public double SweepDegrees
        {
            get
            {
                return sweepDegrees;
            }
            set
            {
                sweepDegrees = value;
                NotifyPropertyChanged();
                GenerateRing();
                Redisplay();
            }
        }

        public double TubeHeight
        {
            get
            {
                return tubeHeight;
            }
            set
            {
                if (tubeHeight != value)
                {
                    tubeHeight = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double TubeThickness
        {
            get
            {
                return thickness;
            }
            set
            {
                if (thickness != value)
                {
                    thickness = value;
                    GenerateRing();
                    Redisplay();
                    NotifyPropertyChanged();
                }
            }
        }

        public double UpperBevel
        {
            get
            {
                return upperBevel;
            }
            set
            {
                if (upperBevel != value)
                {
                    upperBevel = value;
                    GenerateRing();
                    Redisplay();
                    NotifyPropertyChanged();
                }
            }
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParameters();

            DialogResult = true;
            Close();
        }

        private void GenerateRing()
        {
            List<PolarCoordinate> polarProfile = new List<PolarCoordinate>();

            double cx = InnerRadius;

            int rotDivisions = 36;

            Point3D p3d = new Point3D(cx, 0, 0);
            PolarCoordinate pcol = new PolarCoordinate(0, 0, 0);
            pcol.SetPoint3D(p3d);
            polarProfile.Add(pcol);

            p3d = new Point3D(cx, 0, tubeHeight);
            pcol = new PolarCoordinate(0, 0, 0);
            pcol.SetPoint3D(p3d);
            polarProfile.Add(pcol);

            p3d = new Point3D(cx + TubeThickness, 0, tubeHeight - upperBevel);
            pcol = new PolarCoordinate(0, 0, 0);
            pcol.SetPoint3D(p3d);
            polarProfile.Add(pcol);

            p3d = new Point3D(cx + TubeThickness, 0, lowerBevel);
            pcol = new PolarCoordinate(0, 0, 0);
            pcol.SetPoint3D(p3d);
            polarProfile.Add(pcol);

            SweepPolarProfileTheta(polarProfile, cx, 0, sweepDegrees, rotDivisions);
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

        private void SaveEditorParameters()
        {
            EditorParameters.Set("InnerRadius", innerRadius.ToString());
            EditorParameters.Set("TubeHeight", tubeHeight.ToString());
            EditorParameters.Set("TubeThickness", thickness.ToString());
            EditorParameters.Set("UpperBevel", upperBevel.ToString());
            EditorParameters.Set("LowerBevel", lowerBevel.ToString());
            EditorParameters.Set("SweepDegrees", sweepDegrees.ToString());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string s = EditorParameters.Get("InnerRadius");
            if (s != "")
            {
                InnerRadius = Convert.ToDouble(s);
                TubeHeight = EditorParameters.GetDouble("TubeHeight");
                TubeThickness = EditorParameters.GetDouble("TubeThickness");
                UpperBevel = EditorParameters.GetDouble("UpperBevel");
                LowerBevel = EditorParameters.GetDouble("LowerBevel");
                SweepDegrees = EditorParameters.GetDouble("SweepDegrees");
            }
            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            GenerateRing();
            Redisplay();
        }
    }
}