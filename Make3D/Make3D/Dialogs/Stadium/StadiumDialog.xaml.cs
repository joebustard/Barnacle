using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Media3D;
using Point = System.Windows.Point;

namespace Make3D.Dialogs
{
    /// <summary>
    /// Interaction logic for Stadium.xaml
    /// </summary>
    public partial class StadiumDialog : BaseModellerDialog, INotifyPropertyChanged
    {
        private double gap;
        private double height;
        private double radius1;
        private double radius2;
        private List<string> shapes;
        private string shapeStyle;

        public StadiumDialog()
        {
            InitializeComponent();
            ToolName = "Stadium";
            DataContext = this;
            Radius1 = 5;
            Radius2 = 10;
            Gap = 10;
            height = 10;
            shapes = new List<string>();
            shapes.Add("Flat");
            shapes.Add("Sausage");
            NotifyPropertyChanged("Shapes");
            ShapeStyle = "Flat";
            ModelGroup = MyModelGroup;
        }

        public double Gap
        {
            get
            {
                return gap;
            }
            set
            {
                if (value != gap)
                {
                    gap = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double Radius1
        {
            get
            {
                return radius1;
            }
            set
            {
                if (value != radius1)
                {
                    radius1 = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double Radius2
        {
            get
            {
                return radius2;
            }
            set
            {
                if (value != radius2)
                {
                    radius2 = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double ShapeHeight
        {
            get
            {
                return height;
            }
            set
            {
                if (height != value)
                {
                    if (value > 0)
                    {
                        height = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public List<string> Shapes
        {
            get
            {
                return shapes;
            }
            set
            {
                if (value != shapes)
                {
                    shapes = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public string ShapeStyle
        {
            get
            {
                return shapeStyle;
            }
            set
            {
                if (value != shapeStyle)
                {
                    shapeStyle = value;
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
                    UpdateDisplay();
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
                    UpdateDisplay();
                }
            }
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParameters();

            DialogResult = true;
            Close();
        }

        private void GenerateBasicShape()
        {
            double dx = gap / 2.0;
            double cy1 = -dx - radius1;
            double cy2 = dx + radius2;

            double cx1 = 0;
            double cx2 = 0;

            double theta = 0;
            double dt = Math.PI / 20.0;

            List<Point> perimeter = new List<Point>();
            theta = 0;
            while (theta <= Math.PI)
            {
                double x = cx2 + radius2 * Math.Cos(theta);
                double y = cy2 + radius2 * Math.Sin(theta);

                perimeter.Add(new Point(x, y));
                theta += dt;
            }

            while (theta <= 2.0 * Math.PI)
            {
                double x = cx1 + radius1 * Math.Cos(theta);
                double y = cy1 + radius1 * Math.Sin(theta);

                perimeter.Add(new Point(x, y));
                theta += dt;
            }
            Vertices.Clear();
            Faces.Clear();

            for (int i = 0; i < perimeter.Count; i++)
            {
                int j = i + 1;
                if (j == perimeter.Count)
                {
                    j = 0;
                }

                int v0 = AddVertice(perimeter[i].X, 0, perimeter[i].Y);
                int v1 = AddVertice(perimeter[i].X, height, perimeter[i].Y);
                int v2 = AddVertice(perimeter[j].X, height, perimeter[j].Y);
                int v3 = AddVertice(perimeter[j].X, 0, perimeter[j].Y);

                Faces.Add(v0);
                Faces.Add(v1);
                Faces.Add(v2);

                Faces.Add(v0);
                Faces.Add(v2);
                Faces.Add(v3);
            }
            TriangulatePerimiter(perimeter, height);
        }

        private void GenerateSausageShape()
        {
            double dx = gap / 2.0;
            double cy1 = -dx - radius1;
            double cy2 = dx + radius2;

            double cx1 = 0;
            double cx2 = 0;

            double theta = 0;
            double dt = Math.PI / 20.0;

            List<Point> perimeter = new List<Point>();
            theta = Math.PI / 2;
            while (theta >= 0)
            {
                double x = cx2 + radius2 * Math.Cos(theta);
                double y = cy2 + radius2 * Math.Sin(theta);

                perimeter.Add(new Point(x, y));
                theta -= dt;
            }

            theta = 2.0 * Math.PI;

            while (theta >= 1.5 * Math.PI)
            {
                double x = cx1 + radius1 * Math.Cos(theta);
                double y = cy1 + radius1 * Math.Sin(theta);

                perimeter.Add(new Point(x, y));
                theta -= dt;
            }
            Vertices.Clear();
            Faces.Clear();

            double phi = 0;
            double phi2 = 0;
            double numberOfFacets = 72;
            double dphi = (2 * Math.PI) / numberOfFacets;

            double px1 = 0;
            double py1 = 0;

            double px2 = 0;
            double py2 = 0;

            double px3 = 0;
            double py3 = 0;

            double px4 = 0;
            double py4 = 0;

            while (phi <= (2 * Math.PI) - dphi)
            {
                phi2 = phi + dphi;

                for (int i = 0; i < perimeter.Count - 1; i++)
                {
                    int j = i + 1;
                    if (j == perimeter.Count)
                    {
                        j = 0;
                    }
                    px1 = perimeter[i].X * Math.Cos(phi);
                    py1 = perimeter[i].X * Math.Sin(phi);

                    px2 = perimeter[i].X * Math.Cos(phi2);
                    py2 = perimeter[i].X * Math.Sin(phi2);

                    px3 = perimeter[j].X * Math.Cos(phi2);
                    py3 = perimeter[j].X * Math.Sin(phi2);

                    px4 = perimeter[j].X * Math.Cos(phi);
                    py4 = perimeter[j].X * Math.Sin(phi);

                    int v0 = AddVertice(px1, py1, perimeter[i].Y);
                    int v1 = AddVertice(px2, py2, perimeter[i].Y);
                    int v2 = AddVertice(px3, py3, perimeter[j].Y);
                    int v3 = AddVertice(px4, py4, perimeter[j].Y);

                    Faces.Add(v0);
                    Faces.Add(v2);
                    Faces.Add(v1);

                    Faces.Add(v0);
                    Faces.Add(v3);
                    Faces.Add(v2);
                }
                phi += dphi;
            }
        }

        private void SaveEditorParameters()
        {
            EditorParameters.Set("Radius1", radius1.ToString());
            EditorParameters.Set("Radius2", radius2.ToString());
            EditorParameters.Set("Gap", gap.ToString());
            EditorParameters.Set("Height", height.ToString());
            EditorParameters.Set("Shape", shapeStyle);
        }

        private void TriangulatePerimiter(List<Point> points, double thickness)
        {
            TriangulationPolygon ply = new TriangulationPolygon();
            List<PointF> pf = new List<PointF>();
            foreach (Point p in points)
            {
                pf.Add(new PointF((float)p.X, (float)p.Y));
            }
            ply.Points = pf.ToArray();
            List<Triangle> tris = ply.Triangulate();
            foreach (Triangle t in tris)
            {
                int c0 = AddVertice(t.Points[0].X, thickness, t.Points[0].Y);
                int c1 = AddVertice(t.Points[1].X, thickness, t.Points[1].Y);
                int c2 = AddVertice(t.Points[2].X, thickness, t.Points[2].Y);
                Faces.Add(c0);
                Faces.Add(c2);
                Faces.Add(c1);

                c0 = AddVertice(t.Points[0].X, 0.0, t.Points[0].Y);
                c1 = AddVertice(t.Points[1].X, 0.0, t.Points[1].Y);
                c2 = AddVertice(t.Points[2].X, 0.0, t.Points[2].Y);
                Faces.Add(c0);
                Faces.Add(c1);
                Faces.Add(c2);
            }
        }

        private void UpdateDisplay()
        {
            switch (ShapeStyle)
            {
                case "Flat":
                    {
                        GenerateBasicShape();
                    }
                    break;

                case "Sausage":
                    {
                        GenerateSausageShape();
                    }
                    break;
            }

            Redisplay();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GenerateBasicShape();
            Redisplay();
            UpdateCameraPos();
            string s = EditorParameters.Get("Shape");
            if (s != "")
            {
                ShapeStyle = s;
                Radius1 = EditorParameters.GetDouble("Radius1");
                Radius2 = EditorParameters.GetDouble("Radius2");
                Gap = EditorParameters.GetDouble("Gap");
                ShapeHeight = EditorParameters.GetDouble("Height");
                ShapeStyle = s;
            }
        }
    }
}