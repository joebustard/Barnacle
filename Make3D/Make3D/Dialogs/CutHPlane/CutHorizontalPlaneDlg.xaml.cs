using Barnacle.Models;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for CutHorizontalPlane.xaml
    /// </summary>
    public partial class CutHorizontalPlaneDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private const double maxplaneLevel = 300;
        private const double minplaneLevel = -300;
        private bool loaded;
        private HorizontalPlane plane;
        private double planeLevel;
        private string warningText;

        public CutHorizontalPlaneDlg()
        {
            InitializeComponent();
            ToolName = "CutHorizontalPlane";
            DataContext = this;
            ModelGroup = MyModelGroup;
            loaded = false;
            planeLevel = 5;
            plane = new HorizontalPlane(planeLevel);
        }

        public double PlaneLevel
        {
            get
            {
                return planeLevel;
            }
            set
            {
                if (planeLevel != value)
                {
                    if (value >= minplaneLevel && value <= maxplaneLevel)
                    {
                        planeLevel = value;
                        if (plane != null)

                        {
                            plane.MoveTo(PlaneLevel);
                        }
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String PlaneLevelToolTip
        {
            get
            {
                return $"PlaneLevel must be in the range {minplaneLevel} to {maxplaneLevel}";
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

        protected override void Redisplay()
        {
            if (ModelGroup != null)
            {
                ModelGroup.Children.Clear();

                if (floor != null && ShowFloor)
                {
                    ModelGroup.Children.Add(floor.FloorMesh);
                    foreach (GeometryModel3D m in grid.Group.Children)
                    {
                        ModelGroup.Children.Add(m);
                    }
                }

                if (axies != null && ShowAxies)
                {
                    foreach (GeometryModel3D m in axies.Group.Children)
                    {
                        ModelGroup.Children.Add(m);
                    }
                }

                GeometryModel3D gm = GetModel();
                ModelGroup.Children.Add(gm);

                if (plane != null && plane.PlaneMesh != null)
                {
                    ModelGroup.Children.Add(plane.PlaneMesh);
                }
            }
        }

        private void GenerateShape()
        {
            ClearShape();
            //       CutHorizontalPlaneMaker maker = new CutHorizontalPlaneMaker(
            //           planeLevel
            //           );
            //       maker.Generate(Vertices, Faces);
            CentreVertices();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters

            PlaneLevel = EditorParameters.GetDouble("PlaneLevel", 0);
        }

        private void ResetDefaults(object sender, RoutedEventArgs e)
        {
            SetDefaults();
            UpdateDisplay();
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("PlaneLevel", PlaneLevel.ToString());
        }

        private void SetDefaults()
        {
            loaded = false;
            PlaneLevel = 0;

            loaded = true;
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