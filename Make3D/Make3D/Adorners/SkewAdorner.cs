using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Models.Adorners
{
    internal class SkewAdorner : Adorner
    {
        private Bounds3D bounds;
        private Object3D box;

        private List<Point3D> labelLocations;
        private MoveSide moveSide;
        private Object3D selectedThumb;
        private List<Label> thumbLabels;
        private List<Object3D> thumbs;

        public SkewAdorner(PolarCamera camera)
        {
            NotificationManager.ViewUnsubscribe("SkewAdorner");
            Camera = camera;
            Adornments = new Model3DCollection();
            SelectedObjects = new List<Object3D>();
            thumbs = new List<Object3D>();
            selectedThumb = null;
            box = null;

            bounds = new Bounds3D();
            labelLocations = new List<Point3D>();
            thumbLabels = new List<Label>();
            ViewPort = null;
            NotificationManager.Subscribe("SkewAdorner", "ScaleRefresh", OnScaleRefresh);
            moveSide = MoveSide.Back;
        }

        private enum MoveSide
        {
            Left,
            Right,
            Front,
            Back
        }

        public Bounds3D Bounds
        {
            get { return bounds; }
        }

        internal PolarCamera Camera { get; set; }

        public override void AdornObject(Object3D obj)
        {
            selectedThumb = null;

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

            bounds = new Bounds3D();
            Overlay?.Children.Clear();
        }

        internal override void GenerateAdornments()
        {
            Adornments.Clear();
            thumbs.Clear();
            Overlay?.Children.Clear();
            bool addSizeThumbs = true;
            bounds = new Bounds3D();
            foreach (Object3D obj in SelectedObjects)
            {
                bounds += obj.AbsoluteBounds;
                if (!obj.IsSizable())
                {
                    addSizeThumbs = false;
                }
            }
            Point3D midp = bounds.MidPoint();
            Point3D size = bounds.Size();
            CreateAdornments(midp, size, addSizeThumbs);
        }

        internal override bool MouseMove(Point lastPos, Point newPos, MouseEventArgs e, bool ctrlDown)
        {
            bool handled = false;
            // ignore if the left moouse button is up
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (selectedThumb != null)
                {
                    handled = true;
                    MouseMoveThumb(lastPos, newPos);
                }
            }
            return handled;
        }

        internal override void MouseUp(MouseButtonEventArgs e)
        {
            selectedThumb = null;
        }

        internal override bool Select(GeometryModel3D geo)
        {
            bool handled = false;

            if (box != null)
            {
                if (box.Mesh == geo.Geometry)
                {
                    handled = true;
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

        private void CreateAdornments(Point3D position, Point3D size, bool addSizeThumbs)
        {
            box = new Object3D();

            // create the main semi transparent part of of the adorner
            Point3DCollection pnts = new Point3DCollection();
            Int32Collection indices = new Int32Collection();
            Vector3DCollection normals = new Vector3DCollection();
            PrimitiveGenerator.GenerateCube(ref pnts, ref indices, ref normals);
            List<P3D> tmp = new List<P3D>();
            PointUtils.PointCollectionToP3D(pnts, tmp);
            //box.RelativeObjectVertices = pnts;
            box.RelativeObjectVertices = tmp;
            box.TriangleIndices = indices;
            box.Normals = normals;
            box.Position = position;
            box.ScaleMesh(size.X + 0.01, size.Y + 0.01, size.Z + 0.01);

            box.Color = Color.FromArgb(150, 64, 64, 64);
            box.RelativeToAbsolute();
            box.SetMesh();
            //Adornments.Add(GetMesh(box));

            if (addSizeThumbs)
            {
                double thumbSize = 4;
                CreateThumb(position, thumbSize, box.AbsoluteBounds.Width / 2, box.AbsoluteBounds.Height / 2, 0, Colors.Blue, "RightThumb");
                CreateThumb(position, thumbSize, -box.AbsoluteBounds.Width / 2, box.AbsoluteBounds.Height / 2, 0, Colors.Blue, "LeftThumb");
                //CreateThumb(position, thumbSize, 0, box.AbsoluteBounds.Height / 2, 0, Colors.Blue, "TopThumb");
                //CreateThumb(position, thumbSize, 0, -box.AbsoluteBounds.Height / 2, 0, Colors.Blue, "BottomThumb");
                CreateThumb(position, thumbSize, 0, box.AbsoluteBounds.Height / 2, box.AbsoluteBounds.Depth / 2, Colors.Blue, "FrontThumb");
                CreateThumb(position, thumbSize, 0, box.AbsoluteBounds.Height / 2, -box.AbsoluteBounds.Depth / 2, Colors.Blue, "BackThumb");
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
            if (deltaY != 0)
            {
                scaleChange.Y = ((box.Scale.Y - (mmy)) / box.Scale.Y);
            }
            if (scaleChange.Y <= 0)
            {
                scaleChange.Y = 1;
            }
            switch (selectedThumb.Name.ToLower())
            {
                case "rightthumb":
                    {
                        moveSide = MoveSide.Right;
                    }
                    break;

                case "leftthumb":
                    {
                        moveSide = MoveSide.Left;
                    }
                    break;

                case "frontthumb":
                    {
                        moveSide = MoveSide.Front;
                    }
                    break;

                case "backthumb":
                    {
                        moveSide = MoveSide.Back;
                    }
                    break;
            }
            foreach (Object3D obj in SelectedObjects)
            {
                Bounds3D before = obj.AbsoluteBounds;
                obj.SkewMesh((int)moveSide, scaleChange.X, scaleChange.Y, scaleChange.Z);

                obj.Remesh();
                obj.Scale = new Scale3D(obj.AbsoluteBounds.Width, obj.AbsoluteBounds.Height, obj.AbsoluteBounds.Depth);
            }

            NotificationManager.Notify("DocDirty", null);
            NotificationManager.Notify("ScaleUpdated", null);
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
        }
    }
}