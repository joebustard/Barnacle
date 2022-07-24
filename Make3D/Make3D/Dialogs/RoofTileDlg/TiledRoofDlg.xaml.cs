using MakerLib;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for TiledRoof.xaml
    /// </summary>
    public partial class TiledRoofDlg : BaseModellerDialog, INotifyPropertyChanged
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
            if (value >= 10 && value <= 200)
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



private double tileLength;
public double TileLength
{
    get
    {
      return tileLength;
    }
    set
    {
        if ( tileLength != value )
        {
            if (value >= 1 && value <= 10)
            {
              tileLength = value;
              NotifyPropertyChanged();
              UpdateDisplay();
           }
        }
    }
}



private double tileHeight;
public double TileHeight
{
    get
    {
      return tileHeight;
    }
    set
    {
        if ( tileHeight != value )
        {
            if (value >= 1 && value <= 20)
            {
              tileHeight = value;
              NotifyPropertyChanged();
              UpdateDisplay();
           }
        }
    }
}



private double tileWidth;
public double TileWidth
{
    get
    {
      return tileWidth;
    }
    set
    {
        if ( tileWidth != value )
        {
            if (value >= 1 && value <= 20)
            {
              tileWidth = value;
              NotifyPropertyChanged();
              UpdateDisplay();
           }
        }
    }
}



private double gapBetweenTiles;
public double GapBetweenTiles
{
    get
    {
      return gapBetweenTiles;
    }
    set
    {
        if ( gapBetweenTiles != value )
        {
            if (value >= 1 && value <= 10)
            {
              gapBetweenTiles = value;
              NotifyPropertyChanged();
              UpdateDisplay();
           }
        }
    }
}



        public TiledRoofDlg()
        {
            InitializeComponent();
            ToolName = "TiledRoof";
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
            TiledRoofMaker maker = new TiledRoofMaker(
                wallLength, wallHeight, wallWidth, tileLength, tileHeight, tileWidth, gapBetweenTiles
                );
            maker.Generate(Vertices, Faces);
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            
          if ( EditorParameters.Get("Length") !="")
          {
              WallLength= EditorParameters.GetDouble("Length");
          }

          if ( EditorParameters.Get("WallHeight") !="")
          {
              WallHeight= EditorParameters.GetDouble("WallHeight");
          }

          if ( EditorParameters.Get("Width") !="")
          {
              WallWidth= EditorParameters.GetDouble("Width");
          }

          if ( EditorParameters.Get("TileLength") !="")
          {
              TileLength= EditorParameters.GetDouble("TileLength");
          }

          if ( EditorParameters.Get("TileHeight") !="")
          {
              TileHeight= EditorParameters.GetDouble("TileHeight");
          }

          if ( EditorParameters.Get("TileWidth") !="")
          {
              TileWidth= EditorParameters.GetDouble("TileWidth");
          }

          if ( EditorParameters.Get("GapBetweenTiles") !="")
          {
              GapBetweenTiles= EditorParameters.GetDouble("GapBetweenTiles");
          }

        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            
            EditorParameters.Set("WallLength",WallLength.ToString());
            EditorParameters.Set("WallHeight",WallHeight.ToString());
            EditorParameters.Set("WallWidth",WallWidth.ToString());
            EditorParameters.Set("TileLength",TileLength.ToString());
            EditorParameters.Set("TileHeight",TileHeight.ToString());
            EditorParameters.Set("TileWidth",TileWidth.ToString());
            EditorParameters.Set("GapBetweenTiles",GapBetweenTiles.ToString());
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
            WallHeight = 50;
            WallWidth = 5;
            WallLength = 150;
            TileHeight = 10;
            TileLength = 8;
            TileWidth = 4;
            GapBetweenTiles = 1;
            LoadEditorParameters();
            
            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            loaded = true;
            
            

            UpdateDisplay();
        }
    }
}
