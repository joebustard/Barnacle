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
    /// Interaction logic for UBracket.xaml
    /// </summary>
    public partial class UBracketDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private double legGap;
        private double legHeight;
        private double legWidth;
        private bool loaded;
        private UBracketMaker maker;
        private DispatcherTimer regenTimer;
        private double sweepAngle;
        private string warningText;

        public UBracketDlg()
        {
            InitializeComponent();
            ToolName = "UBracket";
            DataContext = this;
            loaded = false;
            maker = new UBracketMaker();
            Properties.Settings.Default.Reload();
            regenTimer = new DispatcherTimer();
            regenTimer.Interval = new TimeSpan(0, 0, Properties.Settings.Default.RegenerationDelay);
            regenTimer.Tick += RegenTimer_Tick;
        }

        public double LegGap
        {
            get
            {
                return legGap;
            }
            set
            {
                if (legGap != value)
                {
                    if (CheckRange(value))
                    {
                        legGap = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String LegGapToolTip
        {
            get
            {
                return ConstructToolTip("legGap");
            }
        }

        public double LegHeight
        {
            get
            {
                return legHeight;
            }
            set
            {
                if (legHeight != value)
                {
                    if (CheckRange(value))
                    {
                        legHeight = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String LegHeightToolTip
        {
            get
            {
                return ConstructToolTip("legHeight");
            }
        }

        public double LegWidth
        {
            get
            {
                return legWidth;
            }
            set
            {
                if (legWidth != value)
                {
                    if (CheckRange(value))
                    {
                        legWidth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String LegWIdthToolTip
        {
            get
            {
                return ConstructToolTip("legWidth");
            }
        }

        public double SweepAngle
        {
            get
            {
                return sweepAngle;
            }
            set
            {
                if (sweepAngle != value)
                {
                    if (CheckRange(value))
                    {
                        sweepAngle = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String SweepAngleToolTip
        {
            get
            {
                return ConstructToolTip("sweepAngle");
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
            UBracketMaker maker = new UBracketMaker();
            maker.SetValues(legHeight, legWidth, legGap, sweepAngle);
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
            LegHeight = EditorParameters.GetDouble("LegHeight", 50);
            LegWidth = EditorParameters.GetDouble("LegWidth", 10);
            LegGap = EditorParameters.GetDouble("LegGap", 20);
            SweepAngle = EditorParameters.GetDouble("SweepAngle", 40);
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
            EditorParameters.Set("LegHeight", LegHeight.ToString());
            EditorParameters.Set("LegWidth", LegWidth.ToString());
            EditorParameters.Set("LegGap", LegGap.ToString());
            EditorParameters.Set("SweepAngle", SweepAngle.ToString());
        }

        private void SetDefaults()
        {
            loaded = false;
            LegHeight = 50;
            LegWidth = 10;
            LegGap = 20;
            SweepAngle = 45;

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