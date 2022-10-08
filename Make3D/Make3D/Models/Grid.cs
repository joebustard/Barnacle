using MakerLib;
using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Models
{
    public class Grid3D
    {
        private double length = 210;

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

        private Model3DGroup group;

        public Model3DGroup Group
        {
            get
            {
                return group;
            }
        }

        public Grid3D()
        {
            group = new Model3DGroup();
            Size = 210;
            DefineModel(group);
        }

        private void DefineModel(Model3DGroup group)
        {
            group.Children.Clear();
            DefineText("Barnacle", group);
            for (double x = -length; x <= length; x += 1)
            {
                MeshGeometry3D xmesh = MakeCubeMesh(0, 0, 0, 1);
                xmesh.ApplyTransformation(new ScaleTransform3D(0.05, 0.1, 2 * length));
                xmesh.ApplyTransformation(new TranslateTransform3D(x, 0, 0));
                Material xmaterial = new DiffuseMaterial(Brushes.LightBlue);
                GeometryModel3D xmodel = new GeometryModel3D(xmesh, xmaterial);
                group.Children.Add(xmodel);
            }

            for (double z = -length; z <= length; z += 1)
            {
                MeshGeometry3D xmesh = MakeCubeMesh(0, 0, 0, 1);
                xmesh.ApplyTransformation(new ScaleTransform3D(2 * length, 0.1, 0.05));
                xmesh.ApplyTransformation(new TranslateTransform3D(0, 0, z));
                Material xmaterial = new DiffuseMaterial(Brushes.LightBlue);
                GeometryModel3D xmodel = new GeometryModel3D(xmesh, xmaterial);
                group.Children.Add(xmodel);
            }

            for (double x = -length; x <= length; x += 10)
            {
                MeshGeometry3D xmesh = MakeCubeMesh(0, 0, 0, 1);
                xmesh.ApplyTransformation(new ScaleTransform3D(0.5, 0.1, 2 * length));
                xmesh.ApplyTransformation(new TranslateTransform3D(x, 0, 0));
                Material xmaterial = new DiffuseMaterial(Brushes.CadetBlue);
                GeometryModel3D xmodel = new GeometryModel3D(xmesh, xmaterial);
                group.Children.Add(xmodel);
            }

            for (double z = -length; z <= length; z += 10)
            {
                MeshGeometry3D xmesh = MakeCubeMesh(0, 0, 0, 1);
                xmesh.ApplyTransformation(new ScaleTransform3D(2 * length, 0.1, 0.5));
                xmesh.ApplyTransformation(new TranslateTransform3D(0, 0, z));
                Material xmaterial = new DiffuseMaterial(Brushes.CadetBlue);
                GeometryModel3D xmodel = new GeometryModel3D(xmesh, xmaterial);
                group.Children.Add(xmodel);
            }
        }

        private void DefineText(string text, Model3DGroup group)
        {
            if (text != null && text != "")
            {
                TextMaker mk = new TextMaker(text, "Tahoma", 14, 0.2, true, false, false);
                Point3DCollection Vertices = new Point3DCollection();
                Int32Collection Faces = new Int32Collection();
                mk.Generate(Vertices, Faces);
                Vector3D v = new Vector3D(-26, 0.1, 85);
                for (int i = 0; i < Vertices.Count; i++)
                {
                    Vertices[i] += v;
                }
                GeometryModel3D gm = GetModel(Vertices, Faces);
                group.Children.Add(gm);
            }
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

        // Make a mesh containing a cube centered at this point.
        private MeshGeometry3D MakeCubeMesh(double x, double y, double z, double width)
        {
            // Create the geometry.
            MeshGeometry3D mesh = new MeshGeometry3D();

            // Define the positions.
            width /= 2;
            Point3D[] points =
            {
                new Point3D(x - width, y - width, z - width),
                new Point3D(x + width, y - width, z - width),
                new Point3D(x + width, y - width, z + width),
                new Point3D(x - width, y - width, z + width),
                new Point3D(x - width, y - width, z + width),
                new Point3D(x + width, y - width, z + width),
                new Point3D(x + width, y + width, z + width),
                new Point3D(x - width, y + width, z + width),
                new Point3D(x + width, y - width, z + width),
                new Point3D(x + width, y - width, z - width),
                new Point3D(x + width, y + width, z - width),
                new Point3D(x + width, y + width, z + width),
                new Point3D(x + width, y + width, z + width),
                new Point3D(x + width, y + width, z - width),
                new Point3D(x - width, y + width, z - width),
                new Point3D(x - width, y + width, z + width),
                new Point3D(x - width, y - width, z + width),
                new Point3D(x - width, y + width, z + width),
                new Point3D(x - width, y + width, z - width),
                new Point3D(x - width, y - width, z - width),
                new Point3D(x - width, y - width, z - width),
                new Point3D(x - width, y + width, z - width),
                new Point3D(x + width, y + width, z - width),
                new Point3D(x + width, y - width, z - width),
            };
            foreach (Point3D point in points) mesh.Positions.Add(point);

            // Define the triangles.
            Tuple<int, int, int>[] triangles =
            {
                 new Tuple<int, int, int>(0, 1, 2),
                 new Tuple<int, int, int>(2, 3, 0),
                 new Tuple<int, int, int>(4, 5, 6),
                 new Tuple<int, int, int>(6, 7, 4),
                 new Tuple<int, int, int>(8, 9, 10),
                 new Tuple<int, int, int>(10, 11, 8),
                 new Tuple<int, int, int>(12, 13, 14),
                 new Tuple<int, int, int>(14, 15, 12),
                 new Tuple<int, int, int>(16, 17, 18),
                 new Tuple<int, int, int>(18, 19, 16),
                 new Tuple<int, int, int>(20, 21, 22),
                 new Tuple<int, int, int>(22, 23, 20),
            };
            foreach (Tuple<int, int, int> tuple in triangles)
            {
                mesh.TriangleIndices.Add(tuple.Item1);
                mesh.TriangleIndices.Add(tuple.Item2);
                mesh.TriangleIndices.Add(tuple.Item3);
            }

            return mesh;
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
    }
}