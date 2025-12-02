using MakerLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for VoxelHull.xaml
    /// </summary>
    public partial class PointHullDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private bool loaded;
        private VoxelHullMaker maker;
        private DispatcherTimer regenTimer;
        private List<Point3D> sourcePoints;
        private string warningText;
        /*
        private string ConstructToolTip(string p)
        {
            string res="";
            if ( maker != null )
            {
              ParamLimit pl = maker.GetLimits(p);
               if ( pl != null )
              {
                  res = $"{p} must be in the range {pl.Low} to {pl.High}";
              }
            }
            return res;
        }
        */

        public PointHullDlg()
        {
            InitializeComponent();
            ToolName = "PointHull";
            DataContext = this;
            loaded = false;
            maker = new VoxelHullMaker();
            Properties.Settings.Default.Reload();
            regenTimer = new DispatcherTimer();
            regenTimer.Interval = new TimeSpan(0, 0, Properties.Settings.Default.RegenerationDelay);
            regenTimer.Tick += RegenTimer_Tick;
            VoxGrid.OnPointsUpdated += VoxGrid_OnPointsUpdated;
            sourcePoints = new List<Point3D>();
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

        private AsyncGeneratorResult GenerateAsync()
        {
            Point3DCollection v1 = new Point3DCollection();
            Int32Collection i1 = new Int32Collection();
            VoxelHullMaker maker = new VoxelHullMaker();
            maker.SetValues(sourcePoints);
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
            if (sourcePoints.Count > 1)
            {
                Viewer.Busy();
                EditingEnabled = false;
                AsyncGeneratorResult result;
                result = await Task.Run(() => GenerateAsync());
                GetVerticesFromAsyncResult(result);
                CentreVertices();
                Viewer.NotBusy();
                EditingEnabled = true;
            }
        }

        private void LoadEditorParameters()
        {
            sourcePoints.Clear();
            // load back the tool specific parameters
            String str = EditorParameters.Get("Pnts");
            if (str != "")
            {
                string[] lines = str.Split('|');
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    string[] words = line.Split(',');
                    if (words.Length == 3)
                    {
                        double x = Convert.ToDouble(words[0]);
                        double y = Convert.ToDouble(words[1]);
                        double z = Convert.ToDouble(words[2]);
                        sourcePoints.Add(new Point3D(x, y, z));
                    }
                }
            }
            VoxGrid.SetPoints(sourcePoints);
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
            VoxGrid.ClearPoints();
            UpdateDisplay();
        }

        private void SaveEditorParmeters()
        {
            String pntsstr = VoxGrid.VoxelText();

            EditorParameters.Set("Pnts", pntsstr);
        }

        private void SetDefaults()
        {
            loaded = false;

            loaded = true;
        }

        private void UpdateDisplay()
        {
            regenTimer.Stop();
            regenTimer.Start();
        }

        private void VoxGrid_OnPointsUpdated(Point3DCollection points)
        {
            sourcePoints.Clear();
            if (points != null)
            {
                foreach (Point3D point in points)
                {
                    sourcePoints.Add(point);
                }
            }
            UpdateDisplay();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WarningText = "";
            LoadEditorParameters();

            Viewer.Clear();
            VoxGrid.Display();
            loaded = true;

            UpdateDisplay();
        }
    }
}