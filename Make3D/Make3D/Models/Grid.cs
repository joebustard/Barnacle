// **************************************************************************
// *   Copyright (c) 2024 Joe Bustard <barnacle3d@gmailcom>                  *
// *                                                                         *
// *   This file is part of the Barnacle 3D application.                     *
// *                                                                         *
// *   This application is free software. You can redistribute it and/or     *
// *   modify it under the terms of the GNU Library General Public           *
// *   License as published by the Free Software Foundation. Either          *
// *   version 2 of the License, or (at your option) any later version.      *
// *                                                                         *
// *   This application is distributed in the hope that it will be useful,   *
// *   but WITHOUT ANY WARRANTY. Without even the implied warranty of        *
// *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
// *   GNU Library General Public License for more details.                  *
// *                                                                         *
// *************************************************************************

using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Models
{
    public partial class Grid3D
    {
        private Model3DGroup group;
        private double length = 210;

        private bool showMillimetres;

        public Grid3D()
        {
            group = new Model3DGroup();
            Size = 210;
            showMillimetres = true;
            DefineModel(group);
        }

        public Model3DGroup Group
        {
            get
            {
                return group;
            }
        }

        public bool ShowMillimetres
        {
            get
            {
                return showMillimetres;
            }

            set
            {
                if (value != showMillimetres)
                {
                    showMillimetres = value;
                    DefineModel(group);
                }
            }
        }

        public double Size
        {
            get
            {
                return length;
            }
            set
            {
                if (length != value)
                {
                    length = value;
                    DefineModel(group);
                }
            }
        }

        internal bool Matches(GeometryModel3D geo)
        {
            foreach (GeometryModel3D gm in group.Children)
            {
                if (gm == geo)
                {
                    return true;
                }
            }
            return false;
        }

        protected GeometryModel3D GetModel(Point3DCollection Vertices, Int32Collection Faces)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.Positions = Vertices;
            mesh.TriangleIndices = Faces;
            mesh.Normals = null;
            GeometryModel3D gm = new GeometryModel3D();
            gm.Geometry = mesh;
            Color cl = Colors.CadetBlue;
            cl.A = 200;
            DiffuseMaterial mt = new DiffuseMaterial();
            mt.Color = cl;
            mt.Brush = new SolidColorBrush(cl);
            gm.Material = mt;
            DiffuseMaterial mtb = new DiffuseMaterial();
            mtb.Color = cl;
            mtb.Brush = new SolidColorBrush(cl);
            gm.BackMaterial = mtb;

            return gm;
        }

        private void AddCubeMesh(MeshGeometry3D mesh, double x, double y, double z, double l, double h, double w)
        {
            // Define the positions.
            l /= 2;
            h = h / 2;
            w = w / 2;
            Point3D[] points =
            {
                new Point3D(x - l, y - h, z - w),//0
                new Point3D(x + l, y - h, z - w),//1
                new Point3D(x + l, y - h, z + w),//2
                new Point3D(x - l, y - h, z + w),//3

                new Point3D(x - l, y + h, z - w),//4
                new Point3D(x + l, y + h, z - w),//5
                new Point3D(x + l, y + h, z + w),//6
                new Point3D(x - l, y + h, z + w),//7
            };
            int startPoint = mesh.Positions.Count;
            foreach (Point3D point in points) mesh.Positions.Add(point);

            // Define the triangles.
            Tuple<int, int, int>[] triangles =
            {
                 new Tuple<int, int, int>(0, 1, 2),
                 new Tuple<int, int, int>(0, 2, 3),
                 new Tuple<int, int, int>(4, 6, 5),
                 new Tuple<int, int, int>(4, 7, 6),
            };
            foreach (Tuple<int, int, int> tuple in triangles)
            {
                mesh.TriangleIndices.Add(tuple.Item1 + startPoint);
                mesh.TriangleIndices.Add(tuple.Item2 + startPoint);
                mesh.TriangleIndices.Add(tuple.Item3 + startPoint);
            }
        }

        private void DefineModel(Model3DGroup group)
        {
            group.Children.Clear();
            if (showMillimetres)
            {
                MeshGeometry3D mmMesh = new MeshGeometry3D();

                DefineText(group);
                int count = 0;
                for (double x = -length; x <= length; x += 1)
                {
                    if (count % 10 != 0)
                    {
                        AddCubeMesh(mmMesh, x, 0, 0, 0.25, 0.15, 2 * length);
                    }
                    count++;
                }
                count = 0;
                for (double z = -length; z <= length; z += 1)
                {
                    if (count % 10 != 0)
                    {
                        AddCubeMesh(mmMesh, 0, 0, z, 2 * length, 0.15, 0.25);
                    }
                    count++;
                }

                Material mmMaterial = new DiffuseMaterial(Brushes.LightBlue);
                GeometryModel3D mmModel = new GeometryModel3D(mmMesh, mmMaterial);
                group.Children.Add(mmModel);
            }
            MeshGeometry3D cmMesh = new MeshGeometry3D();
            for (double x = -length; x <= length; x += 10)
            {
                AddCubeMesh(cmMesh, x, 0, 0, 0.5, 0.2, 2 * length);
            }

            for (double z = -length; z <= length; z += 10)
            {
                AddCubeMesh(cmMesh, 0, 0, z, 2 * length, 0.2, 0.5);
            }
            Material cmmaterial = new DiffuseMaterial(Brushes.CadetBlue);
            GeometryModel3D cmmodel = new GeometryModel3D(cmMesh, cmmaterial);
            group.Children.Add(cmmodel);
        }

        private void DefineText(Model3DGroup group)
        {
            Point3DCollection Vertices = new Point3DCollection();
            Int32Collection Faces = new Int32Collection();

            for (int i = 0; i < tpnts.GetLength(0); i += 3)
            {
                Vertices.Add(new Point3D(tpnts[i], tpnts[i + 1], tpnts[i + 2] + 95));
            }
            for (int i = 0; i < fnums.GetLength(0); i++)
            {
                Faces.Add(fnums[i]);
            }

            GeometryModel3D gm = GetModel(Vertices, Faces);
            group.Children.Add(gm);
        }
    }
}