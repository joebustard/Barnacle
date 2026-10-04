using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Windows.Media;

namespace Barnacle.Models
{
    internal class PlaneControl
    {
        protected Color c1;
        protected Color c2;
        protected double depth;
        protected Int32Collection faces;

        protected GeometryModel3D planeMesh;
        protected Point3DCollection points;
        protected double width;

        internal PlaneControl(double width, double depth, bool invertColours = false)
        {
            points = new Point3DCollection(20);
            SetLocation(0, 0, 0);
            //           int[] indices = new int[] { 0, 1, 2, 0, 2, 3, 4, 5, 7, 5, 6, 7, 8, 9, 11, 9, 10, 11, 12, 13, 15, 13,
            //      14, 15, 16, 17, 19, 17, 18, 19 };

            int[] indices = new int[] { 0, 1, 2, 0, 2, 3 };

            faces = new Int32Collection(indices);
            this.width = width;
            this.depth = depth;
            c1 = Colors.LightGreen;
            c2 = Colors.Red;
            if (invertColours)
            {
                c2 = Colors.LightGreen;
                c1 = Colors.Red;
            }

            planeMesh = CreateMesh(c1, c2);
        }

        public Int32Collection Indices
        {
            get
            {
                return faces;
            }
        }

        public GeometryModel3D PlaneMesh
        {
            get
            {
                return planeMesh;
            }
        }

        public Point3DCollection Points
        {
            get
            {
                return points;
            }
        }

        public GeometryModel3D CreateMesh(Color c1, Color c2)
        {
            GeometryModel3D gm = new GeometryModel3D();
            MeshGeometry3D fl = new MeshGeometry3D();
            fl.Positions = points;
            fl.TriangleIndices = faces;
            gm.Geometry = fl;

            DiffuseMaterial mt = new DiffuseMaterial();
            mt.Color = Colors.LightGreen;
            mt.Brush = new SolidColorBrush(c1);
            gm.Material = mt;

            DiffuseMaterial mtb = new DiffuseMaterial();
            mtb.Color = Colors.Red;
            mtb.Brush = new SolidColorBrush(c2);
            gm.BackMaterial = mtb;
            return gm;
        }

        public void MoveTo(double x, double y, double z)
        {
            SetLocation(x, y, z);
            planeMesh = CreateMesh(c1, c2);
        }

        public virtual void SetLocation(double x, double y, double z)
        {
        }

        internal bool Matches(GeometryModel3D geo)
        {
            return geo == planeMesh;
        }
    }
}