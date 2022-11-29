using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class SpringMaker : MakerBase
    {
        private double innerRadius;
        private double wireRadius;
        private double coilGap;
        private double turns;
        private double facesPerTurn;
        private double wireFacets;

        public SpringMaker(double innerRadius, double wireRadius, double coilGap, double turns, double facesPerTurn, double wireFacets)
        {
            this.innerRadius = innerRadius;
            this.wireRadius = wireRadius;
            this.coilGap = coilGap;
            this.turns = turns;
            this.facesPerTurn = facesPerTurn;
            this.wireFacets = wireFacets;
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            double maxTheta = turns * Math.PI * 2.0;
            double dTheta = Math.PI * 2.0 / facesPerTurn;
            int mainSteps = (int)(turns * facesPerTurn);
            double theta1;
            double theta2;
            double turnOffset = (coilGap + wireRadius) / facesPerTurn;
            List<int> startFacePoints = new List<int>();
            List<int> endFacePoints = new List<int>();
            double voff = 0;
            Point3D endMid = new Point3D(0, 0, 0);
            for (int i = 0; i < mainSteps - 1; i++)
            {
                int j = i + 1;
                if (j >= mainSteps)
                {
                    j = 0;
                }
                theta1 = (double)i * dTheta;
                theta2 = (double)j * dTheta;
                double phi1 = 0.0;
                double phi2;
                double dPhi = (Math.PI * 2.0) / wireFacets;
                for (int l = 0; l < wireFacets; l++)
                {
                    int k = l + 1;
                    if (k >= wireFacets)
                    {
                        k = 0;
                    }
                    phi1 = l * dPhi;
                    phi2 = k * dPhi;

                    Point3D p1 = TorusPoint(theta1, phi1, innerRadius + wireRadius, wireRadius, voff);
                    Point3D p2 = TorusPoint(theta1, phi2, innerRadius + wireRadius, wireRadius, voff);
                    Point3D p3 = TorusPoint(theta2, phi2, innerRadius + wireRadius, wireRadius, voff + turnOffset);
                    Point3D p4 = TorusPoint(theta2, phi1, innerRadius + wireRadius, wireRadius, voff + turnOffset);

                    int v1 = AddVertice(p1);
                    int v2 = AddVertice(p2);
                    int v3 = AddVertice(p3);
                    int v4 = AddVertice(p4);

                    Faces.Add(v1);
                    Faces.Add(v2);
                    Faces.Add(v3);

                    Faces.Add(v1);
                    Faces.Add(v3);
                    Faces.Add(v4);
                    if (i == 0)
                    {
                        startFacePoints.Add(v1);
                    }
                    if (j == mainSteps - 1)
                    {
                        endFacePoints.Add(v4);
                    }
                }
                endMid = TorusPoint(theta1, phi1, innerRadius + wireRadius, 0, voff);
                voff += turnOffset;
            }
            int cp = AddVertice(new Point3D(innerRadius + wireRadius, wireRadius, 0));

            for (int m = 0; m < startFacePoints.Count; m++)
            {
                int n = m + 1;
                if (n >= startFacePoints.Count)
                {
                    n = 0;
                }
                Faces.Add(cp);
                Faces.Add(startFacePoints[n]);
                Faces.Add(startFacePoints[m]);
            }
            int ep = AddVertice(endMid);

            for (int m = 0; m < endFacePoints.Count; m++)
            {
                int n = m + 1;
                if (n >= endFacePoints.Count)
                {
                    n = 0;
                }
                Faces.Add(ep);
                Faces.Add(endFacePoints[m]);
                Faces.Add(endFacePoints[n]);
            }
        }

        private Point3D TorusPoint(double t, double p, double mr, double hr, double yoff)
        {
            double r = mr + hr;
            double x = (mr + hr * Math.Cos(p)) * Math.Cos(t);
            double z = (mr + hr * Math.Cos(p)) * Math.Sin(t);
            double y = hr * Math.Sin(p);
            Point3D res = new Point3D(x, y + yoff, z);
            return res;
        }
    }
}