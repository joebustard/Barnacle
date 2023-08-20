using MakerLib;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for RectGrille.xaml
    /// </summary>
    public partial class RectGrilleDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private string warningText;
        private bool loaded;

        private const double mingrillLength = 5;
        private const double maxgrillLength = 200;
        private double grillLength;

        public double GrillLength
        {
            get
            {
                return grillLength;
            }
            set
            {
                if (grillLength != value)
                {
                    if (value >= mingrillLength && value <= maxgrillLength)
                    {
                        grillLength = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String GrillLengthToolTip
        {
            get
            {
                return $"Grille Length must be in the range {mingrillLength} to {maxgrillLength}";
            }
        }

        private const double mingrillHeight = 5;
        private const double maxgrillHeight = 200;
        private double grillHeight;

        public double GrillHeight
        {
            get
            {
                return grillHeight;
            }
            set
            {
                if (grillHeight != value)
                {
                    if (value >= mingrillHeight && value <= maxgrillHeight)
                    {
                        grillHeight = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String GrilleHeightToolTip
        {
            get
            {
                return $"Grille Height must be in the range {mingrillHeight} to {maxgrillHeight}";
            }
        }

        private const double mingrillWidth = 1;
        private const double maxgrillWidth = 200;
        private double grillWidth;

        public double GrillWidth
        {
            get
            {
                return grillWidth;
            }
            set
            {
                if (grillWidth != value)
                {
                    if (value >= mingrillWidth && value <= maxgrillWidth)
                    {
                        grillWidth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String GrillWidthToolTip
        {
            get
            {
                return $"Grille Width must be in the range {mingrillWidth} to {maxgrillWidth}";
            }
        }

        private bool makeEdge;

        public bool MakeEdge
        {
            get
            {
                return makeEdge;
            }
            set
            {
                if (makeEdge != value)
                {
                    makeEdge = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public String MakeEdgeToolTip
        {
            get
            {
                return $"If selected the outer frame is created.";
            }
        }

        private const double minedgeThickness = 1;
        private const double maxedgeThickness = 200;
        private double edgeThickness;

        public double EdgeThickness
        {
            get
            {
                return edgeThickness;
            }
            set
            {
                if (edgeThickness != value)
                {
                    if (value >= minedgeThickness && value <= maxedgeThickness)
                    {
                        edgeThickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String EdgeThicknessToolTip
        {
            get
            {
                return $"Edge Thickness must be in the range {minedgeThickness} to {maxedgeThickness}";
            }
        }

        private const double minverticalBars = 0;
        private const double maxverticalBars = 100;
        private double verticalBars;

        public double VerticalBars
        {
            get
            {
                return verticalBars;
            }
            set
            {
                if (verticalBars != value)
                {
                    if (value >= minverticalBars && value <= maxverticalBars)
                    {
                        verticalBars = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String VerticalBarsToolTip
        {
            get
            {
                return $"Vertical Bars must be in the range {minverticalBars} to {maxverticalBars}";
            }
        }

        private const double minverticalBarThickness = 1;
        private const double maxverticalBarThickness = 100;
        private double verticalBarThickness;

        public double VerticalBarThickness
        {
            get
            {
                return verticalBarThickness;
            }
            set
            {
                if (verticalBarThickness != value)
                {
                    if (value >= minverticalBarThickness && value <= maxverticalBarThickness)
                    {
                        verticalBarThickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String VerticalBarThicknessToolTip
        {
            get
            {
                return $"Vertical Bar Thickness must be in the range {minverticalBarThickness} to {maxverticalBarThickness}";
            }
        }

        private const double minhorizontalBars = 0;
        private const double maxhorizontalBars = 100;
        private double horizontalBars;

        public double HorizontalBars
        {
            get
            {
                return horizontalBars;
            }
            set
            {
                if (horizontalBars != value)
                {
                    if (value >= minhorizontalBars && value <= maxhorizontalBars)
                    {
                        horizontalBars = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String HorizontalBarsToolTip
        {
            get
            {
                return $"Horizontal Bars must be in the range {minhorizontalBars} to {maxhorizontalBars}";
            }
        }

        private const double minhorizontalBarThickness = 1;
        private const double maxhorizontalBarThickness = 100;
        private double horizontalBarThickness;

        public double HorizontalBarThickness
        {
            get
            {
                return horizontalBarThickness;
            }
            set
            {
                if (horizontalBarThickness != value)
                {
                    if (value >= minhorizontalBarThickness && value <= maxhorizontalBarThickness)
                    {
                        horizontalBarThickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String HorizontalBarThicknessToolTip
        {
            get
            {
                return $"Horizontal Bar Thickness must be in the range {minhorizontalBarThickness} to {maxhorizontalBarThickness}";
            }
        }

        public RectGrilleDlg()
        {
            InitializeComponent();
            ToolName = "RectGrille";
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
            RectGrilleMaker maker = new RectGrilleMaker(grillLength, 
            grillHeight, 
            grillWidth,
            makeEdge, 
            edgeThickness, 
            verticalBars, 
            verticalBarThickness, 
            horizontalBars, 
            horizontalBarThickness );

            maker.Generate(Vertices, Faces);
            CentreVertices();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters

            GrillLength = EditorParameters.GetDouble("GrillLength", 30);

            GrillHeight = EditorParameters.GetDouble("GrillHeight", 40);
            GrillWidth = EditorParameters.GetDouble("GrillWidth", 5);

            MakeEdge = EditorParameters.GetBoolean("MakeEdge", true);

            EdgeThickness = EditorParameters.GetDouble("EdgeThickness", 1);

            VerticalBars = EditorParameters.GetDouble("VerticalBars", 3);

            VerticalBarThickness = EditorParameters.GetDouble("VerticalBarThickness", 2);

            HorizontalBars = EditorParameters.GetDouble("HorizontalBars", 3);

            HorizontalBarThickness = EditorParameters.GetDouble("HorizontalBarThickness", 2);
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("GrillLength", GrillLength.ToString());
            EditorParameters.Set("GrillHeight", GrillHeight.ToString());
            EditorParameters.Set("GrillWidth", GrillWidth.ToString());
            EditorParameters.Set("MakeEdge", MakeEdge.ToString());
            EditorParameters.Set("EdgeThickness", EdgeThickness.ToString());
            EditorParameters.Set("VerticalBars", VerticalBars.ToString());
            EditorParameters.Set("VerticalBarThickness", VerticalBarThickness.ToString());
            EditorParameters.Set("HorizontalBars", HorizontalBars.ToString());
            EditorParameters.Set("HorizontalBarThickness", HorizontalBarThickness.ToString());
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

        private void SetDefaults()
        {
            loaded = false;
            GrillLength = 30;
            GrillHeight = 40;
            GrillWidth = 5;
            MakeEdge = true;
            EdgeThickness = 1;
            VerticalBars = 3;
            VerticalBarThickness = 2;
            HorizontalBars = 3;
            HorizontalBarThickness = 2;

            loaded = true;
        }

        private void ResetDefaults(object sender, RoutedEventArgs e)
        {
            SetDefaults();
            UpdateDisplay();
        }
    }
}