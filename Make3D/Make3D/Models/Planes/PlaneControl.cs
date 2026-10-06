using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using MathsLib;

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
        public PlaneEquation equation;
        public double Radius;
        public bool InvertColours;

        internal PlaneControl(PlaneEquation equation, double radius, bool invertColours = false)
        {
            this.equation = equation;
            this.Radius = radius;
            this.InvertColours = invertColours;
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
            mt.Color = c1;
            mt.Brush = new SolidColorBrush(c1);
            gm.Material = mt;

            DiffuseMaterial mtb = new DiffuseMaterial();
            mtb.Color = c2;
            mtb.Brush = new SolidColorBrush(c2);
            gm.BackMaterial = mtb;
            return gm;
        }

        public virtual void MoveTo(double x, double y, double z)
        {
            equation.Origin.X = x;
            equation.Origin.Y = y;
            equation.Origin.Z = z;
            CreateShape();
            planeMesh = CreateMesh(c1, c2);
        }

        public void CreateShape()
        {
            int numberOfPoints = 20;
            points = equation.GetPointsOnPlane(Radius, numberOfPoints);
            points.Add(new Point3D(equation.Origin.X, equation.Origin.Y, equation.Origin.Z));
            int cp = numberOfPoints;
            faces = new Int32Collection();
            for (int i = 0; i < numberOfPoints; i++)
            {
                int j = i + 1;
                if (j == numberOfPoints)
                {
                    j = 0;
                }
                faces.Add(i);
                faces.Add(j);
                faces.Add(cp);
            }
            this.width = Radius;
            this.depth = Radius;
            c1 = Colors.LightGreen;
            c2 = Colors.Red;
            if (InvertColours)
            {
                c2 = Colors.LightGreen;
                c1 = Colors.Red;
            }

            planeMesh = CreateMesh(c1, c2);
        }

        internal bool Matches(GeometryModel3D geo)
        {
            return geo == planeMesh;
        }
    }
}