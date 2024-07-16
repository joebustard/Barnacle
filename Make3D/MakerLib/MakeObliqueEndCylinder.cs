using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class ObliqueEndCylinderMaker : MakerBase
    {

        private double radius;
        private double mainHeight;
        private double cutHeight;
        private double cutStyle;
        private double cutPoints;

        public ObliqueEndCylinderMaker(double radius, double mainHeight, double cutHeight, double cutStyle, double cutPoints)
        {
            this.radius = radius;
            this.mainHeight = mainHeight;
            this.cutHeight = cutHeight;
            this.cutStyle = cutStyle;
            this.cutPoints = cutPoints;

        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;

            double theta = 0;
            double numDivisions = 72;
            double dTheta = (2.0 * Math.PI) / numDivisions;
            for (int i = 0; i < numDivisions; i++)
            {
                Point p = CalcPoint(theta, radius);
                Point p1 = CalcPoint(theta + dTheta, radius);
                double h = CalcExtraHeight(theta) * cutHeight;
                double h1 = CalcExtraHeight(theta + dTheta) * cutHeight;

                int v0 = AddVertice(p.X, 0, p.Y);
                int v1 = AddVertice(p.X, mainHeight + h, p.Y);
                int v2 = AddVertice(p1.X, mainHeight + h1, p1.Y);
                int v3 = AddVertice(p1.X, 0, p1.Y);

                Faces.Add(v0);
                Faces.Add(v1);
                Faces.Add(v2);

                Faces.Add(v0);
                Faces.Add(v2);
                Faces.Add(v3);

                int vc = AddVertice(0, 0, 0);
                Faces.Add(v0);
                Faces.Add(v3);
                Faces.Add(vc);

                int tc = AddVertice(0, mainHeight+ (cutHeight/2), 0);
                Faces.Add(v1);
                Faces.Add(tc);
                Faces.Add(v2);

                theta += dTheta;
            }
        }

        private double CalcExtraHeight(double v)
        {
            double h = 0;

            switch (cutStyle)
            {
                case 1:
                    {

                        Point p = CalcPoint(v, radius);
                        h = (p.X + radius) / (2.0 * radius);
                        //double delta = Math.PI;
                        //h = Math.Abs((v -delta)/delta);
                    }
                    break;

                case 2:
                    {
                        h = Math.Abs(Math.Cos((v * cutPoints) / 2));
                    }
                    break;

            }
            return h;
        }
    }
}
