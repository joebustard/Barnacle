using AILib;
using Barnacle.Object3DLib;
using MakerLib;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for SurfaceNoise.xaml
    /// </summary>
    public partial class SurfaceNoiseDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private CancellationTokenSource _cts;
        private double frequency;
        private Visibility freqVisible;
        private double intensity;
        private bool loaded;
        private int noiseType;

        private bool perlinSelected;

        //private SurfaceNoiseMaker maker;
        private DispatcherTimer regenTimer;

        private int subdivisionDepth;

        private Object3D theObject;

        private string warningText;

        public SurfaceNoiseDlg()
        {
            InitializeComponent();
            ToolName = "SurfaceNoise";
            DataContext = this;
            loaded = false;
            // maker = new SurfaceNoiseMaker();
            Properties.Settings.Default.Reload();
            regenTimer = new DispatcherTimer();
            regenTimer.Interval = new TimeSpan(0, 0, Properties.Settings.Default.RegenerationDelay);
            regenTimer.Tick += RegenTimer_Tick;
        }

        public double Frequency
        {
            get
            {
                return frequency;
            }
            set
            {
                if (frequency != value)
                {
                    if (CheckRange(value))
                    {
                        frequency = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String FrequencyToolTip
        {
            get
            {
                return ConstructToolTip("frequency");
            }
        }

        public Visibility FrequencyVisible
        {
            get
            {
                return freqVisible;
            }
            set
            {
                if (freqVisible != value)
                {
                    freqVisible = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double Intensity
        {
            get
            {
                return intensity;
            }
            set
            {
                if (intensity != value)
                {
                    if (CheckRange(value))
                    {
                        intensity = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String IntensityToolTip
        {
            get
            {
                return ConstructToolTip("intensity");
            }
        }

        public String NoiseTypeToolTip
        {
            get
            {
                return ConstructToolTip("noiseType");
            }
        }

        public bool PerlinSelected
        {
            get
            {
                return perlinSelected;
            }
            set
            {
                if (perlinSelected != value)
                {
                    perlinSelected = value;
                    if (perlinSelected)
                    {
                        noiseType = 0;
                        FrequencyVisible = Visibility.Visible;
                    }
                    else
                    {
                        noiseType = 1;
                        FrequencyVisible = Visibility.Hidden;
                    }
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public Object3D SourceObject
        {
            get
            {
                return theObject;
            }
            set
            {
                theObject = value;
            }
        }

        public int SubdivisionDepth
        {
            get
            {
                return subdivisionDepth;
            }
            set
            {
                if (subdivisionDepth != value)
                {
                    if (CheckRange(value))
                    {
                        subdivisionDepth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String SubdivisionDepthToolTip
        {
            get
            {
                return ConstructToolTip("subdivisionDepth");
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
            //     if (maker != null)
            //     {
            //          res = maker.CheckLimits(propertyName, v);
            //}
            return res;
        }

        private string ConstructToolTip(string p)
        {
            string res = "";
            /*
            if ( maker != null )
            {
              ParamLimit pl = maker.GetLimits(p);
               if ( pl != null )
              {
                  res = $"{p} must be in the range {pl.Low} to {pl.High}";
              }
            }
            */
            return res;
        }

        private async void GenerateShape()
        {
            if (theObject != null)
            {
                ClearShape();
                Viewer.Busy();
                EditingEnabled = false;
                AILib.NoiseType nt = NoiseType.Perlin;
                if (noiseType == 1)
                {
                    nt = NoiseType.Random;
                }
                _cts = new CancellationTokenSource();
                MeshGeometry3D baseMesh = theObject.Mesh;
                MeshGeometry3D highDensityRoughMesh = await AsyncRoughMeshModifier.MakeMeshRoughAsync(
                       inputMesh: baseMesh,
                       subdivisionDepth: subdivisionDepth,
                       noiseType: (AILib.NoiseType)noiseType,
                       intensity: intensity,
                       frequency: frequency,
                       token: _cts.Token
                   );
                Vertices = highDensityRoughMesh.Positions;
                Faces = highDensityRoughMesh.TriangleIndices;

                CentreVertices();
                Viewer.Model.Geometry = highDensityRoughMesh;
                Viewer.NotBusy();
                EditingEnabled = true;
            }
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            PerlinSelected = EditorParameters.GetBoolean("PerlinSelected", true);

            SubdivisionDepth = EditorParameters.GetInt("SubdivisionDepth", 0);
            Intensity = EditorParameters.GetDouble("Intensity", 1);
            Frequency = EditorParameters.GetDouble("Frequency", 6.5);
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
            EditorParameters.Set("PerlinNoise", PerlinSelected.ToString());
            EditorParameters.Set("SubdivisionDepth", SubdivisionDepth.ToString());
            EditorParameters.Set("Intensity", Intensity.ToString());
            EditorParameters.Set("Frequency", Frequency.ToString());
        }

        private void SetDefaults()
        {
            loaded = false;
            PerlinSelected = true;
            SubdivisionDepth = 0;
            Intensity = 1;
            Frequency = 6.5;

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