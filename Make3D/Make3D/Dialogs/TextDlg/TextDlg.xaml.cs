using MakerLib;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for TextDlg.xaml
    /// </summary>
    public partial class TextDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private ObservableCollection<FontFamily> _systemFonts = new ObservableCollection<FontFamily>();
        private string dataPath;
        private bool fontBold;
        private bool fontItalic;
        private double fontSize;
        private bool loaded;
        private String selectedFont;
        private string text;
        private double thickness;
        private string warningText;

        public TextDlg()
        {
            InitializeComponent();
            loaded = false;
            ToolName = "Text";
            DataContext = this;
            ModelGroup = MyModelGroup;
            text = "A";
            dataPath = "f1M0,0";
        }

        public bool FontBold
        {
            get
            {
                return fontBold;
            }
            set
            {
                if (fontBold != value)
                {
                    fontBold = value;
                    NotifyPropertyChanged();
                    
                    UpdateDisplay();
                }
            }
        }

        public bool FontItalic
        {
            get
            {
                return fontItalic;
            }
            set
            {
                if (fontItalic != value)
                {
                    fontItalic = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double FontSizeText
        {
            get
            {
                return fontSize;
            }
            set
            {
                if (fontSize != value)
                {
                    if (value >= 4)
                    {
                        fontSize = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                        WarningText = "";
                    }
                    else
                    {
                        WarningText = "Minimum FontSize is 4";
                    }
                }
            }
        }

        public String PathData
        {
            get
            {
                return dataPath;
            }
            set
            {
                if (value != dataPath)
                {
                    dataPath = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String SelectedFont
        {
            get
            {
                return selectedFont;
            }
            set
            {
                if (selectedFont != value)
                {
                    selectedFont = value;
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

        public ObservableCollection<FontFamily> SystemFonts
        {
            get { return _systemFonts; }
        }

        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                if (text != value)
                {
                    text = value;
                    NotifyPropertyChanged();

                        UpdateDisplay();
                    
                }
            }
        }

        public double Thickness
        {
            get
            {
                return thickness;
            }
            set
            {
                if (thickness != value)
                {
                    if (value > 0)
                    {
                        thickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                        WarningText = "";
                    }
                    else
                    {
                        WarningText = "Thickness must be > 0";
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

        public void LoadSystemFonts()
        {
            _systemFonts.Clear();

            var fonts = Fonts.SystemFontFamilies.OrderBy(f => f.ToString());

            foreach (var f in fonts)
                _systemFonts.Add(f);
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParmeters();
            DialogResult = true;
            Close();
        }

        private void GenerateShape(bool final = false)
        {
            ClearShape();
            if (text != null && text != "")
            {
                TextMaker mk = new TextMaker(text, selectedFont, fontSize, thickness, final, fontBold, fontItalic);
                string PathCode = mk.Generate(Vertices, Faces);
                PathData = PathCode;
            }
            CentreVertices();
        }

        private void LoadEditorParameters()
        {
            if ( EditorParameters.Get("Text") != "")
            {
                Text = EditorParameters.Get("Text");
                SelectedFont = EditorParameters.Get("FontName");
                FontSizeText = EditorParameters.GetDouble("FontSize");
                Thickness = EditorParameters.GetDouble("Thickness");
                FontBold = EditorParameters.GetBoolean("Bold");
                FontItalic = EditorParameters.GetBoolean("Italic");
            }
        }

        private void SaveEditorParmeters()
        {
            EditorParameters.Set("Text", text);
            EditorParameters.Set("FontName", selectedFont);
            EditorParameters.Set("FontSize", fontSize);
            EditorParameters.Set("Thickness", thickness.ToString());
            EditorParameters.Set("Bold", fontBold.ToString());
            EditorParameters.Set("Italic", fontItalic.ToString());
        }

        private void UpdateDisplay()
        {
            // building shapes is time consuming,dont do until all
            // the default parameters are set
            if (loaded )
            {
                if (SettingsValid())
                {
                    GenerateShape();
                }
                else
                {
                    ClearShape();
                }
                Redisplay();
            }
        }

        private bool SettingsValid()
        {
            bool result = true;
            if ( text == null || text == "")
            {
                result = false;
            }
            if (selectedFont == null || selectedFont =="")
            {
                result = false;
            }
            if ( fontSize < 4.0)
            {
                result = false;
            }
            if ( thickness <= 0)
            {
                result = false;
            }
            return result;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSystemFonts();
            SelectedFont = "Segoe UI";
            FontSizeText = 24.0;
            FontBold = false;
            FontItalic = false;
            Text = "Text";
            Thickness = 10;
            LoadEditorParameters();
            loaded = true;
            warningText = "";

            UpdateCameraPos();
            MyModelGroup.Children.Clear();

            UpdateDisplay();
        }
    }
}