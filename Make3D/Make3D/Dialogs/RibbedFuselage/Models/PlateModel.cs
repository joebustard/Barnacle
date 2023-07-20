using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs.RibbedFuselage.Models
{
    internal class PlateModel
    {
        public enum Orientation
        {
            Side,
            Top,
            Accross
        }

        public Orientation PointOrientation { get; set; }
        private SpaceTreeNode spaceTreeRoot;

        private Int32Collection tris;

        private Point3DCollection vertices;

        public Point3DCollection Vertices
        {
            get
            {
                return vertices;
            }
            set
            {
                vertices = value;
            }
        }

        private int AddVertice(Point3D v)
        {
            int res = -1;
            if (spaceTreeRoot != null)
            {
                res = spaceTreeRoot.Present(v);
            }

            if (res == -1)
            {
                vertices.Add(new Point3D(v.X, v.Y, v.Z));
                res = vertices.Count - 1;
                if (spaceTreeRoot == null)
                {
                    spaceTreeRoot = SpaceTreeNode.Create(v, res);
                }
                else
                {
                    spaceTreeRoot.Add(v, spaceTreeRoot, res);
                }
            }
            return res;
        }

        public System.Windows.Media.Color MeshColour { get; set; }

        public PlateModel()
        {
            Vertices = new Point3DCollection();
            Faces = new Int32Collection();
            MeshColour = Colors.Gray;
            spaceTreeRoot = null;
        }

        public Int32Collection Faces
        {
            get
            {
                return tris;
            }
            set
            {
                tris = value;
            }
        }

        public void AddFace(int c0, int c1, int c2)
        {
            Faces.Add(c0);
            Faces.Add(c1);
            Faces.Add(c2);
        }

        public void ClearShape()
        {
            spaceTreeRoot = null;
            Vertices.Clear();
            Faces.Clear();
            spaceTreeRoot = null;
        }

        private List<PointF> points;

        public GeometryModel3D GetModel()
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.Positions = Vertices;
            mesh.TriangleIndices = Faces;
            mesh.Normals = null;
            GeometryModel3D gm = new GeometryModel3D();
            gm.Geometry = mesh;

            DiffuseMaterial mt = new DiffuseMaterial();
            mt.Color = MeshColour;
            mt.Brush = new SolidColorBrush(MeshColour);
            gm.Material = mt;
            DiffuseMaterial mtb = new DiffuseMaterial();
            mtb.Color = MeshColour;
            mtb.Brush = new SolidColorBrush(MeshColour);
            gm.BackMaterial = mtb;

            return gm;
        }

        internal void SetPoints(List<PointF> dp)
        {
            float left = float.MaxValue;
            float right = float.MinValue;
            float bottom = float.MaxValue;
            float top = float.MinValue;
            foreach (PointF p in dp)
            {
                left = Math.Min(left, p.X);
                right = Math.Max(right, p.X);
                bottom = Math.Min(bottom, p.Y);
                top = Math.Max(top, p.Y);
            }
            float dx = (right - left) / 2;
            float dy = (top - bottom) / 2;
            points = new List<PointF>();
            foreach (PointF p in dp)
            {
                points.Add(new PointF(p.X - left - dx, -(p.Y - bottom - dy)));
            }
            ClearShape();
            switch (PointOrientation)
            {
                case Orientation.Top:
                    {
                        TriangulateTop(points, false);
                    }
                    break;

                case Orientation.Side:
                    {
                        TriangulateSide(points, false);
                    }
                    break;

                case Orientation.Accross:
                    {
                    }
                    break;
            }
        }

        private void TriangulateSide(List<PointF> points, bool invert)
        {
            TriangulationPolygon ply = new TriangulationPolygon();

            ply.Points = points.ToArray();
            List<Triangle> tris = ply.Triangulate();
            foreach (Triangle t in tris)
            {
                int c0 = AddVertice(new Point3D(t.Points[0].X, t.Points[0].Y, 0.0));
                int c1 = AddVertice(new Point3D(t.Points[1].X, t.Points[1].Y, 0.0));
                int c2 = AddVertice(new Point3D(t.Points[2].X, t.Points[2].Y, 0.0));
                if (invert)
                {
                    Faces.Add(c0);
                    Faces.Add(c2);
                    Faces.Add(c1);
                }
                else
                {
                    Faces.Add(c0);
                    Faces.Add(c1);
                    Faces.Add(c2);
                }
            }
        }

        private void TriangulateTop(List<PointF> points, bool invert)
        {
            TriangulationPolygon ply = new TriangulationPolygon();

            ply.Points = points.ToArray();
            List<Triangle> tris = ply.Triangulate();
            foreach (Triangle t in tris)
            {
                int c0 = AddVertice(new Point3D(t.Points[0].X, 0.0, t.Points[0].Y));
                int c1 = AddVertice(new Point3D(t.Points[1].X, 0.0, t.Points[1].Y));
                int c2 = AddVertice(new Point3D(t.Points[2].X, 0.0, t.Points[2].Y));
                if (invert)
                {
                    Faces.Add(c0);
                    Faces.Add(c2);
                    Faces.Add(c1);
                }
                else
                {
                    Faces.Add(c0);
                    Faces.Add(c1);
                    Faces.Add(c2);
                }
            }
        }
    }
}