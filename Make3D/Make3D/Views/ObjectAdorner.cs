using Make3D.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Make3D.Adorners
{
    public class ObjectAdorner
    {
        private Model3DCollection adornments;
        public Model3DCollection Adornments { get { return adornments; } }
        private List<Object3D> thumbs;
        private List<Object3D> taggedObjects;

        public List<Object3D> SelectedObjects
        {
            get { return taggedObjects; }
        }

        private Object3D box;
        private bool boxSelected;
        private Object3D selectedThumb;
        internal PolarCamera Camera { get; set; }
        private Bounds3D bounds;

        public Bounds3D Bounds
        {
            get { return bounds; }
        }

        public ObjectAdorner(PolarCamera camera)
        {
            Camera = camera;
            adornments = new Model3DCollection();
            taggedObjects = new List<Object3D>();
            thumbs = new List<Object3D>();
            selectedThumb = null;
            box = null;
            boxSelected = false;
            bounds = new Bounds3D();
        }

        public void Clear()
        {
            adornments.Clear();
            taggedObjects.Clear();
            thumbs.Clear();
            selectedThumb = null;
            box = null;
            boxSelected = false;
            bounds = new Bounds3D();
        }

        public void AdornObject(Object3D obj)
        {
            selectedThumb = null;
            boxSelected = false;
            taggedObjects.Add(obj);
            GenerateAdornments();
        }

        public void Refresh()
        {
            GenerateAdornments();
        }

        private void GenerateAdornments()
        {
            adornments.Clear();
            thumbs.Clear();
            bounds = new Bounds3D();
            foreach (Object3D obj in taggedObjects)
            {
                bounds += obj.AbsoluteBounds;
            }
            Point3D midp = bounds.MidPoint();
            Point3D size = bounds.Size();
            CreateAdornments(midp, size);
        }

        private void CreateAdornments(Point3D position, Point3D size)
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
            box.Scale = new Scale3D(size.X, size.Y, size.Z);
            box.Scale.Adjust(1.1, 1.1, 1.1);
            box.Color = Color.FromArgb(200, 64, 64, 64);
            box.RelativeToAbsolute();
            box.SetMesh();
            adornments.Add(GetMesh(box));
            double thumbSize = 1;
            if (box.Scale.X > 10 && box.Scale.Y > 10 && box.Scale.Z > 10)
            {
                thumbSize = box.Scale.X / 10.0;
            }
            CreateThumb(position, thumbSize, box.Scale.X / 2, 0, 0, Colors.White, "RightThumb");
            CreateThumb(position, thumbSize, -box.Scale.X / 2, 0, 0, Colors.White, "LeftThumb");
            CreateThumb(position, thumbSize, 0, box.Scale.Y / 2, 0, Colors.White, "TopThumb");
            CreateThumb(position, thumbSize, 0, -box.Scale.Y / 2, 0, Colors.White, "BottomThumb");
            CreateThumb(position, thumbSize, 0, 0, box.Scale.Z / 2, Colors.White, "FrontThumb");
            CreateThumb(position, thumbSize, 0, 0, -box.Scale.Z / 2, Colors.White, "BackThumb");
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

            thumb.Color = col;
            thumb.RelativeToAbsolute();
            thumb.SetMesh();
            thumbs.Add(thumb);
            adornments.Add(GetMesh(thumb));
        }

        internal int NumberOfSelectedObjects()
        {
            return taggedObjects.Count;
        }

        private static GeometryModel3D GetMesh(Object3D obj)
        {
            GeometryModel3D gm = new GeometryModel3D();
            gm.Geometry = obj.Mesh;

            DiffuseMaterial mt = new DiffuseMaterial();
            mt.Color = obj.Color;
            mt.Brush = new SolidColorBrush(obj.Color);
            gm.Material = mt;
            return gm;
        }

        internal bool Select(GeometryModel3D geo)
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

        internal void Nudge(Adorner.NudgeDirection dir, double v)
        {
            switch (dir)
            {
                case Adorner.NudgeDirection.Left:
                    {
                        MoveBox(-v, 0);
                    }
                    break;

                case Adorner.NudgeDirection.Right:
                    {
                        MoveBox(v, 0);
                    }
                    break;

                case Adorner.NudgeDirection.Up:
                    {
                        MoveBox(0, v);
                    }
                    break;

                case Adorner.NudgeDirection.Down:
                    {
                        MoveBox(0, -v);
                    }
                    break;
            }
        }

        internal bool MouseMove(Point lastPos, Point newPos, MouseEventArgs e)
        {
            bool handled = false;
            if (boxSelected)
            {
                handled = true;
                MouseMoveBox(lastPos, newPos);
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

        private void MouseMoveBox(Point lastPos, Point newPos)
        {
            double dr = Math.Sqrt(Camera.Distance);
            double deltaX = (newPos.X - lastPos.X) / dr;
            double deltaY = -(newPos.Y - lastPos.Y) / dr;
            MoveBox(deltaX, deltaY);
        }

        private void MoveBox(double deltaX, double deltaY)
        {
            Point3D positionChange = new Point3D(Camera.DragDelta.X * deltaX, Camera.DragDelta.Y * deltaY, Camera.DragDelta.Z * deltaX);
            box.Position = new Point3D(box.Position.X + positionChange.X,
                                        box.Position.Y + positionChange.Y,
                                        box.Position.Z + positionChange.Z);
            box.Remesh();
            foreach (Object3D obj in taggedObjects)
            {
                obj.Position = new Point3D(obj.Position.X + positionChange.X,
                obj.Position.Y + positionChange.Y,
                obj.Position.Z + positionChange.Z);
                obj.Remesh();
            }
            MoveThumb(box.Position, box.Scale.X / 2, 0, 0, "RightThumb");
            MoveThumb(box.Position, -box.Scale.X / 2, 0, 0, "LeftThumb");
            MoveThumb(box.Position, 0, box.Scale.Y / 2, 0, "TopThumb");
            MoveThumb(box.Position, 0, -box.Scale.Y / 2, 0, "BottomThumb");
            MoveThumb(box.Position, 0, 0, box.Scale.Z / 2, "FrontThumb");
            MoveThumb(box.Position, 0, 0, -box.Scale.Z / 2, "BackThumb");
            NotificationManager.Notify("DocDirty", null);
        }

        private void MouseMoveThumb(Point lastPos, Point newPos)
        {
            double dr = Math.Sqrt(Camera.Distance);
            double deltaX = (newPos.X - lastPos.X) / dr;
            double deltaY = (newPos.Y - lastPos.Y) / dr;
            Point3D scaleChange = new Point3D(0, 0, 0);
            Point3D positionChange = new Point3D(0, 0, 0);
            switch (selectedThumb.Name.ToLower())
            {
                case "rightthumb":
                    {
                        scaleChange.X = deltaX;
                    }
                    break;

                case "leftthumb":
                    {
                        scaleChange.X = -deltaX;
                    }
                    break;

                case "topthumb":
                    {
                        scaleChange.Y = -deltaY;
                    }
                    break;

                case "bottomthumb":
                    {
                        scaleChange.Y = -deltaY;
                    }
                    break;

                case "frontthumb":
                    {
                        //positionChange.X = deltaX;
                        //positionChange.Y = -deltaY;
                    }
                    break;

                case "backthumb":
                    {
                        //positionChange.X = -deltaX;
                        //positionChange.Y = -deltaY;
                    }
                    break;
            }
            foreach (Object3D obj in taggedObjects)
            {
                if (obj.Scale.X + scaleChange.X > 0)
                {
                    obj.Scale.X += scaleChange.X;
                }
                if (obj.Scale.Y + scaleChange.Y > 0)
                {
                    obj.Scale.Y += scaleChange.Y;
                }
                if (obj.Scale.Z + scaleChange.Z > 0)
                {
                    obj.Scale.Z += scaleChange.Z;
                }
                obj.Position = new Point3D(obj.Position.X + positionChange.X,
                obj.Position.Y + positionChange.Y,
                obj.Position.Z + positionChange.Z);
                obj.Remesh();
            }
            if (box.Scale.X + scaleChange.X > 0)
            {
                box.Scale.X += scaleChange.X;
            }
            if (box.Scale.Y + scaleChange.Y > 0)
            {
                box.Scale.Y += scaleChange.Y;
            }
            if (box.Scale.Z + scaleChange.Z > 0)
            {
                box.Scale.Z += scaleChange.Z;
            }
            box.Position = new Point3D(box.Position.X + positionChange.X,
            box.Position.Y + positionChange.Y,
            box.Position.Z + positionChange.Z);
            box.Remesh();
            MoveThumb(box.Position, box.Scale.X / 2, 0, 0, "RightThumb");
            MoveThumb(box.Position, -box.Scale.X / 2, 0, 0, "LeftThumb");
            MoveThumb(box.Position, 0, box.Scale.Y / 2, 0, "TopThumb");
            MoveThumb(box.Position, 0, -box.Scale.Y / 2, 0, "BottomThumb");
            MoveThumb(box.Position, 0, 0, box.Scale.Z / 2, "FrontThumb");
            MoveThumb(box.Position, 0, 0, -box.Scale.Z / 2, "BackThumb");
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

        internal void MouseUp()
        {
            selectedThumb = null;
            boxSelected = false;
        }
    }
}