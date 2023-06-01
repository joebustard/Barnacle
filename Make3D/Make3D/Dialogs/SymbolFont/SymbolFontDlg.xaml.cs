using MakerLib;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for SymbolFont.xaml
    /// </summary>
    public partial class SymbolDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private string warningText;
        private bool loaded;

        private const double minsymbolLength = 2;
        private const double maxsymbolLength = 200;
        private double symbolLength;
        private string symbolFont;

        public string SymbolFont
        {
            get { return symbolFont; }
            set
            {
                if (value != symbolFont)
                {
                    symbolFont = value;
                }
            }
        }

        public double SymbolLength
        {
            get
            {
                return symbolLength;
            }
            set
            {
                if (symbolLength != value)
                {
                    if (value >= minsymbolLength && value <= maxsymbolLength)
                    {
                        symbolLength = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String SymbolLengthToolTip
        {
            get
            {
                return $"Symbol Length must be in the range {minsymbolLength} to {maxsymbolLength}";
            }
        }

        private const double minsymbolHeight = 2;
        private const double maxsymbolHeight = 200;
        private double symbolHeight;

        public double SymbolHeight
        {
            get
            {
                return symbolHeight;
            }
            set
            {
                if (symbolHeight != value)
                {
                    if (value >= minsymbolHeight && value <= maxsymbolHeight)
                    {
                        symbolHeight = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String SymbolHeightToolTip
        {
            get
            {
                return $"Symbol Height must be in the range {minsymbolHeight} to {maxsymbolHeight}";
            }
        }

        private const double minsymbolWidth = 2;
        private const double maxsymbolWidth = 200;
        private double symbolWidth;

        public double SymbolWidth
        {
            get
            {
                return symbolWidth;
            }
            set
            {
                if (symbolWidth != value)
                {
                    if (value >= minsymbolWidth && value <= maxsymbolWidth)
                    {
                        symbolWidth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String SymbolWidthToolTip
        {
            get
            {
                return $"Symbol Width must be in the range {minsymbolWidth} to {maxsymbolWidth}";
            }
        }

        private string symbolCode;

        public string SymbolCode
        {
            get
            {
                return symbolCode;
            }
            set
            {
                if (symbolCode != value)
                {
                    symbolCode = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public SymbolDlg()
        {
            InitializeComponent();
            ToolName = "SymbolFont";
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
            SymbolFontMaker maker = new SymbolFontMaker(symbolLength, symbolHeight, symbolWidth, symbolCode, symbolFont);
            maker.Generate(Vertices, Faces);
            CentreVertices();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters

            SymbolLength = EditorParameters.GetDouble("SymbolLength", 10);

            SymbolHeight = EditorParameters.GetDouble("SymbolHeight", 10);

            SymbolWidth = EditorParameters.GetDouble("SymbolWidth", 10);

            SymbolCode = EditorParameters.Get("SymbolCode");
            SymbolFont = EditorParameters.Get("SymbolCode");
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("SymbolLength", SymbolLength.ToString());
            EditorParameters.Set("SymbolHeight", SymbolHeight.ToString());
            EditorParameters.Set("SymbolWidth", SymbolWidth.ToString());
            EditorParameters.Set("SymbolCode", SymbolCode.ToString());
            EditorParameters.Set("SymbolFont", SymbolFont.ToString());
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
            SymbolSelection.OnSymbolChanged = OnSymbolChanged;
        }

        private void OnSymbolChanged(string symbol, string fontName)
        {
            SymbolCode = symbol;
            SymbolFont = fontName;
        }

        private void SetDefaults()
        {
            loaded = false;
            SymbolLength = 10;
            SymbolHeight = 10;
            SymbolWidth = 10;
            SymbolCode = "A";

            loaded = true;
        }

        private void ResetDefaults(object sender, RoutedEventArgs e)
        {
            SetDefaults();
            UpdateDisplay();
        }
    }
}