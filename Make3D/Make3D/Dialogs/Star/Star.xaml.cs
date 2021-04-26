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
    public partial class Star : BaseModellerDialog, INotifyPropertyChanged
    {
        private double centreRadius;
        private bool facetChecked;
        private bool flatChecked;
        private int numberOfPoints;

        private double pointLength;

        public Star()
        {
            InitializeComponent();
            ToolName = "Star";
            DataContext = this;
        }

        public double CentreRadius
        {
            get
            {
                return centreRadius;
            }
            set
            {
                if (centreRadius != value)
                {
                    centreRadius = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool FacetChecked
        {
            get
            {
                return facetChecked;
            }
            set
            {
                if (facetChecked != value)
                {
                    facetChecked = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool FlatChecked
        {
            get
            {
                return flatChecked;
            }
            set
            {
                if (flatChecked != value)
                {
                    flatChecked = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int NumberOfPoints
        {
            get
            {
                return numberOfPoints;
            }
            set
            {
                if (numberOfPoints != value)
                {
                    numberOfPoints = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double PointLength
        {
            get
            {
                return pointLength;
            }
            set
            {
                if (pointLength != value)
                {
                    pointLength = value;
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

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParmeters();
            DialogResult = true;
            Close();
        }

        private void GenerateShape()
        {
            ClearShape();

            double theta = 0;
            double dtheta;
            int divs = numberOfPoints * 2;
            dtheta = Math.PI * 2.0 / divs;
            int i = 0;
            List<Point> pnts = new List<Point>();
            for (i = 0; i < divs; i++)
            {
                double x = centreRadius * Math.Cos(theta);
                double y = centreRadius * Math.Sin(theta);

                pnts.Add(new Point(x, y));

                if (i % 2 == 1)
                {
                    x = (pointLength + centreRadius) * Math.Cos(theta);
                    y = (pointLength + centreRadius) * Math.Sin(theta);

                    pnts.Add(new Point(x, y));
                }
                theta += dtheta;
            }
            i = 0;
            bool skip = false;
            while (i < pnts.Count)
            {
                int p1 = AddVertice(new Point3D(pnts[i].X, 0, pnts[i].Y));
                int p2 = AddVertice(new Point3D(pnts[i + 1].X, 0, pnts[i + 1].Y));
                int j = i + 2;
                if (j >= pnts.Count)
                {
                    j = 0;
                }
                int p3 = AddVertice(new Point3D(pnts[j].X, 0, pnts[j].Y));

                Faces.Add(p1);
                Faces.Add(p2);
                Faces.Add(p3);
                if (skip)
                {
                    i += 1;
                }
                i++;
                skip = !skip;
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FacetChecked = false;
            FlatChecked = true;
            NumberOfPoints = 2;
            PointLength = 20;
            CentreRadius = 10;

            LoadEditorParameters();
            GenerateShape();
            UpdateCameraPos();
            MyModelGroup.Children.Clear();

            Redisplay();
        }
    }
}