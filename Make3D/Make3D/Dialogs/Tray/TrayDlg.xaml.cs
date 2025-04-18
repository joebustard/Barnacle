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
    /// Interaction logic for DripTray.xaml
    /// </summary>
    public partial class TrayDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private double baseLength;
        private double baseWidth;
        private bool loaded;
        private TrayMaker maker;
        private DispatcherTimer regenTimer;
        private double topLength;
        private double topWidth;
        private double trayHeight;
        private double wallThickness;
        private string warningText;

        public TrayDlg()
        {
            InitializeComponent();
            ToolName = "Tray";
            DataContext = this;
            loaded = false;
            maker = new TrayMaker();
            Properties.Settings.Default.Reload();
            regenTimer = new DispatcherTimer();
            regenTimer.Interval = new TimeSpan(0, 0, Properties.Settings.Default.RegenerationDelay);
            regenTimer.Tick += RegenTimer_Tick;
        }

        public double BaseLength
        {
            get
            {
                return baseLength;
            }
            set
            {
                if (baseLength != value)
                {
                    if (CheckRange(value))
                    {
                        baseLength = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String BaseLengthToolTip
        {
            get
            {
                return ConstructToolTip("baseLength");
            }
        }

        public double BaseWidth
        {
            get
            {
                return baseWidth;
            }
            set
            {
                if (baseWidth != value)
                {
                    if (CheckRange(value))
                    {
                        baseWidth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String BaseWidthToolTip
        {
            get
            {
                return ConstructToolTip("baseWidth");
            }
        }

        public double TopLength
        {
            get
            {
                return topLength;
            }
            set
            {
                if (topLength != value)
                {
                    if (CheckRange(value))
                    {
                        topLength = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String TopLengthToolTip
        {
            get
            {
                return ConstructToolTip("topLength");
            }
        }

        public double TopWidth
        {
            get
            {
                return topWidth;
            }
            set
            {
                if (topWidth != value)
                {
                    if (CheckRange(value))
                    {
                        topWidth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String TopWidthToolTip
        {
            get
            {
                return ConstructToolTip("topWidth");
            }
        }

        public double TrayHeight
        {
            get
            {
                return trayHeight;
            }
            set
            {
                if (trayHeight != value)
                {
                    if (CheckRange(value))
                    {
                        trayHeight = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String TrayHeightToolTip
        {
            get
            {
                return ConstructToolTip("trayHeight");
            }
        }

        public double WallThickness
        {
            get
            {
                return wallThickness;
            }
            set
            {
                if (wallThickness != value)
                {
                    if (CheckRange(value))
                    {
                        wallThickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String WallThicknessToolTip
        {
            get
            {
                return ConstructToolTip("wallThickness");
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
            TrayMaker maker = new TrayMaker();
            maker.SetValues(topLength, topWidth, baseLength, baseWidth, trayHeight, wallThickness);
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
            TopLength = EditorParameters.GetDouble("TopLength", 100);
            TopWidth = EditorParameters.GetDouble("TopWidth", 100);
            BaseLength = EditorParameters.GetDouble("BaseLength", 80);
            BaseWidth = EditorParameters.GetDouble("BaseWidth", 80);
            TrayHeight = EditorParameters.GetDouble("TrayHeight", 10);
            WallThickness = EditorParameters.GetDouble("WallThickness", 1);
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
            EditorParameters.Set("TopLength", TopLength.ToString());
            EditorParameters.Set("TopWidth", TopWidth.ToString());
            EditorParameters.Set("BaseLength", BaseLength.ToString());
            EditorParameters.Set("BaseWidth", BaseWidth.ToString());
            EditorParameters.Set("TrayHeight", TrayHeight.ToString());
            EditorParameters.Set("WallThickness", WallThickness.ToString());
        }

        private void SetDefaults()
        {
            loaded = false;
            TopLength = 100;
            TopWidth = 100;
            BaseLength = 80;
            BaseWidth = 80;
            TrayHeight = 10;
            WallThickness = 1;

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