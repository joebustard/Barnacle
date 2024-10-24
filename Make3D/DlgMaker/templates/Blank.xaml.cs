using MakerLib;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Runtime.CompilerServices;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for Blank.xaml
    /// </summary>
    public partial class BlankDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private string warningText;
        private bool loaded;
        private BlankMaker maker;
        //TOOLPROPS
        private string ConstructToolTip(string p)
        {
            string res=";
            if ( maker != null )
            {
              ParamLimit pl = maker.GetLimits(p);
               if ( pl != null )
              {
                  res = $"{p} must be in the range {pl.Low} to {pl.High}";
              }
            }
            return res;
        }

        public BlankDlg()
        {
            InitializeComponent();
            ToolName = "Blank";
            DataContext = this;
            ModelGroup = MyModelGroup;
            loaded = false;
            maker = new BlankMaker();
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
            maker.SetValues(//MAKEPARAMETERS
                );
            maker.Generate(Vertices, Faces);
            CentreVertices();
        }

        private bool CheckRange(double v, [CallerMemberName] String propertyName = "")
        {
            bool res = false;
            if (maker != null)
            {
                res = maker.CheckLimits(propertyName, v);
            }
            return res;
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            //LOADPARMETERS
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            //SAVEPARMETERS
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

            //SETPROPERTIES

            UpdateDisplay();
        }

        private void SetDefaults()
        {
            loaded = false;
            //SETDEFAULTS
            loaded = true;
        }

        private void ResetDefaults(object sender, RoutedEventArgs e)
        {
            SetDefaults();
            UpdateDisplay();
        }
    }
}