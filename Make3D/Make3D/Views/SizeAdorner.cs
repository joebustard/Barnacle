﻿using Make3D.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Make3D.Adorners
{
    public class SizeAdorner : Adorner
    {
        private List<Object3D> thumbs;

        private Object3D box;
        private bool boxSelected;
        private Object3D selectedThumb;
        internal PolarCamera Camera { get; set; }
        private Bounds3D bounds;

        public Bounds3D Bounds
        {
            get { return bounds; }
        }

        public SizeAdorner(PolarCamera camera)
        {
            Camera = camera;
            Adornments = new Model3DCollection();
            SelectedObjects = new List<Object3D>();
            thumbs = new List<Object3D>();
            selectedThumb = null;
            box = null;
            boxSelected = false;
            bounds = new Bounds3D();
            NotificationManager.Subscribe("ScaleRefresh", OnScaleRefresh);
        }

        private void OnScaleRefresh(object param)
        {
            Object3D ob = param as Object3D;
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
        }

        public override void AdornObject(Object3D obj)
        {
            selectedThumb = null;
            boxSelected = false;
            obj.CalcScale(false);
            SelectedObjects.Add(obj);
            GenerateAdornments();
        }

        internal override void GenerateAdornments()
        {
            Adornments.Clear();
            thumbs.Clear();
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

        private void CreateAdornments(Point3D position, Point3D size, bool addSizeThumbs)
        {
            box = new Object3D();

            // create the main semi transparent part of of the adorner
            Point3DCollection pnts = new Point3DCollection();
            Int32Collection indices = new Int32Collection();
            Vector3DCollection normals = new Vector3DCollection();
            PrimitiveGenerator.GenerateCube(ref pnts, ref indices, ref normals);
            box.RelativeObjectVertices = pnts;
            box.TriangleIndices = indices;
            box.Normals = normals;
            box.Position = position;
            box.ScaleMesh(size.X + 0.01, size.Y + 0.01, size.Z + 0.01);
            //box.Scale.Adjust(1.1, 1.1, 1.1);
            box.Color = Color.FromArgb(150, 64, 64, 64);
            box.RelativeToAbsolute();
            box.SetMesh();
            Adornments.Add(GetMesh(box));

            if (addSizeThumbs)
            {
                double thumbSize = 4;
                CreateThumb(position, thumbSize, box.AbsoluteBounds.Width / 2, 0, 0, Colors.White, "RightThumb");
                CreateThumb(position, thumbSize, -box.AbsoluteBounds.Width / 2, 0, 0, Colors.White, "LeftThumb");
                CreateThumb(position, thumbSize, 0, box.AbsoluteBounds.Height / 2, 0, Colors.White, "TopThumb");
                CreateThumb(position, thumbSize, 0, -box.AbsoluteBounds.Height / 2, 0, Colors.White, "BottomThumb");
                CreateThumb(position, thumbSize, 0, 0, box.AbsoluteBounds.Depth / 2, Colors.White, "FrontThumb");
                CreateThumb(position, thumbSize, 0, 0, -box.AbsoluteBounds.Depth / 2, Colors.White, "BackThumb");
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
            thumb.RelativeObjectVertices = pnts;
            thumb.TriangleIndices = indices;
            thumb.Normals = normals;

            position.X += x;
            position.Y += y;
            position.Z += z;
            thumb.Position = position;
            thumb.Scale = new Scale3D(thumbSize, thumbSize, thumbSize);
            thumb.ScaleMesh(thumbSize, thumbSize, thumbSize);
            thumb.Color = col;
            thumb.RelativeToAbsolute();
            thumb.SetMesh();
            thumbs.Add(thumb);
            Adornments.Add(GetMesh(thumb));
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

        internal override void Nudge(Adorner.NudgeDirection dir, double v)
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

        internal override bool MouseMove(Point lastPos, Point newPos, MouseEventArgs e, bool ctrlDown)
        {
            bool handled = false;
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
            return handled;
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
                NotificationManager.Notify("DocDirty", null);
            }
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
                            if (Camera.Orientation == PolarCamera.Orientations.Front)
                            {
                                scaleChange.X = ((box.Scale.X + (mmx)) / box.Scale.X);
                                positionChange.X += ((box.AbsoluteBounds.Width * scaleChange.X) - box.AbsoluteBounds.Width) / 2.0;
                            }
                            else
                            {
                                if (Camera.Orientation == PolarCamera.Orientations.Back)
                                {
                                    scaleChange.X = ((box.Scale.X - (mmx)) / box.Scale.X);
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
                            if (Camera.Orientation == PolarCamera.Orientations.Front)
                            {
                                scaleChange.X = ((box.Scale.X - (mmx)) / box.Scale.X);
                                positionChange.X -= ((box.AbsoluteBounds.Width * scaleChange.X) - box.AbsoluteBounds.Width) / 2.0;
                            }
                            else
                            {
                                if (Camera.Orientation == PolarCamera.Orientations.Back)
                                {
                                    scaleChange.X = ((box.Scale.X + (mmx)) / box.Scale.X);
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
                            scaleChange.Y = ((box.Scale.Y - (mmy)) / box.Scale.Y);
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
                            scaleChange.Y = ((box.Scale.Y + (mmy)) / box.Scale.Y);
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
                            if (Camera.Orientation == PolarCamera.Orientations.Left)
                            {
                                scaleChange.Z = ((box.Scale.Z + (mmx)) / box.Scale.Z);
                                positionChange.Z += ((box.AbsoluteBounds.Depth * scaleChange.Z) - box.AbsoluteBounds.Depth) / 2.0;
                            }
                            else
                            {
                                if (Camera.Orientation == PolarCamera.Orientations.Right)
                                {
                                    scaleChange.Z = ((box.Scale.Z - (mmx)) / box.Scale.Z);
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
                            if (Camera.Orientation == PolarCamera.Orientations.Left)
                            {
                                scaleChange.Z = ((box.Scale.Z - (mmx)) / box.Scale.Z);
                                positionChange.Z -= ((box.AbsoluteBounds.Depth * scaleChange.Z) - box.AbsoluteBounds.Depth) / 2.0;
                            }
                            else
                            {
                                if (Camera.Orientation == PolarCamera.Orientations.Right)
                                {
                                    scaleChange.Z = ((box.Scale.Z + (mmx)) / box.Scale.Z);
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
            MoveThumb(box.Position, box.AbsoluteBounds.Width / 2, 0, 0, "RightThumb");
            MoveThumb(box.Position, -box.AbsoluteBounds.Width / 2, 0, 0, "LeftThumb");
            MoveThumb(box.Position, 0, box.AbsoluteBounds.Height / 2, 0, "TopThumb");
            MoveThumb(box.Position, 0, -box.AbsoluteBounds.Height / 2, 0, "BottomThumb");
            MoveThumb(box.Position, 0, 0, box.AbsoluteBounds.Depth / 2, "FrontThumb");
            MoveThumb(box.Position, 0, 0, -box.AbsoluteBounds.Depth / 2, "BackThumb");
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

        internal override void MouseUp()
        {
            selectedThumb = null;
            boxSelected = false;
        }
    }
}