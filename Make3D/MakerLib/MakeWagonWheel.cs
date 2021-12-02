using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class WagonWheelMaker : MakerBase
    {
        private double axleBore;
        private double hubRadius;
        private double hubThickness;
        private double numberOfSpokes;
        private double rimDepth;
        private double rimInnerRadius;
        private double rimThickness;
        private double spokeRadius;

        public WagonWheelMaker(double hubRadius, double hubThickness, double rimInnerRadius, double rimThickness, double rimDepth, double numberOfSpokes, double spokeRadius, double axleBore)
        {
            this.hubRadius = hubRadius;
            this.hubThickness = hubThickness;
            this.rimInnerRadius = rimInnerRadius;
            this.rimThickness = rimThickness;
            this.rimDepth = rimDepth;
            this.numberOfSpokes = numberOfSpokes;
            this.spokeRadius = spokeRadius;
            this.axleBore = axleBore;
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            GenerateHub();
            GenerateRim();
            GenerateSpokes();
        }

        private void AddProfilePnt(List<PolarCoordinate> polarProfile, double x, double z)
        {
            Point3D p3d = new Point3D(x, 0, z);
            PolarCoordinate pcol = new PolarCoordinate(0, 0, 0);
            pcol.SetPoint3D(p3d);
            polarProfile.Add(pcol);
        }

        private void GenerateHub()
        {
            List<PolarCoordinate> polarProfile = new List<PolarCoordinate>();
            int rotDivisions = 120;
            double px = axleBore;
            double halfheight = hubThickness / 2;
            AddProfilePnt(polarProfile, px, -halfheight);
            AddProfilePnt(polarProfile, px, halfheight);
            AddProfilePnt(polarProfile, px + hubRadius, halfheight);
            AddProfilePnt(polarProfile, px + hubRadius, -halfheight);
            SweepPolarProfileTheta(polarProfile, px, 0, 360, rotDivisions);
        }

        private void GenerateRim()
        {
            List<PolarCoordinate> polarProfile = new List<PolarCoordinate>();
            int rotDivisions = 120;
            double px = axleBore + hubRadius + rimInnerRadius;
            double halfheight = rimThickness / 2;
            AddProfilePnt(polarProfile, px, -halfheight);
            AddProfilePnt(polarProfile, px, halfheight);
            AddProfilePnt(polarProfile, px + rimDepth, halfheight);
            AddProfilePnt(polarProfile, px + rimDepth, -halfheight);
            SweepPolarProfileTheta(polarProfile, px, 0, 360, rotDivisions, false);
        }

        private void GenerateSpokes()
        {
            if (numberOfSpokes >= 2)
            {
                double dspoke = (Math.PI * 2) / numberOfSpokes;
                double mp = axleBore + hubRadius + (0.5 * rimInnerRadius);
                double spokeLength = rimInnerRadius + (0.5 * hubRadius) + (0.5 * rimDepth);

                for (int i = 0; i < numberOfSpokes; i++)
                {
                    Object3D spoke = new Object3D();
                    spoke.BuildPrimitive("cylinder");
                    // Scale the cylinder
                    // rotate to floor
                    // move to calculated position
                    // rotate to correct orientation
                    double theta = i * dspoke;
                    // merge the points and faces from the object into the wheel
                }
            }
        }
    }
}