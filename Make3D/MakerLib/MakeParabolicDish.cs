using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class ParabolicDishMaker : MakerBase
    {
        private double radius;
        private double wallThickness;
        private int pitch;

        public ParabolicDishMaker(double radius, double wallThickness, int pitch)
        {
            this.radius = radius;
            this.wallThickness = wallThickness;
            this.pitch = pitch;
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            // basically generate a simple parabola using the equation
            // y = ( x / pitch) * ( x/pitch) + c;
            // where c = 0 for outer wall and c = wallthickness for inner wall.
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            double sweepangle = 360;
            int numsegs = 72;

            // outter wall
            List<PolarCoordinate> profile = new List<PolarCoordinate>();
            double c = 0;
            double my = 0;
            for (double x = 0.0; x < radius; x += 1.0)
            {
                double y = (x / pitch) * (x / pitch) + c;
                my = y;
                PolarCoordinate pc = new PolarCoordinate(0, 0, 0);
                pc.SetPoint3D(new Point3D(x, 0, y));
                profile.Add(pc);
            }

            SweepPolarProfile(profile, 360, numsegs, true, true);
            // outter wall
            profile = new List<PolarCoordinate>();
            c = wallThickness;
            for (double x = 0.0; x < radius; x += 1.0)
            {
                double y = (x / pitch) * (x / pitch) + c;
                if (y > my)
                {
                    y = my;
                }
                PolarCoordinate pc = new PolarCoordinate(0, 0, 0);
                pc.SetPoint3D(new Point3D(x, 0, y));
                profile.Add(pc);
            }

            SweepPolarProfile(profile, 360, numsegs, false, false);
            double dt = 2.0 * Math.PI;
            double r1 = radius;
            double r2 = r1 - wallThickness;

            for (int i = 0; i < numsegs; i++)
            {
                int j = i + 1;
                if (j == numsegs)
                {
                    j = 0;
                }
                double theta1 = (i * dt);
                double theta2 = (j * dt);
                double x1 = r1 * Math.Sin(theta1);
                double z1 = r1 * Math.Cos(theta1);

                double x2 = r2 * Math.Sin(theta1);
                double z2 = r2 * Math.Cos(theta1);

                double x3 = r1 * Math.Sin(theta2);
                double z3 = r1 * Math.Cos(theta2);

                double x4 = r2 * Math.Sin(theta2);
                double z4 = r2 * Math.Cos(theta2);
                Point3D p1 = new Point3D(x1, my, z1);
                Point3D p2 = new Point3D(x2, my, z2);
                Point3D p3 = new Point3D(x3, my, z3);
                Point3D p4 = new Point3D(x4, my, z4);

                int v1 = AddVertice(p1);
                int v2 = AddVertice(p2);
                int v3 = AddVertice(p3);
                int v4 = AddVertice(p4);

                Faces.Add(v1);
                Faces.Add(v3);
                Faces.Add(v2);

                Faces.Add(v1);
                Faces.Add(v4);
                Faces.Add(v3);
            }
        }

        internal void SweepPolarProfile(List<PolarCoordinate> polarProfile, double sweepRange, int numSegs, bool clear, bool invert)
        {
            // now we have a lovely copy of the profile in polar coordinates.
            if (clear)
            {
                Vertices.Clear();
                Faces.Clear();
            }

            double sweep = sweepRange * (Math.PI * 2.0) / 360.0;
            double da = sweep / (numSegs - 1);
            for (int i = 0; i < numSegs; i++)
            {
                double a = da * i;
                int j = i + 1;
                if (j == numSegs)
                {
                    if (sweepRange == 360)
                    {
                        j = 0;
                    }
                    else
                    {
                        // dont connect end to start if the sweep doesn't go all the way round
                        break;
                    }
                }
                double b = da * j;

                for (int index = 0; index < polarProfile.Count - 1; index++)
                {
                    int index2 = index + 1;
                    if (index2 == polarProfile.Count)
                    {
                        index2 = 0;
                    }
                    PolarCoordinate pc1 = polarProfile[index].Clone();
                    PolarCoordinate pc2 = polarProfile[index2].Clone();
                    PolarCoordinate pc3 = polarProfile[index2].Clone();
                    PolarCoordinate pc4 = polarProfile[index].Clone();
                    pc1.Theta -= a;
                    pc2.Theta -= a;
                    pc3.Theta -= b;
                    pc4.Theta -= b;

                    Point3D p1 = pc1.GetPoint3D();
                    Point3D p2 = pc2.GetPoint3D();
                    Point3D p3 = pc3.GetPoint3D();
                    Point3D p4 = pc4.GetPoint3D();

                    int v1 = AddVertice(p1);
                    int v2 = AddVertice(p2);
                    int v3 = AddVertice(p3);
                    int v4 = AddVertice(p4);
                    if (invert)
                    {
                        Faces.Add(v1);
                        Faces.Add(v3);
                        Faces.Add(v2);

                        Faces.Add(v1);
                        Faces.Add(v4);
                        Faces.Add(v3);
                    }
                    else
                    {
                        Faces.Add(v1);
                        Faces.Add(v2);
                        Faces.Add(v3);

                        Faces.Add(v1);
                        Faces.Add(v3);
                        Faces.Add(v4);
                    }
                }
            }
        }
    }
}