using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace Make3D.Dialogs
{
    /// <summary>
    /// Interaction logic for SpurGearDialog.xaml
    /// </summary>
    public partial class SpurGearDialog : BaseModellerDialog, INotifyPropertyChanged
    {
        private double innerRadius;
        private double outterRadius;
        private int numberOfTeeth;
        private double teethBaseWidth;
        private double teethBaseHeight;
        private double teethTopWidth;
        private double teethTopHeight;
        private bool updateDisplayWhenChanged;
        List<System.Windows.Point> points;
        public SpurGearDialog()
        {
            InitializeComponent();
            DataContext = this;
            ToolName = "SpurGear";
            updateDisplayWhenChanged = false;
            points = new List<System.Windows.Point>();
        }

        public double InnerRadius
        {
            get => innerRadius;
            set
            {
                if (innerRadius != value)
                {
                    innerRadius = value;
                    NotifyPropertyChanged();
                    Redisplay();
                }
            }
        }

        private void Redisplay()
        {
            if (updateDisplayWhenChanged)
            {
                GenerateShape();
            }
        }

        public double OutterRadius
        {
            get => outterRadius;
            set
            {
                if (outterRadius != value)
                {
                    outterRadius = value;
                    NotifyPropertyChanged();
                    Redisplay();
                }
            }
        }
        public int NumberOfTeeth
        {
            get => numberOfTeeth;
            set
            {
                if (numberOfTeeth != value)
                {
                    numberOfTeeth = value;
                    NotifyPropertyChanged();
                    Redisplay();
                }
            }
        }
        public double TeethBaseWidth
        {
            get => teethBaseWidth;
            set
            {
                if (teethBaseWidth != value)
                {
                    teethBaseWidth = value;
                    NotifyPropertyChanged();
                    Redisplay();
                }
            }
        }
        public double TeethBaseHeight
        {
            get => teethBaseHeight;
            set
            {
                if (teethBaseHeight != value)
                {
                    teethBaseHeight = value;
                    NotifyPropertyChanged();
                    Redisplay();
                }
            }
        }
        public double TeethTopWidth
        {
            get => teethTopWidth;
            set
            {
                if (teethTopWidth != value)
                {
                    teethTopWidth = value;
                    NotifyPropertyChanged();
                    Redisplay();
                }
            }
        }
        public double TeethTopHeight
        {
            get => teethTopHeight;
            set
            {
                if (teethTopHeight != value)
                {
                    teethTopHeight = value;
                    NotifyPropertyChanged();
                    Redisplay();
                }
            }
        }

        private void BaseModellerDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (EditorParameters.Get("InnerRadius") == "")
            {
                InnerRadius = 10;
                OutterRadius = 30;
                NumberOfTeeth = 20;
                TeethBaseHeight = 5;
                TeethBaseWidth = 5;
                TeethTopHeight = 2;
                TeethTopWidth = 1;
            }

            GenerateShape();
            updateDisplayWhenChanged = true;
            UpdateCameraPos();
        }

        private void GenerateShape()
        {
            if (numberOfTeeth > 2)
            {
                if (innerRadius > 0)
                {
                    double actualOutterRadius = outterRadius;

                    if (outterRadius <= innerRadius)
                    {
                        actualOutterRadius = innerRadius;
                    }
                    double circumference = Math.PI * 2 * outterRadius;
                    double minTeethWidth = circumference / numberOfTeeth;
                    double actualBaseWidth = teethBaseWidth;
                    if (teethBaseWidth > minTeethWidth)
                    {
                        actualBaseWidth = minTeethWidth;
                    }

                    double totalGapLeft = circumference - (actualBaseWidth * numberOfTeeth);

                    double gapPerTooth = totalGapLeft / numberOfTeeth;
                    if (gapPerTooth < 0)
                    {
                        gapPerTooth = 0;
                    }

                    points.Clear();
                    double gapDeltaTheta = gapPerTooth / circumference * Math.PI * 2;
                    double toothDeltaTheta = teethBaseWidth / circumference * Math.PI * 2;

                    double theta = 0;
                    bool tooth = true;
                    while (theta <= Math.PI * 2)
                    {
                        if (tooth && (Math.PI * 2 - theta> toothDeltaTheta))
                        {
                            // add a tooth
                            // simple spike for the moment
                            double x = outterRadius * Math.Cos(theta);
                            double y = outterRadius * Math.Sin(theta);
                            points.Add(new System.Windows.Point(x, y));


                            x = (outterRadius + teethBaseHeight) * Math.Cos(theta);
                            y = (outterRadius + teethBaseHeight) * Math.Sin(theta);
                            points.Add(new System.Windows.Point(x, y));

                            theta += (toothDeltaTheta / 2);

                            x = (outterRadius + teethBaseHeight + teethTopHeight) * Math.Cos(theta);
                            y = (outterRadius + teethBaseHeight + teethTopHeight) * Math.Sin(theta);
                            points.Add(new System.Windows.Point(x, y));

                            theta += (toothDeltaTheta / 2);
                            x = (outterRadius + teethBaseHeight) * Math.Cos(theta);
                            y = (outterRadius + teethBaseHeight) * Math.Sin(theta);
                            points.Add(new System.Windows.Point(x, y));

                            x = outterRadius * Math.Cos(theta);
                            y = outterRadius * Math.Sin(theta);
                            points.Add(new System.Windows.Point(x, y));
                        }
                        else
                        {
                            // Add a gap
                            theta += gapDeltaTheta;
                            if (theta < Math.PI * 2)
                            {
                                double x = outterRadius * Math.Cos(theta);
                                double y = outterRadius * Math.Sin(theta);
                                points.Add(new System.Windows.Point(x, y));
                            }
                        }
                        tooth = !tooth;
                    }
                    DisplayFlatView(points);
                    Vertices.Clear();
                    Faces.Clear();

                    // generate side triangles so original points are already in list
                    for (int i = 0; i < points.Count; i++)
                    {
                        CreateSideFace(i);
                    }
                    // triangulate the basic polygon
                    TriangulationPolygon ply = new TriangulationPolygon();
                    List<PointF> pf = new List<PointF>();
                    foreach (System.Windows.Point p in points)
                    {
                        pf.Add(new PointF((float)p.X, (float)p.Y));
                    }
                    ply.Points = pf.ToArray();
                    List<Triangle> tris = ply.Triangulate();
                    foreach (Triangle t in tris)
                    {
                        int c0 = AddVertice(t.Points[0].X, t.Points[0].Y, 0.0);
                        int c1 = AddVertice(t.Points[1].X, t.Points[1].Y, 0.0);
                        int c2 = AddVertice(t.Points[2].X, t.Points[2].Y, 0.0);
                        Faces.Add(c0);
                        Faces.Add(c2);
                        Faces.Add(c1);

                        c0 = AddVertice(t.Points[0].X, t.Points[0].Y, 1);
                        c1 = AddVertice(t.Points[1].X, t.Points[1].Y, 1);
                        c2 = AddVertice(t.Points[2].X, t.Points[2].Y, 1);
                        Faces.Add(c0);
                        Faces.Add(c1);
                        Faces.Add(c2);
                    }
                    GeometryModel3D gm = GetModel();
                    MyModelGroup.Children.Clear();
                    MyModelGroup.Children.Add(gm);
                }

            }


        }

        private void CreateSideFace(int i)
        {
            int v = i + 1;
            if (v == points.Count)
            {
                v = 0;
            }

            int c0 = AddVertice(points[i].X, points[i].Y, 0.0);
            int c1 = AddVertice(points[i].X, points[i].Y, 1);
            int c2 = AddVertice(points[v].X, points[v].Y, 1);
            int c3 = AddVertice(points[v].X, points[v].Y, 0.0);
            Faces.Add(c0);
            Faces.Add(c2);
            Faces.Add(c1);

            Faces.Add(c0);
            Faces.Add(c3);
            Faces.Add(c2);
        }
        private void DisplayFlatView(List<System.Windows.Point> pnts)
        {
            FlatView.Children.Clear();
            double cx = FlatView.ActualWidth / 2;
            double cy = FlatView.ActualHeight / 2;
            Polyline pl = new Polyline();
            pl.Stroke = System.Windows.Media.Brushes.Blue;
            pl.StrokeThickness = 1;
            List<System.Windows.Point> canvasPnts = new List<System.Windows.Point>();
            foreach (System.Windows.Point p in pnts)
            {
                System.Windows.Point np = new System.Windows.Point(cx + p.X, cy + p.Y);
                pl.Points.Add(np);
            }
            FlatView.Children.Add(pl);
        }
    }
}
