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
        private bool facet2Checked;
        private bool facetChecked;
        private bool flatChecked;
        private int numberOfPoints;
        private double pointLength;
        private double thickness;

        public Star()
        {
            InitializeComponent();
            ToolName = "Star";
            DataContext = this;
            ModelGroup = MyModelGroup;
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
                    UpdateDisplay();
                }
            }
        }

        public bool Facet2Checked
        {
            get
            {
                return facet2Checked;
            }
            set
            {
                if (facet2Checked != value)
                {
                    facet2Checked = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
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
                    UpdateDisplay();
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
                    UpdateDisplay();
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
                    UpdateDisplay();
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

        public double Thickness
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

        private void GenerateFacettedStar()
        {
            double theta = 0;
            double dtheta;
            int divs = numberOfPoints;
            dtheta = Math.PI * 2.0 / divs;
            int i = 0;
            List<Point> pnts = new List<Point>();
            List<Point> innerpnts = new List<Point>();
            List<Point> midpnts = new List<Point>();
            List<Point> outerpnts = new List<Point>();
            for (i = 0; i < divs; i++)
            {
                double x = centreRadius * Math.Cos(theta);
                double y = centreRadius * Math.Sin(theta);

                innerpnts.Add(new Point(x, y));
                theta += dtheta / 2.0;

                x = centreRadius * Math.Cos(theta);
                y = centreRadius * Math.Sin(theta);
                midpnts.Add(new Point(x, y));

                x = (pointLength + centreRadius) * Math.Cos(theta);
                y = (pointLength + centreRadius) * Math.Sin(theta);

                outerpnts.Add(new Point(x, y));

                theta += dtheta / 2;
            }
            // bottom of Points
            i = 0;
            double h2 = thickness / 2.0;

            while (i < innerpnts.Count)
            {
                int j = i + 1;
                if (j >= innerpnts.Count)
                {
                    j = 0;
                }

                int p1 = AddVertice(new Point3D(innerpnts[i].X, h2, innerpnts[i].Y));
                int p2 = AddVertice(new Point3D(midpnts[i].X, 0, midpnts[i].Y));
                int p3 = AddVertice(new Point3D(outerpnts[i].X, h2, outerpnts[i].Y));
                int p4 = AddVertice(new Point3D(innerpnts[j].X, h2, innerpnts[j].Y));

                int p5 = AddVertice(new Point3D(midpnts[i].X, thickness, midpnts[i].Y));

                // bottom of points
                Faces.Add(p1);
                Faces.Add(p3);
                Faces.Add(p2);

                Faces.Add(p2);
                Faces.Add(p3);
                Faces.Add(p4);

                // top of points
                Faces.Add(p1);
                Faces.Add(p5);
                Faces.Add(p3);

                Faces.Add(p5);
                Faces.Add(p4);
                Faces.Add(p3);
                i++;
            }

            i = 0;

            while (i < innerpnts.Count)
            {
                int j = i + 1;
                if (j >= innerpnts.Count)
                {
                    j = 0;
                }

                int p1 = AddVertice(new Point3D(innerpnts[i].X, h2, innerpnts[i].Y));
                int p2 = AddVertice(new Point3D(midpnts[i].X, 0, midpnts[i].Y));
                int p3 = AddVertice(new Point3D(0, h2, 0));
                int p4 = AddVertice(new Point3D(innerpnts[j].X, h2, innerpnts[j].Y));

                int p5 = AddVertice(new Point3D(midpnts[i].X, thickness, midpnts[i].Y));

                // bottom of points
                Faces.Add(p1);
                Faces.Add(p2);
                Faces.Add(p3);

                Faces.Add(p2);
                Faces.Add(p4);
                Faces.Add(p3);

                // top of points
                Faces.Add(p1);
                Faces.Add(p3);
                Faces.Add(p5);

                Faces.Add(p5);
                Faces.Add(p3);
                Faces.Add(p4);
                i++;
            }
        }

        private void GenerateFacettedStar2()
        {
            double theta = 0;
            double dtheta;
            int divs = numberOfPoints;
            dtheta = Math.PI * 2.0 / divs;
            int i = 0;
            List<Point> pnts = new List<Point>();
            List<Point> innerpnts = new List<Point>();
            List<Point> midpnts = new List<Point>();
            List<Point> outerpnts = new List<Point>();
            for (i = 0; i < divs; i++)
            {
                double x = centreRadius * Math.Cos(theta);
                double y = centreRadius * Math.Sin(theta);

                innerpnts.Add(new Point(x, y));
                theta += dtheta / 2.0;

                x = centreRadius * Math.Cos(theta);
                y = centreRadius * Math.Sin(theta);
                midpnts.Add(new Point(x, y));

                x = (pointLength + centreRadius) * Math.Cos(theta);
                y = (pointLength + centreRadius) * Math.Sin(theta);

                outerpnts.Add(new Point(x, y));

                theta += dtheta / 2;
            }
            // bottom of Points
            i = 0;
            double h2 = thickness / 2.0;

            while (i < innerpnts.Count)
            {
                int j = i + 1;
                if (j >= innerpnts.Count)
                {
                    j = 0;
                }

                int p1 = AddVertice(new Point3D(innerpnts[i].X, h2, innerpnts[i].Y));
                int p2 = AddVertice(new Point3D(midpnts[i].X, 0, midpnts[i].Y));
                int p3 = AddVertice(new Point3D(outerpnts[i].X, h2, outerpnts[i].Y));
                int p4 = AddVertice(new Point3D(innerpnts[j].X, h2, innerpnts[j].Y));

                int p5 = AddVertice(new Point3D(midpnts[i].X, thickness, midpnts[i].Y));

                // bottom of points
                Faces.Add(p1);
                Faces.Add(p3);
                Faces.Add(p2);

                Faces.Add(p2);
                Faces.Add(p3);
                Faces.Add(p4);

                // top of points
                Faces.Add(p1);
                Faces.Add(p5);
                Faces.Add(p3);

                Faces.Add(p5);
                Faces.Add(p4);
                Faces.Add(p3);
                i++;
            }

            i = 0;

            while (i < innerpnts.Count)
            {
                int j = i + 1;
                if (j >= innerpnts.Count)
                {
                    j = 0;
                }

                int p1 = AddVertice(new Point3D(innerpnts[i].X, h2, innerpnts[i].Y));
                int p2 = AddVertice(new Point3D(midpnts[i].X, 0, midpnts[i].Y));
                int p3 = AddVertice(new Point3D(0, 0, 0));
                int p4 = AddVertice(new Point3D(innerpnts[j].X, h2, innerpnts[j].Y));

                int p5 = AddVertice(new Point3D(0, thickness, 0));
                int p6 = AddVertice(new Point3D(midpnts[i].X, thickness, midpnts[i].Y));
                // bottom of points
                Faces.Add(p1);
                Faces.Add(p2);
                Faces.Add(p3);

                Faces.Add(p2);
                Faces.Add(p4);
                Faces.Add(p3);

                // top of points
                Faces.Add(p1);
                Faces.Add(p5);
                Faces.Add(p6);

                Faces.Add(p6);
                Faces.Add(p5);
                Faces.Add(p4);

                i++;
            }
        }

        private void GenerateFlatStar()
        {
            double theta = 0;
            double dtheta;
            int divs = numberOfPoints * 2;
            dtheta = Math.PI * 2.0 / divs;
            int i = 0;
            List<Point> pnts = new List<Point>();
            List<Point> innerpnts = new List<Point>();
            List<Point> outerpnts = new List<Point>();
            for (i = 0; i < divs; i++)
            {
                double x = centreRadius * Math.Cos(theta);
                double y = centreRadius * Math.Sin(theta);

                pnts.Add(new Point(x, y));
                innerpnts.Add(new Point(x, y));
                if (i % 2 == 0)
                {
                    outerpnts.Add(new Point(x, y));
                }

                if (i % 2 == 1)
                {
                    x = (pointLength + centreRadius) * Math.Cos(theta);
                    y = (pointLength + centreRadius) * Math.Sin(theta);

                    pnts.Add(new Point(x, y));
                    outerpnts.Add(new Point(x, y));
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
                if (skip)
                {
                    Faces.Add(p1);
                    Faces.Add(p2);
                    Faces.Add(p3);
                }
                else
                {
                    Faces.Add(p1);
                    Faces.Add(p3);
                    Faces.Add(p2);
                }
                if (skip)
                {
                    i += 1;
                }
                i++;
                skip = !skip;
            }

            for (i = 0; i < innerpnts.Count; i++)
            {
                int j = i + 1;
                if (j >= innerpnts.Count)
                {
                    j = 0;
                }
                int p1 = AddVertice(new Point3D(innerpnts[i].X, 0, innerpnts[i].Y));
                int p2 = AddVertice(new Point3D(innerpnts[j].X, 0, innerpnts[j].Y));
                int p3 = AddVertice(new Point3D(0, 0, 0));
                Faces.Add(p1);
                Faces.Add(p2);
                Faces.Add(p3);
                p1 = AddVertice(new Point3D(innerpnts[i].X, thickness, innerpnts[i].Y));
                p2 = AddVertice(new Point3D(innerpnts[j].X, thickness, innerpnts[j].Y));
                p3 = AddVertice(new Point3D(0, thickness, 0));
                Faces.Add(p1);
                Faces.Add(p3);
                Faces.Add(p2);
            }
            i = 0;
            skip = false;
            while (i < pnts.Count)
            {
                int p1 = AddVertice(new Point3D(pnts[i].X, thickness, pnts[i].Y));
                int p2 = AddVertice(new Point3D(pnts[i + 1].X, thickness, pnts[i + 1].Y));
                int j = i + 2;
                if (j >= pnts.Count)
                {
                    j = 0;
                }
                int p3 = AddVertice(new Point3D(pnts[j].X, thickness, pnts[j].Y));
                if (skip)
                {
                    Faces.Add(p1);
                    Faces.Add(p3);
                    Faces.Add(p2);
                }
                else
                {
                    Faces.Add(p1);
                    Faces.Add(p2);
                    Faces.Add(p3);
                }
                if (skip)
                {
                    i += 1;
                }
                i++;
                skip = !skip;
            }

            for (i = 0; i < outerpnts.Count; i++)
            {
                int j = i + 1;
                if (j >= outerpnts.Count)
                {
                    j = 0;
                }
                int p1 = AddVertice(new Point3D(outerpnts[i].X, 0, outerpnts[i].Y));
                int p2 = AddVertice(new Point3D(outerpnts[i].X, thickness, outerpnts[i].Y));
                int p3 = AddVertice(new Point3D(outerpnts[j].X, thickness, outerpnts[j].Y));
                int p4 = AddVertice(new Point3D(outerpnts[j].X, 0, outerpnts[j].Y));
                Faces.Add(p1);
                Faces.Add(p2);
                Faces.Add(p3);

                Faces.Add(p1);
                Faces.Add(p3);
                Faces.Add(p4);
            }
        }

        private void GenerateShape()
        {
            ClearShape();

            if (FlatChecked)
            {
                GenerateFlatStar();
            }

            if (FacetChecked)
            {
                GenerateFacettedStar();
            }
            if (Facet2Checked)
            {
                GenerateFacettedStar2();
            }
        }

        private void LoadEditorParameters()
        {
            FlatChecked = EditorParameters.GetBoolean("FlatChecked", true);
            FacetChecked = EditorParameters.GetBoolean("FacetChecked");
            Facet2Checked = EditorParameters.GetBoolean("Facet2Checked");
            CentreRadius = EditorParameters.GetDouble("CentreRadius", 10);
            NumberOfPoints = EditorParameters.GetInt("NumberOfPoints", 2);
            PointLength = EditorParameters.GetDouble("PointLength", 10);
        }

        private void SaveEditorParmeters()
        {
            EditorParameters.Set("FlatChecked", FlatChecked.ToString());
            EditorParameters.Set("FacetChecked", FacetChecked.ToString());
            EditorParameters.Set("Facet2Checked", Facet2Checked.ToString());
            EditorParameters.Set("CentreRadius", CentreRadius.ToString());
            EditorParameters.Set("NumberOfPoints", NumberOfPoints.ToString());
            EditorParameters.Set("PointLength", PointLength.ToString());
        }

        private void UpdateDisplay()
        {
            GenerateShape();
            Redisplay();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FacetChecked = false;
            FlatChecked = true;
            NumberOfPoints = 2;
            PointLength = 20;
            CentreRadius = 10;
            Thickness = 10;
            LoadEditorParameters();
            GenerateShape();
            UpdateCameraPos();
            MyModelGroup.Children.Clear();

            Redisplay();
        }
    }
}