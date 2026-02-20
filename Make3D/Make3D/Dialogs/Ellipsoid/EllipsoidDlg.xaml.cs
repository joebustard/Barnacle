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
    /// Interaction logic for Ellipsoid.xaml
    /// </summary>
    public partial class EllipsoidDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private bool half;
        private double leftLength;
        private bool loaded;
        private EllipsoidMaker maker;
        private DispatcherTimer regenTimer;
        private double rightLength;
        private double shapeHeight;
        private double shapeWidth;
        private string warningText;

        public EllipsoidDlg()
        {
            InitializeComponent();
            ToolName = "Ellipsoid";
            DataContext = this;
            loaded = false;
            maker = new EllipsoidMaker();
            Properties.Settings.Default.Reload();
            regenTimer = new DispatcherTimer();
            regenTimer.Interval = new TimeSpan(0, 0, Properties.Settings.Default.RegenerationDelay);
            regenTimer.Tick += RegenTimer_Tick;
        }

        public bool Half
        {
            get
            {
                return half;
            }
            set
            {
                if (half != value)
                {
                    half = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public String HalfToolTip
        {
            get
            {
                return "If set then Ellipsoid is split in half";
            }
        }

        public double LeftLength
        {
            get
            {
                return leftLength;
            }
            set
            {
                if (leftLength != value)
                {
                    if (CheckRange(value))
                    {
                        leftLength = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String LeftLengthToolTip
        {
            get
            {
                return ConstructToolTip("leftLength");
            }
        }

        public double RightLength
        {
            get
            {
                return rightLength;
            }
            set
            {
                if (rightLength != value)
                {
                    if (CheckRange(value))
                    {
                        rightLength = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String RightLengthToolTip
        {
            get
            {
                return ConstructToolTip("rightLength");
            }
        }

        public double ShapeHeight
        {
            get
            {
                return shapeHeight;
            }
            set
            {
                if (shapeHeight != value)
                {
                    if (CheckRange(value))
                    {
                        shapeHeight = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String ShapeToolTip
        {
            get
            {
                return ConstructToolTip("shapeHeight");
            }
        }

        public double ShapeWidth
        {
            get
            {
                return shapeWidth;
            }
            set
            {
                if (shapeWidth != value)
                {
                    if (CheckRange(value))
                    {
                        shapeWidth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String ShapeWidthToolTip
        {
            get
            {
                return ConstructToolTip("shapewidth");
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
            EllipsoidMaker maker = new EllipsoidMaker();
            maker.SetValues(rightLength, leftLength, shapeHeight, shapeWidth, half);
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
            RightLength = EditorParameters.GetDouble("RightLength", 20); LeftLength = EditorParameters.GetDouble("LeftLength", 20);
            ShapeHeight = EditorParameters.GetDouble("ShapeHeight", 20);
            ShapeWidth = EditorParameters.GetDouble("ShapeWidth", 20);
            Half = EditorParameters.GetBoolean("Half", false);
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
            EditorParameters.Set("RightLength", RightLength.ToString());
            EditorParameters.Set("LeftLength", LeftLength.ToString());
            EditorParameters.Set("ShapeHeight", ShapeHeight.ToString());
            EditorParameters.Set("ShapeWidth", ShapeWidth.ToString());
            EditorParameters.Set("Half", Half.ToString());
        }

        private void SetDefaults()
        {
            loaded = false;
            RightLength = 20;
            LeftLength = 20;
            ShapeHeight = 20;
            ShapeWidth = 20;
            Half = false;

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