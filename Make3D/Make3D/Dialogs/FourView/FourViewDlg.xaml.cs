using MakerLib;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Collections.Generic;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for FourView.xaml
    /// </summary>
    public partial class FourViewDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private double bias;
        private int distalSteps;
        private string frontView;
        private int horizontalSteps;
        private string leftView;
        private bool loaded;
        private FourViewMaker maker;
        private DispatcherTimer regenTimer;
        private string rightView;
        private string topView;
        private string warningText;

        public FourViewDlg()
        {
            InitializeComponent();
            ToolName = "FourView";
            DataContext = this;
            loaded = false;
            maker = new FourViewMaker();
            Properties.Settings.Default.Reload();
            regenTimer = new DispatcherTimer();
            regenTimer.Interval = new TimeSpan(0, 0, Properties.Settings.Default.RegenerationDelay);
            regenTimer.Tick += RegenTimer_Tick;
            LeftPathEditor.OnFlexiPathChanged += LeftPathChanged;
            RightPathEditor.OnFlexiPathChanged += RightPathChanged;
            FrontPathEditor.OnFlexiPathChanged += FrontPathChanged;
            TopPathEditor.OnFlexiPathChanged += TopPathChanged;
        }

        public double Bias
        {
            get
            {
                return bias;
            }
            set
            {
                if (bias != value)
                {
                    if (CheckRange(value))
                    {
                        bias = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String BiasToolTip
        {
            get
            {
                return ConstructToolTip("bias");
            }
        }

        public int DistalSteps
        {
            get
            {
                return distalSteps;
            }
            set
            {
                if (distalSteps != value)
                {
                    if (CheckRange(value))
                    {
                        distalSteps = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String DistalStepsToolTip
        {
            get
            {
                return ConstructToolTip("distalSteps");
            }
        }

        public int HorizontalSteps
        {
            get
            {
                return horizontalSteps;
            }
            set
            {
                if (horizontalSteps != value)
                {
                    if (CheckRange(value))
                    {
                        horizontalSteps = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String HorizontalStepsToolTip
        {
            get
            {
                return ConstructToolTip("horizontalSteps");
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

        private void FrontPathChanged(List<Point> points)
        {
            frontView = FrontPathEditor.GetPath();
            UpdateDisplay();
        }

        private AsyncGeneratorResult GenerateAsync(string leftview, string rightview, string topview, string sideview, int horizontalSteps, int distalSteps, double bias)
        {
            Point3DCollection v1 = new Point3DCollection();
            Int32Collection i1 = new Int32Collection();
            FourViewMaker maker = new FourViewMaker();
            maker.SetValues(frontView, leftView, rightView, topView, horizontalSteps, distalSteps, bias);
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
            string pl = LeftPathEditor.GetPath();
            string pr = RightPathEditor.GetPath();
            string pt = TopPathEditor.GetPath();
            string ps = LeftPathEditor.GetPath();

            if (pl != "" && pr != "" && pt != "" && ps != "")
            {
                if (LeftPathEditor.PathClosed && RightPathEditor.PathClosed && FrontPathEditor.PathClosed && TopPathEditor.PathClosed)
                {
                    Viewer.Busy();
                    EditingEnabled = false;
                    AsyncGeneratorResult result;
                    result = await Task.Run(() => GenerateAsync(pl, pr, pt, ps, horizontalSteps, distalSteps, bias));
                    GetVerticesFromAsyncResult(result);
                    CentreVertices();
                    Viewer.NotBusy();
                    EditingEnabled = true;
                }
            }
        }

        private void LeftPathChanged(List<Point> points)
        {
            leftView = LeftPathEditor.GetPath();
            UpdateDisplay();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            FrontPathEditor.FromString(EditorParameters.Get("FrontView"));
            LeftPathEditor.FromString(EditorParameters.Get("LeftView"));
            RightPathEditor.FromString(EditorParameters.Get("RightView"));
            TopPathEditor.FromString(EditorParameters.Get("TopView"));
            HorizontalSteps = EditorParameters.GetInt("HorizontalSteps", 100);
            DistalSteps = EditorParameters.GetInt("DistalSteps", 100);
            Bias = EditorParameters.GetDouble("Bias", 0);
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

        private void RightPathChanged(List<Point> points)
        {
            rightView = RightPathEditor.GetPath();
            UpdateDisplay();
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            EditorParameters.Set("FrontView", FrontPathEditor.AbsolutePathString);
            EditorParameters.Set("LeftView", LeftPathEditor.AbsolutePathString);
            EditorParameters.Set("RightView", RightPathEditor.AbsolutePathString);
            EditorParameters.Set("TopView", TopPathEditor.AbsolutePathString);
            EditorParameters.Set("HorizontalSteps", HorizontalSteps.ToString());
            EditorParameters.Set("DistalSteps", DistalSteps.ToString());
            EditorParameters.Set("Bias", Bias.ToString());
        }

        private void SetDefaults()
        {
            loaded = false;
            frontView = "";
            leftView = "";
            rightView = "";
            topView = "";
            HorizontalSteps = 100;
            DistalSteps = 100;
            Bias = 0;

            loaded = true;
        }

        private void TopPathChanged(List<Point> points)
        {
            topView = TopPathEditor.GetPath();
            UpdateDisplay();
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