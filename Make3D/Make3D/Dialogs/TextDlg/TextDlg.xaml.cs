using MakerLib;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for TextDlg.xaml
    /// </summary>
    public partial class TextDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private string text;
        private string warningText;
        private string dataPath;
        public String PathData
        {
            get { return dataPath; }
            set
            {
                if (value != dataPath)
                {
                    dataPath = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public TextDlg()
        {
            InitializeComponent();
            ToolName = "TextDlg";
            DataContext = this;
            ModelGroup = MyModelGroup;
            text = "A";
            dataPath = "f1M0,0";
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
                    GenerateShape();
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
            if (text != null && text != "")
            {
                TextMaker mk = new TextMaker(text, "Tahoma", 32, 10);
                string PathCode = mk.Generate(Vertices, Faces);
                PathData = PathCode;
            }
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
        }

        private void UpdateDisplay()
        {
            GenerateShape();
            Redisplay();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadEditorParameters();
            GenerateShape();
            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            warningText = "";
            Redisplay();
        }
    }
}