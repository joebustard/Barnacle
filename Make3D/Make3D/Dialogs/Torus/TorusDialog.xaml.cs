using System;
using System.Collections.Generic;
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
    /// Interaction logic for TorusDialog.xaml
    /// </summary>
    public partial class TorusDialog : BaseModellerDialog
    {
        private int curveType = 1;
        private double dPhi;
        private double dTheta;
        private double horizontalRadius;
        private int knobFactor = 2;
        private double mainRadius;
        private double power = 1;
        private double stretch;
        private double verticalRadius;

        public TorusDialog()
        {
            InitializeComponent();
            DataContext = this;
            mainRadius = 5;
            horizontalRadius = 4;
            verticalRadius = 4;
            dTheta = 0.1;
            dPhi = 0.2;
            stretch = 4;
            ToolName = "Torus";
            ModelGroup = MyModelGroup;
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
            EditorParameters.Set("MainRadius", mainRadius.ToString());
            EditorParameters.Set("HorizontalRadius", horizontalRadius.ToString());
            EditorParameters.Set("VerticalRadius", verticalRadius.ToString());
            EditorParameters.Set("Stretch", stretch.ToString());
            EditorParameters.Set("Curve", curveType.ToString());
            EditorParameters.Set("Knobs", knobFactor.ToString());
            DialogResult = true;
            Close();
        }

        private void GeneratePoints()
        {
            Vertices.Clear();
            Faces.Clear();
            int mainSteps = (int)((Math.PI * 2.0) / dTheta);
            int subSteps = (int)((Math.PI * 2.0) / dPhi);
            if (TypeAButton.IsChecked == true)
            {
                power = 1;
            }
            if (TypeBButton.IsChecked == true)
            {
                power = 3;
            }
            if (TypeCButton.IsChecked == true)
            {
                power = 5;
            }
            double theta1;
            double theta2;
            for (int i = 0; i < mainSteps; i++)
            {
                int j = i + 1;
                if (j >= mainSteps)
                {
                    j = 0;
                }
                theta1 = (double)i * dTheta;
                theta2 = (double)j * dTheta;

                double phi1;
                double phi2;

                for (int l = 0; l < subSteps; l++)
                {
                    int k = l + 1;
                    if (k >= subSteps)
                    {
                        k = 0;
                    }
                    phi1 = l * dPhi;
                    phi2 = k * dPhi;
                    Point3D p1 = TorusPoint(theta1, phi1);
                    Point3D p2 = TorusPoint(theta1, phi2);
                    Point3D p3 = TorusPoint(theta2, phi2);
                    Point3D p4 = TorusPoint(theta2, phi1);
                    int v1 = AddVertice(p1);
                    int v2 = AddVertice(p2);
                    int v3 = AddVertice(p3);
                    int v4 = AddVertice(p4);

                    Faces.Add(v1);
                    Faces.Add(v2);
                    Faces.Add(v3);

                    Faces.Add(v1);
                    Faces.Add(v3);
                    Faces.Add(v4);
                }
            }
        }

        private void HorizontalRadiusBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextValue(sender, ref horizontalRadius, 1, 100);
            UpdateDisplay();
        }

        private void HorizontalRadiusMinus_Click(object sender, RoutedEventArgs e)
        {
            if (horizontalRadius > 0)
            {
                horizontalRadius -= 1;
                UpdateDisplay();
                HorizontalRadiusBox.Text = horizontalRadius.ToString();
            }
        }

        private void HorizontalRadiusPlus_Click(object sender, RoutedEventArgs e)
        {
            if (horizontalRadius < 100)
            {
                horizontalRadius += 1;
                UpdateDisplay();
                HorizontalRadiusBox.Text = horizontalRadius.ToString();
            }
        }

        private void LumpSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            knobFactor = (int)LumpSlider.Value;
            UpdateDisplay();
        }

        private void MainRadiusBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextValue(sender, ref mainRadius, 1, 100);
            UpdateDisplay();
        }

        private void MainRadiusMinus_Click(object sender, RoutedEventArgs e)
        {
            if (mainRadius > 1)
            {
                mainRadius -= 1;
                UpdateDisplay();
                MainRadiusBox.Text = mainRadius.ToString();
            }
        }

        private void MainRadiusPlus_Click(object sender, RoutedEventArgs e)
        {
            if (mainRadius < 100)
            {
                mainRadius += 1;
                UpdateDisplay();
                MainRadiusBox.Text = mainRadius.ToString();
            }
        }

        private void StretchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextValue(sender, ref stretch, 1.0, 100);
            UpdateDisplay();
        }

        private void StretchMinus_Click(object sender, RoutedEventArgs e)
        {
            if (stretch > 1)
            {
                stretch -= 1;
                UpdateDisplay();
                StretchBox.Text = stretch.ToString();
            }
        }

        private void StretchPlus_Click(object sender, RoutedEventArgs e)
        {
            if (stretch < 100)
            {
                stretch += 1;
                UpdateDisplay();
                StretchBox.Text = stretch.ToString();
            }
        }

        private void TextValue(object sender, ref double d, double low, double high)
        {
            TextBox b = sender as TextBox;
            if (b != null)
            {
                try
                {
                    double v = Convert.ToDouble(b.Text);
                    if (v >= low && v <= high)
                    {
                        d = v;
                    }
                }
                catch
                {
                }
            }
        }

        private Point3D TorusPoint(double t, double p)
        {
            Point3D res = new Point3D(0, 0, 0);
            if (curveType >= 1 && curveType < 4)
            {
                res = TorusPoint2(t, p);
            }
            else
            {
                res = TorusPoint3(t, p);
            }

            return res;
        }

        private Point3D TorusPoint2(double t, double p)
        {
            double x = (mainRadius + horizontalRadius * (Math.Pow(Math.Cos(p), power))) * Math.Cos(t);
            double z = (mainRadius + verticalRadius * (Math.Pow(Math.Cos(p), power))) * Math.Sin(t);
            double y = stretch * Math.Sin(p);
            Point3D res = new Point3D(x, y, z);
            return res;
        }

        private Point3D TorusPoint3(double t, double p)
        {
            double x = (mainRadius + horizontalRadius * (Math.Cos(p) * Math.Abs(Math.Sin(knobFactor * t)))) * Math.Cos(t);
            double z = (mainRadius + verticalRadius * (Math.Cos(p) * Math.Abs(Math.Sin(knobFactor * t)))) * Math.Sin(t);
            double y = stretch * Math.Sin(p);
            Point3D res = new Point3D(x, y, z);
            return res;
        }

        private void TypeAButton_Checked(object sender, RoutedEventArgs e)
        {
            if (LumpsLabel != null)
            {
                LumpsLabel.Visibility = Visibility.Hidden;
                LumpSlider.Visibility = Visibility.Hidden;
                curveType = 1;
                UpdateDisplay();
            }
        }

        private void TypeBButton_Checked(object sender, RoutedEventArgs e)
        {
            LumpsLabel.Visibility = Visibility.Hidden;
            LumpSlider.Visibility = Visibility.Hidden;
            curveType = 2;
            UpdateDisplay();
        }

        private void TypeCButton_Checked(object sender, RoutedEventArgs e)
        {
            LumpsLabel.Visibility = Visibility.Hidden;
            LumpSlider.Visibility = Visibility.Hidden;
            curveType = 3;
            UpdateDisplay();
        }

        private void TypeDButton_Checked(object sender, RoutedEventArgs e)
        {
            curveType = 4;
            UpdateDisplay();
            LumpsLabel.Visibility = Visibility.Visible;
            LumpSlider.Visibility = Visibility.Visible;
        }

        private void UpdateDisplay()
        {
            GeneratePoints();
            CentreVertices();
            Redisplay();
        }

        private void VerticalRadiusBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextValue(sender, ref verticalRadius, 1, 100);
            UpdateDisplay();
        }

        private void VerticalRadiusMinus_Click(object sender, RoutedEventArgs e)
        {
            if (verticalRadius > 0)
            {
                verticalRadius -= 1;
                UpdateDisplay();
                VerticalRadiusBox.Text = verticalRadius.ToString();
            }
        }

        private void VerticalRadiusPlus_Click(object sender, RoutedEventArgs e)
        {
            if (verticalRadius < 100)
            {
                verticalRadius += 1;
                UpdateDisplay();
                VerticalRadiusBox.Text = verticalRadius.ToString();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            String s = EditorParameters.Get("MainRadius");
            if (s != "")
            {
                mainRadius = Convert.ToDouble(s);
            }

            s = EditorParameters.Get("HorizontalRadius");
            if (s != "")
            {
                horizontalRadius = Convert.ToDouble(s);
            }

            s = EditorParameters.Get("VerticalRadius");
            if (s != "")
            {
                verticalRadius = Convert.ToDouble(s);
            }
            s = EditorParameters.Get("Stretch");
            if (s != "")
            {
                stretch = Convert.ToDouble(s);
            }
            s = EditorParameters.Get("Curve");
            if (s != "")
            {
                curveType = Convert.ToInt16(s);
            }
            s = EditorParameters.Get("Knobs");
            if (s != "")
            {
                knobFactor = Convert.ToInt16(s);
            }
            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            MainRadiusBox.Text = mainRadius.ToString();
            HorizontalRadiusBox.Text = horizontalRadius.ToString();
            VerticalRadiusBox.Text = verticalRadius.ToString();
            StretchBox.Text = stretch.ToString();
            LumpSlider.Value = knobFactor;
            UpdateDisplay();
            switch (curveType)
            {
                case 1:
                    {
                        TypeAButton.IsChecked = true;
                    }
                    break;

                case 2:
                    {
                        TypeBButton.IsChecked = true;
                    }
                    break;

                case 3:
                    {
                        TypeCButton.IsChecked = true;
                    }
                    break;

                case 4:
                    {
                        TypeDButton.IsChecked = true;
                    }
                    break;
            }
        }
    }
}