using EarClipperLib;
using Make3D.Models;
using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml;

namespace Make3D.Dialogs
{
    /// <summary>
    /// Interaction logic for ReoriginDlg.xaml

    /// <summary>
    /// Interaction logic for Reorigin.xaml
    /// </summary>
    public partial class ReoriginDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private Object3D localOb;
        private Object3D original;

        public ReoriginDlg()
        {
            InitializeComponent();
            ToolName = "Reorigin";
            DataContext = this;
            ModelGroup = MyModelGroup;
        }

        private enum NudgeDirection
        {
            Left,
            Right,
            Up,
            Down, Forward,
            Back
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

        internal void SetObject(Object3D ob)
        {
            localOb = ob.Clone();
            localOb.Position = new Point3D(0, 0, 0);
            original = ob;
        }

        protected GeometryModel3D GetModel(Object3D ob)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.Positions = ob.AbsoluteObjectVertices;
            mesh.TriangleIndices = ob.TriangleIndices;
            mesh.Normals = null;
            GeometryModel3D gm = new GeometryModel3D();
            gm.Geometry = mesh;

            DiffuseMaterial mt = new DiffuseMaterial();
            mt.Color = ob.Color;
            mt.Brush = new SolidColorBrush(ob.Color);
            gm.Material = mt;
            DiffuseMaterial mtb = new DiffuseMaterial();
            mtb.Color = Colors.CornflowerBlue;
            mtb.Brush = new SolidColorBrush(Colors.Green);
            gm.BackMaterial = mtb;

            return gm;
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParmeters();
            double dx = localOb.Position.X;
            double dy = localOb.Position.Y;
            double dz = localOb.Position.Z;
            Point3DCollection pn = new Point3DCollection();
            for (int i = 0; i < localOb.RelativeObjectVertices.Count; i++)
            {
                pn.Add(new Point3D(localOb.RelativeObjectVertices[i].X + dx,
                    localOb.RelativeObjectVertices[i].Y + dy,
                    localOb.RelativeObjectVertices[i].Z + dz));
            }
            localOb.RelativeObjectVertices = pn;
            original.Position = localOb.Position;
            original.RelativeObjectVertices = localOb.RelativeObjectVertices;
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

                if (localOb != null)
                {
                    localOb.Remesh();
                    GeometryModel3D gm = GetModel(localOb);
                    MyModelGroup.Children.Add(gm);
                }
            }
        }

        private void GenerateShape()
        {
            ClearShape();
        }

        private void Grid_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            ModifierKeys mk = e.KeyboardDevice.Modifiers;

            bool shift = mk.HasFlag(ModifierKeys.Shift);
            bool ctrl = mk.HasFlag(ModifierKeys.Control);
            switch (e.Key)
            {
                case Key.Left:
                    {
                        if (shift)
                        {
                            Nudge(-0.1, 0, 0);
                        }
                        else
                        {
                            Nudge(-1, 0, 0);
                        }
                    }
                    break;

                case Key.Right:
                    {
                        if (shift)
                        {
                            Nudge(0.1, 0, 0);
                        }
                        else
                        {
                            Nudge(1, 0, 0);
                        }
                    }
                    break;

                case Key.Up:
                    {
                        if (ctrl)
                        {
                            if (shift)
                            {
                                Nudge(0, 0, 0.1);
                            }
                            else
                            {
                                Nudge(0, 0, 1);
                            }
                        }
                        else
                        {
                            if (shift)
                            {
                                Nudge(0, 0.1, 0);
                            }
                            else
                            {
                                Nudge(0, 1, 0);
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
                                Nudge(0, 0, -0.1);
                            }
                            else
                            {
                                Nudge(0, 0, -1);
                            }
                        }
                        else
                        {
                            if (shift)
                            {
                                Nudge(0, -0.1, 0);
                            }
                            else
                            {
                                Nudge(0, -1, 0);
                            }
                        }
                    }
                    break;
            }
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
        }

        private void Nudge(double deltaX, double deltaY, double deltaZ)
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
                localOb.Position = new Point3D(localOb.Position.X + positionChange.X, localOb.Position.Y + positionChange.Y, localOb.Position.Z + positionChange.Z);
                Redisplay();
            }
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadEditorParameters();
            GenerateShape();
            UpdateCameraPos();
            MyModelGroup.Children.Clear();

            Redisplay();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Point3D min = new Point3D(double.MaxValue, double.MaxValue, double.MaxValue);
            Point3D max = new Point3D(double.MinValue, double.MinValue, double.MinValue);
            PointUtils.MinMax(localOb.RelativeObjectVertices, ref min, ref max);
            double dx = -(min.X + (max.X - min.X) / 2.0);
            double dy = -(min.Y + (max.Y - min.Y) / 2.0);
            double dz = -(min.Z + (max.Z - min.Z) / 2.0);

          
            Point3DCollection pn = new Point3DCollection();
            for (int i = 0; i < localOb.RelativeObjectVertices.Count; i++)
            {
                pn.Add(new Point3D(localOb.RelativeObjectVertices[i].X + dx,
                    localOb.RelativeObjectVertices[i].Y + dy,
                    localOb.RelativeObjectVertices[i].Z + dz));
            }
            localOb.RelativeObjectVertices = pn;
            localOb.Position = new Point3D(0, 0, 0);
            localOb.Remesh();
            Redisplay();
        }
    }
}