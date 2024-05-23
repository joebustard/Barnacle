using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Models
{
    public class Axies
    {
        public double Size { get; set; }

        private Model3DGroup group;

        public Model3DGroup Group
        {
            get
            {
                return group;
            }
        }

        public Axies()
        {
            Size = 210;
            group = new Model3DGroup();
            DefineModel(group);
        }

        private void DefineModel(Model3DGroup group)
        {
            // Origin cube.
            MeshGeometry3D mesh = MeshUtils.MakeBorder(0, 0, 0, 0.1);
            Material material = new DiffuseMaterial(Brushes.Yellow);
            GeometryModel3D model = new GeometryModel3D(mesh, material);
            group.Children.Add(model);

            const double thickness = 0.3;
            double length = Size;

            // X axis.
            MeshGeometry3D xmesh = MeshUtils.MakeBorder(0, 0, 0, 1);
            xmesh.ApplyTransformation(new ScaleTransform3D(length, thickness+0.1, thickness));
            xmesh.ApplyTransformation(new TranslateTransform3D(length / 2, 0.1, 0));
            Material xmaterial = new DiffuseMaterial(Brushes.Red);
            GeometryModel3D xmodel = new GeometryModel3D(xmesh, xmaterial);
            group.Children.Add(xmodel);

            // Y axis cube.
            MeshGeometry3D ymesh = MeshUtils.MakeBorder(0, 0, 0, 1);
            ymesh.ApplyTransformation(new ScaleTransform3D(thickness, length, thickness));
            ymesh.ApplyTransformation(new TranslateTransform3D(0, length / 2, 0));
            Material ymaterial = new DiffuseMaterial(Brushes.Green);
            GeometryModel3D ymodel = new GeometryModel3D(ymesh, ymaterial);
            group.Children.Add(ymodel);

            // Z axis cube.
            MeshGeometry3D zmesh = MeshUtils.MakeBorder(0, 0, 0, 1);
            Transform3DGroup zgroup = new Transform3DGroup();
            zgroup.Children.Add(new ScaleTransform3D(thickness, thickness+.1, length));
            zgroup.Children.Add(new TranslateTransform3D(0, 0.1, length / 2));
            zmesh.ApplyTransformation(zgroup);
            Material zmaterial = new DiffuseMaterial(Brushes.Blue);
            GeometryModel3D zmodel = new GeometryModel3D(zmesh, zmaterial);
            group.Children.Add(zmodel);
        }
    }
}