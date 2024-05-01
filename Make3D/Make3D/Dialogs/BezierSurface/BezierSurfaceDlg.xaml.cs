using Barnacle.Dialogs.BezierSurface;
using Barnacle.Models;
using Barnacle.Models.Adorners;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for BezierSurface.xaml
    /// </summary>
    public partial class BezierSurfaceDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private ControlPointManager controlPoints;
        private Point lastMousePos;
        private int lastSelectedPointColumn;
        private int lastSelectedPointRow;
        private Surface surface;
        private double surfaceThickness;
        private String editedPresetText;
        private Dictionary<String, Preset> presets;

        public BezierSurfaceDlg()
        {
            InitializeComponent();
            base.RestoreSizeAndLocation();
            ToolName = "BezierSurface";
            DataContext = this;
            ModelGroup = MyModelGroup;
            availableDimensions = new List<string>();
            availableDimensions.Add("4 x 4");
            availableDimensions.Add("7 x 7");
            availableDimensions.Add("13 x 13");
            availableDimensions.Add("19 x 19");
            selectedDimensions = "13 x 13";
            controlPoints = new ControlPointManager();
            controlPoints.SetDimensions(9, 9);
            surfaceThickness = 1;
            surface = new Surface();
            surface.controlPointManager = controlPoints;
            surface.Thickness = surfaceThickness;
            presets = new Dictionary<string, Preset>();
            presetNames = new List<string>();
            editedPresetText = "";
            LoadPresets();
        }

        public String EditedPresetText
        {
            get { return editedPresetText; }

            set
            {
                if (editedPresetText != value)
                {
                    editedPresetText = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private List<string> presetNames;

        public List<string> PresetNames
        {
            get { return presetNames; }

            set
            {
                if (presetNames != value)
                {
                    presetNames = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private void LoadPresetFile(string dataPath, bool user)
        {
            if (File.Exists(dataPath))
            {
                String[] lines = System.IO.File.ReadAllLines(dataPath);
                if (lines.GetLength(0) > 0)
                {
                    for (int i = 0; i < lines.GetLength(0); i++)
                    {
                        string[] words = lines[i].Split('=');
                        if (words.GetLength(0) == 3)
                        {
                            String pntstr = words[1];
                            Preset p = new Preset();
                            p.Name = words[0];
                            p.Dim = Convert.ToInt32(words[1]);
                            p.Points = words[2];
                            presets[words[0]] = p;
                        }
                    }
                }
            }
        }

        public struct Preset
        {
            public String Name { get; set; }
            public int Dim { get; set; }
            public String Points { get; set; }
        }

        public void LoadPresets()
        {
            // can have two sets of presets one in the installed data and the other user defined.
            try
            {
                presets.Clear();
                PresetNames.Clear();
                string dataPath = "";
                // some paths will want to show all presets some will only want to show presets of
                // their own

                dataPath = AppDomain.CurrentDomain.BaseDirectory + "data\\BezierSurfacePresetPaths.txt";
                LoadPresetFile(dataPath, false);

                dataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "\\Barnacle\\" + ToolName + "PresetPaths.txt";
                LoadPresetFile(dataPath, true);

                foreach (String s in presets.Keys)
                {
                    PresetNames.Add(s);
                }
            }
            catch
            {
            }
        }

        private void SavePresetClick(object sender, RoutedEventArgs e)
        {
            String preset = editedPresetText;
            if (!String.IsNullOrEmpty(preset))
            {
                if (presets.ContainsKey(preset))
                {
                    MessageBox.Show("A preset with that name already exists", "Error");
                }
                else
                {
                    int dim = controlPoints.PatchColumns;
                    SaveAsPreset(ToolName, preset, dim, controlPoints.ToString());
                }
            }
        }

        private void SaveAsPreset(string toolName, string preset, int dim, string points)
        {
            if (!String.IsNullOrEmpty(toolName) && !String.IsNullOrEmpty(preset) && !String.IsNullOrEmpty(points))
            {
                string dataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "\\Barnacle\\" + ToolName + "PresetPaths.txt";
                System.IO.File.AppendAllText(dataPath, $"{preset}={dim}={points}\n");

                Preset p = new Preset();
                p.Name = preset;
                p.Dim = dim;
                p.Points = points;
                presets[preset] = p;
                PresetNames.Clear();
                foreach (String s in presets.Keys)
                {
                    PresetNames.Add(s);
                }
                NotifyPropertyChanged("PresetNames");
            }
        }

        private void ChangeDimensions(string s)
        {
            switch (s)
            {
                case "4 x 4":
                    {
                        controlPoints.SetDimensions(4, 4);
                    }
                    break;

                case "7 x 7":
                    {
                        controlPoints.SetDimensions(7, 7);
                    }
                    break;

                case "19 x 19":
                    {
                        controlPoints.SetDimensions(19, 19);
                    }
                    break;

                case "11 x 11":
                    {
                        controlPoints.SetDimensions(11, 11);
                    }
                    break;

                case "13 x 13":
                    {
                        controlPoints.SetDimensions(13, 13);
                    }
                    break;

                default:
                    break;
            }
        }

        public override bool ShowAxies
        {
            get
            {
                return showAxies;
            }

            set
            {
                if (showAxies != value)
                {
                    showAxies = value;
                    NotifyPropertyChanged();
                    Redisplay();
                }
            }
        }

        public override bool ShowFloor
        {
            get
            {
                return showFloor;
            }

            set
            {
                if (showFloor != value)
                {
                    showFloor = value;
                    NotifyPropertyChanged();
                    Redisplay();
                }
            }
        }

        public double SurfaceThickness
        {
            get
            {
                return surfaceThickness;
            }

            set
            {
                if (surfaceThickness != value)
                {
                    surfaceThickness = value;
                    if (surface != null)
                    {
                        surface.Thickness = surfaceThickness;
                    }
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        internal bool KeyDownHandler(Key key, bool shift, bool ctrl)
        {
            bool handled = false;
            switch (key)
            {
                case Key.F:
                    {
                        FloorPoint();
                        handled = true;
                    }
                    break;

                case Key.Up:
                    {
                        handled = true;
                        if (ctrl)
                        {
                            if (shift)
                            {
                                Nudge(Adorner.NudgeDirection.Back, 0.1);
                            }
                            else
                            {
                                Nudge(Adorner.NudgeDirection.Back, 1.0);
                            }
                        }
                        else
                        {
                            if (shift)
                            {
                                Nudge(Adorner.NudgeDirection.Up, 0.1);
                            }
                            else
                            {
                                Nudge(Adorner.NudgeDirection.Up, 1.0);
                            }
                        }
                    }
                    break;

                case Key.Down:
                    {
                        handled = true;
                        if (ctrl)
                        {
                            if (shift)
                            {
                                Nudge(Adorner.NudgeDirection.Forward, 0.1);
                            }
                            else
                            {
                                Nudge(Adorner.NudgeDirection.Forward, 1.0);
                            }
                        }
                        else
                        {
                            if (shift)
                            {
                                Nudge(Adorner.NudgeDirection.Down, 0.1);
                            }
                            else
                            {
                                Nudge(Adorner.NudgeDirection.Down, 1.0);
                            }
                        }
                    }
                    break;

                case Key.Left:
                    {
                        handled = true;
                        if (shift)
                        {
                            Nudge(Adorner.NudgeDirection.Left, 0.1);
                        }
                        else
                        {
                            Nudge(Adorner.NudgeDirection.Left, 1.0);
                        }
                    }
                    break;

                case Key.Right:
                    {
                        handled = true;
                        if (shift)
                        {
                            Nudge(Adorner.NudgeDirection.Right, 0.1);
                        }
                        else
                        {
                            Nudge(Adorner.NudgeDirection.Right, 1.0);
                        }
                    }
                    break;
            }
            return handled;
        }

        private void FloorPoint()
        {
            controlPoints.FloorPoints();
            GenerateShape();
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParmeters();
            base.SaveSizeAndLocation();
            DialogResult = true;
            Close();
        }

        protected override void Redisplay()
        {
            if (ModelGroup != null)
            {
                ModelGroup.Children.Clear();

                if (floor != null && ShowFloor)
                {
                    ModelGroup.Children.Add(floor.FloorMesh);
                    foreach (GeometryModel3D m in grid.Group.Children)
                    {
                        ModelGroup.Children.Add(m);
                    }
                }

                if (axies != null && ShowAxies)
                {
                    foreach (GeometryModel3D m in axies.Group.Children)
                    {
                        ModelGroup.Children.Add(m);
                    }
                }
                GeometryModel3D gm = GetModel();
                ModelGroup.Children.Add(gm);
                List<GeometryModel3D> ml = controlPoints.Models;
                foreach (GeometryModel3D m in ml)
                {
                    if (m != null)
                    {
                        MyModelGroup.Children.Add(m);
                    }
                }
            }
        }

        protected override GeometryModel3D GetModel()
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.Positions = Vertices;
            mesh.TriangleIndices = Faces;
            mesh.Normals = null;
            GeometryModel3D gm = new GeometryModel3D();
            gm.Geometry = mesh;

            Color transparentSurfaceColour = System.Windows.Media.Color.FromArgb(128, 128, 255, 128);
            DiffuseMaterial mt = new DiffuseMaterial();
            mt.Color = meshColour;
            mt.Brush = new SolidColorBrush(transparentSurfaceColour);
            gm.Material = mt;
            DiffuseMaterial mtb = new DiffuseMaterial();
            mtb.Color = meshColour;
            mtb.Brush = new SolidColorBrush(transparentSurfaceColour);
            gm.BackMaterial = mtb;

            return gm;
        }

        private void DwnDiag1_Click(object sender, RoutedEventArgs e)
        {
            controlPoints.UpDiagPoints(0, 0, controlPoints.PatchRows, controlPoints.PatchColumns, -1);

            controlPoints.GenerateWireFrames();
            UpdateDisplay();
        }

        private void DwnDiag2_Click(object sender, RoutedEventArgs e)
        {
            controlPoints.UpDiagPoints(controlPoints.PatchRows, 0, 0, controlPoints.PatchColumns, -1);

            controlPoints.GenerateWireFrames();
            UpdateDisplay();
        }

        private void DwnX_Click(object sender, RoutedEventArgs e)
        {
            controlPoints.UpXPoints(-1);
            controlPoints.GenerateWireFrames();
            UpdateDisplay();
        }

        private void DwnXZ_Click(object sender, RoutedEventArgs e)
        {
            controlPoints.UpXPoints(-1);
            controlPoints.UpZPoints(1);
            controlPoints.GenerateWireFrames();
            UpdateDisplay();
        }

        private void DwnZ_Click(object sender, RoutedEventArgs e)
        {
            controlPoints.UpZPoints(-1);
            controlPoints.GenerateWireFrames();
            UpdateDisplay();
        }

        private void GenerateShape()
        {
            ClearShape();
            surface.GenerateSurface(Vertices, Faces);
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            draggingPoints = false;
            if (sender != PresetCombos)
            {
                e.Handled = KeyDownHandler(e.Key, Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift), Keyboard.IsKeyDown(Key.LeftCtrl));
            }
        }

        private bool draggingPoints = false;

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {

            bool leftButton = (e.LeftButton == MouseButtonState.Pressed);
            if (leftButton)
            {
                lastMousePos = e.GetPosition(viewport3D1);
                oldMousePos = lastMousePos;

                lastHitModel = null;
                lastHitPoint = new Point3D(0, 0, 0);
                HitTest(viewport3D1, e.GetPosition(viewport3D1));
                bool shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

                if (lastHitModel != null)
                {
                    bool alreadySelected = false;
                    int row = -1;
                    int col = -1;
                    if (controlPoints.CheckHit(lastHitModel, shift, ref alreadySelected, ref row, ref col))
                    {
                        if (!shift && !alreadySelected)
                        {
                            controlPoints.DeselectAll();
                        }

                        controlPoints.Select(row, col);
                        if (alreadySelected)
                        {
                            draggingPoints = true;
                        }
                        else
                        {
                            draggingPoints = false;
                        }
                        Redisplay();
                        viewport3D1.Focus();
                    }
                    e.Handled = true;
                }
            }
            if (!e.Handled)
            {
                base.Viewport_MouseDown(viewport3D1, e);
                draggingPoints = false;
            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            bool leftButton = (e.LeftButton == MouseButtonState.Pressed);
            if (leftButton && draggingPoints)
            {
                Point newPos = e.GetPosition(viewport3D1);
                bool ctrlDown = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
                MouseMovePoints(lastMousePos, newPos, ctrlDown);
                lastMousePos = newPos;
            }
            else
            {
                base.Viewport_MouseMove(sender, e);
            }
        }

        private List<String> availableDimensions;

        public List<String> AvailableDimensions
        {
            get { return availableDimensions; }

            set
            {
                if (value != availableDimensions)
                {
                    availableDimensions = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            System.Diagnostics.Debug.WriteLine($" Grid_MouseUp draggingPoint {draggingPoints} ");
            //draggingPoints = false;

        }

        private void LoadEditorParameters()
        {
            controlPoints.PatchRows = EditorParameters.GetInt("Rows", 13);
            controlPoints.PatchColumns = EditorParameters.GetInt("Columns", 13);
            string pnts = EditorParameters.Get("Points");
            if (!String.IsNullOrEmpty(pnts))
            {
                string[] coords = pnts.Split(' ');
                int i = 0;
                for (int r = 0; r < controlPoints.PatchRows; r++)
                {
                    for (int c = 0; c < controlPoints.PatchColumns; c++)
                    {
                        if (i + 2 < coords.GetLength(0))
                        {
                            double x = Convert.ToDouble(coords[i++]);
                            double y = Convert.ToDouble(coords[i++]);
                            double z = Convert.ToDouble(coords[i++]);
                            controlPoints.SetPointPos(r, c, x, y, z);
                        }
                    }
                }
                controlPoints.GenerateWireFrames();
            }
        }

        private void MoveBox(double deltaX, double deltaY, double deltaZ)
        {
            Point3D positionChange = new Point3D(0, 0, 0);
            PolarCamera.Orientations ori = Camera.HorizontalOrientation;
            switch (ori)
            {
                case PolarCamera.Orientations.Front:
                    {
                        positionChange = new Point3D(1 * deltaX, 1 * deltaY, -1 * deltaZ);
                    }
                    break;

                case PolarCamera.Orientations.Back:
                    {
                        positionChange = new Point3D(-1 * deltaX, 1 * deltaY, 1 * deltaZ);
                    }
                    break;

                case PolarCamera.Orientations.Left:
                    {
                        positionChange = new Point3D(1 * deltaZ, 1 * deltaY, 1 * deltaX);
                    }
                    break;

                case PolarCamera.Orientations.Right:
                    {
                        positionChange = new Point3D(-1 * deltaZ, 1 * deltaY, -1 * deltaX);
                    }
                    break;
            }

            if (positionChange != null)
            {
                if (lastSelectedPointRow != -1)
                {
                    controlPoints.MoveSelectedPoints(positionChange);
                    GenerateShape();
                }
            }
            Redisplay();
        }

        private void Nudge(Adorner.NudgeDirection dir, double v)
        {
            switch (dir)
            {
                case Adorner.NudgeDirection.Left:
                    {
                        MoveBox(-v, 0, 0);
                    }
                    break;

                case Adorner.NudgeDirection.Right:
                    {
                        MoveBox(v, 0, 0);
                    }
                    break;

                case Adorner.NudgeDirection.Up:
                    {
                        MoveBox(0, v, 0);
                    }
                    break;

                case Adorner.NudgeDirection.Down:
                    {
                        MoveBox(0, -v, 0);
                    }
                    break;

                case Adorner.NudgeDirection.Forward:
                    {
                        MoveBox(0, 0, -v);
                    }
                    break;

                case Adorner.NudgeDirection.Back:
                    {
                        MoveBox(0, 0, v);
                    }
                    break;
            }
        }

        private void ResetControlPoints_Click(object sender, RoutedEventArgs e)
        {
            if (ConfirmDiscard())
            {
                controlPoints.ResetControlPoints();
                UpdateDisplay();
            }
        }

        private bool ConfirmDiscard()
        {
            MessageBoxResult mbr = MessageBox.Show("This will reset all control points. Any changes will be lost.", "Caution", MessageBoxButton.OKCancel);
            return (mbr == MessageBoxResult.OK);
        }

        private void ResetControlPointsBow_Click(object sender, RoutedEventArgs e)
        {
            if (ConfirmDiscard())
            {
                controlPoints.ResetControlPointsBow();
                UpdateDisplay();
            }
        }

        private void ResetControlPointsDisk_Click(object sender, RoutedEventArgs e)
        {
            if (ConfirmDiscard())
            {
                controlPoints.ResetControlPointsCircle();

                UpdateDisplay();
            }
        }

        private string selectedDimensions;

        public String SelectedDimensions
        {
            get { return selectedDimensions; }

            set
            {
                if (value != selectedDimensions)
                {
                    if (ConfirmDiscard())
                    {
                        selectedDimensions = value;
                        ChangeDimensions(selectedDimensions);
                        UpdateDisplay();
                        NotifyPropertyChanged();
                    }
                }
            }
        }

        private void ResetControlPointsTube_Click(object sender, RoutedEventArgs e)
        {
            if (ConfirmDiscard())
            {
                controlPoints.ResetControlPointsHalfTube();
                UpdateDisplay();
            }
        }

        private void SaveEditorParmeters()
        {
            EditorParameters.Set("Rows", controlPoints.PatchRows.ToString());
            EditorParameters.Set("Columns", controlPoints.PatchColumns.ToString());
            EditorParameters.Set("Points", controlPoints.ToString());
        }

        private void UpdateDisplay()
        {
            GenerateShape();
            Redisplay();
        }

        private void UpDiag1_Click(object sender, RoutedEventArgs e)
        {
            controlPoints.UpDiagPoints(0, 0, controlPoints.PatchRows, controlPoints.PatchColumns, 1);

            controlPoints.GenerateWireFrames();
            UpdateDisplay();
        }

        private void UpDiag2_Click(object sender, RoutedEventArgs e)
        {
            controlPoints.UpDiagPoints(controlPoints.PatchRows, 0, 0, controlPoints.PatchColumns, 1);

            controlPoints.GenerateWireFrames();
            UpdateDisplay();
        }

        private void UpX_Click(object sender, RoutedEventArgs e)
        {
            controlPoints.UpXPoints(1);
            controlPoints.GenerateWireFrames();
            UpdateDisplay();
        }

        private void UpXZ_Click(object sender, RoutedEventArgs e)
        {
            controlPoints.UpXPoints(1);
            controlPoints.UpZPoints(1);
            controlPoints.GenerateWireFrames();
            UpdateDisplay();
        }

        private void UpZ_Click(object sender, RoutedEventArgs e)
        {
            controlPoints.UpZPoints(1);
            controlPoints.GenerateWireFrames();
            UpdateDisplay();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadEditorParameters();
            NotifyPropertyChanged("AvailableDimensions");
            GenerateShape();
            UpdateCameraPos();
            MyModelGroup.Children.Clear();

            Redisplay();
        }

        private void MouseMovePoints(Point lastPos, Point newPos, bool ctrlDown)
        {
            double dr = Math.Sqrt(Camera.Distance);
            double deltaX = (newPos.X - lastPos.X) / dr;

            double deltaY;
            double deltaZ;

            if (!ctrlDown)
            {
                deltaY = -(newPos.Y - lastPos.Y) / dr;
                deltaZ = 0;
            }
            else
            {
                deltaY = 0;
                deltaZ = -(newPos.Y - lastPos.Y) / dr;
            }

            Point3D positionChange = new Point3D(0, 0, 0);
            PolarCamera.Orientations ori = Camera.HorizontalOrientation;
            switch (ori)
            {
                case PolarCamera.Orientations.Front:
                    {
                        positionChange = new Point3D(1 * deltaX, 1 * deltaY, -1 * deltaZ);
                    }
                    break;

                case PolarCamera.Orientations.Back:
                    {
                        positionChange = new Point3D(-1 * deltaX, 1 * deltaY, 1 * deltaZ);
                    }
                    break;

                case PolarCamera.Orientations.Left:
                    {
                        positionChange = new Point3D(1 * deltaZ, 1 * deltaY, 1 * deltaX);
                    }
                    break;

                case PolarCamera.Orientations.Right:
                    {
                        positionChange = new Point3D(-1 * deltaZ, 1 * deltaY, -1 * deltaX);
                    }
                    break;
            }

            controlPoints.MoveSelectedPoints(positionChange);
            GenerateShape();
        }
    }
}