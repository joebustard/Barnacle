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
    /// Interaction logic for HoneyCombGrille.xaml
    /// </summary>
    public partial class HoneyCombGrilleDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private double beamLength;
        private double beamWidth;
        private double edgeSize;
        private double grilleLength;
        private double grillHeight;
        private double grillThickness;
        private bool loaded;
        private HoneyCombGrilleMaker maker;
        private DispatcherTimer regenTimer;
        private bool showEdge;
        private string warningText;

        public HoneyCombGrilleDlg()
        {
            InitializeComponent();
            ToolName = "HoneycombGrille";
            DataContext = this;
            loaded = false;
            maker = new HoneyCombGrilleMaker();
            Properties.Settings.Default.Reload();
            regenTimer = new DispatcherTimer();
            regenTimer.Interval = new TimeSpan(0, 0, Properties.Settings.Default.RegenerationDelay);
            regenTimer.Tick += RegenTimer_Tick;
        }

        public double BeamLength
        {
            get
            {
                return beamLength;
            }
            set
            {
                if (beamLength != value)
                {
                    if (CheckRange(value))
                    {
                        beamLength = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String BeamLengthToolTip
        {
            get
            {
                return ConstructToolTip("beamLength");
            }
        }

        public double BeamWidth
        {
            get
            {
                return beamWidth;
            }
            set
            {
                if (beamWidth != value)
                {
                    if (CheckRange(value))
                    {
                        beamWidth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String BeamWidthToolTip
        {
            get
            {
                return ConstructToolTip("beamWidth");
            }
        }

        public double EdgeSize
        {
            get
            {
                return edgeSize;
            }
            set
            {
                if (edgeSize != value)
                {
                    if (CheckRange(value))
                    {
                        edgeSize = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String EdgeSizeToolTip
        {
            get
            {
                return ConstructToolTip("edgeSize");
            }
        }

        public double GrilleLength
        {
            get
            {
                return grilleLength;
            }
            set
            {
                if (grilleLength != value)
                {
                    if (CheckRange(value))
                    {
                        grilleLength = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String GrilleLengthToolTip
        {
            get
            {
                return ConstructToolTip("grilleLength");
            }
        }

        public double GrillHeight
        {
            get
            {
                return grillHeight;
            }
            set
            {
                if (grillHeight != value)
                {
                    if (CheckRange(value))
                    {
                        grillHeight = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String GrillHeightToolTip
        {
            get
            {
                return ConstructToolTip("grillHeight");
            }
        }

        public double GrillThickness
        {
            get
            {
                return grillThickness;
            }
            set
            {
                if (grillThickness != value)
                {
                    if (CheckRange(value))
                    {
                        grillThickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String GrillThicknessToolTip
        {
            get
            {
                return ConstructToolTip("grillThickness");
            }
        }

        public bool ShowEdge
        {
            get
            {
                return showEdge;
            }
            set
            {
                if (showEdge != value)
                {
                    showEdge = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public String ShowEdgeToolTip
        {
            get
            {
                return ConstructToolTip("showEdge");
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
            HoneyCombGrilleMaker maker = new HoneyCombGrilleMaker();
            maker.SetValues(grilleLength, grillHeight, grillThickness, beamLength, beamWidth, edgeSize, showEdge
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
            GrilleLength = EditorParameters.GetDouble("GrilleLength", 100);
            GrillHeight = EditorParameters.GetDouble("GrillHeight", 50);
            GrillThickness = EditorParameters.GetDouble("GrillThickness", 2);
            BeamLength = EditorParameters.GetDouble("BeamLength", 10);
            BeamWidth = EditorParameters.GetDouble("BeamWidth", 2);
            EdgeSize = EditorParameters.GetDouble("EdgeSize", 3);
            ShowEdge = EditorParameters.GetBoolean("ShowEdge", true);
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
            EditorParameters.Set("GrilleLength", GrilleLength.ToString());
            EditorParameters.Set("GrillHeight", GrillHeight.ToString());
            EditorParameters.Set("GrillThickness", GrillThickness.ToString());
            EditorParameters.Set("BeamLength", BeamLength.ToString());
            EditorParameters.Set("BeamWidth", BeamWidth.ToString());
            EditorParameters.Set("EdgeSize", EdgeSize.ToString());
            EditorParameters.Set("ShowEdge", ShowEdge.ToString());
        }

        private void SetDefaults()
        {
            loaded = false;
            GrilleLength = 100;
            GrillHeight = 50;
            GrillThickness = 2;
            BeamLength = 10;
            BeamWidth = 2;
            EdgeSize = 3;
            ShowEdge = true;

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