using MakerLib;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for PiercedRing.xaml
    /// </summary>
    public partial class PiercedRingDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private string warningText;
        private bool loaded;
        private PiercedRingMaker maker;
        private DispatcherTimer regenTimer;
        
private double diskRadius;
public double DiskRadius
{
    get
    {
      return diskRadius;
    }
    set
    {
        if ( diskRadius != value )
        {
            if (CheckRange(value))
            {
              diskRadius = value;
              NotifyPropertyChanged();
              UpdateDisplay();
           }
        }
    }
}
public String DiskRadiusToolTip
{
    get
    {
        return ConstructToolTip("diskRadius");
    }
}

private double diskInnerRadius;
public double DiskInnerRadius
{
    get
    {
      return diskInnerRadius;
    }
    set
    {
        if ( diskInnerRadius != value )
        {
            if (CheckRange(value))
            {
              diskInnerRadius = value;
              NotifyPropertyChanged();
              UpdateDisplay();
           }
        }
    }
}
public String DiskInnerRadiusToolTip
{
    get
    {
        return ConstructToolTip("diskInnerRadius");
    }
}

private double holeRadius;
public double HoleRadius
{
    get
    {
      return holeRadius;
    }
    set
    {
        if ( holeRadius != value )
        {
            if (CheckRange(value))
            {
              holeRadius = value;
              NotifyPropertyChanged();
              UpdateDisplay();
           }
        }
    }
}
public String HoleRadiusToolTip
{
    get
    {
        return ConstructToolTip("holeRadius");
    }
}

private double distanceFromEdge;
public double DistanceFromEdge
{
    get
    {
      return distanceFromEdge;
    }
    set
    {
        if ( distanceFromEdge != value )
        {
            if (CheckRange(value))
            {
              distanceFromEdge = value;
              NotifyPropertyChanged();
              UpdateDisplay();
           }
        }
    }
}
public String DistanceFromEdgeToolTip
{
    get
    {
        return ConstructToolTip("distanceFromEdge");
    }
}

private double numberOfHoles;
public double NumberOfHoles
{
    get
    {
      return numberOfHoles;
    }
    set
    {
        if ( numberOfHoles != value )
        {
            if (CheckRange(value))
            {
              numberOfHoles = value;
              NotifyPropertyChanged();
              UpdateDisplay();
           }
        }
    }
}
public String NumberOfHolesToolTip
{
    get
    {
        return ConstructToolTip("numberOfHoles");
    }
}

private double diskHeight;
public double DiskHeight
{
    get
    {
      return diskHeight;
    }
    set
    {
        if ( diskHeight != value )
        {
            if (CheckRange(value))
            {
              diskHeight = value;
              NotifyPropertyChanged();
              UpdateDisplay();
           }
        }
    }
}
public String DiskHeightToolTip
{
    get
    {
        return ConstructToolTip("diskHeight");
    }
}

        private string ConstructToolTip(string p)
        {
            string res="";
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

        public PiercedRingDlg()
        {
            InitializeComponent();
            ToolName = "PiercedRing";
            DataContext = this;
            loaded = false;
            maker = new PiercedRingMaker();
            Properties.Settings.Default.Reload();
            regenTimer = new DispatcherTimer();
            regenTimer.Interval = new TimeSpan(0, 0, Properties.Settings.Default.RegenerationDelay);
            regenTimer.Tick += RegenTimer_Tick;
        }
        private void RegenTimer_Tick(object sender, EventArgs e)
        {
            regenTimer.Stop();
            Regenerate();
        }
        private void Regenerate()
        {
            if (loaded)
            {
                GenerateShape();
                Viewer.Model = GetModel();
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
            if (regenTimer.IsEnabled)
            {
                regenTimer.Stop();
                Regenerate();
            }
            else
            {
                base.SaveSizeAndLocation();
                SaveEditorParmeters();
                DialogResult = true;
                Close();
            }
        }

        private async void GenerateShape()
        {
            ClearShape();
            Viewer.Busy();
            EditingEnabled = false;
            AsyncGeneratorResult result;
            result = await Task.Run(() => GenerateAsync());
            GetVerticesFromAsyncResult(result);
            CentreVertices();
            Viewer.NotBusy();
            EditingEnabled = true;
        }

  private AsyncGeneratorResult GenerateAsync()
        {
            Point3DCollection v1 = new Point3DCollection();
            Int32Collection i1 = new Int32Collection();
            PiercedRingMaker maker = new PiercedRingMaker();
            maker.SetValues(diskRadius, diskInnerRadius, holeRadius, distanceFromEdge, numberOfHoles, diskHeight
                );
            maker.Generate(v1, i1);

            AsyncGeneratorResult res = new AsyncGeneratorResult();
            // extract the vertices and indices to thread safe arrays
            // while still in the async function
            res.points = new Point3D[v1.Count];
            for (int i = 0; i < v1.Count; i++)
            {
                res.points[i] = new Point3D(v1[i].X, v1[i].Y, v1[i].Z);
            }
            res.indices = new int[i1.Count];
            for (int i = 0; i < i1.Count; i++)
            {
                res.indices[i] = i1[i];
            }
            v1.Clear();
            i1.Clear();
            return (res);
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
            DiskRadius= EditorParameters.GetDouble("DiskRadius",10);DiskInnerRadius= EditorParameters.GetDouble("DiskInnerRadius",2);HoleRadius= EditorParameters.GetDouble("HoleRadius",1);DistanceFromEdge= EditorParameters.GetDouble("DistanceFromEdge",1);NumberOfHoles= EditorParameters.GetDouble("NumberOfHoles",4);DiskHeight= EditorParameters.GetDouble("DiskHeight",1);
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            EditorParameters.Set("DiskRadius",DiskRadius.ToString());EditorParameters.Set("DiskInnerRadius",DiskInnerRadius.ToString());EditorParameters.Set("HoleRadius",HoleRadius.ToString());EditorParameters.Set("DistanceFromEdge",DistanceFromEdge.ToString());EditorParameters.Set("NumberOfHoles",NumberOfHoles.ToString());EditorParameters.Set("DiskHeight",DiskHeight.ToString());
        }

        private void UpdateDisplay()
        {
            regenTimer.Stop();
            regenTimer.Start();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WarningText = "";
            LoadEditorParameters();

            Viewer.Clear();
            loaded = true;

            

            UpdateDisplay();
        }

        private void SetDefaults()
        {
            loaded = false;
                DiskRadius = 10;
    DiskInnerRadius = 2;
    HoleRadius = 1;
    DistanceFromEdge = 1;
    NumberOfHoles = 4;
    DiskHeight = 1;

            loaded = true;
        }

        private void ResetDefaults(object sender, RoutedEventArgs e)
        {
            SetDefaults();
            UpdateDisplay();
        }
    }
}
