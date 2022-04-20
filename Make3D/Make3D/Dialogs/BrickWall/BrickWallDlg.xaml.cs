using MakerLib;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for BrickWall.xaml
    /// </summary>
    public partial class BrickWallDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private string warningText;
        private bool loaded ;

        

private double wallLength;
public double WallLength
{
    get
    {
      return wallLength;
    }
    set
    {
        if ( wallLength != value )
        {
            if (value >= 1 && value <= 200)
            {
              wallLength = value;
              NotifyPropertyChanged();
              UpdateDisplay();
           }
        }
    }
}



private double wallHeight;
public double WallHeight
{
    get
    {
      return wallHeight;
    }
    set
    {
        if ( wallHeight != value )
        {
            if (value >= 1 && value <= 200)
            {
              wallHeight = value;
              NotifyPropertyChanged();
              UpdateDisplay();
           }
        }
    }
}



private double wallWidth;
public double WallWidth
{
    get
    {
      return wallWidth;
    }
    set
    {
        if ( wallWidth != value )
        {
            if (value >= 1 && value <= 200)
            {
              wallWidth = value;
              NotifyPropertyChanged();
              UpdateDisplay();
           }
        }
    }
}



private double largeBrickLength;
public double LargeBrickLength
{
    get
    {
      return largeBrickLength;
    }
    set
    {
        if ( largeBrickLength != value )
        {
            if (value >= 1 && value <= 100)
            {
              largeBrickLength = value;
              NotifyPropertyChanged();
              UpdateDisplay();
           }
        }
    }
}



private double smallBrickLength;
public double SmallBrickLength
{
    get
    {
      return smallBrickLength;
    }
    set
    {
        if ( smallBrickLength != value )
        {
            if (value >= 1 && value <= 100)
            {
              smallBrickLength = value;
              NotifyPropertyChanged();
              UpdateDisplay();
           }
        }
    }
}



private double brickHeight;
public double BrickHeight
{
    get
    {
      return brickHeight;
    }
    set
    {
        if ( brickHeight != value )
        {
            if (value >= 1 && value <= 100)
            {
              brickHeight = value;
              NotifyPropertyChanged();
              UpdateDisplay();
           }
        }
    }
}



private double mortarGap;
public double MortarGap
{
    get
    {
      return mortarGap;
    }
    set
    {
        if ( mortarGap != value )
        {
            if (value >= 0 && value <= 100)
            {
              mortarGap = value;
              NotifyPropertyChanged();
              UpdateDisplay();
           }
        }
    }
}



        public BrickWallDlg()
        {
            InitializeComponent();
            ToolName = "BrickWall";
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
            BrickWallMaker maker = new BrickWallMaker(
                wallLength, wallHeight, wallWidth, largeBrickLength, smallBrickLength, brickHeight, mortarGap
                );
            maker.Generate(Vertices, Faces);
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            
          if ( EditorParameters.Get("WallLength") !="")
          {
              WallLength= EditorParameters.GetDouble("WallLength");
          }

          if ( EditorParameters.Get("WallHeight") !="")
          {
              WallHeight= EditorParameters.GetDouble("WallHeight");
          }

          if ( EditorParameters.Get("WallWidth") !="")
          {
              WallWidth= EditorParameters.GetDouble("WallWidth");
          }

          if ( EditorParameters.Get("LargeBrickLength") !="")
          {
              LargeBrickLength= EditorParameters.GetDouble("LargeBrickLength");
          }

          if ( EditorParameters.Get("SmallBrickLength") !="")
          {
              SmallBrickLength= EditorParameters.GetDouble("SmallBrickLength");
          }

          if ( EditorParameters.Get("BrickHeight") !="")
          {
              BrickHeight= EditorParameters.GetDouble("BrickHeight");
          }

          if ( EditorParameters.Get("MortarGap") !="")
          {
              MortarGap= EditorParameters.GetDouble("MortarGap");
          }

        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            
            EditorParameters.Set("WallLength",WallLength.ToString());
            EditorParameters.Set("WallHeight",WallHeight.ToString());
            EditorParameters.Set("WallWidth",WallWidth.ToString());
            EditorParameters.Set("LargeBrickLength",LargeBrickLength.ToString());
            EditorParameters.Set("SmallBrickLength",SmallBrickLength.ToString());
            EditorParameters.Set("BrickHeight",BrickHeight.ToString());
            EditorParameters.Set("MortarGap",MortarGap.ToString());
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
