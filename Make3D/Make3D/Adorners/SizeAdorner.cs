using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace Barnacle.Models.Adorners
{
    public class SizeAdorner : Adorner
    {
        private const double tbMargin = 10;
        private const double textBoxHeight = 25;
        private const double textBoxWidth = 50;
        private Bounds3D bounds;
        private Object3D box;
        private bool boxSelected;

        private DispatcherTimer keyboardTimer;

        private Dictionary<string, Point3D> label3DOffsets;

        private Dictionary<string, Point3D> labelLocations;

        private DispatcherTimer refreshTimer;
        private Point3D scaleChange = new Point3D(1, 1, 1);
        private Object3D selectedThumb;

        private Dictionary<string, Control> thumbLabels;

        private List<Object3D> thumbs;
        public SizeAdorner(PolarCamera camera)
        {
            NotificationManager.ViewUnsubscribe("SizeAdorner");
    
            Camera = camera;
            Adornments = new Model3DCollection();
            SelectedObjects = new List<Object3D>();
            thumbs = new List<Object3D>();
            selectedThumb = null;
            box = null;
            boxSelected = false;
            bounds = new Bounds3D();

            labelLocations = new Dictionary<string, Point3D>();
            label3DOffsets = new Dictionary<string, Point3D>();
            //thumbLabels = new List<Label>();
            thumbLabels = new Dictionary<string, Control>();
            ViewPort = null;
            refreshTimer = new DispatcherTimer();
            refreshTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000);
            refreshTimer.Tick += RefreshTimer_Tick;

            keyboardTimer = new DispatcherTimer();
            keyboardTimer.Interval = new TimeSpan(0, 0, 5);
            keyboardTimer.Tick += KeyboardTimer_Tick;

            NotificationManager.Subscribe("SizeAdorner", "ScaleRefresh", OnScaleRefresh);
            NotificationManager.Subscribe("SizeAdorner", "CameraMoved", OnCameraMoved);
            NotificationManager.Subscribe("SizeAdorner", "CameraOrientation", OnCameraOrientation);
        }

        private void OnCameraOrientation(object param)
        {
            if (labelLocations.Count > 0)
            {
                UpdateLabelVisibility();
            }
        }

        private void OnCameraMoved(object param)
        {
            if (labelLocations.Count > 0)
            {
                UpdateLabelVisibility();
            }
        }

        public Bounds3D Bounds
        {
            get { return bounds; }
        }

        internal PolarCamera Camera { get; set; }

        public override void AdornObject(Object3D obj)
        {
            selectedThumb = null;
            boxSelected = false;
            obj.CalcScale(false);
            SelectedObjects.Add(obj);
            GenerateAdornments();
        }

        public override void Clear()
        {
            Adornments.Clear();
            SelectedObjects.Clear();
            thumbs.Clear();
            selectedThumb = null;
            box = null;
            boxSelected = false;
            bounds = new Bounds3D();
            Overlay?.Children.Clear();
            labelLocations.Clear();
            label3DOffsets.Clear();
        }

        internal override void GenerateAdornments()
        {
            Adornments.Clear();
            thumbs.Clear();
            labelLocations.Clear();
            label3DOffsets.Clear();
            Overlay?.Children.Clear();
            bool addSizeThumbs = true;
            foreach (Object3D obj in SelectedObjects)
            {
                if (!obj.IsSizable())
                {
                    addSizeThumbs = false;
                }
            }
            Point3D midp, size;
            CaclBoxSize(out midp, out size);
            CreateAdornments(midp, size, addSizeThumbs);
        }

        internal override bool MouseMove(Point lastPos, Point newPos, MouseEventArgs e, bool ctrlDown)
        {
            bool handled = false;
            // ignore if the left moouse button is up
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (boxSelected)
                {
                    handled = true;
                    MouseMoveBox(lastPos, newPos, ctrlDown);
                }
                else
                {
                    if (selectedThumb != null)
                    {
                        handled = true;
                        MouseMoveThumb(lastPos, newPos);
                    }
                }
                if (labelLocations.Count > 0)
                {
                    UpdateLabelVisibility();
                }
            }
            return handled;
        }

        private void UpdateLabelVisibility()
        {
            switch (Camera.VerticalOrientation)
            {
                case PolarCamera.Orientations.Top:
                    {
                        TopLabelVisibility();
                    }
                    break;
                case PolarCamera.Orientations.Bottom:
                    {
                        BottomLabelVisibility();
                    }
                    break;

            }
        }

        private void BottomLabelVisibility()
        {
            switch (Camera.HorizontalOrientation)
            {
                case PolarCamera.Orientations.Front:
                    {
                        SetLabelVisibility("TopThumb", false);
                        SetLabelVisibility("BottomThumb", true);
                        SetLabelVisibility("LeftThumb", false);
                        SetLabelVisibility("RightThumb", true);
                        SetLabelVisibility("FrontThumb", true);
                        SetLabelVisibility("BackThumb", false);
                    }
                    break;

                case PolarCamera.Orientations.Back:
                    {
                        SetLabelVisibility("TopThumb", false);
                        SetLabelVisibility("BottomThumb", true);
                        SetLabelVisibility("LeftThumb", true);
                        SetLabelVisibility("RightThumb", false);
                        SetLabelVisibility("FrontThumb", false);
                        SetLabelVisibility("BackThumb", true);
                    }
                    break;
                case PolarCamera.Orientations.Left:
                    {
                        SetLabelVisibility("TopThumb", false);
                        SetLabelVisibility("BottomThumb", true);
                        SetLabelVisibility("LeftThumb", true);
                        SetLabelVisibility("RightThumb", false);
                        SetLabelVisibility("FrontThumb", true);
                        SetLabelVisibility("BackThumb", false);
                    }
                    break;
                case PolarCamera.Orientations.Right:
                    {
                        SetLabelVisibility("TopThumb", false);
                        SetLabelVisibility("BottomThumb", true);
                        SetLabelVisibility("LeftThumb", false);
                        SetLabelVisibility("RightThumb", true);
                        SetLabelVisibility("FrontThumb", false);
                        SetLabelVisibility("BackThumb", true);
                    }
                    break;
            }
        }

        private void TopLabelVisibility()
        {
            switch (Camera.HorizontalOrientation)
            {
                case PolarCamera.Orientations.Front:
                    {
                        SetLabelVisibility("TopThumb", true);
                        SetLabelVisibility("BottomThumb", false);
                        SetLabelVisibility("LeftThumb", false);
                        SetLabelVisibility("RightThumb", true);
                        SetLabelVisibility("FrontThumb", true);
                        SetLabelVisibility("BackThumb", false);
                    }
                    break;

                case PolarCamera.Orientations.Back:
                    {
                        SetLabelVisibility("TopThumb", true);
                        SetLabelVisibility("BottomThumb", false);
                        SetLabelVisibility("LeftThumb", true);
                        SetLabelVisibility("RightThumb", false);
                        SetLabelVisibility("FrontThumb", false);
                        SetLabelVisibility("BackThumb", true);
                    }
                    break;
                case PolarCamera.Orientations.Left:
                    {
                        SetLabelVisibility("TopThumb", true);
                        SetLabelVisibility("BottomThumb", false);
                        SetLabelVisibility("LeftThumb", true);
                        SetLabelVisibility("RightThumb", false);
                        SetLabelVisibility("FrontThumb", true);
                        SetLabelVisibility("BackThumb", false);
                    }
                    break;
                case PolarCamera.Orientations.Right:
                    {
                        SetLabelVisibility("TopThumb", true);
                        SetLabelVisibility("BottomThumb", false);
                        SetLabelVisibility("LeftThumb", false);
                        SetLabelVisibility("RightThumb", true);
                        SetLabelVisibility("FrontThumb", false);
                        SetLabelVisibility("BackThumb", true);
                    }
                    break;
            }

        }

        private void SetLabelVisibility(string v1, bool v2)
        {
            if (thumbLabels.ContainsKey(v1))
            {
                Visibility vs;
                if (v2)
                {
                    vs = Visibility.Visible;
                }
                else
                {
                    vs = Visibility.Hidden;
                }
                thumbLabels[v1].Visibility = vs;
                PositionLabel(v1);
            }

        }

        internal override void MouseUp()
        {
            selectedThumb = null;
            boxSelected = false;
        }

        internal override void Nudge(Adorner.NudgeDirection dir, double v)
        {
            Overlay.Children.Clear();
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

        internal override bool Select(GeometryModel3D geo)
        {
            bool handled = false;

            if (box != null)
            {
                if (box.Mesh == geo.Geometry)
                {
                    handled = true;
                    boxSelected = true;
                }
                else
                {
                    foreach (Object3D thumb in thumbs)
                    {
                        if (thumb.Mesh == geo.Geometry)
                        {
                            handled = true;
                            selectedThumb = thumb;
                            break;
                        }
                    }
                }
            }
            return handled;
        }

        private void AddLabel(string name, double x, double y, double z, string v2, HorizontalAlignment ha)
        {
            labelLocations[name] = new Point3D(x, y, z);
            TextBox l = new TextBox();
            l.Text = v2;
            l.Background = new SolidColorBrush(Color.FromArgb(64, 255, 255, 255));
            l.FontWeight = FontWeights.Bold;
            l.FontSize = 18;
            l.Tag = name;
            l.Name = name;
            l.MinWidth = textBoxWidth;
            l.Height = textBoxHeight;
            l.TextChanged += L_TextChanged;
            l.HorizontalAlignment = ha;
            l.GotFocus += L_GotFocus;

            thumbLabels[name] = l;
        }

        private void CaclBoxSize(out Point3D midp, out Point3D size)
        {
            bounds = new Bounds3D();
            foreach (Object3D obj in SelectedObjects)
            {
                bounds += obj.AbsoluteBounds;
            }
            midp = bounds.MidPoint();
            size = bounds.Size();
        }

        private void Create3DLabelOffset(string name, double x, double y, double z)
        {
            label3DOffsets[name] = new Point3D(x, y,z);
        }

        private void CreateAdornments(Point3D position, Point3D size, bool addSizeThumbs)
        {
            CreateTransparentBox(position, size);
            Adornments.Add(GetMesh(box));

            if (addSizeThumbs)
            {
                double thumbSize = 4;
                CreateThumb(position, thumbSize, box.AbsoluteBounds.Width / 2, 0, 0, Colors.White, "RightThumb");
                Create3DLabelOffset("RightThumb", tbMargin, 0,0);

                CreateThumb(position, thumbSize, -box.AbsoluteBounds.Width / 2, 0, 0, Colors.White, "LeftThumb");
                Create3DLabelOffset("LeftThumb", -tbMargin, 0,0);

                CreateThumb(position, thumbSize, 0, box.AbsoluteBounds.Height / 2, 0, Colors.White, "TopThumb");
                Create3DLabelOffset("TopThumb", 0,  tbMargin ,0);

                CreateThumb(position, thumbSize, 0, -box.AbsoluteBounds.Height / 2, 0, Colors.White, "BottomThumb");
                Create3DLabelOffset("BottomThumb", 0, -tbMargin,0);

                CreateThumb(position, thumbSize, 0, 0, box.AbsoluteBounds.Depth / 2, Colors.White, "FrontThumb");
                Create3DLabelOffset("FrontThumb", 0, 0,tbMargin);

                CreateThumb(position, thumbSize, 0, 0, -box.AbsoluteBounds.Depth / 2, Colors.White, "BackThumb");
                Create3DLabelOffset("BackThumb", 0, 0,-tbMargin);

                CreateTextLabels(size);
                UpdateLabelVisibility();
            }
        }

        private void CreateTextLabels(Point3D size, String name = "")
        {
            labelLocations.Clear();
            thumbLabels.Clear();
            Overlay.Children.Clear();
            foreach (Object3D th in thumbs)
            {
                if (name == "" || th.Name == name)
                {
                    switch (th.Name)
                    {
                        case "TopThumb":
                            {
                                AddLabel("TopThumb", th.Position.X, th.Position.Y, th.Position.Z, size.Y.ToString("F3"), HorizontalAlignment.Center);
                            }
                            break;

                        case "BottomThumb":
                            {
                                AddLabel("BottomThumb", th.Position.X, th.Position.Y, th.Position.Z, size.Y.ToString("F3"), HorizontalAlignment.Center);
                            }
                            break;

                        case "LeftThumb":
                            {
                                FormattedText formattedText = new FormattedText(size.X.ToString("F3"), System.Globalization.CultureInfo.CurrentCulture,
                                                                   FlowDirection.LeftToRight, new Typeface("Arial"), 14, Brushes.Black);
                                FormattedText txt = formattedText;
                               // label3DOffsets["LeftThumb"] = new Point(-100, -textBoxHeight / 2);
                                AddLabel("LeftThumb", th.AbsoluteBounds.Lower.X, th.Position.Y, th.Position.Z, size.X.ToString("F3"), HorizontalAlignment.Right);
                            }
                            break;

                        case "RightThumb":
                            {
                                AddLabel("RightThumb", th.AbsoluteBounds.Upper.X, th.Position.Y, th.Position.Z, size.X.ToString("F3"), HorizontalAlignment.Left);
                            }
                            break;

                        case "FrontThumb":
                            {
                               
                                    AddLabel("FrontThumb", th.AbsoluteBounds.Upper.X, th.Position.Y, th.Position.Z, size.Z.ToString("F3"), HorizontalAlignment.Center);
                                
                            }
                            break;

                        case "BackThumb":
                            {
                                    AddLabel("BackThumb", th.AbsoluteBounds.Upper.X, th.Position.Y, th.Position.Z, size.Z.ToString("F3"), HorizontalAlignment.Center);
                            }
                            break;
                    }
                }
            }

            if (labelLocations.Count > 0)
            {
                PositionLabels();
            }
        }

        private void CreateThumb(Point3D position, double thumbSize, double x, double y, double z, Color col, string name)
        {
            Object3D thumb = new Object3D();

            Point3DCollection pnts = new Point3DCollection();
            Int32Collection indices = new Int32Collection();
            Vector3DCollection normals = new Vector3DCollection();
            PrimitiveGenerator.GenerateCube(ref pnts, ref indices, ref normals);
            thumb.Name = name;
            List<P3D> tmp = new List<P3D>();
            PointUtils.PointCollectionToP3D(pnts, tmp);

            thumb.RelativeObjectVertices = tmp;
            thumb.TriangleIndices = indices;
            thumb.Normals = normals;

            position.X += x;
            position.Y += y;
            position.Z += z;
            thumb.Position = position;
            thumb.Scale = new Scale3D(thumbSize, thumbSize, thumbSize);
            thumb.ScaleMesh(thumbSize, thumbSize, thumbSize);
            thumb.Color = col;
            thumb.Remesh();
            thumbs.Add(thumb);
            Adornments.Add(GetMesh(thumb));
        }

        private void CreateTransparentBox(Point3D position, Point3D size)
        {
            // create the main semi transparent part of of the adorner
            box = new Object3D();

            Point3DCollection pnts = new Point3DCollection();
            Int32Collection indices = new Int32Collection();
            Vector3DCollection normals = new Vector3DCollection();
            PrimitiveGenerator.GenerateCube(ref pnts, ref indices, ref normals);
            List<P3D> tmp = new List<P3D>();
            PointUtils.PointCollectionToP3D(pnts, tmp);

            box.RelativeObjectVertices = tmp;
            box.TriangleIndices = indices;
            box.Normals = normals;
            box.Position = position;
            box.ScaleMesh(size.X + 0.01, size.Y + 0.01, size.Z + 0.01);

            box.Color = Color.FromArgb(150, 64, 64, 64);
            box.Remesh();
            box.CalcScale(false);
        }

        private void FocusChanged()
        {
            AutomaticScaleUpdate();
        }

        private void AutomaticScaleUpdate()
        {
            keyboardTimer.Stop();
            if (scaleChange.X != 1 || scaleChange.Y != 1 || scaleChange.Z != 1)
            {
                UpdateScale();
                refreshTimer?.Start();
            }

            scaleChange = new Point3D(1, 1, 1);
           
        }

        private void KeyboardTimer_Tick(object sender, EventArgs e)
        {
            AutomaticScaleUpdate();
        }

        private void L_GotFocus(object sender, RoutedEventArgs e)
        {
            FocusChanged();
        }

        private void L_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox)
            {
                keyboardTimer.Stop();
                TextBox tx = (sender as TextBox);
                double dim = 0;
                if (ValidDimension(tx.Text, out dim))
                {
                    string name = tx.Tag.ToString();
                    switch (name)
                    {
                        case "RightThumb":
                        case "LeftThumb":
                            {
                                if (SelectedObjects.Count > 1)
                                {
                                    scaleChange.X = dim / box.Scale.X;
                                }
                                else
                                {
                                    scaleChange.X = dim / SelectedObjects[0].Scale.X;
                                }
                            }
                            break;

                        case "TopThumb":
                        case "BottomThumb":
                            {
                                if (SelectedObjects.Count > 1)
                                {
                                    scaleChange.Y = dim / box.Scale.Y;
                                }
                                else
                                {
                                    scaleChange.Y = dim / SelectedObjects[0].Scale.Y;
                                }
                            }
                            break;

                        case "FrontThumb":
                        case "BackThumb":
                            {
                                if (SelectedObjects.Count > 1)
                                {
                                    scaleChange.Z = dim / box.Scale.Z;
                                }
                                else
                                {
                                    scaleChange.Z = dim / SelectedObjects[0].Scale.Z;
                                }
                            }
                            break;

                        default:
                            break;
                    }
                }
                keyboardTimer.Start();
            }
        }

        private void MouseMoveBox(Point lastPos, Point newPos, bool ctrlDown)
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

            MoveBox(deltaX, deltaY, deltaZ);
        }

        private void MouseMoveThumb(Point lastPos, Point newPos)
        {
            double dr = Math.Sqrt(Camera.Distance);

            double deltaX = (newPos.X - lastPos.X);
            double deltaY = (newPos.Y - lastPos.Y);
            var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            var dpiYProperty = typeof(SystemParameters).GetProperty("Dpi", BindingFlags.NonPublic | BindingFlags.Static);

            var dpiX = (int)dpiXProperty.GetValue(null, null);
            var dpiY = (int)dpiYProperty.GetValue(null, null);
            double mmx = (deltaX * 25.4) / dpiX;
            double mmy = (deltaY * 25.4) / dpiY;
            mmx /= dr;
            mmy /= dr;

            Point3D scaleChange = new Point3D(1, 1, 1);
            Point3D positionChange = new Point3D(0, 0, 0);
            switch (selectedThumb.Name.ToLower())
            {
                case "rightthumb":
                    {
                        if (deltaX != 0)
                        {
                            if (Camera.HorizontalOrientation == PolarCamera.Orientations.Front)
                            {
                                //scaleChange.X = ((box.Scale.X + (mmx)) / box.Scale.X);
                                scaleChange.X = (1 + mmx);
                                positionChange.X += ((box.AbsoluteBounds.Width * scaleChange.X) - box.AbsoluteBounds.Width) / 2.0;
                            }
                            else
                            {
                                if (Camera.HorizontalOrientation == PolarCamera.Orientations.Back)
                                {
                                    //scaleChange.X = ((box.Scale.X - (mmx)) / box.Scale.X);
                                    scaleChange.X = (1 - mmx);
                                    positionChange.X += ((box.AbsoluteBounds.Width * scaleChange.X) - box.AbsoluteBounds.Width) / 2.0;
                                }
                            }
                        }
                        if (scaleChange.X <= 0)
                        {
                            scaleChange.X = 1;
                        }
                    }
                    break;

                case "leftthumb":
                    {
                        if (deltaX != 0)
                        {
                            if (Camera.HorizontalOrientation == PolarCamera.Orientations.Front)
                            {
                                //scaleChange.X = ((box.Scale.X - (mmx)) / box.Scale.X);
                                scaleChange.X = (1 - mmx);
                                positionChange.X -= ((box.AbsoluteBounds.Width * scaleChange.X) - box.AbsoluteBounds.Width) / 2.0;
                            }
                            else
                            {
                                if (Camera.HorizontalOrientation == PolarCamera.Orientations.Back)
                                {
                                    // scaleChange.X = ((box.Scale.X + (mmx)) / box.Scale.X);
                                    scaleChange.X = (1 + mmx);
                                    positionChange.X -= ((box.AbsoluteBounds.Width * scaleChange.X) - box.AbsoluteBounds.Width) / 2.0;
                                }
                            }
                        }
                        if (scaleChange.X <= 0)
                        {
                            scaleChange.X = 1;
                        }
                    }
                    break;

                case "topthumb":
                    {
                        if (deltaY != 0)
                        {
                            //scaleChange.Y = ((box.Scale.Y - (mmy)) / box.Scale.Y);
                            scaleChange.Y = (1 - mmy);
                            positionChange.Y += ((box.AbsoluteBounds.Height * scaleChange.Y) - box.AbsoluteBounds.Height) / 2.0;
                        }
                        if (scaleChange.Y <= 0)
                        {
                            scaleChange.Y = 1;
                        }
                    }
                    break;

                case "bottomthumb":
                    {
                        if (deltaY != 0)
                        {
                            //scaleChange.Y = ((box.Scale.Y + (mmy)) / box.Scale.Y);
                            scaleChange.Y = (1 + mmy);
                            positionChange.Y -= ((box.AbsoluteBounds.Height * scaleChange.Y) - box.AbsoluteBounds.Height) / 2.0;
                        }
                        if (scaleChange.Y <= 0)
                        {
                            scaleChange.Y = 1;
                        }
                    }
                    break;

                case "frontthumb":
                    {
                        if (deltaX != 0)
                        {
                            if (Camera.HorizontalOrientation == PolarCamera.Orientations.Left)
                            {
                                //scaleChange.Z = ((box.Scale.Z + (mmx)) / box.Scale.Z);
                                scaleChange.Z = (1 + mmx);
                                positionChange.Z += ((box.AbsoluteBounds.Depth * scaleChange.Z) - box.AbsoluteBounds.Depth) / 2.0;
                            }
                            else
                            {
                                if (Camera.HorizontalOrientation == PolarCamera.Orientations.Right)
                                {
                                    //scaleChange.Z = ((box.Scale.Z - (mmx)) / box.Scale.Z);
                                    scaleChange.Z = (1 - mmx);
                                    positionChange.Z = ((box.AbsoluteBounds.Depth * scaleChange.Z) - box.AbsoluteBounds.Depth) / 2.0;
                                }
                            }
                            //scaleChange.Z = ((box.Scale.Z + (deltaX)) / box.Scale.Z);
                        }
                        if (scaleChange.Z <= 0)
                        {
                            scaleChange.Z = 1;
                        }
                    }
                    break;

                case "backthumb":
                    {
                        if (deltaX != 0)
                        {
                            if (Camera.HorizontalOrientation == PolarCamera.Orientations.Left)
                            {
                                //scaleChange.Z = ((box.Scale.Z - (mmx)) / box.Scale.Z);
                                scaleChange.Z = (1 - mmx);
                                positionChange.Z -= ((box.AbsoluteBounds.Depth * scaleChange.Z) - box.AbsoluteBounds.Depth) / 2.0;
                            }
                            else
                            {
                                if (Camera.HorizontalOrientation == PolarCamera.Orientations.Right)
                                {
                                    //scaleChange.Z = ((box.Scale.Z + (mmx)) / box.Scale.Z);
                                    scaleChange.Z = (1 + mmx);
                                    positionChange.Z = -((box.AbsoluteBounds.Depth * scaleChange.Z) - box.AbsoluteBounds.Depth) / 2.0;
                                }
                            }
                            // scaleChange.Z = ((box.Scale.Z - (deltaX)) / box.Scale.Z);
                        }
                        if (scaleChange.Z <= 0)
                        {
                            scaleChange.Z = 1;
                        }
                    }
                    break;
            }
            foreach (Object3D obj in SelectedObjects)
            {
                Bounds3D before = obj.AbsoluteBounds;
                obj.ScaleMesh(scaleChange.X, scaleChange.Y, scaleChange.Z);
                obj.Position = new Point3D(obj.Position.X + positionChange.X,
                obj.Position.Y + positionChange.Y,
                obj.Position.Z + positionChange.Z);
                obj.Remesh();
                obj.Scale = new Scale3D(obj.AbsoluteBounds.Width, obj.AbsoluteBounds.Height, obj.AbsoluteBounds.Depth);
            }
            if (box.Scale.X * scaleChange.X > 0)
            {
                box.Scale.X *= scaleChange.X;
            }
            if (box.Scale.Y * scaleChange.Y > 0)
            {
                box.Scale.Y *= scaleChange.Y;
            }
            if (box.Scale.Z * scaleChange.Z > 0)
            {
                box.Scale.Z *= scaleChange.Z;
            }
            box.ScaleMesh(scaleChange.X, scaleChange.Y, scaleChange.Z);
            box.Position = new Point3D(box.Position.X + positionChange.X,
            box.Position.Y + positionChange.Y,
            box.Position.Z + positionChange.Z);
            box.Remesh();
            box.CalcScale(false);
            MoveThumb(box.Position, box.AbsoluteBounds.Width / 2, 0, 0, "RightThumb");
            MoveThumb(box.Position, -box.AbsoluteBounds.Width / 2, 0, 0, "LeftThumb");
            MoveThumb(box.Position, 0, box.AbsoluteBounds.Height / 2, 0, "TopThumb");
            MoveThumb(box.Position, 0, -box.AbsoluteBounds.Height / 2, 0, "BottomThumb");
            MoveThumb(box.Position, 0, 0, box.AbsoluteBounds.Depth / 2, "FrontThumb");
            MoveThumb(box.Position, 0, 0, -box.AbsoluteBounds.Depth / 2, "BackThumb");

            CreateTextLabels(box.AbsoluteBounds.Size(), selectedThumb.Name);
            NotificationManager.Notify("DocDirty", null);
            NotificationManager.Notify("ScaleUpdated", null);
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
                if (box != null)
                {
                    box.Position = new Point3D(box.Position.X + positionChange.X,
                                                box.Position.Y + positionChange.Y,
                                                box.Position.Z + positionChange.Z);
                    box.Remesh();
                    foreach (Object3D obj in SelectedObjects)
                    {
                        obj.Position = new Point3D(obj.Position.X + positionChange.X,
                        obj.Position.Y + positionChange.Y,
                        obj.Position.Z + positionChange.Z);
                        obj.Remesh();
                        NotificationManager.Notify("PositionUpdated", obj);
                    }
                    MoveThumb(box.Position, box.AbsoluteBounds.Width / 2, 0, 0, "RightThumb");
                    MoveThumb(box.Position, -box.AbsoluteBounds.Width / 2, 0, 0, "LeftThumb");
                    MoveThumb(box.Position, 0, box.AbsoluteBounds.Height / 2, 0, "TopThumb");
                    MoveThumb(box.Position, 0, -box.AbsoluteBounds.Height / 2, 0, "BottomThumb");
                    MoveThumb(box.Position, 0, 0, box.AbsoluteBounds.Depth / 2, "FrontThumb");
                    MoveThumb(box.Position, 0, 0, -box.AbsoluteBounds.Depth / 2, "BackThumb");
                    CreateTextLabels(box.AbsoluteBounds.Size());
                    NotificationManager.Notify("DocDirty", null);
                }
            }
        }

        private void MoveThumb(Point3D p, double v1, double v2, double v3, string name)
        {
            foreach (Object3D thumb in thumbs)
            {
                if (thumb.Name == name)
                {
                    thumb.Position = new Point3D(p.X + v1, p.Y + v2, p.Z + v3);
                    thumb.Remesh();
                    break;
                }
            }
        }

        private void OnScaleRefresh(object param)
        {
            Object3D ob = param as Object3D;
            GenerateAdornments();
            NotificationManager.Notify("RefreshAdorners", null);
        }

        private void PositionLabel(string v)
        {
            if (labelLocations.ContainsKey(v))
            {
                Point3D loc = Location(labelLocations[v], label3DOffsets[v]);
                Point point = CameraUtils.Convert3DPoint(loc, ViewPort);

                Canvas.SetLeft(thumbLabels[v], point.X );
                Canvas.SetTop(thumbLabels[v], point.Y );
                if (!Overlay.Children.Contains(thumbLabels[v]))
                {
                    Overlay.Children.Add(thumbLabels[v]);
                }
            }
        }

        private Point3D Location(Point3D p1,Point3D p2)
        {
            return new Point3D(p1.X + p2.X,
                                p1.Y + p2.Y,
                                p1.Z + p2.Z);
        }

        private void PositionLabels()
        {
            if (ViewPort != null)
            {
                PositionLabel("TopThumb");
                PositionLabel("BottomThumb");
                PositionLabel("RightThumb");
                PositionLabel("LeftThumb");
                PositionLabel("FrontThumb");
                PositionLabel("BackThumb");
            }
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            refreshTimer?.Stop();
            GenerateAdornments();
            NotificationManager.Notify("DocDirty", null);
            NotificationManager.Notify("ScaleUpdated", null);
        }

        private void UpdateScale()
        {
            foreach (Object3D obj in SelectedObjects)
            {
                Bounds3D before = obj.AbsoluteBounds;
                obj.ScaleMesh(scaleChange.X, scaleChange.Y, scaleChange.Z);
                obj.Remesh();
                obj.CalcScale(false);
            }
            Point3D midp;
            Point3D size;
            CaclBoxSize(out midp, out size);
            if (box != null)
            {
                box.ScaleMesh(scaleChange.X, scaleChange.Y, scaleChange.Z);
                box.Remesh();
                box.CalcScale(false);
            }
        }
        private bool ValidDimension(string text, out double dim)
        {
            bool valid = false;
            dim = 0;
            if (!String.IsNullOrEmpty(text))
            {
                if (text.Contains("."))
                {
                    try
                    {
                        dim = Convert.ToDouble(text);
                        if (dim > 0.0)
                        {
                            valid = true;
                        }
                    }
                    catch
                    {

                    }
                }
            }
            return valid;
        }
    }
}