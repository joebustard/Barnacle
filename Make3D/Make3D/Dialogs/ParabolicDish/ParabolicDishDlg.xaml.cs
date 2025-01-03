using MakerLib;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for ParabolicDish.xaml
    /// </summary>
    public partial class ParabolicDishDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private string warningText;
        private bool loaded;



        private double radius;
        public double Radius
        {
            get
            {
                return radius;
            }
            set
            {
                if (radius != value)
                {
                    if (value >= 1 && value <= 100)
                    {
                        radius = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }



        private double wallThickness;
        public double WallThickness
        {
            get
            {
                return wallThickness;
            }
            set
            {
                if (wallThickness != value)
                {
                    if (value >= 0.5 && value <= 99.5)
                    {
                        wallThickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }



        private int pitch;
        public int Pitch
        {
            get
            {
                return pitch;
            }
            set
            {
                if (pitch != value)
                {
                    if (value >= 1 && value <= 20)
                    {
                        pitch = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }



        public ParabolicDishDlg()
        {
            InitializeComponent();
            ToolName = "ParabolicDish";
            DataContext = this;
            pitch = 5;
            radius = 10;
            wallThickness = 1; 
            ModelGroup = MyModelGroup;
            loaded = false;
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
            ParabolicDishMaker maker = new ParabolicDishMaker(radius, wallThickness, pitch);
            maker.Generate(Vertices, Faces);
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters

            if (EditorParameters.Get("Radius") != "")
            {
                Radius = EditorParameters.GetDouble("Radius");
            }

            if (EditorParameters.Get("WallThickness") != "")
            {
                WallThickness = EditorParameters.GetDouble("WallThickness");
            }

            if (EditorParameters.Get("Pitch") != "")
            {
                Pitch = EditorParameters.GetInt("Pitch");
            }

        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("Radius", Radius.ToString());
            EditorParameters.Set("WallThickness", WallThickness.ToString());
            EditorParameters.Set("Pitch", Pitch.ToString());
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
            MeshColour = System.Windows.Media.Colors.Teal;
            LoadEditorParameters();

            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            loaded = true;
            UpdateDisplay();
        }
    }
}
