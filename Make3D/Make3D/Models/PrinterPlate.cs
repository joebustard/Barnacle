using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Models
{
    internal class PrinterPlate
    {
        private Color borderColour;
        private double borderThickness;
        private Model3DGroup group;

        private double height;

        private double width;

        public PrinterPlate()
        {
            Width = 210;
            Height = 210;
            BorderThickness = 8;
            BorderColour = Colors.SandyBrown;
            group = new Model3DGroup();
            DefineModel(group);
        }

        public Color BorderColour
        {
            get
            {
                return borderColour;
            }
            set
            {
                if (value != borderColour)
                {
                    borderColour = value;
                    DefineModel(group);
                }
            }
        }

        public double BorderThickness
        {
            get
            {
                return borderThickness;
            }
            set
            {
                if (value != borderThickness)
                {
                    borderThickness = value;
                    DefineModel(group);
                }
            }
        }

        public Model3DGroup Group
        {
            get
            {
                return group;
            }
        }

        public double Height
        {
            get
            {
                return height;
            }
            set
            {
                if (value != height)
                {
                    height = value;
                    DefineModel(group);
                }
            }
        }

        public double Width
        {
            get
            {
                return width;
            }
            set
            {
                if (value != width)
                {
                    width = value;
                    DefineModel(group);
                }
            }
        }

        private void DefineModel(Model3DGroup group)
        {
            if (group != null)
            {
                group.Children.Clear();
                const double thickness = 0.2;
                double length = Height;
                // left cube.
                MeshGeometry3D lmesh = MeshUtils.MakeCubeMesh(0, 0, 0, 1);
                Transform3DGroup lgroup = new Transform3DGroup();
                lgroup.Children.Add(new ScaleTransform3D(BorderThickness, thickness, length));
                lmesh.ApplyTransformation(lgroup);
                lmesh.ApplyTransformation(new TranslateTransform3D(-Width / 2, 0, 0));
                Material lmaterial = new DiffuseMaterial(new SolidColorBrush(BorderColour));
                GeometryModel3D lmodel = new GeometryModel3D(lmesh, lmaterial);
                group.Children.Add(lmodel);

                lmesh = MeshUtils.MakeCubeMesh(0, 0, 0, 1);
                lgroup = new Transform3DGroup();
                lgroup.Children.Add(new ScaleTransform3D(BorderThickness, thickness, length));
                lmesh.ApplyTransformation(lgroup);
                lmesh.ApplyTransformation(new TranslateTransform3D(Width / 2, 0, 0));
                lmaterial = new DiffuseMaterial(new SolidColorBrush(BorderColour));
                lmodel = new GeometryModel3D(lmesh, lmaterial);
                group.Children.Add(lmodel);

                lmesh = MeshUtils.MakeCubeMesh(0, 0, 0, 1);
                lgroup = new Transform3DGroup();
                lgroup.Children.Add(new ScaleTransform3D(Width, thickness, BorderThickness));
                lmesh.ApplyTransformation(lgroup);
                lmesh.ApplyTransformation(new TranslateTransform3D(0, 0, -Height / 2));
                lmaterial = new DiffuseMaterial(new SolidColorBrush(BorderColour));
                lmodel = new GeometryModel3D(lmesh, lmaterial);
                group.Children.Add(lmodel);

                lmesh = MeshUtils.MakeCubeMesh(0, 0, 0, 1);
                lgroup = new Transform3DGroup();
                lgroup.Children.Add(new ScaleTransform3D(Width, thickness, BorderThickness));
                lmesh.ApplyTransformation(lgroup);
                lmesh.ApplyTransformation(new TranslateTransform3D(0, 0, Height / 2));
                lmaterial = new DiffuseMaterial(new SolidColorBrush(BorderColour));
                lmodel = new GeometryModel3D(lmesh, lmaterial);
                group.Children.Add(lmodel);
            }
        }
    }
}