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
        private double length;
        private bool showVolume;
        public PrinterPlate()
        {
            Width = 210;
            Height = 210;
            Length = 210;
            BorderThickness = 8;
            BorderColour = Colors.CadetBlue;
            group = new Model3DGroup();
            showVolume = true;
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

        public double Length
        {
            get
            {
                return length;
            }
            set
            {
                if (value != length)
                {
                    length = value;
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
        public bool ShowVolume
        {
            get { return showVolume; }
            set
            {
                if (showVolume != value)
                {
                    showVolume = value;
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

                // left cube.
                MeshGeometry3D lmesh = MeshUtils.MakeCubeMesh(0, 0, 0, 1);
                Transform3DGroup lgroup = new Transform3DGroup();
                lgroup.Children.Add(new ScaleTransform3D(BorderThickness, thickness, width));
                lmesh.ApplyTransformation(lgroup);
                lmesh.ApplyTransformation(new TranslateTransform3D(-length / 2, 0, 0));
                Material lmaterial = new DiffuseMaterial(new SolidColorBrush(BorderColour));
                GeometryModel3D lmodel = new GeometryModel3D(lmesh, lmaterial);
                group.Children.Add(lmodel);

                lmesh = MeshUtils.MakeCubeMesh(0, 0, 0, 1);
                lgroup = new Transform3DGroup();
                lgroup.Children.Add(new ScaleTransform3D(BorderThickness, thickness, width));
                lmesh.ApplyTransformation(lgroup);
                lmesh.ApplyTransformation(new TranslateTransform3D(length / 2, 0, 0));
                lmaterial = new DiffuseMaterial(new SolidColorBrush(BorderColour));
                lmodel = new GeometryModel3D(lmesh, lmaterial);
                group.Children.Add(lmodel);

                lmesh = MeshUtils.MakeCubeMesh(0, 0, 0, 1);
                lgroup = new Transform3DGroup();
                lgroup.Children.Add(new ScaleTransform3D(length, thickness, BorderThickness));
                lmesh.ApplyTransformation(lgroup);
                lmesh.ApplyTransformation(new TranslateTransform3D(0, 0, -width / 2));
                lmaterial = new DiffuseMaterial(new SolidColorBrush(BorderColour));
                lmodel = new GeometryModel3D(lmesh, lmaterial);
                group.Children.Add(lmodel);

                lmesh = MeshUtils.MakeCubeMesh(0, 0, 0, 1);
                lgroup = new Transform3DGroup();
                lgroup.Children.Add(new ScaleTransform3D(length, thickness, BorderThickness));
                lmesh.ApplyTransformation(lgroup);
                lmesh.ApplyTransformation(new TranslateTransform3D(0, 0, width / 2));
                lmaterial = new DiffuseMaterial(new SolidColorBrush(BorderColour));
                lmodel = new GeometryModel3D(lmesh, lmaterial);
                group.Children.Add(lmodel);

                if (showVolume)
                {
                    lmesh = MeshUtils.MakeCubeMesh(0, 0, 0, 1);
                    lgroup = new Transform3DGroup();
                    lgroup.Children.Add(new ScaleTransform3D(BorderThickness, thickness, width));
                    lmesh.ApplyTransformation(lgroup);
                    lmesh.ApplyTransformation(new TranslateTransform3D(-length / 2, height, 0));
                    lmaterial = new DiffuseMaterial(new SolidColorBrush(BorderColour));
                    lmodel = new GeometryModel3D(lmesh, lmaterial);
                    group.Children.Add(lmodel);

                    lmesh = MeshUtils.MakeCubeMesh(0, 0, 0, 1);
                    lgroup = new Transform3DGroup();
                    lgroup.Children.Add(new ScaleTransform3D(BorderThickness, thickness, width));
                    lmesh.ApplyTransformation(lgroup);
                    lmesh.ApplyTransformation(new TranslateTransform3D(length / 2, height, 0));
                    lmaterial = new DiffuseMaterial(new SolidColorBrush(BorderColour));
                    lmodel = new GeometryModel3D(lmesh, lmaterial);
                    group.Children.Add(lmodel);

                    lmesh = MeshUtils.MakeCubeMesh(0, 0, 0, 1);
                    lgroup = new Transform3DGroup();
                    lgroup.Children.Add(new ScaleTransform3D(length, thickness, BorderThickness));
                    lmesh.ApplyTransformation(lgroup);
                    lmesh.ApplyTransformation(new TranslateTransform3D(0, height, -width / 2));
                    lmaterial = new DiffuseMaterial(new SolidColorBrush(BorderColour));
                    lmodel = new GeometryModel3D(lmesh, lmaterial);
                    group.Children.Add(lmodel);

                    lmesh = MeshUtils.MakeCubeMesh(0, 0, 0, 1);
                    lgroup = new Transform3DGroup();
                    lgroup.Children.Add(new ScaleTransform3D(length, thickness, BorderThickness));
                    lmesh.ApplyTransformation(lgroup);
                    lmesh.ApplyTransformation(new TranslateTransform3D(0, height, width / 2));
                    lmaterial = new DiffuseMaterial(new SolidColorBrush(BorderColour));
                    lmodel = new GeometryModel3D(lmesh, lmaterial);
                    group.Children.Add(lmodel);
                }
            }
        }
    }
}