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
    /// Interaction logic for PiercedDisk.xaml
    /// </summary>
    public partial class PiercedDiskDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private double diskHeight;
        private double diskRadius;
        private double holeDistanceFromEdge;
        private double holeRadius;
        private bool loaded;
        private PiercedDiskMaker maker;
        private double numberOfHoles;
        private DispatcherTimer regenTimer;
        private string warningText;

        public PiercedDiskDlg()
        {
            InitializeComponent();
            ToolName = "PiercedDisk";
            DataContext = this;
            loaded = false;
            maker = new PiercedDiskMaker();
            Properties.Settings.Default.Reload();
            regenTimer = new DispatcherTimer();
            regenTimer.Interval = new TimeSpan(0, 0, Properties.Settings.Default.RegenerationDelay);
            regenTimer.Tick += RegenTimer_Tick;
        }

        public double DiskHeight
        {
            get
            {
                return diskHeight;
            }
            set
            {
                if (diskHeight != value)
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

        public double DiskRadius
        {
            get
            {
                return diskRadius;
            }
            set
            {
                if (diskRadius != value)
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

        public double HoleDistanceFromEdge
        {
            get
            {
                return holeDistanceFromEdge;
            }
            set
            {
                if (holeDistanceFromEdge != value)
                {
                    if (CheckRange(value))
                    {
                        holeDistanceFromEdge = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String HoleDistanceFromEdgeToolTip
        {
            get
            {
                return ConstructToolTip("holeDistanceFromEdge");
            }
        }

        public double HoleRadius
        {
            get
            {
                return holeRadius;
            }
            set
            {
                if (holeRadius != value)
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

        public double NumberOfHoles
        {
            get
            {
                return numberOfHoles;
            }
            set
            {
                if (numberOfHoles != value)
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

        private bool CheckRange(double v, [CallerMemberName] String propertyName = "")
        {
            bool res = false;
            if (maker != null)
            {
                res = maker.CheckLimits(propertyName, v);
            }
            return res;
        }

        private string ConstructToolTip(string p)
        {
            string res = "";
            if (maker != null)
            {
                ParamLimit pl = maker.GetLimits(p);
                if (pl != null)
                {
                    res = $"{p} must be in the range {pl.Low} to {pl.High}";
                }
            }
            return res;
        }

        private AsyncGeneratorResult GenerateAsync()
        {
            Point3DCollection v1 = new Point3DCollection();
            Int32Collection i1 = new Int32Collection();
            PiercedDiskMaker maker = new PiercedDiskMaker();
            maker.SetValues(diskRadius, holeRadius, holeDistanceFromEdge, numberOfHoles, diskHeight
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

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            DiskRadius = EditorParameters.GetDouble("DiskRadius", 10); HoleRadius = EditorParameters.GetDouble("HoleRadius", 1); HoleDistanceFromEdge = EditorParameters.GetDouble("HoleDistanceFromEdge", 1); NumberOfHoles = EditorParameters.GetDouble("NumberOfHoles", 4); DiskHeight = EditorParameters.GetDouble("DiskHeight", 1);
        }

        private void Regenerate()
        {
            if (loaded)
            {
                GenerateShape();
                Viewer.Model = GetModel();
            }
        }

        private void RegenTimer_Tick(object sender, EventArgs e)
        {
            regenTimer.Stop();
            Regenerate();
        }

        private void ResetDefaults(object sender, RoutedEventArgs e)
        {
            SetDefaults();
            UpdateDisplay();
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            EditorParameters.Set("DiskRadius", DiskRadius.ToString()); EditorParameters.Set("HoleRadius", HoleRadius.ToString()); EditorParameters.Set("HoleDistanceFromEdge", HoleDistanceFromEdge.ToString()); EditorParameters.Set("NumberOfHoles", NumberOfHoles.ToString()); EditorParameters.Set("DiskHeight", DiskHeight.ToString());
        }

        private void SetDefaults()
        {
            loaded = false;
            DiskRadius = 10;
            HoleRadius = 1;
            HoleDistanceFromEdge = 1;
            NumberOfHoles = 4;
            DiskHeight = 1;

            loaded = true;
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
    }
}