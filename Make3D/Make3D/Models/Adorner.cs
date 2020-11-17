using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Make3D.Models
{
    public class Adorner
    {
        private Model3DCollection adornments;
        public enum NudgeDirection
        {
            Left,
            Right,
            Up,
            Down,
            Forward,
            Back
        }
        private List<Object3D> taggedObjects;
        public List<Object3D> SelectedObjects
        {
            get { return taggedObjects; }
            set { taggedObjects = value; }
        }
        public Model3DCollection Adornments
        {
            get { return adornments; }
            set { adornments = value; }
        }

        internal virtual bool Select(GeometryModel3D geo)
        {
            bool handled = false;


            return handled;
        }
        internal virtual bool MouseMove(Point lastPos, Point newPos, MouseEventArgs e, bool ctrlDown)
        {
            bool handled = false;
           
            return handled;
        }

        internal virtual void Nudge(Adorner.NudgeDirection dir, double v)
        {
           
        }
        internal virtual void MouseUp()
        {

        }
        internal int NumberOfSelectedObjects()
        {
            return SelectedObjects.Count;
        }
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

        internal virtual void GenerateAdornments()
        {
            
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
    }
}