using Barnacle.Dialogs.MeshEditor;
using Barnacle.Models;
using Barnacle.Models.Adorners;
using Barnacle.Object3DLib;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for MeshEditor.xaml
    /// </summary>
    public partial class MeshEditorDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private Int32Collection editingFaceIndices;

        // these are the points and indices of the shape we are editing.
        // We can't use the normal Mesh and Faces as these will be be used for the "soper Mesh"
        // of wire frame rects and selectable points.
        private Point3DCollection editingPoints;

        private Point lastMousePos;
        private int lastSelectedPoint;
        private MeshTriangle lastSelectedTriangle;
        private Mesh mesh;

        private Vector3D moveVector;
        private Point3D offsetOrigin;
        private SculptingTool sculptingTool;
        private Int32Collection selectedPointIndices;
        private bool showWireFrame;

        public MeshEditorDlg()
        {
            InitializeComponent();
            mesh = new Mesh();
            editingPoints = new Point3DCollection();
            selectedPointIndices = new Int32Collection();
            editingFaceIndices = new Int32Collection();

            DataContext = this;

            lastSelectedPoint = -1;
            lastSelectedTriangle = null;
            ModelGroup = MyModelGroup;
            sculptingTool = new SculptingTool();
            moveVector = new Vector3D(0, 0, 0);
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

        public bool ShowWireFrame
        {
            get
            {
                return showWireFrame;
            }
            set
            {
                if (showWireFrame != value)
                {
                    showWireFrame = value;
                    NotifyPropertyChanged();
                }
            }
        }

        internal void HandleKeyDown(Key key, bool shift, bool ctrl)
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

        internal void SetInitialMesh(Point3DCollection points, Int32Collection triangleIndices)
        {
            editingPoints.Clear();
            editingFaceIndices.Clear();
            Bounds3D bnds = new Bounds3D();
            foreach (Point3D p in points)
            {
                bnds.Adjust(p);
            }
            offsetOrigin = new Point3D(bnds.MidPoint().X, -bnds.Lower.Y, bnds.MidPoint().Z);
            foreach (Point3D p in points)
            {
                Point3D np = new Point3D(p.X, p.Y - bnds.Lower.Y, p.Z);
                editingPoints.Add(np);
            }
            foreach (int i in triangleIndices)
            {
                editingFaceIndices.Add(i);
            }
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            Vertices.Clear();
            Faces.Clear();
            mesh.Export(Vertices, Faces);
            mesh.Clear();
            DialogResult = true;
            Close();
        }

        protected override void Redisplay()
        {
            if (MyModelGroup != null)
            {
                MyModelGroup.Children.Clear();

                if (floor != null && ShowFloor)
                {
                    MyModelGroup.Children.Add(floor.FloorMesh);
                    foreach (GeometryModel3D m in grid.Group.Children)
                    {
                        MyModelGroup.Children.Add(m);
                    }
                }

                if (axies != null && ShowAxies)
                {
                    foreach (GeometryModel3D m in axies.Group.Children)
                    {
                        MyModelGroup.Children.Add(m);
                    }
                }
                MyModelGroup.Children.Add(mesh.GetModels());
                if (lastSelectedPoint != -1)
                {
                    MyModelGroup.Children.Add(mesh.Vertices[lastSelectedPoint].Model);
                }
            }
        }

        private void ClearSelection_Click(object sender, RoutedEventArgs e)
        {
            mesh.SelectAll(false);
        }

        private void CreateInitialMesh()
        {
            for (int i = 0; i < editingPoints.Count; i++)
            {
                mesh.AddVertex(editingPoints[i]);
            }
            for (int i = 0; i < editingFaceIndices.Count; i += 3)
            {
                mesh.AddFace(editingFaceIndices[i], editingFaceIndices[i + 1], editingFaceIndices[i + 2]);
            }

            mesh.Initialise();
        }

        private void CreateWireFrame()
        {
        }

        private void Divide_Click(object sender, RoutedEventArgs e)
        {
            mesh.DivideSelectedFaces();

            Redisplay();
        }

        private void DivideLong_Click(object sender, RoutedEventArgs e)
        {
            mesh.DivideLongSideSelectedFaces();
            //     GenerateSuperMesh();
            Redisplay();
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
            HitTest(viewport3D1, e.GetPosition(viewport3D1));
            bool shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            if (lastHitModel != null)
            {
                if (sculptingTool != null)
                {
                    lastHitPoint = new Point3D(1.66, 24.34, 29.13);
                    if (mesh.SelectToolPoints(sculptingTool, lastHitPoint))
                    {
                        Redisplay();
                    }
                }

                base.Viewport_MouseDown(viewport3D1, e);
            }
        }

        private void Grid_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Point newPos = e.GetPosition(viewport3D1);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (Math.Abs(newPos.X - lastMousePos.X) > 2 || Math.Abs(newPos.Y - lastMousePos.Y) > 2)
                {
                    mesh.MoveControlPoints();
                    lastHitModel = null;
                    lastHitPoint = new Point3D(0, 0, 0);
                    HitTest(viewport3D1, newPos);
                    if (lastHitModel != null)
                    {
                        if (mesh.SelectToolPoints(sculptingTool, lastHitPoint))
                        {
                            Redisplay();
                        }
                        else
                        {
                            base.Viewport_MouseMove(viewport3D1, e);
                        }
                    }
                }
            }
            else
            {
                base.Viewport_MouseMove(viewport3D1, e);
            }
            lastMousePos = newPos;
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void MeshGrid_KeyDown(object sender, KeyEventArgs e)
        {
            HandleKeyDown(e.Key, Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift), Keyboard.IsKeyDown(Key.LeftCtrl));
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
                if (lastSelectedPoint != -1)
                {
                    mesh.MovePoint(lastSelectedPoint, positionChange);
                }
                else
                {
                    mesh.MoveSelectedTriangles(positionChange);
                }
            }
            Redisplay();
        }

        private void MovePoint(int pindex, double deltaX, double deltaY, double deltaZ)
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
                mesh.MovePoint(pindex, positionChange);
            }
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

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            mesh.SelectAll(true);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            // we should have been loaded with an editing mesh.
            // If not create base one
            if (editingPoints.Count == 0)
            {
                double radius = 30;
                //GenerateCube(ref editingPoints, ref editingFaceIndices, 50);
                GenerateSphere(editingPoints, editingFaceIndices, new Point3D(0, radius, 0), radius, 20, 20);
                /*
                editingPoints.Add(new Point3D(1, 2, 0));
                editingPoints.Add(new Point3D(3, 2, 0));
                editingPoints.Add(new Point3D(2, 4, 0));

                editingPoints.Add(new Point3D(2, 0, 0));
                editingPoints.Add(new Point3D(4, 4, 0));
                editingPoints.Add(new Point3D(0, 4, 0));

                // center,
                editingFaceIndices.Add(0);
                editingFaceIndices.Add(1);
                editingFaceIndices.Add(2);

                // bottom,
                editingFaceIndices.Add(3);
                editingFaceIndices.Add(1);
                editingFaceIndices.Add(0);

                // right,
                editingFaceIndices.Add(1);
                editingFaceIndices.Add(4);
                editingFaceIndices.Add(2);

                // left,
                editingFaceIndices.Add(0);
                editingFaceIndices.Add(2);
                editingFaceIndices.Add(5);
                CreateInitialMesh();
                mesh.Faces[0].Selected = true;
                mesh.DiagnosticSplitSelectedFaces();
                */
            }
            CreateInitialMesh();

            Redisplay();
        }
    }
}