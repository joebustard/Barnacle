﻿using Make3D.Models;
using System.Windows.Media.Media3D;

namespace Make3D.Dialogs.MeshEditor
{
    public class MeshVertex
    {
        public Point3D Position { get; set; }
        private bool selected;

        public bool Selected
        {
            get { return selected; }

            set
            {
                selected = value;
                SetModelMaterials();
            }
        }

        public DiffuseMaterial SelectedMaterial
        {
            get;
            set;
        }

        public DiffuseMaterial UnselectedMaterial
        {
            get;
            set;
        }

        public DiffuseMaterial BackMaterial
        {
            get;
            set;
        }

        private GeometryModel3D model;

        public GeometryModel3D Model
        {
            get { return model; }
            set
            {
                if (model != value)
                {
                    model = value;
                }
            }
        }

        private void SetModelMaterials()
        {
            // don't try to update model materials if one of them isn't
            // defined yet
            if (model != null && SelectedMaterial != null)
            {
                if (selected)
                {
                    model.Material = SelectedMaterial;
                }
                else
                {
                    model.Material = UnselectedMaterial;
                }
                model.BackMaterial = BackMaterial;
            }
        }

        public MeshVertex()
        {
            Selected = false;
            Model = null;
            SelectedMaterial = null;
            UnselectedMaterial = null;
            BackMaterial = null;
        }

        private int AddPoint(Point3DCollection positions, Point3D v)
        {
            int res = -1;
            for (int i = 0; i < positions.Count; i++)
            {
                if (PointUtils.equals(positions[i], v.X, v.Y, v.Z))
                {
                    res = i;
                    break;
                }
            }

            if (res == -1)
            {
                positions.Add(new Point3D(v.X, v.Y, v.Z));
                res = positions.Count - 1;
            }
            return res;
        }

        internal void CreateModel()
        {
            double si = 0.5;
            model = new GeometryModel3D();

            Point3D p = Position;
            MeshGeometry3D faces = new MeshGeometry3D();
            int v0 = AddPoint(faces.Positions, new Point3D(p.X, p.Y + si, p.Z));
            int v1 = AddPoint(faces.Positions, new Point3D(p.X - si, p.Y, p.Z - si));
            int v2 = AddPoint(faces.Positions, new Point3D(p.X + si, p.Y, p.Z - si));
            int v3 = AddPoint(faces.Positions, new Point3D(p.X, p.Y, p.Z + si));
            int v4 = AddPoint(faces.Positions, new Point3D(p.X, p.Y - si, p.Z));

            faces.TriangleIndices.Add(v0);
            faces.TriangleIndices.Add(v2);
            faces.TriangleIndices.Add(v1);

            faces.TriangleIndices.Add(v0);
            faces.TriangleIndices.Add(v3);
            faces.TriangleIndices.Add(v2);

            faces.TriangleIndices.Add(v0);
            faces.TriangleIndices.Add(v1);
            faces.TriangleIndices.Add(v3);

            faces.TriangleIndices.Add(v4);
            faces.TriangleIndices.Add(v1);
            faces.TriangleIndices.Add(v2);

            faces.TriangleIndices.Add(v4);
            faces.TriangleIndices.Add(v2);
            faces.TriangleIndices.Add(v3);

            faces.TriangleIndices.Add(v4);
            faces.TriangleIndices.Add(v3);
            faces.TriangleIndices.Add(v1);

            model = new GeometryModel3D();

            model.Geometry = faces;
            model.BackMaterial = BackMaterial;
            model.Material = UnselectedMaterial;
        }

        internal bool CheckHit(GeometryModel3D m, bool shift)
        {
            bool res = false;
            if (m == model)
            {
                if (shift == true)
                {
                    Selected = true;
                }
                else
                {
                    Selected = !Selected;
                }
                res = true;
            }
            return res;
        }

        internal void MovePosition(Point3D positionChange)
        {
            Position = new Point3D(
                Position.X + positionChange.X,
                Position.Y + positionChange.Y,
                Position.Z + positionChange.Z);

            Point3D p = Position;
            double si = 0.5;
            MeshGeometry3D faces = model.Geometry as MeshGeometry3D;
            faces.Positions.Clear();
            int v0 = AddPoint(faces.Positions, new Point3D(p.X, p.Y + si, p.Z));
            int v1 = AddPoint(faces.Positions, new Point3D(p.X - si, p.Y, p.Z - si));
            int v2 = AddPoint(faces.Positions, new Point3D(p.X + si, p.Y, p.Z - si));
            int v3 = AddPoint(faces.Positions, new Point3D(p.X, p.Y, p.Z + si));
            int v4 = AddPoint(faces.Positions, new Point3D(p.X, p.Y - si, p.Z));
        }
    }
}