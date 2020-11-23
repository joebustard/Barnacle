using Make3D.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Make3D.Views
{
    public class RotationAdorner : Adorner
    {
        private Object3D sphere;
        private Object3D xAxis;
        private Object3D yAxis;
        private Object3D zAxis;
        private bool selectedSphere = false;
        private Bounds3D bounds;

        public RotationAdorner(PolarCamera camera)
        {
            Camera = camera;
            Adornments = new Model3DCollection();
            SelectedObjects = new List<Object3D>();
            sphere = null;
            selectedSphere = false;
            bounds = new Bounds3D();
            NotificationManager.Subscribe("ScaleRefresh", OnScaleRefresh);
        }

        public Bounds3D Bounds
        {
            get { return bounds; }
        }

        internal PolarCamera Camera { get; set; }

        internal override bool Select(GeometryModel3D geo)
        {
            bool handled = false;
            if (sphere != null)
            {
                if (sphere.Mesh == geo.Geometry)
                {
                    handled = true;
                    selectedSphere = true;
                }
            }

            return handled;
        }

        private void OnScaleRefresh(object param)
        {
        }

        public override void AdornObject(Object3D obj)
        {
            obj.CalcScale(false);
            SelectedObjects.Add(obj);
            GenerateAdornments();
        }

        internal override void GenerateAdornments()
        {
            Adornments.Clear();

            bounds = new Bounds3D();
            foreach (Object3D obj in SelectedObjects)
            {
                bounds += obj.AbsoluteBounds;
            }
            Point3D midp = bounds.MidPoint();
            Point3D size = bounds.Size();
            CreateAdornments(midp, size);
        }

        private void CreateAdornments(Point3D position, Point3D size)
        {
            xAxis = CreateAxis(position, 100, 1, 1, Colors.Red, "XAxis");
            Adornments.Add(GetMesh(xAxis));
            yAxis = CreateAxis(position, 1, 100, 1, Colors.Green, "YAxis");
            Adornments.Add(GetMesh(yAxis));
            zAxis = CreateAxis(position, 1, 1, 100, Colors.Yellow, "ZAxis");
            Adornments.Add(GetMesh(zAxis));

            sphere = new Object3D();
            // create the main semi transparent part of of the adorner
            Point3DCollection pnts = new Point3DCollection();
            Int32Collection indices = new Int32Collection();
            Vector3DCollection normals = new Vector3DCollection();
            PrimitiveGenerator.GenerateSphere(ref pnts, ref indices, ref normals);
            sphere.RelativeObjectVertices = pnts;
            sphere.TriangleIndices = indices;
            sphere.Normals = normals;
            sphere.Position = position;
            sphere.ScaleMesh(size.X * 2, size.Y * 2, size.Z * 2);
            //box.Scale.Adjust(1.1, 1.1, 1.1);
            sphere.Color = Color.FromArgb(150, 64, 64, 64);
            sphere.RelativeToAbsolute();
            sphere.SetMesh();
            Adornments.Add(GetMesh(sphere));
        }

        private Object3D CreateAxis(Point3D position, double xSize, double ySize, double zSize, Color col, string name)
        {
            Object3D rect = new Object3D();

            Point3DCollection pnts = new Point3DCollection();
            Int32Collection indices = new Int32Collection();
            Vector3DCollection normals = new Vector3DCollection();
            PrimitiveGenerator.GenerateCube(ref pnts, ref indices, ref normals);
            rect.Name = name;
            rect.RelativeObjectVertices = pnts;
            rect.TriangleIndices = indices;
            rect.Normals = normals;

            rect.Position = position;
            rect.ScaleMesh(xSize, ySize, zSize);
            rect.Color = col;
            rect.RelativeToAbsolute();
            rect.SetMesh();

            return rect;
        }

        internal override bool MouseMove(Point lastPos, Point newPos, MouseEventArgs e, bool ctrlDown)
        {
            bool handled = false;
            if (e.LeftButton == MouseButtonState.Pressed && selectedSphere)
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

                Rotate(deltaX, deltaY, deltaZ);
                handled = true;
            }
            return handled;
        }

        private void Rotate(double deltaX, double deltaY, double deltaZ)
        {
            Point3D positionChange = new Point3D(0, 0, 0);
            PolarCamera.Orientations ori = Camera.Orientation;
            switch (ori)
            {
                case PolarCamera.Orientations.Front:
                    {
                        positionChange = new Point3D(-1 * deltaY, 1 * deltaX, -1 * deltaZ);
                    }
                    break;

                case PolarCamera.Orientations.Back:
                    {
                        positionChange = new Point3D(1 * deltaY, 1 * deltaX, 1 * deltaZ);
                    }
                    break;

                case PolarCamera.Orientations.Left:
                    {
                        positionChange = new Point3D(1 * deltaZ, 1 * deltaX, -1 * deltaY);
                    }
                    break;

                case PolarCamera.Orientations.Right:
                    {
                        positionChange = new Point3D(1 * deltaZ, 1 * deltaX, 1 * deltaY);
                    }
                    break;
            }

            if (positionChange != null)
            {
                RotateSingleObject(positionChange, xAxis);
                RotateSingleObject(positionChange, yAxis);
                RotateSingleObject(positionChange, zAxis);
                foreach (Object3D obj in SelectedObjects)
                {
                    RotateSingleObject(positionChange, obj);
                    NotificationManager.Notify("PositionUpdated", obj);
                }
                NotificationManager.Notify("DocDirty", null);
            }
        }

        private static void RotateSingleObject(Point3D positionChange, Object3D obj)
        {
            obj.Rotate(positionChange);
            obj.Remesh();
        }

        internal override void MouseUp()
        {
            selectedSphere = false;
        }
    }
}