using MakerLib;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for SymbolFont.xaml
    /// </summary>
    public partial class SymbolDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private const double maxsymbolLength = 200;
        private const double minsymbolLength = 5;
        private bool loaded;
        private string oldFont = "";
        private string oldSymbol = "";
        private Visibility show3D;
        private Visibility showBusy;
        private string symbolCode;
        private string symbolFont;
        private double symbolLength;
        private string warningText;
        private DispatcherTimer timer;

        public SymbolDlg()
        {
            InitializeComponent();
            ToolName = "Symbol";
            DataContext = this;
            ModelGroup = MyModelGroup;
            loaded = false;
        }

        public Visibility Show3D
        {
            get { return show3D; }

            set
            {
                if (show3D != value)
                {
                    show3D = value;
                    NotifyPropertyChanged();
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

        public Visibility ShowBusy
        {
            get { return showBusy; }

            set
            {
                if (showBusy != value)
                {
                    showBusy = value;
                    NotifyPropertyChanged();
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
            get { return symbolLength; }

            set
            {
                if (value != symbolLength)
                {
                    if (value >= minsymbolLength && value <= maxsymbolLength)
                    {
                        symbolLength = value;
                        WarningText = "";
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                    else
                    {
                        WarningText = $"Symbol Length must be in the range {minsymbolLength} to {maxsymbolLength}";
                    }
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
            SymbolFontMaker maker = new SymbolFontMaker(symbolCode, symbolFont, symbolLength);
            oldSymbol = symbolCode;
            oldFont = symbolFont;
            maker.Generate(Vertices, Faces);

            CentreVertices();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            SymbolFont = EditorParameters.Get("SymbolFont");
            SymbolCode = EditorParameters.Get("SymbolCode");
            SymbolLength = EditorParameters.GetDouble("SymbolLength", 25);
        }

        private void OnSymbolChanged(string symbol, string fontName)
        {
            SymbolFont = fontName;
            SymbolCode = symbol;
        }

        private void ResetDefaults(object sender, RoutedEventArgs e)
        {
            SetDefaults();
            UpdateDisplay();
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            EditorParameters.Set("SymbolCode", SymbolCode.ToString());
            EditorParameters.Set("SymbolFont", SymbolFont.ToString());
            EditorParameters.Set("SymbolLength", SymbolLength.ToString());
        }

        private void SetDefaults()
        {
            loaded = false;
            SymbolCode = "A";

            loaded = true;
        }

        private void UpdateDisplay()
        {
            if (loaded)
            {
                ClearShape();
                ShowBusy = Visibility.Visible;

                timer.Start();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WarningText = "";
            LoadEditorParameters();

            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            loaded = true;
            timer = new DispatcherTimer();
            timer.Interval = new System.TimeSpan(0, 0, 1);
            timer.Tick += Timer_Tick;
            UpdateDisplay();
            SymbolSelection.OnSymbolChanged = OnSymbolChanged;
            ShowBusy = Visibility.Hidden;
            Show3D = Visibility.Visible;
        }

        private void Timer_Tick(object sender, System.EventArgs e)
        {
            timer.Stop();
            GenerateShape();
            Redisplay();
            ShowBusy = Visibility.Hidden;
        }
    }
}