using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Models
{
    internal class HorizontalPlane
    {
        private Int32Collection faces;

        private GeometryModel3D planeMesh;

        private Vector3DCollection planeNormals;

        private Point3DCollection planeObjectVertices;

        private Int32Collection planeTriangleIndices;

        private Point3DCollection points;

        public HorizontalPlane(double planeLevel)
        {
            points = new Point3DCollection(20);
            SetLocation(planeLevel);
            int[] indices = new int[] { 0, 1, 2, 0, 2, 3, 4, 5, 7, 5, 6, 7, 8, 9, 11, 9, 10, 11, 12, 13, 15, 13,
       14, 15, 16, 17, 19, 17, 18, 19 };

            faces = new Int32Collection(indices);
            planeMesh = CreateMesh();
        }

        public GeometryModel3D HorizontalPlaneMesh
        {
            get { return planeMesh; }
        }

        public Point3DCollection HorizontalPlaneObjectVertices
        {
            get
            {
                return planeObjectVertices;
            }
            set
            {
                if (planeObjectVertices != value)
                {
                    planeObjectVertices = value;
                }
            }
        }

        public Point3DCollection HorizontalPlanePoints3D
        {
            get
            {
                return points;
            }
        }

        public Int32Collection HorizontalPlanePointsIndices
        {
            get
            {
                return new Int32Collection(faces);
            }
        }

        public Int32Collection HorizontalPlaneTriangleIndices
        {
            get
            {
                return planeTriangleIndices;
            }
            set
            {
                if (planeTriangleIndices != value)
                {
                    planeTriangleIndices = value;
                    // NotifyPropertyChanged();
                }
            }
        }

        public GeometryModel3D PlaneMesh
        {
            get { return planeMesh; }
        }

        private Vector3DCollection HorizontalPlaneNormals
        {
            get
            {
                return planeNormals;
            }
            set
            {
                if (planeNormals != value)
                {
                    planeNormals = value;
                }
            }
        }

        public GeometryModel3D CreateMesh()
        {
            GeometryModel3D gm = new GeometryModel3D();
            MeshGeometry3D fl = new MeshGeometry3D();
            fl.Positions = HorizontalPlanePoints3D;
            fl.TriangleIndices = HorizontalPlanePointsIndices;
            gm.Geometry = fl;

            DiffuseMaterial mt = new DiffuseMaterial();
            mt.Color = Colors.LightGreen;
            mt.Brush = new SolidColorBrush(Colors.LightGreen);
            gm.Material = mt;

            DiffuseMaterial mtb = new DiffuseMaterial();
            mtb.Color = Colors.Red;
            mtb.Brush = new SolidColorBrush(Colors.Red);
            gm.BackMaterial = mtb;
            return gm;
        }

        public void MoveTo(double y)
        {
            SetLocation(y);
            planeMesh = CreateMesh();
        }

        public void SetLocation(double v)
        {
            double x = 210.0; // floor width / 2
            double z = 210.0; // floor length / 2
            double thick = -.1; // give the floor some depth so it's not a 2 dimensional plane

            points = new Point3DCollection(20);
            Point3D point;
            //top of the floor
            point = new Point3D(-x, v, z);// HorizontalPlane Index - 0
            points.Add(point);
            point = new Point3D(x, v, z);// HorizontalPlane Index - 1
            points.Add(point);
            point = new Point3D(x, v, -z);// HorizontalPlane Index - 2
            points.Add(point);
            point = new Point3D(-x, v, -z);// HorizontalPlane Index - 3
            points.Add(point);
            //front side
            point = new Point3D(-x, v, z);// HorizontalPlane Index - 4
            points.Add(point);
            point = new Point3D(-x, v - thick, z);// HorizontalPlane Index - 5
            points.Add(point);
            point = new Point3D(x, v - thick, z);// HorizontalPlane Index - 6
            points.Add(point);
            point = new Point3D(x, v, z);// HorizontalPlane Index - 7
            points.Add(point);
            //right side
            point = new Point3D(x, v, z);// HorizontalPlane Index - 8
            points.Add(point);
            point = new Point3D(x, v - thick, z);// HorizontalPlane Index - 9
            points.Add(point);
            point = new Point3D(x, v - thick, -z);// HorizontalPlane Index - 10
            points.Add(point);
            point = new Point3D(x, v, -z);// HorizontalPlane Index - 11
            points.Add(point);
            //back side
            point = new Point3D(x, v, -z);// HorizontalPlane Index - 12
            points.Add(point);
            point = new Point3D(x, v - thick, -z);// HorizontalPlane Index - 13
            points.Add(point);
            point = new Point3D(-x, v - thick, -z);// HorizontalPlane Index - 14
            points.Add(point);
            point = new Point3D(-x, v, -z);// HorizontalPlane Index - 15
            points.Add(point);
            //left side
            point = new Point3D(-x, v, -z);// HorizontalPlane Index - 16
            points.Add(point);
            point = new Point3D(-x, v - thick, -z);// HorizontalPlane Index - 17
            points.Add(point);
            point = new Point3D(-x, v - thick, z);// HorizontalPlane Index - 18
            points.Add(point);
            point = new Point3D(-x, v, z);// HorizontalPlane Index - 19
            points.Add(point);
        }

        internal bool Matches(GeometryModel3D geo)
        {
            return geo == planeMesh;
        }
    }
}