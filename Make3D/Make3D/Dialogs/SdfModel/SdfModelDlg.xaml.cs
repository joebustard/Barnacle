using MakerLib;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Barnacle.Dialogs.SdfModel;
using System.Windows.Input;
using Barnacle.ViewModels;
using System.Collections.ObjectModel;
using Barnacle.Dialogs.SdfModel.Operations;
using System.Data;
using System.Windows.Controls;
using System.Collections.Generic;
using Barnacle.Dialogs.SdfModel.Primitives;
using System.Windows.Forms;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for SdfModel.xaml
    /// </summary>
    public partial class SdfModelDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private bool loaded;
        private SdfModelMaker maker;

        private int nextRecordId;
        private double nextY;
        private DispatcherTimer regenTimer;
        private int selectedPrimitive;
        private bool showCurrent = true;
        private string warningText;

        public SdfModelDlg()
        {
            InitializeComponent();
            ToolName = "SdfModel";
            DataContext = this;
            loaded = false;
            maker = new SdfModelMaker();
            maker.Resolution = 1.0;
            Properties.Settings.Default.Reload();
            regenTimer = new DispatcherTimer();
            regenTimer.Interval = new TimeSpan(0, 0, Properties.Settings.Default.RegenerationDelay);
            regenTimer.Tick += RegenTimer_Tick;
            MoveCommand = new RelayCommand(OnMoveSelected);
            StepRecords = new ObservableCollection<StepRecord>();

            selectedPrimitive = -1;
            nextRecordId = -1;
            nextY = 0;
            Viewer.OnPreviewUserKey = PreviewViewerKey;
        }

        public ICommand MoveCommand
        {
            get; set;
        }

        public int SelectedPrimitive
        {
            get
            {
                return selectedPrimitive;
            }
            set
            {
                if (selectedPrimitive != value)
                {
                    selectedPrimitive = value;

                    SelectStep(selectedPrimitive);
                    NotifyPropertyChanged();
                }
            }
        }

        public ObservableCollection<StepRecord> StepRecords
        {
            get; set;
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

        public void StepsChanged()
        {
            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            //
            //    regenTimer.Stop();
            //regenTimer.Start();
            if (loaded)
            {
                Viewer.MultiModels.Children.Clear();
                GenerateShape();
                GeometryModel3D sdfModel = GetModel();
                Model3DGroup group = new Model3DGroup();
                Viewer.MultiModels.Children.Add(sdfModel);
                if (selectedPrimitive != -1 && showCurrent)
                {
                    GeometryModel3D highlightModel = GenerateShapeForSelectedPrimitive();
                    if (highlightModel != null)
                    {
                        Viewer.MultiModels.Children.Add(highlightModel);
                    }
                }
                Viewer.Refresh();
            }
        }

        internal string GetStepsAsText()
        {
            string s = "";
            for (int i = 0; i < StepRecords.Count; i++)
            {
                s += StepRecords[i].ToString();
                if (i < StepRecords.Count - 1)
                {
                    s += ",";
                }
            }
            return s;
        }

        internal void MoveObject(int index, double x, double y, double z)
        {
            if (index >= 0 && index < StepRecords.Count)
            {
                StepRecords[index].Move(x, y, z);
                UpdateDisplay();
            }
        }

        internal void SetSteps(string s)
        {
            StepRecords.Clear();
            nextRecordId = 0;
            string[] words = s.Split(',');
            if (words != null && words.GetLength(0) > 1)
            {
                int i = 0;
                while (i < words.GetLength(0))
                {
                    StepRecord sr = new StepRecord();
                    sr.Id = nextRecordId++;
                    sr.OnStepsChanged = StepsChanged;
                    sr.PrimitiveType = words[i++];
                    Point3D pos = new Point3D(0, 0, 0);
                    try
                    {
                        pos.X = Convert.ToDouble(words[i++]);
                        pos.Y = Convert.ToDouble(words[i++]);
                        pos.Z = Convert.ToDouble(words[i++]);
                        sr.Position = pos;
                        sr.SizeX = Convert.ToDouble(words[i++]);
                        sr.SizeY = Convert.ToDouble(words[i++]);
                        sr.SizeZ = Convert.ToDouble(words[i++]);
                        sr.RotX = Convert.ToDouble(words[i++]);
                        sr.RotY = Convert.ToDouble(words[i++]);
                        sr.RotZ = Convert.ToDouble(words[i++]);
                        sr.Thickness = Convert.ToDouble(words[i++]);
                        sr.OpType = words[i++];
                        sr.Blend = Convert.ToDouble(words[i++]);
                        StepRecords.Add(sr);
                    }
                    catch (Exception ex)
                    {
                        LoggerLib.Logger.LogException(ex);
                    }
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
                maker.Resolution = 0.5;
                Regenerate();
                base.SaveSizeAndLocation();
                SaveEditorParmeters();
                DialogResult = true;
                Close();
            }
        }

        private void AddBoxAtTop()
        {
            nextRecordId++;
            StepRecord rec = new StepRecord();
            rec.Move(0, (int)nextY, 0); ;
            rec.PrimitiveType = "Box";
            rec.Id = nextRecordId;
            rec.OpType = "Smooth Union";
            rec.OnStepsChanged = StepsChanged;
            StepRecords.Add(rec);
        }

        private void AddClick(object sender, RoutedEventArgs e)
        {
            // maker.stepManager.AddBoxAtTop();
            // UpdateStepRecords();
            AddBoxAtTop();
            SelectedPrimitive = StepRecords.Count - 1;
            UpdateDisplay();
        }

        private void BaseModellerDialog_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }

        private bool CheckRange(double v, [CallerMemberName] String propertyName = "")
        {
            bool res = false;
            if (maker != null)
            {
                // res = maker.CheckLimits(propertyName, v);
            }
            return res;
        }

        private string ConstructToolTip(string p)
        {
            string res = "";
            if (maker != null)
            {
                // ParamLimit pl = maker.GetLimits(p);
                //  if (pl != null)
                //   {
                //       res = $"{p} must be in the range {pl.Low} to {pl.High}";
                //   }
            }
            return res;
        }

        private AsyncGeneratorResult GenerateAsync()
        {
            AsyncGeneratorResult res = new AsyncGeneratorResult();
            /*
                Point3DCollection index = new Point3DCollection();
                Int32Collection i1 = new Int32Collection();
                SdfModelMaker maker = new SdfModelMaker();
                maker.SetValues(
                    );
                maker.Generate(index, i1);

                // extract the vertices and indices to thread safe arrays
                // while still in the async function
                res.points = new Point3D[index.Count];
                for (int i = 0; i < index.Count; i++)
                {
                    res.points[i] = new Point3D(index[i].X, index[i].Y, index[i].Z);
                }
                res.indices = new int[i1.Count];
                for (int i = 0; i < i1.Count; i++)
                {
                    res.indices[i] = i1[i];
                }
                index.Clear();
                i1.Clear();
                */
            return (res);
        }

        private async void GenerateShape()
        {
            ClearShape();
            Viewer.Busy();
            EditingEnabled = false;

            /*
            AsyncGeneratorResult result;
            result = await Task.Run(() => GenerateAsync());
            GetVerticesFromAsyncResult(result);
            */
            maker.ClearSteps();
            foreach (StepRecord sp in StepRecords)
            {
                maker.AddStep(sp.PrimitiveType, sp.Position, sp.SizeX, sp.SizeY, sp.SizeZ, sp.RotX, sp.RotY, sp.RotZ, sp.OpType, sp.Blend, sp.Thickness);
            }
            nextY = maker.Generate(Vertices, Faces);
            //CentreVertices();
            Viewer.NotBusy();
            EditingEnabled = true;
        }

        private GeometryModel3D GenerateShapeForSelectedPrimitive()
        {
            if (selectedPrimitive >= 0 && selectedPrimitive < StepRecords.Count && StepRecords[selectedPrimitive].Selected)
            {
                Point3DCollection verts = new Point3DCollection();
                Int32Collection faces = new Int32Collection();
                StepRecord sp = StepRecords[selectedPrimitive];
                maker.ClearSteps();
                maker.AddStep(sp.PrimitiveType, sp.Position, sp.SizeX * 1.1, sp.SizeY * 1.1, sp.SizeZ * 1.1, sp.RotX, sp.RotY, sp.RotZ, "null", sp.Blend, sp.Thickness);
                maker.Generate(verts, faces);
                return GetModel(verts, faces);
            }
            else
            {
                return null;
            }
        }

        private GeometryModel3D GetModel(Point3DCollection vertices, Int32Collection faces)
        {
            MeshGeometry3D mesh = null;
            GeometryModel3D gm = null;
            Color meshColour = Color.FromArgb(200, 255, 0, 0);
            if (vertices != null && vertices.Count >= 3 && faces != null && faces.Count >= 3)
            {
                mesh = new MeshGeometry3D();
                mesh.Positions = vertices;
                mesh.TriangleIndices = faces;
                mesh.Normals = null;
                gm = new GeometryModel3D();
                gm.Geometry = mesh;

                DiffuseMaterial mt = new DiffuseMaterial();
                mt.Color = meshColour;
                mt.Brush = new SolidColorBrush(meshColour);
                gm.Material = mt;
                DiffuseMaterial mtb = new DiffuseMaterial();
                mtb.Color = Colors.CornflowerBlue;
                mtb.Brush = new SolidColorBrush(Colors.Green);
                gm.BackMaterial = mtb;
            }
            return gm;
        }

        private SdfOperation GetStepOperation(string[] words, ref int i)
        {
            SdfOperation op = null;
            switch (words[i++].ToLower())
            {
                case "union":
                    {
                        op = new SdfUnionOp();
                    }
                    break;

                case "subtraction":
                    {
                        op = new SdfSubtractionOp();
                    }
                    break;

                case "intersection":
                    {
                        op = new SdfIntersectionOp();
                    }
                    break;

                case "smooth union":
                    {
                        op = new SdfSmoothUnionOp();
                        double b = Convert.ToDouble(words[i++]);
                        (op as SdfSmoothUnionOp).Blend = b;
                    }
                    break;

                case "smooth subtraction":
                    {
                        op = new SdfSmoothSubtractionOp();
                        double b = Convert.ToDouble(words[i++]);
                        (op as SdfSmoothSubtractionOp).Blend = b;
                    }
                    break;

                case "smooth intersection":
                    {
                        op = new SdfSmoothIntersectionOp();
                        double b = Convert.ToDouble(words[i++]);
                        (op as SdfSmoothIntersectionOp).Blend = b;
                    }
                    break;

                case "xor":
                    {
                        op = new SdfXorOp();
                    }
                    break;

                case "smooth xor":
                    {
                        op = new SdfSmoothXorOp();
                        double b = Convert.ToDouble(words[i++]);
                        (op as SdfSmoothXorOp).Blend = b;
                    }
                    break;
            }

            return op;
        }

        private void HideClick(object sender, RoutedEventArgs e)
        {
            showCurrent = !showCurrent;
            UpdateDisplay();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters

            string s = EditorParameters.Get("Steps");
            if (s != "")
            {
                SetSteps(s);
            }
        }

        private void OnMoveSelected(object obj)
        {
            string s = obj as string;
            if (selectedPrimitive != -1)
            {
                switch (s.ToLower())
                {
                    case "left":
                        {
                            MoveObject(selectedPrimitive, -1, 0, 0);
                        }
                        break;

                    case "right":
                        {
                            MoveObject(selectedPrimitive, 1, 0, 0);
                        }
                        break;

                    case "up":
                        {
                            MoveObject(selectedPrimitive, 0, 1, 0);
                        }
                        break;

                    case "down":
                        {
                            MoveObject(selectedPrimitive, 0, -1, 0);
                        }
                        break;

                    case "forward":
                        {
                            MoveObject(selectedPrimitive, 0, 0, 1);
                        }
                        break;

                    case "back":
                        {
                            MoveObject(selectedPrimitive, 0, 0, -1);
                        }
                        break;
                }

                UpdateDisplay();
            }
        }

        private bool PreviewViewerKey(Key key, bool shift, bool ctrl, bool alt)
        {
            bool handled = false;
            double dist = 1;
            if (shift)
            {
                dist = 0.1;
            }
            switch (key)
            {
                case Key.Up:
                    {
                        if (ctrl)
                        {
                            MoveObject(selectedPrimitive, 0, 0, -dist);
                        }
                        else
                        {
                            MoveObject(selectedPrimitive, 0, dist, 0);
                        }
                        handled = true;
                    }
                    break;

                case Key.Down:
                    {
                        if (ctrl)
                        {
                            MoveObject(selectedPrimitive, 0, 0, dist);
                        }
                        else
                        {
                            MoveObject(selectedPrimitive, 0, -dist, 0);
                        }
                        handled = true;
                    }
                    break;

                case Key.Left:
                    {
                        MoveObject(selectedPrimitive, -dist, 0, 0);
                        handled = true;
                    }
                    break;

                case Key.Right:
                    {
                        MoveObject(selectedPrimitive, dist, 0, 0);
                        handled = true;
                    }
                    break;
                    /*
                                    case Key.F:
                                        {
                                            if (selectedPrimitive != -1)
                                            {
                                                switch (StepRecords[selectedPrimitive].PrimitiveType.ToLower())
                                                {
                                                    case "box":
                                                        {
                                                        }
                                                        break;

                                                    case "sphere":
                                                        {
                                                            Point3D p = StepRecords[selectedPrimitive].Position;
                                                            p.Y = StepRecords[selectedPrimitive].Radius;
                                                            StepRecords[selectedPrimitive].Position = p;
                                                        }
                                                        break;

                                                    case "torus":
                                                        {
                                                        }
                                                        break;

                                                    case "triangle":
                                                        {
                                                        }
                                                        break;

                                                    case "cylinder":
                                                        {
                                                        }
                                                        break;
                                                }
                                                UpdateDisplay();
                                            }
                                            handled = true;
                                        }
                                        break;
                                        */
            }

            return handled;
        }

        private void Regenerate()
        {
            if (loaded)
            {
                GenerateShape();
                GeometryModel3D sdfModel = GetModel();
                Model3DGroup group = new Model3DGroup();
                Viewer.MultiModels.Children.Add(sdfModel);
                if (selectedPrimitive != -1)
                {
                    GeometryModel3D highlightModel = GenerateShapeForSelectedPrimitive();
                    if (highlightModel != null)
                    {
                        Viewer.MultiModels.Children.Add(highlightModel);
                    }
                }
            }
        }

        private void RegenTimer_Tick(object sender, EventArgs e)
        {
            regenTimer.Stop();
            Regenerate();
        }

        private void ResetDefaults(object sender, RoutedEventArgs e)
        {
            StepRecords.Clear();
            nextRecordId = -1;
            nextY = 0;
            AddBoxAtTop();
            UpdateDisplay();
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            String s = GetStepsAsText();
            EditorParameters.Set("Steps", s);
        }

        private void SelectStep(int selectedPrimitive)
        {
            foreach (StepRecord record in StepRecords)
            {
                record.Selected = false;
            }
            if (selectedPrimitive >= 0 && selectedPrimitive < StepRecords.Count)
            {
                StepRecords[selectedPrimitive].Selected = true;
            }
            UpdateDisplay();
        }

        private void SetDefaults()
        {
            loaded = false;

            loaded = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WarningText = "";
            LoadEditorParameters();

            Viewer.Clear();

            NotifyPropertyChanged("PrimTypes");
            NotifyPropertyChanged("StepRecords");
            loaded = true;

            UpdateDisplay();
        }
    }
}