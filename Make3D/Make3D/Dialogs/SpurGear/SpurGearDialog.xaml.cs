using MakerLib;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for BevelledGear.xaml
    /// </summary>
    public partial class SpurGearDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private double baseRadius;
        private double baseThickness;
        private double boreHoleRadius;
        private bool fillBase;
        private double gearHeight;
        private bool loaded;
        private GearMaker maker;
        private int numberOfTeeth;
        private DispatcherTimer regenTimer;
        private Visibility showBaseThickness;
        private double toothLength;
        private string warningText;

        public SpurGearDlg()
        {
            InitializeComponent();
            ToolName = "SpurGear";
            DataContext = this;
            loaded = false;
            maker = new GearMaker();
            Properties.Settings.Default.Reload();
            regenTimer = new DispatcherTimer();
            regenTimer.Interval = new TimeSpan(0, 0, Properties.Settings.Default.RegenerationDelay);
            regenTimer.Tick += RegenTimer_Tick;
        }

        public double BaseRadius
        {
            get
            {
                return baseRadius;
            }
            set
            {
                if (baseRadius != value)
                {
                    if (CheckRange(value))
                    {
                        baseRadius = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String BaseRadiusToolTip
        {
            get
            {
                return ConstructToolTip("baseRadius");
            }
        }

        public double BaseThickness
        {
            get
            {
                return baseThickness;
            }
            set
            {
                if (baseThickness != value)
                {
                    if (CheckRange(value))
                    {
                        baseThickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String BaseThicknessToolTip
        {
            get
            {
                return ConstructToolTip("baseThickness");
            }
        }

        public double BoreHoleRadius
        {
            get
            {
                return boreHoleRadius;
            }
            set
            {
                if (boreHoleRadius != value)
                {
                    if (CheckRange(value))
                    {
                        boreHoleRadius = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String BoreHoleRadiusToolTip
        {
            get
            {
                return ConstructToolTip("boreHoleRadius") + ". Set to 0 if no hole is required.";
            }
        }

        public bool FillBase
        {
            get
            {
                return fillBase;
            }
            set
            {
                if (fillBase != value)
                {
                    fillBase = value;
                    if (fillBase)
                    {
                        ShowBaseThickness = Visibility.Visible;
                    }
                    else
                    {
                        ShowBaseThickness = Visibility.Hidden;
                    }
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public String FillBaseToolTip
        {
            get
            {
                return ConstructToolTip("fillBase");
            }
        }

        public double GearHeight
        {
            get
            {
                return gearHeight;
            }
            set
            {
                if (gearHeight != value)
                {
                    if (CheckRange(value))
                    {
                        gearHeight = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String GearHeightToolTip
        {
            get
            {
                return ConstructToolTip("gearHeight");
            }
        }

        public int NumberOfTeeth
        {
            get
            {
                return numberOfTeeth;
            }
            set
            {
                if (numberOfTeeth != value)
                {
                    if (CheckRange(value))
                    {
                        numberOfTeeth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String NumberOfTeethToolTip
        {
            get
            {
                return ConstructToolTip("numberOfTeeth");
            }
        }

        public Visibility ShowBaseThickness
        {
            get
            {
                return showBaseThickness;
            }
            set
            {
                if (value != showBaseThickness)
                {
                    showBaseThickness = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double ToothLength
        {
            get
            {
                return toothLength;
            }
            set
            {
                if (toothLength != value)
                {
                    if (CheckRange(value))
                    {
                        toothLength = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String ToothLengthToolTip
        {
            get
            {
                return ConstructToolTip("toothLength");
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
            GearMaker maker = new GearMaker();
            maker.SetValues(baseRadius, gearHeight, toothLength, numberOfTeeth, boreHoleRadius, baseThickness, fillBase, false);
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
            // load back the tool specific parametes
            BaseRadius = EditorParameters.GetDouble("BaseRadius", 10);
            GearHeight = EditorParameters.GetDouble("GearHeight", 5);
            ToothLength = EditorParameters.GetDouble("ToothLength", 5);
            NumberOfTeeth = EditorParameters.GetInt("NumberOfTeeth", 10);
            BoreHoleRadius = EditorParameters.GetDouble("BoreHoleRadius", 2);
            BaseThickness = EditorParameters.GetDouble("BaseThickness", 1);
            FillBase = EditorParameters.GetBoolean("FillBase", false);
            if (fillBase)
            {
                ShowBaseThickness = Visibility.Visible;
            }
            else
            {
                ShowBaseThickness = Visibility.Hidden;
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
            EditorParameters.Set("BaseRadius", BaseRadius.ToString());
            EditorParameters.Set("GearHeight", GearHeight.ToString());
            EditorParameters.Set("ToothLength", ToothLength.ToString());
            EditorParameters.Set("NumberOfTeeth", NumberOfTeeth.ToString());
            EditorParameters.Set("BoreHoleRadius", BoreHoleRadius.ToString());
            EditorParameters.Set("BaseThickness", BaseThickness.ToString());
            EditorParameters.Set("FillBase",
            FillBase.ToString());
        }

        private void SetDefaults()
        {
            loaded = false;
            BaseRadius = 10;
            GearHeight = 5;
            ToothLength = 5;
            NumberOfTeeth = 10;
            BoreHoleRadius = 2;
            BaseThickness = 1;
            FillBase = false;

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