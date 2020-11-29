using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Make3D.Dialogs
{
    /// <summary>
    /// Interaction logic for ShapeLoftDialog.xaml
    /// </summary>
    public partial class ShapeLoftDialog : BaseModellerDialog
    {
        public int degreeStep = 1;
        private List<Point> bottomPoints;
        private List<Point> bottomShell;

        private double sizeX;
        private double sizeY;
        private double sizeZ;
        private List<Point> topPoints;
        private List<Point> topShell;

        public ShapeLoftDialog()
        {
            InitializeComponent();
            topPoints = null;
            bottomPoints = null;
            bottomShell = new List<Point>();
            topShell = new List<Point>();
            sizeX = 10;
            sizeY = 10;
            sizeZ = 10;
            DataContext = this;
            EditorParameters.ToolName = "TwoShape";
        }

        private void OnBottomPointsChanged(List<Point> pnts)
        {
            bottomPoints = pnts;
            Redraw();
        }

        private void OnTopPointsChanged(List<Point> pnts)
        {
            topPoints = pnts;
            Redraw();
        }

        private void Redraw()
        {
            if (topPoints != null && bottomPoints != null)
            {
                bottomShell.Clear(); ;
                topShell.Clear();
                double dTheta = (degreeStep / 360.0) * Math.PI * 2;
                double dBottom = (Math.PI * 2) / (bottomPoints.Count);
                double dTop = (Math.PI * 2) / (topPoints.Count);
                for (double theta = 0; theta < Math.PI * 2; theta += dTheta)
                {
                    Point st = new Point(0, 0);
                    Point nd = new Point(0, 1); ;
                    int bottomIndex = (int)(theta / dBottom);
                    if (bottomIndex < bottomPoints.Count - 1)
                    {
                        st = bottomPoints[bottomIndex];
                        nd = bottomPoints[bottomIndex + 1];
                    }
                    else
                    {
                        st = bottomPoints[bottomIndex];
                        nd = bottomPoints[0];
                    }

                    double res = theta - (bottomIndex * dBottom);
                    double t = 0;
                    if (res > 0)
                    {
                        t = res / dBottom;
                    }
                    if (t < 0 || t > 1)
                    {
                    }

                    double px = st.X + (t * (nd.X - st.X));
                    double py = st.Y + (t * (nd.Y - st.Y));
                    bottomShell.Add(new Point(px, py));

                    int topIndex = (int)(theta / dTop);
                    if (topIndex < topPoints.Count - 1)
                    {
                        st = topPoints[topIndex];
                        nd = topPoints[topIndex + 1];
                    }
                    else
                    {
                        st = topPoints[topIndex];
                        nd = topPoints[0];
                    }

                    res = theta - (topIndex * dTop);
                    t = 0;
                    if (res > 0)
                    {
                        t = res / dTop;
                    }
                    if (t < 0 || t > 1)
                    {
                    }
                    px = st.X + (t * (nd.X - st.X));
                    py = st.Y + (t * (nd.Y - st.Y));
                    topShell.Add(new Point(px, py));
                }
                Vertices.Clear();

                int numUp = 10;
                Vertices.Add(new Point3D(0, 0, 0));
                for (int i = 0; i < bottomShell.Count; i++)
                {
                    double dt = 1.0 / numUp;

                    for (int j = 0; j <= numUp; j++)
                    {
                        double t = j * dt;

                        double lx1 = bottomShell[i].X + (t * (topShell[i].X - bottomShell[i].X));
                        double ly1 = bottomShell[i].Y + (t * (topShell[i].Y - bottomShell[i].Y));

                        Point3D v = new Point3D(lx1 * sizeX / 2, t * sizeY / 2, ly1 * sizeZ / 2);
                        Vertices.Add(v);
                    }
                }
                Vertices.Add(new Point3D(0, 1 * sizeY / 2, 0));
                int topPoint = Vertices.Count - 1;
                Faces.Clear();
                int offset = numUp + 1;
                int f = 1;
                for (int i = 0; i < bottomShell.Count - 1; i++)
                {
                    f = i * offset + 1;
                    Faces.Add(0);
                    Faces.Add(f);
                    Faces.Add(f + offset);

                    for (int j = 0; j < numUp; j++)
                    {
                        Faces.Add(f);
                        Faces.Add(f + offset + 1);
                        Faces.Add(f + offset);

                        Faces.Add(f);

                        Faces.Add(f + 1);
                        Faces.Add(f + offset + 1);
                        f++;
                    }
                    Faces.Add(f);
                    Faces.Add(topPoint);
                    Faces.Add(f + offset);
                }

                GeometryModel3D gm = GetModel();
                MyModelGroup.Children.Clear();
                MyModelGroup.Children.Add(gm);
            }
        }

        private void SizeX_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                sizeX = Convert.ToDouble(SizeX.Text);
                Redraw();
            }
            catch (Exception)
            {
            }
        }

        private void SizeY_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                sizeY = Convert.ToDouble(SizeY.Text);
                Redraw();
            }
            catch (Exception)
            {
            }
        }

        private void SizeZ_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                sizeZ = Convert.ToDouble(SizeZ.Text);
                Redraw();
            }
            catch (Exception)
            {
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TopShape.Header = "Top Shape";
            TopShape.OnPointsChanged += OnTopPointsChanged;

            BottomShape.Header = "Bottom Shape";
            BottomShape.OnPointsChanged += OnBottomPointsChanged;

            String s = EditorParameters.Get("TopNumberOfPoints");
            if (s != "")
            {
                int v = Convert.ToInt16(s);

                s = EditorParameters.Get("TopDistances");
                List<double> dists = GetDoubles(v, s);
                s = EditorParameters.Get("TopRotation");
                double r = Convert.ToDouble(s);
                TopShape.SetPoints(v, dists, r);

                s = EditorParameters.Get("BottomNumberOfPoints");
                v = Convert.ToInt16(s);
                s = EditorParameters.Get("BottomDistances");
                dists = GetDoubles(v, s);
                s = EditorParameters.Get("BottomRotation");
                r = Convert.ToDouble(s);
                BottomShape.SetPoints(v, dists, r);

                s = EditorParameters.Get("ScaleX");
                if (s != "")
                {
                    sizeX = Convert.ToDouble(s);
                    sizeX = Math.Round(sizeX * 100) / 100;
                    s = EditorParameters.Get("ScaleY");
                    sizeY = Convert.ToDouble(s);
                    s = EditorParameters.Get("ScaleZ");
                    sizeZ = Convert.ToDouble(s);
                }
            }

            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            SizeX.Text = sizeX.ToString();
            SizeY.Text = sizeY.ToString();
            SizeZ.Text = sizeZ.ToString();
        }

        private List<double> GetDoubles(int v, string s)
        {
            List<double> res = new List<double>();
            string[] words = s.Split(',');
            for (int i = 0; i < v && i < words.GetLength(0); i++)
            {
                double d = Convert.ToDouble(words[i]);
                res.Add(d);
            }
            return res;
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            EditorParameters.Set("TopNumberOfPoints", TopShape.NumberOfPoints.ToString());
            EditorParameters.Set("TopDistances", TopShape.GetDistances());
            EditorParameters.Set("TopRotation", TopShape.RotationDegrees.ToString());

            EditorParameters.Set("BottomNumberOfPoints", BottomShape.NumberOfPoints.ToString());
            EditorParameters.Set("BottomDistances", BottomShape.GetDistances());
            EditorParameters.Set("BottomRotation", BottomShape.RotationDegrees.ToString());

            EditorParameters.Set("ScaleX", sizeX.ToString());
            EditorParameters.Set("ScaleY", sizeY.ToString());
            EditorParameters.Set("ScaleZ", sizeZ.ToString());
            DialogResult = true;
            Close();
        }
    }
}