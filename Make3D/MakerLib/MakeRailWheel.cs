using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class RailWheelMaker : MakerBase
    {
        private double axleBoreRadius;
        private double flangeRadius;
        private double flangeThickness;
        private double hubRadius;
        private double hubThickness;
        private double mainRadius;
        private double mainThickness;

        public RailWheelMaker(double mainRadius, double mainThickness, double flangeRadius, double flangeThickness, double hubRadius, double hubThickness, double axleBore)
        {
            this.mainRadius = mainRadius;
            this.mainThickness = mainThickness;
            this.flangeRadius = flangeRadius;
            this.flangeThickness = flangeThickness;
            this.hubRadius = hubRadius;
            this.hubThickness = hubThickness;
            this.axleBoreRadius = axleBore;
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            GenerateWithHub();
        }

        private void AddProfilePnt(List<PolarCoordinate> polarProfile, double x, double z)
        {
            Point3D p3d = new Point3D(x, 0, z);
            PolarCoordinate pcol = new PolarCoordinate(0, 0, 0);
            pcol.SetPoint3D(p3d);
            polarProfile.Add(pcol);
        }

        private void GenerateWithHub()
        {
            if (axleBoreRadius > 0)
            {
                List<PolarCoordinate> polarProfile = new List<PolarCoordinate>();
                if (flangeThickness > mainThickness)
                {
                    flangeThickness = mainThickness;
                }

                double px = axleBoreRadius;

                int rotDivisions = 120;
                double halfheight = mainThickness / 2;
                AddProfilePnt(polarProfile, px, -halfheight);
                AddProfilePnt(polarProfile, px, halfheight);
                AddProfilePnt(polarProfile, px, halfheight + hubThickness);
                AddProfilePnt(polarProfile, px + hubRadius, halfheight + hubThickness);
                AddProfilePnt(polarProfile, px + hubRadius, halfheight);
                AddProfilePnt(polarProfile, px + mainRadius, halfheight);
                AddProfilePnt(polarProfile, px + mainRadius, halfheight + hubThickness);
                AddProfilePnt(polarProfile, px + mainRadius + flangeThickness, halfheight + hubThickness);

                AddProfilePnt(polarProfile, px + mainRadius + (1.5 * flangeThickness), -halfheight + flangeThickness);
                double cx = px + mainRadius + (1.5 * flangeThickness);
                double cy = -halfheight + flangeRadius;
                double theta = 0;
                double dt = Math.PI / 10;
                for (theta = 0; theta <= Math.PI; theta += dt)
                {
                    double x = cx + (flangeRadius * Math.Sin(theta));
                    double y = cy + (flangeThickness * Math.Cos(theta));

                    AddProfilePnt(polarProfile, x, y);
                }

                SweepPolarProfileTheta(polarProfile, px, 0, 360, rotDivisions);
            }
        }
    }
}