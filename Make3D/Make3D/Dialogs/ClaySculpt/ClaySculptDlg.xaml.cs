using MakerLib;
using sdflib;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for ClaySculpt.xaml
    /// </summary>
    public partial class ClaySculptDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private string warningText;
        private bool loaded;


        private string toolShape;
        public string ToolShape
        {
            get { return toolShape; }
            set
            {
                if (toolShape != value)
                {
                    toolShape = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }
        private ObservableCollection<String> toolShapeItems;
        public ObservableCollection<String> ToolShapeItems
        {
            get { return toolShapeItems; }
            set
            {
                if (toolShapeItems != value)
                {
                    toolShapeItems = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public String toolShapeToolTip
        {
            get { return "ToolShape Text"; }
        }

        private const double mintoolsSize = 1;
        private const double maxtoolsSize = 10;
        private double toolsSize;
        public double ToolsSize
        {
            get
            {
                return toolsSize;
            }
            set
            {
                if (toolsSize != value)
                {
                    if (value >= mintoolsSize && value <= maxtoolsSize)
                    {
                        toolsSize = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }
        public String ToolsSizeToolTip
        {
            get
            {
                return $"ToolsSize must be in the range {mintoolsSize} to {maxtoolsSize}";
            }
        }


        private const double mintoolStrength = 1;
        private const double maxtoolStrength = 10;
        private double toolStrength;
        public double ToolStrength
        {
            get
            {
                return toolStrength;
            }
            set
            {
                if (toolStrength != value)
                {
                    if (value >= mintoolStrength && value <= maxtoolStrength)
                    {
                        toolStrength = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }
        public String ToolStrengthToolTip
        {
            get
            {
                return $"ToolStrength must be in the range {mintoolStrength} to {maxtoolStrength}";
            }
        }



        private bool toolInverse;
        public bool ToolInverse
        {
            get
            {
                return toolInverse;
            }
            set
            {
                if (toolInverse != value)
                {

                    toolInverse = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();

                }
            }
        }
        public String ToolInverseToolTip
        {
            get
            {
                return $"Invert the behaviour of the tool.";
            }
        }

        private Isdf sdf;
        private int maxX = 132;
        private int maxY = 132;
        private int maxZ = 132;
        public ClaySculptDlg()
        {
            InitializeComponent();
            ToolName = "ClaySculpt";
            DataContext = this;
            ModelGroup = MyModelGroup;
            loaded = false;
            sdf = new Sdf();
            sdf.SetDimension(maxX, maxY, maxZ);
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

        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters

            ToolShape = EditorParameters.Get("ToolShape");


            ToolsSize = EditorParameters.GetDouble("ToolsSize", 30);


            ToolStrength = EditorParameters.GetDouble("ToolStrength", 5);


            ToolInverse = EditorParameters.GetBoolean("ToolInverse", false);


        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("ToolShape", ToolShape);
            EditorParameters.Set("ToolsSize", ToolsSize.ToString());
            EditorParameters.Set("ToolStrength", ToolStrength.ToString());
            EditorParameters.Set("ToolInverse", ToolInverse.ToString());
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
            ToolShape = "Inflate";
            ToolsSize = 30;
            ToolStrength = 5;
            ToolInverse = false;

            loaded = true;
        }
        private void ResetDefaults(object sender, RoutedEventArgs e)
        {
            SetDefaults();
            UpdateDisplay();
        }
    }
}
