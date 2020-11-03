using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Make3D.Models
{
    public class Axies
    {

        Model3DGroup group;
        public Model3DGroup Group
        {
            get
            {
                return group;
            }
        }
        public Axies()
        {
            group = new Model3DGroup();
            DefineModel(group);
        }
        private void DefineModel(Model3DGroup group)
        {
            // Origin cube.
            MeshGeometry3D mesh = MakeCubeMesh(0, 0, 0, 0.1);
            Material material = new DiffuseMaterial(Brushes.Yellow);
            GeometryModel3D model = new GeometryModel3D(mesh, material);
            group.Children.Add(model);

            const double thickness = 0.1;
            const double length = 210;

            // X axis.
            MeshGeometry3D xmesh = MakeCubeMesh(0, 0, 0, 1);
            xmesh.ApplyTransformation(new ScaleTransform3D(length, thickness, thickness));
            xmesh.ApplyTransformation(new TranslateTransform3D(length / 2, 0, 0));
            Material xmaterial = new DiffuseMaterial(Brushes.Red);
            GeometryModel3D xmodel = new GeometryModel3D(xmesh, xmaterial);
            group.Children.Add(xmodel);

            // Y axis cube.
            MeshGeometry3D ymesh = MakeCubeMesh(0, 0, 0, 1);
            ymesh.ApplyTransformation(new ScaleTransform3D(thickness, length, thickness));
            ymesh.ApplyTransformation(new TranslateTransform3D(0, length / 2, 0));
            Material ymaterial = new DiffuseMaterial(Brushes.Green);
            GeometryModel3D ymodel = new GeometryModel3D(ymesh, ymaterial);
            group.Children.Add(ymodel);

            // Z axis cube.
            MeshGeometry3D zmesh = MakeCubeMesh(0, 0, 0, 1);
            Transform3DGroup zgroup = new Transform3DGroup();
            zgroup.Children.Add(new ScaleTransform3D(thickness, thickness, length));
            zgroup.Children.Add(new TranslateTransform3D(0, 0, length / 2));
            zmesh.ApplyTransformation(zgroup);
            Material zmaterial = new DiffuseMaterial(Brushes.Blue);
            GeometryModel3D zmodel = new GeometryModel3D(zmesh, zmaterial);
            group.Children.Add(zmodel);
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
    }
}
