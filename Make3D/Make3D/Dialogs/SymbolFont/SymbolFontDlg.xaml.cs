using MakerLib;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

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

 
        private Visibility show3D;

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

        private Visibility showBusy;

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
            ToolName = "Symbol";
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

        private  void GenerateShape()
        {
            /*
                //ShowBusy = Visibility.Visible;
                //Show3D = Visibility.Hidden;
                ClearShape();
                SymbolFontMaker maker = new SymbolFontMaker(symbolLength, symbolHeight, symbolWidth, symbolCode, symbolFont);
                SymbolFontMaker.ResultDetails ret = await Task.Run(async () =>  await maker.GenerateAsync(Vertices,Faces));

                Vertices.Clear();
                Faces.Clear();
                foreach( Point3D p in ret.pnts)
                {
                    Vertices.Add( new Point3D(p.X,p.Y,p.Z));
                }
                foreach( int i in ret.faces)
                {
                    int j = i;
                    Faces.Add(j);
                }
                */
            if (symbolCode != oldSymbol || symbolFont != oldFont)
            {
                ClearShape();
                SymbolFontMaker maker = new SymbolFontMaker(symbolCode, symbolFont);
                oldSymbol = symbolCode;
                oldFont = symbolFont;
                maker.Generate(Vertices, Faces);
                CentreVertices();
            }
        }
        private string oldSymbol="";
        private string oldFont="";
        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            SymbolFont = EditorParameters.Get("SymbolFont");
            SymbolCode = EditorParameters.Get("SymbolCode");
            
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            EditorParameters.Set("SymbolCode", SymbolCode.ToString());
            EditorParameters.Set("SymbolFont", SymbolFont.ToString());
        }

        private void UpdateDisplay()
        {
            
            if (loaded)
            {
                DateTime start = DateTime.Now;
                
                GenerateShape();
                DateTime end = DateTime.Now;
                TimeSpan ts = end - start;
                Debug($"Update display took {ts.TotalSeconds} seconds");
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
            ShowBusy = Visibility.Hidden;
            Show3D = Visibility.Visible;
        }

        private  void OnSymbolChanged(string symbol, string fontName)
        {
            SymbolFont = fontName;
            SymbolCode = symbol;                       
        }

        private void SetDefaults()
        {
            loaded = false;
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