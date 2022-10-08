using Barnacle.EditorParameterLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for Barnacle.DialogsDialog.xaml
    /// </summary>
    public partial class LinearLoftDialog : BaseModellerDialog, INotifyPropertyChanged

    {
        private List<Point> line;

        private int numDivisions = 10;

        private int selectedPoint;
        private bool snap;
        private double gridX;
        private double gridY;

        public LinearLoftDialog()
        {
            InitializeComponent();

            EditorParameters = new EditorParameters();
            EditorParameters.ToolName = "LinearLoft";
            selectedPoint = -1;
            DataContext = this;
            Camera.Distance = 2.0 * Camera.Distance;
            ModelGroup = MyModelGroup;
            snap = false;
            gridX = -1;
            gridY = -1;
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
                    el.MouseUp += LineCanvas_MouseUp;
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
            EditorParameters.Set("Snap", snap.ToString());
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
        }

        private void LineCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (selectedPoint != -1 && e.LeftButton == MouseButtonState.Pressed)
            {
                System.Windows.Point position = e.GetPosition(LineCanvas);
                double px = position.X / LineCanvas.ActualWidth;
                if (snap)
                {
                    int ioff = (int)(px * 20);
                    px = (double)ioff / 20.0;
                }
                double py = line[selectedPoint].Y;
                line[selectedPoint] = new Point(px, py);

                UpdateLine();
            }
            else
            {
                selectedPoint = -1;
            }
            //UpdateDisplay();
        }

        private void LineCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            UpdateDisplay();
        }

        private void RedrawLine()
        {
            LineCanvas.Children.Clear();
            gridX = LineCanvas.ActualWidth / 20.0;
            gridY = LineCanvas.ActualHeight / 20.0;
            for (int i = 0; i < 20; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    Ellipse el = new Ellipse();
                    Canvas.SetLeft(el, i * gridX - 3);
                    Canvas.SetTop(el, (j + 1) * gridY - 3);
                    el.Width = 6;
                    el.Height = 6;
                    el.Fill = Brushes.AliceBlue;
                    el.Stroke = Brushes.CadetBlue;
                    LineCanvas.Children.Add(el);
                }
            }
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
                Redisplay();
            }
        }

        private void UpdateDisplay()
        {
            UpdateLine();
            GenerateShape();
            RedrawShape();
        }

        private void UpdateLine()
        {
            RedrawLine();
            DisplayPoints();
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
                s = EditorParameters.Get("Snap");
                if (s.ToLower() == "true")
                {
                    snap = true;
                    SnapBox.IsChecked = true;
                }
            }
            else
            {
                double divs = 20;
                double ds = 1 / divs;
                for (double d = 0; d <= 1.0; d += ds)
                {
                    line.Add(new Point(0.5, d));
                }
            }
            UpdateCameraPos();
            UpdateDisplay();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            snap = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            snap = false;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateLine();
        }
    }
}