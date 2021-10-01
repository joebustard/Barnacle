using Barnacle.Dialogs.BezierSurface;
using Barnacle.Models;
using Barnacle.Models.Adorners;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        internal void KeyDown(Key key, bool shift, bool ctrl)
        {
            switch (key)
            {
                case Key.Up:
                    {
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
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParmeters();
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
            KeyDown(e.Key, Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift), Keyboard.IsKeyDown(Key.LeftCtrl));
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            bool leftButton = (e.LeftButton == MouseButtonState.Pressed);
            if (leftButton)
            {
                lastMousePos = e.GetPosition(viewport3D1);
            }
            lastHitModel = null;
            lastHitPoint = new Point3D(0, 0, 0);
            HitTest(viewport3D1, sender, e);
            bool shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            if (lastHitModel != null)
            {
                if (controlPoints.CheckHit(lastHitModel, shift, ref lastSelectedPointRow, ref lastSelectedPointColumn))
                {
                    Redisplay();
                    viewport3D1.Focus();
                }
            }

            base.Viewport_MouseDown(viewport3D1, e);
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
        }

        private void MoveBox(double deltaX, double deltaY, double deltaZ)
        {
            Point3D positionChange = new Point3D(0, 0, 0);
            PolarCamera.Orientations ori = Camera.Orientation;
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
                    controlPoints.MovePoint(lastSelectedPointRow, lastSelectedPointColumn, positionChange);
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

        private void Viewport_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadEditorParameters();
            GenerateShape();
            UpdateCameraPos();
            MyModelGroup.Children.Clear();

            Redisplay();
        }
    }
}