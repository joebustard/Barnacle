using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class PieMaker : MakerBase
    {
        private const int steps = 36;
        private double radius;
        private double centreThickness;
        private double edgeThickness;
        private double sweep;

        public PieMaker(double radius, double centreThickness, double edgeThickness, double sweep)
        {
            this.radius = radius;
            this.centreThickness = centreThickness;
            this.edgeThickness = edgeThickness;
            this.sweep = sweep;
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            MakeIt();
        }

        private void MakeIt()
        {
            double theta = 0;
            double upperTheta = DegToRad(sweep);
            double theta2;
            Point rotatedPoint;
            double dt = upperTheta / steps;
            int cp = AddVertice(0, 0, 0);
            int cpTop = AddVertice(0, centreThickness, 0);

            // bottom
            for (int i = 0; i < steps; i++)
            {
                theta = i * dt;
                theta2 = theta + dt;
                rotatedPoint = CalcPoint(theta, radius);

                int p0 = AddVertice(rotatedPoint.X, 0, rotatedPoint.Y);
                rotatedPoint = CalcPoint(theta2, radius);
                int p1 = AddVertice(rotatedPoint.X, 0, rotatedPoint.Y);
                AddFace(cp, p0, p1);
            }

            // top
            for (int i = 0; i < steps; i++)
            {
                theta = i * dt;
                theta2 = theta + dt;
                rotatedPoint = CalcPoint(theta, radius);

                int p0 = AddVertice(rotatedPoint.X, edgeThickness, rotatedPoint.Y);
                rotatedPoint = CalcPoint(theta2, radius);
                int p1 = AddVertice(rotatedPoint.X, edgeThickness, rotatedPoint.Y);
                AddFace(cpTop, p1, p0);
            }

            // close rounded edge
            for (int i = 0; i < steps; i++)
            {
                theta = i * dt;
                theta2 = theta + dt;
                rotatedPoint = CalcPoint(theta, radius);

                int p0 = AddVertice(rotatedPoint.X, 0, rotatedPoint.Y);
                int p1 = AddVertice(rotatedPoint.X, edgeThickness, rotatedPoint.Y);
                rotatedPoint = CalcPoint(theta2, radius);
                int p2 = AddVertice(rotatedPoint.X, edgeThickness, rotatedPoint.Y);
                int p3 = AddVertice(rotatedPoint.X, 0, rotatedPoint.Y);
                AddFace(p0, p1, p2);
                AddFace(p0, p2, p3);
            }

            // close start
            rotatedPoint = CalcPoint(0, radius);
            int p4 = AddVertice(rotatedPoint.X, 0, rotatedPoint.Y);
            int p5 = AddVertice(rotatedPoint.X, edgeThickness, rotatedPoint.Y);
            AddFace(cp, p5, p4);
            AddFace(cpTop, p5, cp);

            rotatedPoint = CalcPoint(DegToRad(sweep), radius);
            p4 = AddVertice(rotatedPoint.X, 0, rotatedPoint.Y);
            p5 = AddVertice(rotatedPoint.X, edgeThickness, rotatedPoint.Y);
            AddFace(p4, p5, cp);
            AddFace(cpTop, cp, p5);
        }
    }
}