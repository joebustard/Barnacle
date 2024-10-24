using MakerLib;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for RailWheel.xaml
    /// </summary>
    public partial class RailWheelDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private string warningText;
        private bool loaded;

        
private const double minaxleBoreRadius = 0;
private const double maxaxleBoreRadius = 100;
private double axleBoreRadius;
public double AxleBoreRadius
{
    get
    {
      return axleBoreRadius;
    }
    set
    {
        if ( axleBoreRadius != value )
        {
            if (value >= minaxleBoreRadius && value <= maxaxleBoreRadius)
            {
              axleBoreRadius = value;
              NotifyPropertyChanged();
              UpdateDisplay();
           }
        }
    }
}
public String AxleBoreRadiusToolTip
{
    get
    {
        return $"AxleBoreRadius must be in the range {minaxleBoreRadius} to {maxaxleBoreRadius}";
    }
}


private const double minhubRadius = 0;
private const double maxhubRadius = 100;
private double hubRadius;
public double HubRadius
{
    get
    {
      return hubRadius;
    }
    set
    {
        if ( hubRadius != value )
        {
            if (value >= minhubRadius && value <= maxhubRadius)
            {
              hubRadius = value;
              NotifyPropertyChanged();
              UpdateDisplay();
           }
        }
    }
}
public String HubRadiusToolTip
{
    get
    {
        return $"HubRadius must be in the range {minhubRadius} to {maxhubRadius}";
    }
}


private const double minhubHeight = 0;
private const double maxhubHeight = 100;
private double hubHeight;
public double HubHeight
{
    get
    {
      return hubHeight;
    }
    set
    {
        if ( hubHeight != value )
        {
            if (value >= minhubHeight && value <= maxhubHeight)
            {
              hubHeight = value;
              NotifyPropertyChanged();
              UpdateDisplay();
           }
        }
    }
}
public String HubHeightToolTip
{
    get
    {
        return $"HubHeight must be in the range {minhubHeight} to {maxhubHeight}";
    }
}


private const double minrimRadius = 0;
private const double maxrimRadius = 100;
private double rimRadius;
public double RimRadius
{
    get
    {
      return rimRadius;
    }
    set
    {
        if ( rimRadius != value )
        {
            if (value >= minrimRadius && value <= maxrimRadius)
            {
              rimRadius = value;
              NotifyPropertyChanged();
              UpdateDisplay();
           }
        }
    }
}
public String RimRadiusToolTip
{
    get
    {
        return $"RimRadius must be in the range {minrimRadius} to {maxrimRadius}";
    }
}


private const double minupperRimThickness = 0.1;
private const double maxupperRimThickness = 100;
private double upperRimThickness;
public double UpperRimThickness
{
    get
    {
      return upperRimThickness;
    }
    set
    {
        if ( upperRimThickness != value )
        {
            if (value >= minupperRimThickness && value <= maxupperRimThickness)
            {
              upperRimThickness = value;
              NotifyPropertyChanged();
              UpdateDisplay();
           }
        }
    }
}
public String UpperRimThicknessToolTip
{
    get
    {
        return $"UpperRimThickness must be in the range {minupperRimThickness} to {maxupperRimThickness}";
    }
}


private const double minlowerRimThickness = 0.2;
private const double maxlowerRimThickness = 100;
private double lowerRimThickness;
public double LowerRimThickness
{
    get
    {
      return lowerRimThickness;
    }
    set
    {
        if ( lowerRimThickness != value )
        {
            if (value >= minlowerRimThickness && value <= maxlowerRimThickness)
            {
              lowerRimThickness = value;
              NotifyPropertyChanged();
              UpdateDisplay();
           }
        }
    }
}
public String LowerRimThicknessToolTip
{
    get
    {
        return $"LowerRimThickness must be in the range {minlowerRimThickness} to {maxlowerRimThickness}";
    }
}


private const double minflangeRadius = 0;
private const double maxflangeRadius = 100;
private double flangeRadius;
public double FlangeRadius
{
    get
    {
      return flangeRadius;
    }
    set
    {
        if ( flangeRadius != value )
        {
            if (value >= minflangeRadius && value <= maxflangeRadius)
            {
              flangeRadius = value;
              NotifyPropertyChanged();
              UpdateDisplay();
           }
        }
    }
}
public String FlangeRadiusToolTip
{
    get
    {
        return $"FlangeRadius must be in the range {minflangeRadius} to {maxflangeRadius}";
    }
}


private const double minflangeHeight = 0.2;
private const double maxflangeHeight = 100;
private double flangeHeight;
public double FlangeHeight
{
    get
    {
      return flangeHeight;
    }
    set
    {
        if ( flangeHeight != value )
        {
            if (value >= minflangeHeight && value <= maxflangeHeight)
            {
              flangeHeight = value;
              NotifyPropertyChanged();
              UpdateDisplay();
           }
        }
    }
}
public String FlangeHeightToolTip
{
    get
    {
        return $"FlangeHeight must be in the range {minflangeHeight} to {maxflangeHeight}";
    }
}



        public RailWheelDlg()
        {
            InitializeComponent();
            ToolName = "RailWheel";
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
            RailWheelMaker maker = new RailWheelMaker(
                axleBoreRadius, hubRadius, hubHeight, rimRadius, upperRimThickness, lowerRimThickness, flangeRadius, flangeHeight
                );
            maker.Generate(Vertices, Faces);
            CentreVertices();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            
            AxleBoreRadius= EditorParameters.GetDouble("AxleBoreRadius",1);


            HubRadius= EditorParameters.GetDouble("HubRadius",1);


            HubHeight= EditorParameters.GetDouble("HubHeight",3);


            RimRadius= EditorParameters.GetDouble("RimRadius",7);


            UpperRimThickness= EditorParameters.GetDouble("UpperRimThickness",1);


            LowerRimThickness= EditorParameters.GetDouble("LowerRimThickness",2);


            FlangeRadius= EditorParameters.GetDouble("FlangeRadius",9);


            FlangeHeight= EditorParameters.GetDouble("FlangeHeight",1);


        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            
            EditorParameters.Set("AxleBoreRadius",AxleBoreRadius.ToString());
            EditorParameters.Set("HubRadius",HubRadius.ToString());
            EditorParameters.Set("HubHeight",HubHeight.ToString());
            EditorParameters.Set("RimRadius",RimRadius.ToString());
            EditorParameters.Set("UpperRimThickness",UpperRimThickness.ToString());
            EditorParameters.Set("LowerRimThickness",LowerRimThickness.ToString());
            EditorParameters.Set("FlangeRadius",FlangeRadius.ToString());
            EditorParameters.Set("FlangeHeight",FlangeHeight.ToString());
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

        private void SetDefaults()
        {
            loaded = false;
                AxleBoreRadius = 1;
    HubRadius = 1;
    HubHeight = 3;
    RimRadius = 7;
    UpperRimThickness = 1;
    LowerRimThickness = 2;
    FlangeRadius = 9;
    FlangeHeight = 1;

            loaded = true;
        }
        private void ResetDefaults(object sender, RoutedEventArgs e)
        {
            SetDefaults();
            UpdateDisplay();
        }
    }
}
