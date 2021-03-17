using Make3D.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace Make3D.Dialogs
{
    /// <summary>
    /// Interaction logic for Make3D.DialogsDialog.xaml
    /// </summary>
    public partial class LinearLoftDialog : BaseModellerDialog, INotifyPropertyChanged

    {
        private List<Point> line;
        private int numDivisions = 36;

        private int selectedPoint;

        public LinearLoftDialog()
        {
            InitializeComponent();

            EditorParameters = new EditorParameters();
            EditorParameters.ToolName = "LinearLoft";
            selectedPoint = -1;
            DataContext = this;
            Camera.Distance = 2.0 * Camera.Distance;
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            GeneratePointParams();
            DialogResult = true;
            Close();
        }

        private void DisplayPoints()
        {
            if (line != null)
            {
                double rad = 3;
                for (int i = 0; i < line.Count; i++)
                {
                    if (selectedPoint == i)
                    {
                        rad = 12;
                    }
                    else
                    {
                        rad = 6;
                    }
                    System.Windows.Point p = line[i];
                    double x = p.X * LineCanvas.ActualWidth;
                    double y = (1 - p.Y) * LineCanvas.ActualHeight;
                    Ellipse el = new Ellipse();

                    Canvas.SetLeft(el, x - rad);
                    Canvas.SetTop(el, y - rad);
                    el.Width = 2 * rad;
                    el.Height = 2 * rad;
                    el.Fill = System.Windows.Media.Brushes.Red;
                    el.MouseDown += LineCanvas_MouseDown;
                    el.MouseMove += LineCanvas_MouseMove;
                    LineCanvas.Children.Add(el);
                }
            }
        }

        private void GeneratePointParams()
        {
            String s = "";
            for (int i = 0; i < line.Count; i++)
            {
                s += line[i].X.ToString() + "," + line[i].Y.ToString();
                if (i < line.Count - 1)
                {
                    s += ",";
                }
            }
            EditorParameters.Set("Points", s);
            EditorParameters.Set("NumberOfSides", numDivisions.ToString());
        }

        private void GenerateShape()
        {
            if (line != null)
            {
                int numSpokes = numDivisions;
                double deltaTheta = 360.0 / numSpokes; // in degrees
                Point3D[,] spokeVertices = new Point3D[numSpokes, line.Count];
                for (int i = 0; i < line.Count; i++)
                {
                    spokeVertices[0, i] = new Point3D(line[i].X * 20, line[i].Y * 20, 0);
                }

                for (int i = 1; i < numSpokes; i++)
                {
                    double theta = i * deltaTheta;
                    double rad = Math.PI * theta / 180.0;
                    for (int j = 0; j < line.Count; j++)
                    {
                        double x = spokeVertices[0, j].X * Math.Cos(rad);
                        double z = spokeVertices[0, j].X * Math.Sin(rad);
                        spokeVertices[i, j] = new Point3D(x, spokeVertices[0, j].Y, z);
                    }
                }
                Vertices.Clear();
                Vertices.Add(new Point3D(0, 0, 0));
                for (int i = 0; i < numSpokes; i++)
                {
                    for (int j = 0; j < line.Count; j++)
                    {
                        Vertices.Add(spokeVertices[i, j]);
                    }
                }
                int topPoint = Vertices.Count;
                Vertices.Add(new Point3D(0, 20, 0));
                Faces.Clear();
                int spOff;
                int spOff2;
                for (int i = 0; i < numSpokes; i++)
                {
                    spOff = i * line.Count + 1;
                    spOff2 = (i + 1) * line.Count + 1;
                    if (i == numSpokes - 1)
                    {
                        spOff2 = 1;
                    }
                    // base
                    Faces.Add(0);
                    Faces.Add(spOff);
                    Faces.Add(spOff2);

                    for (int j = 0; j < line.Count - 1; j++)
                    {
                        Faces.Add(spOff + j);
                        Faces.Add(spOff2 + j + 1);
                        Faces.Add(spOff2 + j);

                        Faces.Add(spOff + j);
                        Faces.Add(spOff + j + 1);
                        Faces.Add(spOff2 + j + 1);
                    }

                    // Top
                    Faces.Add(spOff + line.Count - 1);
                    Faces.Add(topPoint);
                    Faces.Add(spOff2 + line.Count - 1);
                }

                spOff = (numSpokes - 1) * line.Count + 1;
                spOff2 = 1;
                // base
                Faces.Add(0);
                Faces.Add(spOff);
                Faces.Add(spOff2);
            }
        }

        private void HDivSlide_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            numDivisions = (int)e.NewValue;
            UpdateDisplay();
        }

        private void LineCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            selectedPoint = -1;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                double rad = 3;
                System.Windows.Point position = e.GetPosition(LineCanvas);
                for (int i = 0; i < line.Count; i++)
                {
                    System.Windows.Point p = line[i];
                    double x = p.X * LineCanvas.ActualWidth;
                    double y = (1 - p.Y) * LineCanvas.ActualHeight;
                    if (position.X >= x - rad && position.X <= x + rad)
                    {
                        if (position.Y >= y - rad && position.Y <= y + rad)
                        {
                            selectedPoint = i;

                            break;
                        }
                    }
                }
            }
            UpdateDisplay();
        }

        private void LineCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (selectedPoint != -1 && e.LeftButton == MouseButtonState.Pressed)
            {
                System.Windows.Point position = e.GetPosition(LineCanvas);

                line[selectedPoint] = new Point(position.X / LineCanvas.ActualWidth, line[selectedPoint].Y);
            }
            else
            {
                selectedPoint = -1;
            }
            UpdateDisplay();
        }

        private void LineCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void RedrawLine()
        {
            LineCanvas.Children.Clear();
            if (line != null)
            {
                double x1;
                double y1;
                double x2;
                double y2;
                for (int i = 0; i < line.Count - 1; i++)
                {
                    x1 = line[i].X * LineCanvas.ActualWidth;
                    y1 = (1 - line[i].Y) * LineCanvas.ActualHeight;

                    x2 = line[i + 1].X * LineCanvas.ActualWidth;
                    y2 = (1 - line[i + 1].Y) * LineCanvas.ActualHeight;
                    Line ln = new Line();
                    ln.X1 = x1;
                    ln.Y1 = y1;
                    ln.X2 = x2;
                    ln.Y2 = y2;
                    ln.Stroke = new SolidColorBrush(Colors.Black);
                    LineCanvas.Children.Add(ln);
                }
            }
        }

        private void RedrawShape()
        {
            if (line != null)
            {
                GeometryModel3D gm = new GeometryModel3D();
                gm = GetModel();
                MyModelGroup.Children.Clear();

                if (floor != null)
                {
                    MyModelGroup.Children.Add(floor.FloorMesh);
                    foreach (GeometryModel3D m in grid.Group.Children)
                    {
                        MyModelGroup.Children.Add(m);
                    }
                }
                MyModelGroup.Children.Add(gm);
            }
        }

        private void UpdateDisplay()
        {
            RedrawLine();
            DisplayPoints();
            GenerateShape();
            RedrawShape();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MyModelGroup.Children.Clear();
            line = new List<Point>();
            String s = EditorParameters.Get("Points");
            if (s != "")
            {
                string[] words = s.Split(',');

                for (int i = 0; i < words.GetLength(0); i += 2)
                {
                    line.Add(new System.Windows.Point(Convert.ToDouble(words[i]), Convert.ToDouble(words[i + 1])));
                }

                s = EditorParameters.Get("NumberOfSides");
                numDivisions = Convert.ToInt16(s);
                HDivSlide.Value = numDivisions;
            }
            else
            {
                line.Add(new Point(0.5, 0));
                line.Add(new Point(0.5, 0.1));
                line.Add(new Point(0.5, 0.2));
                line.Add(new Point(0.5, 0.3));
                line.Add(new Point(0.5, 0.4));
                line.Add(new Point(0.5, 0.5));
                line.Add(new Point(0.5, 0.6));
                line.Add(new Point(0.5, 0.7));
                line.Add(new Point(0.5, 0.8));
                line.Add(new Point(0.5, 0.9));
                line.Add(new Point(0.5, 1));
            }
            UpdateCameraPos();
            UpdateDisplay();
        }
    }
}