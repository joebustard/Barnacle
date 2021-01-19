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
        private double radius1;
        private double radius2;
        private double gap;
        private double height;
        private string shapeStyle;
        private List<string> shapes;

        public double ShapeHeight
        {
            get { return height; }
            set
            {
                if (height != value)
                {
                    if (value > 0)
                    {
                        height = value;
                        NotifyPropertyChanged();
                        Refresh();
                    }
                }
            }
        }

        public double Radius1
        {
            get { return radius1; }
            set
            {
                if (value != radius1)
                {
                    radius1 = value;
                    NotifyPropertyChanged();
                    Refresh();
                }
            }
        }

        private void Refresh()
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

        private void GenerateSausageShape()
        {
        }

        public double Radius2
        {
            get { return radius2; }
            set
            {
                if (value != radius2)
                {
                    radius2 = value;
                    NotifyPropertyChanged();
                    Refresh();
                }
            }
        }

        public double Gap
        {
            get { return gap; }
            set
            {
                if (value != gap)
                {
                    gap = value;
                    NotifyPropertyChanged();
                    Refresh();
                }
            }
        }

        public List<string> Shapes
        {
            get { return shapes; }
            set
            {
                if (value != shapes)
                {
                    shapes = value;
                    NotifyPropertyChanged();
                    Refresh();
                }
            }
        }

        public string ShapeStyle
        {
            get { return shapeStyle; }
            set
            {
                if (value != shapeStyle)
                {
                    shapeStyle = value;
                    NotifyPropertyChanged();
                    Refresh();
                }
            }
        }

        public override bool ShowFloor
        {
            get { return showFloor; }
            set
            {
                if (showFloor != value)
                {
                    showFloor = value;
                    NotifyPropertyChanged();
                    Refresh();
                }
            }
        }

        public override bool ShowAxies
        {
            get { return showAxies; }
            set
            {
                if (showAxies != value)
                {
                    showAxies = value;
                    NotifyPropertyChanged();
                    Refresh();
                }
            }
        }

        public StadiumDialog()
        {
            InitializeComponent();
            ToolName = "Stadium";
            DataContext = this;
            Radius1 = 10;
            Radius2 = 10;
            Gap = 10;
            height = 10;
            shapes = new List<string>();
            shapes.Add("Flat");
            shapes.Add("Sausage");
            NotifyPropertyChanged("Shapes");
            ShapeStyle = "Flat";
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
            }
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

                //  CentreVertices();
                GeometryModel3D gm = GetModel();

                MyModelGroup.Children.Add(gm);
            }
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParameters();

            DialogResult = true;
            Close();
        }

        private void SaveEditorParameters()
        {
            EditorParameters.Set("Radius1", radius1.ToString());
            EditorParameters.Set("Radius2", radius2.ToString());
            EditorParameters.Set("Gap", gap.ToString());
            EditorParameters.Set("Height", height.ToString());
            EditorParameters.Set("Shape", shapeStyle);
        }
    }
}