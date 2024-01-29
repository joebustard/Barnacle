using Barnacle.Object3DLib;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Models.Adorners
{
    public class Adorner
    {
        private Model3DCollection adornments;
        private List<Object3D> taggedObjects;

        public enum NudgeDirection
        {
            Left,
            Right,
            Up,
            Down,
            Forward,
            Back
        }

        public Model3DCollection Adornments
        {
            get { return adornments; }
            set { adornments = value; }
        }

        public Canvas Overlay { get; internal set; }

        public List<Object3D> SelectedObjects
        {
            get { return taggedObjects; }
            set { taggedObjects = value; }
        }

        public Viewport3D ViewPort { get; set; }

        public virtual void AdornObject(Object3D obj)
        {
        }

        public virtual void Clear()
        {
            Adornments.Clear();
            SelectedObjects.Clear();
        }

        public void Refresh()
        {
            GenerateAdornments();
        }

        internal static GeometryModel3D GetMesh(Object3D obj)
        {
            GeometryModel3D gm = new GeometryModel3D();
            gm.Geometry = obj.Mesh;

            DiffuseMaterial mt = new DiffuseMaterial();
            mt.Color = obj.Color;
            mt.Brush = new SolidColorBrush(obj.Color);
            gm.Material = mt;
            return gm;
        }

        internal virtual void GenerateAdornments()
        {
        }

        internal virtual bool MouseMove(Point lastPos, Point newPos, MouseEventArgs e, bool ctrlDown)
        {
            bool handled = false;

            return handled;
        }

        internal virtual void MouseUp(MouseButtonEventArgs e)
        {
        }

        internal virtual void Mousedown(MouseButtonEventArgs e)
        {
        }

        internal virtual void Nudge(Adorner.NudgeDirection dir, double v)
        {
        }

        internal int NumberOfSelectedObjects()
        {
            return SelectedObjects.Count;
        }

        internal virtual bool Select(GeometryModel3D geo)
        {
            bool handled = false;

            return handled;
        }
    }
}