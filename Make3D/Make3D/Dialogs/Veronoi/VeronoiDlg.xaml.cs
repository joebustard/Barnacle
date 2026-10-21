using AiLib;
using AiWpf3D;
using Barnacle.Object3DLib;
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
    /// Interaction logic for Veronoi.xaml
    /// </summary>
    public partial class VeronoiDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private bool explodeChecked;
        private bool loaded;

        // private VeronoiMaker maker;
        private DispatcherTimer regenTimer;

        private double resolution;
        private double wallThickness;
        private string warningText;

        public VeronoiDlg()
        {
            InitializeComponent();
            ToolName = "Veronoi";
            DataContext = this;
            loaded = false;
            //   maker = new VeronoiMaker();
            Properties.Settings.Default.Reload();
            regenTimer = new DispatcherTimer();
            regenTimer.Interval = new TimeSpan(0, 0, Properties.Settings.Default.RegenerationDelay);
            regenTimer.Tick += RegenTimer_Tick;
        }

        public bool ExplodeChecked
        {
            get
            {
                return explodeChecked;
            }
            set
            {
                if (value != explodeChecked)
                {
                    explodeChecked = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double Resolution
        {
            get
            {
                return resolution;
            }
            set
            {
                if (resolution != value)
                {
                    if (CheckRange(value))
                    {
                        resolution = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String ResolutionToolTip
        {
            get
            {
                return ConstructToolTip("resolution");
            }
        }

        public Object3D SourceObject
        {
            get;
            set;
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
            bool res = true;
            /*  if (maker != null)
              {
                  res = maker.CheckLimits(propertyName, v);
              } */
            return res;
        }

        private string ConstructToolTip(string p)
        {
            string res = "";
            /*    if (maker != null)
                {
                    ParamLimit pl = maker.GetLimits(p);
                    if (pl != null)
                    {
                        res = $"{p} must be in the range {pl.Low} to {pl.High}";
                    }
                }
                */
            return res;
        }

        private async void GenerateShape()
        {
            ClearShape();
            Viewer.Busy();
            EditingEnabled = false;

            Vertices.Clear();
            Faces.Clear();
            MeshGeometry3D gm;
            if (explodeChecked)
            {
                gm = VoronoiMeshConverter.ConvertToVoronoiMesh(SourceObject.Mesh, resolution);
            }
            else
            {
                gm = VoronoiNetworkGenerator.CreateVoronoiNetwork(SourceObject.Mesh, 0.5, 6);
            }
            Vertices = gm.Positions.Clone();
            Faces = gm.TriangleIndices.Clone();
            gm = null;
            CentreVertices();
            Viewer.Model = GetModel();
            Viewer.NotBusy();
            EditingEnabled = true;
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            Resolution = EditorParameters.GetDouble("Resolution", 1); WallThickness = EditorParameters.GetDouble("WallThickness", 1);
            ExplodeChecked = EditorParameters.GetBoolean("Exploded", true);
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
            EditorParameters.Set("Resolution", Resolution.ToString());
            EditorParameters.Set("WallThickness", WallThickness.ToString());
            EditorParameters.Set("Exploded", ExplodeChecked.ToString());
        }

        private void SetDefaults()
        {
            loaded = false;
            Resolution = 1;
            WallThickness = 2.5;

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