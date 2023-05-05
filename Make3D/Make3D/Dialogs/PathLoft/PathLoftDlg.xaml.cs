using MakerLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for PathLoft.xaml
    /// </summary>
    public partial class PathLoftDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private string warningText;
        private bool loaded;

        private const double minloftHeight = 0.1;
        private const double maxloftHeight = 200;
        private double loftHeight;

        public double LoftHeight
        {
            get
            {
                return loftHeight;
            }
            set
            {
                if (loftHeight != value)
                {
                    if (value >= minloftHeight && value <= maxloftHeight)
                    {
                        loftHeight = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String LoftHeightToolTip
        {
            get
            {
                return $"LoftHeight must be in the range {minloftHeight} to {maxloftHeight}";
            }
        }

        private const double minloftThickness = 0.1;
        private const double maxloftThickness = 200;
        private double loftThickness;

        private List<Point> pathPoints;

        public double LoftThickness

        {
            get
            {
                return loftThickness;
            }
            set
            {
                if (loftThickness != value)
                {
                    if (value >= minloftThickness && value <= maxloftThickness)
                    {
                        loftThickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String LoftThicknessToolTip
        {
            get
            {
                return $"LoftThickness must be in the range {minloftThickness} to {maxloftThickness}";
            }
        }

        public PathLoftDlg()
        {
            InitializeComponent();
            ToolName = "PathLoft";
            DataContext = this;
            ModelGroup = MyModelGroup;
            loaded = false;
            PathEditor.OnFlexiPathChanged += PathPointsChanged;
        }

        private double pathXSize;
        private double pathYSize;
        private double tlx = 0;
        private double tly = 0;
        private double brx = 0;
        private double bry = 0;
        private double xRes = 0.25;
        private double yRes = 0.25;

        private void PathPointsChanged(List<Point> points)
        {
            tlx = 0;
            tly = 0;
            brx = 0;
            bry = 0;
            Get2DBounds(points, ref tlx, ref tly, ref brx, ref bry);
            if (tlx < double.MaxValue)
            {
                double pathXSize = brx - tlx;
                pathYSize = bry - tly;

                double mx = tlx + pathXSize / 2.0;
                double my = tly + pathYSize / 2.0;
                pathPoints.Clear();
                foreach (Point p in points)
                {
                    pathPoints.Add(new Point((p.X - mx) / pathXSize, (p.Y - my) / pathYSize));
                }
                xRes = 0.25 / pathXSize;
                yRes = 0.25 / pathYSize;
                GenerateShape();
                Redisplay();
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

        public string WarningText
        {
            get
            {
                return warningText;
            }
            set
            {
                if (warningText != value)
                {
                    warningText = value;
                    NotifyPropertyChanged();
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
            // PathLoftMaker maker = new PathLoftMaker(loftHeight, loftThickness);
            //  maker.Generate(Vertices, Faces);
            DistanceCell2D cell = new DistanceCell2D();
            cell.SetPoint(DistanceCell2D.TopLeft, new Point(10, 100), 7);
            cell.SetPoint(DistanceCell2D.TopRight, new Point(100, 100), 8);
            cell.SetPoint(DistanceCell2D.BottomLeft, new Point(10, 10), 5);
            cell.SetPoint(DistanceCell2D.BottomRight, new Point(100, 10), 6);
            cell.SetCentre(9);
            float[] testVals =
            {
                11.0F,
                12.0F,
                13.0F,
                14.0F
            };
            cell.CreateSubCells(testVals);
            cell.Dump();
            CentreVertices();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters

            LoftHeight = EditorParameters.GetDouble("LoftHeight", 1);
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("LoftHeight", LoftHeight.ToString());
        }

        private void UpdateDisplay()
        {
            if (loaded)
            {
                GenerateShape();
                Redisplay();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WarningText = "";
            LoadEditorParameters();

            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            loaded = true;

            UpdateDisplay();
        }
    }
}