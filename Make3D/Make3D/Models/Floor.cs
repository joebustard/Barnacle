using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Make3D.Models
{
    public class Floor
    {
        public Floor()
        {
            floorMesh = CreateMesh();
        }
        public Point3DCollection FloorPoints3D
        {
            get
            {
                double x = 210.0; // floor width / 2
                double z = 210.0; // floor length / 2
                double floorDepth = -.1; // give the floor some depth so it's not a 2 dimensional plane

                Point3DCollection points = new Point3DCollection(20);
                Point3D point;
                //top of the floor
                point = new Point3D(-x, 0, z);// Floor Index - 0
                points.Add(point);
                point = new Point3D(x, 0, z);// Floor Index - 1
                points.Add(point);
                point = new Point3D(x, 0, -z);// Floor Index - 2
                points.Add(point);
                point = new Point3D(-x, 0, -z);// Floor Index - 3
                points.Add(point);
                //front side
                point = new Point3D(-x, 0, z);// Floor Index - 4
                points.Add(point);
                point = new Point3D(-x, floorDepth, z);// Floor Index - 5
                points.Add(point);
                point = new Point3D(x, floorDepth, z);// Floor Index - 6
                points.Add(point);
                point = new Point3D(x, 0, z);// Floor Index - 7
                points.Add(point);
                //right side
                point = new Point3D(x, 0, z);// Floor Index - 8
                points.Add(point);
                point = new Point3D(x, floorDepth, z);// Floor Index - 9
                points.Add(point);
                point = new Point3D(x, floorDepth, -z);// Floor Index - 10
                points.Add(point);
                point = new Point3D(x, 0, -z);// Floor Index - 11
                points.Add(point);
                //back side
                point = new Point3D(x, 0, -z);// Floor Index - 12
                points.Add(point);
                point = new Point3D(x, floorDepth, -z);// Floor Index - 13
                points.Add(point);
                point = new Point3D(-x, floorDepth, -z);// Floor Index - 14
                points.Add(point);
                point = new Point3D(-x, 0, -z);// Floor Index - 15
                points.Add(point);
                //left side
                point = new Point3D(-x, 0, -z);// Floor Index - 16
                points.Add(point);
                point = new Point3D(-x, floorDepth, -z);// Floor Index - 17
                points.Add(point);
                point = new Point3D(-x, floorDepth, z);// Floor Index - 18
                points.Add(point);
                point = new Point3D(-x, 0, z);// Floor Index - 19
                points.Add(point);
                return points;
            }
        }

        public Int32Collection FloorPointsIndices
        {
            get
            {
                int[] indices = new int[] { 0, 1, 2, 0, 2, 3, 4, 5, 7, 5, 6, 7, 8, 9, 11, 9, 10, 11, 12, 13, 15, 13,
       14, 15, 16, 17, 19, 17, 18, 19 };

                return new Int32Collection(indices);
            }
        }

        private GeometryModel3D floorMesh;
        private Vector3DCollection floorNormals;
        private Point3DCollection floorObjectVertices;
        private Int32Collection floorTriangleIndices;

        public Int32Collection FloorTriangleIndices
        {
            get
            {
                return floorTriangleIndices;
            }
            set
            {
                if (floorTriangleIndices != value)
                {
                    floorTriangleIndices = value;
                    // NotifyPropertyChanged();
                }
            }
        }

        public GeometryModel3D CreateMesh()
        {
            GeometryModel3D gm = new GeometryModel3D();
            MeshGeometry3D fl = new MeshGeometry3D();
            fl.Positions = FloorPoints3D;
            fl.TriangleIndices = FloorPointsIndices;
            gm.Geometry = fl;

            DiffuseMaterial mt = new DiffuseMaterial();
            mt.Color = Colors.LightGray;
            mt.Brush = new SolidColorBrush(Color.FromArgb(60, 200, 200, 255));
            gm.Material = mt;

            DiffuseMaterial mtb = new DiffuseMaterial();
            mtb.Color = Colors.Gray;
            mtb.Brush = new SolidColorBrush(Color.FromArgb(60, 100, 100, 100)); ;
            gm.BackMaterial = mtb;
            return gm;
        }

        private Vector3DCollection FloorNormals
        {
            get
            {
                return floorNormals;
            }
            set
            {
                if (floorNormals != value)
                {
                    floorNormals = value;
                    // NotifyPropertyChanged();
                }
            }
        }

        public GeometryModel3D FloorMesh
        {
              get { return floorMesh; }
        }
        public Point3DCollection FloorObjectVertices
        {
            get
            {
                return floorObjectVertices;
            }
            set
            {
                if (floorObjectVertices != value)
                {
                    floorObjectVertices = value;
                    // NotifyPropertyChanged();
                }
            }
        }

        internal bool Matches(GeometryModel3D geo)
        {
            return geo == floorMesh;
        }
    }
}