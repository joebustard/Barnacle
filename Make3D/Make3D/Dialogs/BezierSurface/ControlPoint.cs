/**************************************************************************
*   Copyright (c) 2024 Joe Bustard <barnacle3d@gmailcom>                  *
*                                                                         *
*   This file is part of the Barnacle 3D application.                     *
*                                                                         *
*   This application is free software; you can redistribute it and/or     *
*   modify it under the terms of the GNU Library General Public           *
*   License as published by the Free Software Foundation; either          *
*   version 2 of the License, or (at your option) any later version.      *
*                                                                         *
*   This application is distributed in the hope that it will be useful,   *
*   but WITHOUT ANY WARRANTY; without even the implied warranty of        *
*   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
*   GNU Library General Public License for more details.                  *
*                                                                         *
**************************************************************************/

using Barnacle.Object3DLib;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs.BezierSurface
{
    internal class ControlPoint
    {
        private void SetModelMaterials()
        {
            // don't try to update model materials if one of them isn't defined yet
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

        public ControlPoint(double x, double y, double z)
        {
            Position = new Point3D(x, y, z);
            CreateMaterials();
            CreateInitialModel();
        }

        private void CreateMaterials()
        {
            BackMaterial = CreateMaterial(Brushes.Black);
            SelectedMaterial = CreateMaterial(Brushes.Red); ;
            UnselectedMaterial = CreateMaterial(Brushes.DarkBlue);
        }

        private DiffuseMaterial CreateMaterial(SolidColorBrush brush)
        {
            DiffuseMaterial res = new DiffuseMaterial();
            res.Brush = brush;
            res.Color = brush.Color;
            return res;
        }

        public Point3D Position { get; set; }
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

        internal void CreateInitialModel()
        {
            double si = 2;
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

        internal bool CheckHit(GeometryModel3D m)
        {
            bool res = false;
            if (m == model)
            {
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
            MoveControlMarker();
        }

        public void MoveControlMarker()
        {
            Point3D p = Position;
            double si = 2;
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