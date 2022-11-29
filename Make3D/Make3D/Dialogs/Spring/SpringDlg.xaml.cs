using MakerLib;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for Spring.xaml
    /// </summary>
    public partial class SpringDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private string warningText;
        private bool loaded;

        private double innerRadius;

        public double InnerRadius
        {
            get
            {
                return innerRadius;
            }
            set
            {
                if (innerRadius != value)
                {
                    if (value >= 0 && value <= 100)
                    {
                        innerRadius = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        private double wireRadius;

        public double WireRadius
        {
            get
            {
                return wireRadius;
            }
            set
            {
                if (wireRadius != value)
                {
                    if (value >= 0.1 && value <= 100)
                    {
                        wireRadius = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        private double coilGap;

        public double CoilGap
        {
            get
            {
                return coilGap;
            }
            set
            {
                if (coilGap != value)
                {
                    if (value >= 0 && value <= 100)
                    {
                        coilGap = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        private double turns;

        public double Turns
        {
            get
            {
                return turns;
            }
            set
            {
                if (turns != value)
                {
                    if (value >= 1 && value <= 100)
                    {
                        turns = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        private double facesPerTurn;

        public double FacesPerTurn
        {
            get
            {
                return facesPerTurn;
            }
            set
            {
                if (facesPerTurn != value)
                {
                    if (value >= 10 && value <= 360)
                    {
                        facesPerTurn = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        private double wireFacets;

        public double WireFacets
        {
            get
            {
                return wireFacets;
            }
            set
            {
                if (wireFacets != value)
                {
                    if (value >= 10 && value <= 360)
                    {
                        wireFacets = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public SpringDlg()
        {
            InitializeComponent();
            ToolName = "Spring";
            DataContext = this;
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
            SpringMaker maker = new SpringMaker(innerRadius, wireRadius, coilGap, turns, facesPerTurn, wireFacets);
            maker.Generate(Vertices, Faces);
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters

            InnerRadius = EditorParameters.GetDouble("InnerRadius", 5);

            WireRadius = EditorParameters.GetDouble("WireRadius", 2);

            CoilGap = EditorParameters.GetDouble("CoilGap", 4);

            Turns = EditorParameters.GetDouble("Turns", 5);

            FacesPerTurn = EditorParameters.GetDouble("FacesPerTurn", 20);

            WireFacets = EditorParameters.GetDouble("WireFacets", 20);
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("InnerRadius", InnerRadius.ToString());
            EditorParameters.Set("WireRadius", WireRadius.ToString());
            EditorParameters.Set("CoilGap", CoilGap.ToString());
            EditorParameters.Set("Turns", Turns.ToString());
            EditorParameters.Set("FacesPerTurn", FacesPerTurn.ToString());
            EditorParameters.Set("WireFacets", WireFacets.ToString());
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