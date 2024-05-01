using Barnacle.Dialogs.BezierSurface;
using Barnacle.Models;
using Barnacle.Models.Adorners;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public BezierSurfaceDlg()
        {
            InitializeComponent();
            base.RestoreSizeAndLocation();
            ToolName = "BezierSurface";
            DataContext = this;
            ModelGroup = MyModelGroup;

            controlPoints = new ControlPointManager();
            surfaceThickness = 1;
            surface = new Surface();
            surface.controlPointManager = controlPoints;
            surface.Thickness = surfaceThickness;
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
            e.Handled = KeyDownHandler(e.Key, Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift), Keyboard.IsKeyDown(Key.LeftCtrl));
        }

        private bool draggingPoints = false;

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
          //  draggingPoints = false;
            bool leftButton = (e.LeftButton == MouseButtonState.Pressed);
            if (leftButton)
            {
                lastMousePos = e.GetPosition(viewport3D1);

                lastHitModel = null;
                lastHitPoint = new Point3D(0, 0, 0);
                HitTest(viewport3D1, lastMousePos);
                bool shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
                if (lastHitModel != null)
                {
                    if (controlPoints.CheckHit(lastHitModel, shift | draggingPoints, ref lastSelectedPointRow, ref lastSelectedPointColumn))
                    {
                        System.Diagnostics.Debug.WriteLine($" Grid_MouseDown draggingPoint {draggingPoints} ");
                        draggingPoints = true;
                        Redisplay();
                        viewport3D1.Focus();
                        e.Handled = true;
                    }
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
                    //controlPoints.MovePoint(lastSelectedPointRow, lastSelectedPointColumn, positionChange);
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
            controlPoints.ResetControlPoints();
            UpdateDisplay();
        }

        private void ResetControlPointsBow_Click(object sender, RoutedEventArgs e)
        {
            controlPoints.ResetControlPointsBow();
            UpdateDisplay();
        }

        private void ResetControlPointsDisk_Click(object sender, RoutedEventArgs e)
        {
            controlPoints.ResetControlPointsCircle();
            UpdateDisplay();
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