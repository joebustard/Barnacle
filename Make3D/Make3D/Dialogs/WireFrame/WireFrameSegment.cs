using Make3D.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Make3D.Dialogs.WireFrame
{
    internal class WireFrameSegment
    {
        private Point3D endPoint;
        private GeometryModel3D model;

        private Point3D startPoint;
        private double thickness;
        private Int32Collection tris;
        private int v;
        private Point3DCollection vertices;

        public WireFrameSegment()
        {
            startPoint = new Point3D(0, 0, 0);
            endPoint = new Point3D(0, 0, 0);
            thickness = 1;
        }

        public WireFrameSegment(Point3D position1, Point3D position2, double v)
        {
            this.startPoint = position1;
            this.endPoint = position2;
            this.thickness = v;
            vertices = new Point3DCollection();
            tris = new Int32Collection();
            UpdateModel();
        }

        public Point3D EndPoint
        {
            get
            {
                return endPoint;
            }
            set
            {
                if (endPoint != value)
                {
                    endPoint = value;
                    UpdateModel();
                }
            }
        }

        public GeometryModel3D Model
        {
            get
            {
                return model;
            }
            set
            {
                if (model != value)
                {
                    model = value;
                }
            }
        }

        public Point3D StartPoint
        {
            get
            {
                return startPoint;
            }
            set
            {
                if (startPoint != value)
                {
                    startPoint = value;
                    UpdateModel();
                }
            }
        }

        public double Thickness
        {
            get
            {
                return thickness;
            }
            set
            {
                if (thickness != value)
                {
                    thickness = value;
                    UpdateModel();
                }
            }
        }

        private void AddTriangle(int v1, int v2, int v3)
        {
            tris.Add(v1);
            tris.Add(v2);
            tris.Add(v3);
        }

        private DiffuseMaterial CreateMaterial(SolidColorBrush brush)
        {
            DiffuseMaterial res = new DiffuseMaterial();
            res.Brush = brush;
            res.Color = brush.Color;
            return res;
        }

        private void UpdateModel()
        {
            if (startPoint != null && endPoint != null && thickness > 0)
            {
                // Get a vector between the points.
                Vector3D v = endPoint - startPoint;
                // dont do anything if the seqment is tiny
                if (v.Length > 0.000001)
                {
                    // Get perpendicular vectors.
                    Vector3D vp1, vp2;
                    double angle = Vector3D.AngleBetween(v, VectorUtils.YVector());
                    if ((angle > 10) && (angle < 170))
                        vp1 = Vector3D.CrossProduct(v, VectorUtils.YVector());
                    else
                        vp1 = Vector3D.CrossProduct(v, VectorUtils.ZVector());
                    vp2 = Vector3D.CrossProduct(v, vp1);

                    // Give the perpendicular vectors length thickness.
                    vp2 *= thickness / vp2.Length;
                    vp1 *= thickness / vp1.Length;
                    vertices.Clear();
                    tris.Clear();
                    Point3D p1 = startPoint + vp2 + vp1;
                    Point3D p2 = startPoint - vp2 + vp1;

                    Point3D p3 = startPoint - vp2 - vp1;
                    Point3D p4 = startPoint + vp2 - vp1;

                    Point3D p5 = endPoint + vp2 + vp1;
                    Point3D p6 = endPoint - vp2 + vp1;

                    Point3D p7 = endPoint - vp2 - vp1;
                    Point3D p8 = endPoint + vp2 - vp1;
                    vertices.Add(p1);
                    vertices.Add(p2);
                    vertices.Add(p3);
                    vertices.Add(p4);
                    vertices.Add(p5);
                    vertices.Add(p6);
                    vertices.Add(p7);
                    vertices.Add(p8);
                    AddTriangle(3, 2, 6);
                    AddTriangle(3, 6, 7);

                    AddTriangle(2, 1, 5);
                    AddTriangle(2, 5, 6);

                    AddTriangle(1, 0, 4);
                    AddTriangle(1, 4, 5);

                    AddTriangle(0, 3, 7);
                    AddTriangle(0, 7, 4);

                    AddTriangle(7, 6, 5);
                    AddTriangle(7, 5, 4);

                    AddTriangle(2, 3, 0);
                    AddTriangle(2, 0, 1);

                    MeshGeometry3D faces = new MeshGeometry3D();
                    faces.Positions = vertices;
                    faces.TriangleIndices = tris;
                    model = new GeometryModel3D();

                    model.Geometry = faces;
                    model.BackMaterial = CreateMaterial(Brushes.Black);
                    model.Material = CreateMaterial(Brushes.Black);
                }
            }
        }
    }
}