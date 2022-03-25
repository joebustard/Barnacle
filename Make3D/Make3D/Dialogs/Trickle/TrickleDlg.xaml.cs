using MakerLib;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for Trickle.xaml
    /// </summary>
    public partial class TrickleDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private string warningText;
        private bool loaded ;

        

private double radius;
public double Radius
{
    get
    {
      return radius;
    }
    set
    {
        if ( radius != value )
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



private double side;
public double Side
{
    get
    {
      return side;
    }
    set
    {
        if ( side != value )
        {
            if (value >= 1 && value <= 200)
            {
              side = value;
              NotifyPropertyChanged();
              UpdateDisplay();
           }
        }
    }
}



private double thickness;
public double Thickness
{
    get
    {
      return thickness;
    }
    set
    {
        if ( thickness != value )
        {
            if (value >= 0.1 && value <= 100)
            {
              thickness = value;
              NotifyPropertyChanged();
              UpdateDisplay();
           }
        }
    }
}



        public TrickleDlg()
        {
            InitializeComponent();
            ToolName = "Trickle";
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
            TrickleMaker maker = new TrickleMaker(
                radius, side, thickness
                );
            maker.Generate(Vertices, Faces);
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            
          if ( EditorParameters.Get("Radius") !="")
          {
              Radius= EditorParameters.GetDouble("Radius");
          }

          if ( EditorParameters.Get("Side") !="")
          {
              Side= EditorParameters.GetDouble("Side");
          }

          if ( EditorParameters.Get("Thickness") !="")
          {
              Thickness= EditorParameters.GetDouble("Thickness");
          }

        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            
            EditorParameters.Set("Radius",Radius.ToString());
            EditorParameters.Set("Side",Side.ToString());
            EditorParameters.Set("Thickness",Thickness.ToString());
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
            Side = 20;
            Thickness = 5;
            Radius = 5;
            LoadEditorParameters();
            
            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            loaded = true;
            UpdateDisplay();
        }
    }
}
