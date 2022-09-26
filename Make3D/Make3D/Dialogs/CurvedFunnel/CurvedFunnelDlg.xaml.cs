using MakerLib;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for CurvedFunnel.xaml
    /// </summary>
    public partial class CurvedFunnelDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private string warningText;
        private bool loaded ;

        

private double radius=10;
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



private double factorA=0.75;
public double FactorA
{
    get
    {
      return factorA;
    }
    set
    {
        if ( factorA != value )
        {
            if (value >= 0.1 && value <= 10)
            {
              factorA = value;
              NotifyPropertyChanged();
              UpdateDisplay();
           }
        }
    }
}



private double wallThickness=1;
public double WallThickness
{
    get
    {
      return wallThickness;
    }
    set
    {
        if ( wallThickness != value )
        {
            if (value >= 1 && value <= 10)
            {
              wallThickness = value;
              NotifyPropertyChanged();
              UpdateDisplay();
           }

        }
    }
}



private double shapeHeight=20;
public double ShapeHeight
{
    get
    {
      return shapeHeight;
    }
    set
    {
        if ( shapeHeight != value )
        {
            if (value >= 5 && value <= 100)
            {
              shapeHeight = value;
              NotifyPropertyChanged();
              UpdateDisplay();
           }
        }
    }
}



        public CurvedFunnelDlg()
        {
            InitializeComponent();
            ToolName = "CurvedFunnel";
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
            if (Radius - WallThickness <= 0)
            {
                WarningText = "Radius must be bigger than Wall Thickness";
            }
            else
            {
                WarningText = "";
                CurvedFunnelMaker maker = new CurvedFunnelMaker( radius, factorA, wallThickness, shapeHeight );
                maker.Generate(Vertices, Faces);
                CentreVertices();
            }
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            
          if ( EditorParameters.Get("Radius") !="")
          {
              Radius= EditorParameters.GetDouble("Radius");
          }

          if ( EditorParameters.Get("FactorA") !="")
          {
              FactorA= EditorParameters.GetDouble("FactorA");
          }

          if ( EditorParameters.Get("WallThickness") !="")
          {
              WallThickness= EditorParameters.GetDouble("WallThickness");
          }

          if ( EditorParameters.Get("Height") !="")
          {
              ShapeHeight= EditorParameters.GetDouble("Height");
          }

        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            
            EditorParameters.Set("Radius",Radius.ToString());
            EditorParameters.Set("FactorA",FactorA.ToString());
            EditorParameters.Set("WallThickness",WallThickness.ToString());
            EditorParameters.Set("Height",ShapeHeight.ToString());
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
