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
    public partial class Wheel : BaseModellerDialog, INotifyPropertyChanged
    {
        private double axelBore;
        private double hubInner;
        private double hubOutter;
        private List<String> hubStyles;
        private double hubThickness;
        private List<String> rimStyles;
        private string selectedHubStyle;
        private double tyreDepth;

        public Wheel()
        {
            InitializeComponent();
            ToolName = "Wheel";
            DataContext = this;
            tyreDepth = 50;
            hubInner = 50;
            hubOutter = 100;
            axelBore = 10;
            hubThickness = 20;
            hubStyles = new List<string>();
            rimStyles = new List<string>();
        }

        public double AxelBore
        {
            get
            {
                return axelBore;
            }
            set
            {
                if (axelBore != value)
                {
                    axelBore = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double HubInner
        {
            get
            {
                return hubInner;
            }
            set
            {
                if (hubInner != value)
                {
                    hubInner = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double HubOutter
        {
            get
            {
                return hubOutter;
            }
            set
            {
                if (hubOutter != value)
                {
                    hubOutter = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public List<String> HubStyles
        {
            get
            {
                return hubStyles;
            }
            set
            {
                if (hubStyles != value)
                {
                    hubStyles = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public List<String> RimStyles
        {
            get
            {
                return rimStyles;
            }
            set
            {
                rimStyles = value;
                NotifyPropertyChanged();
                UpdateDisplay();
            }
        }

        public string SelectedHubStyle
        {
            get
            {
                return selectedHubStyle;
            }
            set
            {
                if (selectedHubStyle != value)
                {
                    selectedHubStyle = value;
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

        public double TyreDepth
        {
            get
            {
                return tyreDepth;
            }
            set
            {
                if (tyreDepth != value)
                {
                    tyreDepth = value;
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

        private void CreateSideFace(List<Point> points, int i, double thickness, bool rev)
        {
            int v = i + 1;
            if (v == points.Count)
            {
                v = 0;
            }

            int c0 = AddVertice(points[i].X, points[i].Y, 0.0);
            int c1 = AddVertice(points[i].X, points[i].Y, thickness);
            int c2 = AddVertice(points[v].X, points[v].Y, thickness);
            int c3 = AddVertice(points[v].X, points[v].Y, 0.0);
            if (rev)
            {
                Faces.Add(c0);
                Faces.Add(c2);
                Faces.Add(c1);

                Faces.Add(c0);
                Faces.Add(c3);
                Faces.Add(c2);
            }
            else
            {
                Faces.Add(c0);
                Faces.Add(c1);
                Faces.Add(c2);

                Faces.Add(c0);
                Faces.Add(c2);
                Faces.Add(c3);
            }
        }

        private void GenerateShape()
        {
            ClearShape();
            List<Point> points = new List<Point>();
            double numSpoke = 0;
            double gapDTheta = 0;
            double spokeDTheta = 0;
            double spokeTipDTheta = 0;
            double numSubs = 10;
            GetHubParams(ref numSpoke, ref gapDTheta, ref spokeDTheta, ref spokeTipDTheta);
            double actualInner = hubInner;
            if (axelBore > actualInner)
            {
                actualInner = axelBore + 1;
            }
            double actualOutter = hubOutter;
            if (actualOutter <= actualInner)
            {
                actualOutter = actualInner + 1;
            }

            double theta = 0;
            bool inSpoke = true;
            while (theta <= Math.PI * 2)
            {
                if (inSpoke)
                {
                    // create spoke points
                    theta += spokeTipDTheta;
                    // creat gap points
                    // Add a gap
                    double dt = spokeDTheta / numSubs;
                    for (int i = 0; i < numSubs; i++)
                    {
                        theta += dt;
                        if (theta < Math.PI * 2)
                        {
                            double x = actualOutter * Math.Cos(theta);
                            double y = actualOutter * Math.Sin(theta);
                            points.Add(new System.Windows.Point(x, y));
                        }
                    }
                }
                else
                {
                    theta += spokeTipDTheta;
                    // creat gap points
                    // Add a gap
                    double dt = gapDTheta / numSubs;
                    for (int i = 0; i < numSubs; i++)
                    {
                        theta += dt;
                        if (theta < Math.PI * 2)
                        {
                            double x = actualInner * Math.Cos(theta);
                            double y = actualInner * Math.Sin(theta);
                            points.Add(new System.Windows.Point(x, y));
                        }
                    }
                }
                inSpoke = !inSpoke;
            }
            // generate side triangles so original points are already in list
            for (int i = 0; i < points.Count; i++)
            {
                CreateSideFace(points, i, hubThickness, true);
            }
        }

        private void GetHubParams(ref double numSpoke, ref double spokeGapDTheta, ref double spokeDTheta, ref double spokeTipDTheta)
        {
            Double twop = Math.PI * 2.0;
            switch (selectedHubStyle)
            {
                case "1":
                    {
                        numSpoke = 8;
                        spokeGapDTheta = twop / (numSpoke * 2.0);
                        spokeDTheta = spokeGapDTheta;
                        spokeTipDTheta = 0;
                    }
                    break;

                case "2":
                    {
                        numSpoke = 8;
                        spokeGapDTheta = twop / ((numSpoke + 1) * 2.0);
                        spokeDTheta = spokeGapDTheta / 2;
                        spokeTipDTheta = spokeDTheta;
                    }
                    break;

                case "3":
                    {
                        numSpoke = 12;
                        spokeGapDTheta = twop / ((numSpoke + 1) * 2.0);
                        spokeDTheta = 0.9 * spokeGapDTheta;
                        spokeTipDTheta = (spokeGapDTheta - spokeDTheta) / 2.0;
                    }
                    break;
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

        private void UpdateDisplay()
        {
            GenerateShape();
            Redisplay();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            hubStyles.Add("1");
            hubStyles.Add("2");
            hubStyles.Add("3");
            selectedHubStyle = "1";

            rimStyles.Add("1");
            rimStyles.Add("2");
            rimStyles.Add("3");

            LoadEditorParameters();
            GenerateShape();
            UpdateCameraPos();
            MyModelGroup.Children.Clear();

            Redisplay();
        }
    }
}