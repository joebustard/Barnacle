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
    /// Interaction logic for CrossGrille.xaml
    /// </summary>
    public partial class CrossGrilleDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private double crossBeamWidth;
        private double edgeWidth;
        private double grilleHeight;
        private double grilleLength;
        private double grilleWidth;
        private bool loaded;
        private bool makeEdge;
        private CrossGrillMaker maker;
        private int numberOfCrossBeams;
        private DispatcherTimer regenTimer;
        private Visibility showEdgeControls;
        private string warningText;

        public CrossGrilleDlg()
        {
            InitializeComponent();
            ToolName = "CrossGrille";
            DataContext = this;
            loaded = false;
            maker = new CrossGrillMaker();
            Properties.Settings.Default.Reload();
            regenTimer = new DispatcherTimer();
            regenTimer.Interval = new TimeSpan(0, 0, Properties.Settings.Default.RegenerationDelay);
            regenTimer.Tick += RegenTimer_Tick;
        }

        public double CrossBeamWidth
        {
            get
            {
                return crossBeamWidth;
            }
            set
            {
                if (crossBeamWidth != value)
                {
                    if (CheckRange(value))
                    {
                        crossBeamWidth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String CrossBeamWidthToolTip
        {
            get
            {
                return ConstructToolTip("crossBeamWidth");
            }
        }

        public double EdgeWidth
        {
            get
            {
                return edgeWidth;
            }
            set
            {
                if (edgeWidth != value)
                {
                    if (CheckRange(value))
                    {
                        edgeWidth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String EdgeWidthToolTip
        {
            get
            {
                return ConstructToolTip("edgeWidth");
            }
        }

        public double GrilleHeight
        {
            get
            {
                return grilleHeight;
            }
            set
            {
                if (grilleHeight != value)
                {
                    if (CheckRange(value))
                    {
                        grilleHeight = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
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

        public double GrilleWidth
        {
            get
            {
                return grilleWidth;
            }
            set
            {
                if (grilleWidth != value)
                {
                    if (CheckRange(value))
                    {
                        grilleWidth = value;
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
                return ConstructToolTip("grilleHeight");
            }
        }

        public String GrillWidthToolTip
        {
            get
            {
                return ConstructToolTip("grilleWidth");
            }
        }

        public bool MakeEdge
        {
            get
            {
                return makeEdge;
            }
            set
            {
                if (makeEdge != value)
                {
                    makeEdge = value;
                    NotifyPropertyChanged();
                    if (makeEdge)
                    {
                        ShowEdgeControls = Visibility.Visible;
                    }
                    else
                    {
                        ShowEdgeControls = Visibility.Hidden;
                    }
                    UpdateDisplay();
                }
            }
        }

        public int NumberOfCrossBeams
        {
            get
            {
                return numberOfCrossBeams;
            }
            set
            {
                if (numberOfCrossBeams != value)
                {
                    if (CheckRange(value))
                    {
                        numberOfCrossBeams = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String NumberOfCrossBeamsToolTip
        {
            get
            {
                return ConstructToolTip("numberOfCrossBeams");
            }
        }

        public Visibility ShowEdgeControls
        {
            get
            {
                return showEdgeControls;
            }
            set
            {
                if (value != showEdgeControls)
                {
                    showEdgeControls = value;
                    NotifyPropertyChanged();
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
            CrossGrillMaker maker = new CrossGrillMaker();
            maker.SetValues(grilleLength,
                            grilleHeight,
                            grilleWidth,
                            numberOfCrossBeams,
                            crossBeamWidth,
                            makeEdge,
                            edgeWidth);
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
            GrilleHeight = EditorParameters.GetDouble("GrilleHeight", 50);
            GrilleWidth = EditorParameters.GetDouble("GrilleWidth", 2);
            NumberOfCrossBeams = EditorParameters.GetInt("NumberOfCrossBeams", 1);
            CrossBeamWidth = EditorParameters.GetDouble("CrossBeamWidth", 2);
            MakeEdge = EditorParameters.GetBoolean("MakeEdge", true);
            EdgeWidth = EditorParameters.GetDouble("EdgeWidth", 1);
            if (makeEdge)
            {
                ShowEdgeControls = Visibility.Visible;
            }
            else
            {
                ShowEdgeControls = Visibility.Hidden;
            }
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
            EditorParameters.Set("GrilleHeight", GrilleHeight.ToString());
            EditorParameters.Set("GrilleWidth", GrilleWidth.ToString());
            EditorParameters.Set("NumberOfCrossBeams", NumberOfCrossBeams.ToString());
            EditorParameters.Set("CrossBeamWidth", CrossBeamWidth.ToString());
            EditorParameters.Set("MakeEdge", MakeEdge.ToString());
            EditorParameters.Set("EdgeWidth", EdgeWidth.ToString());
        }

        private void SetDefaults()
        {
            loaded = false;
            GrilleLength = 100;
            GrilleHeight = 50;
            GrilleWidth = 2;
            NumberOfCrossBeams = 1;
            CrossBeamWidth = 2;
            MakeEdge = true;
            EdgeWidth = 1;
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