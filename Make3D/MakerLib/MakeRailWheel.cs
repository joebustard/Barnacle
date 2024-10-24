using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class RailWheelMaker : MakerBase
    {
        private double flangeDiameter;
        private double flangeHeight;
        private double hubDiameter;
        private double hubHeight;
        private double upperRimDiameter;
        private double lowerRimDiameter;
        private double rimThickness;
        private double rimHeight;
        private double axleBoreDiameter;

        public RailWheelMaker()
        {
            paramLimits = new ParamLimits();
            SetLimits();
        }

        private void SetLimits()
        {
            paramLimits.AddLimit("flangeDiameter", 0.1, 100.0);
            paramLimits.AddLimit("flangeHeight", 0.1, 100.0);
            paramLimits.AddLimit("hubDiameter", 0.1, 100.0);
            paramLimits.AddLimit("hubHeight", 0.1, 100.0);
            paramLimits.AddLimit("upperRimDiameter", 0.1, 100.0);
            paramLimits.AddLimit("lowerRimDiameter", 0.1, 100.0);
            paramLimits.AddLimit("rimThickness", 0.1, 100.0);
            paramLimits.AddLimit("rimHeight", 0.1, 100.0);
            paramLimits.AddLimit("axleBoreDiameter", 0.1, 100.0);
        }

        public void SetValues(double flangeDiameter, double flangeHeight, double hubDiameter, double hubHeight, double upperRimDiameter, double lowerRimDiameter, double rimThickness, double rimHeight, double axleBoreDiameter)
        {
            this.flangeDiameter = flangeDiameter;
            this.flangeHeight = flangeHeight;
            this.hubDiameter = hubDiameter;
            this.hubHeight = hubHeight;
            this.upperRimDiameter = upperRimDiameter;
            this.lowerRimDiameter = lowerRimDiameter;
            this.rimThickness = rimThickness;
            this.rimHeight = rimHeight;
            this.axleBoreDiameter = axleBoreDiameter;
        }

        public RailWheelMaker(double flangeDiameter, double flangeHeight, double hubDiameter, double hubHeight, double upperRimDiameter, double lowerRimDiameter, double rimThickness, double rimHeight, double axleBoreDiameter)
        {
            paramLimits = new ParamLimits();
            this.flangeDiameter = flangeDiameter;
            this.flangeHeight = flangeHeight;
            this.hubDiameter = hubDiameter;
            this.hubHeight = hubHeight;
            this.upperRimDiameter = upperRimDiameter;
            this.lowerRimDiameter = lowerRimDiameter;
            this.rimThickness = rimThickness;
            this.rimHeight = rimHeight;
            this.axleBoreDiameter = axleBoreDiameter;
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
            double axleBoreRadius = axleBoreDiameter / 2.0;
            double px = axleBoreRadius;
            double flangeRadius = (flangeDiameter / 2.0) - axleBoreRadius;
            double upperRimOuterRadius = (upperRimDiameter / 2.0) - axleBoreRadius;
            double upperRimInnerRadius = (upperRimOuterRadius - rimThickness);
            double lowerRimRadius = (lowerRimDiameter / 2.0) - axleBoreRadius;
            double hubRadius = (hubDiameter / 2.0) - axleBoreRadius;
            double capRadius = flangeHeight / 2.0;
            double cx = flangeRadius - capRadius + px;
            double cy = capRadius;

            List<PolarCoordinate> polarProfile = new List<PolarCoordinate>();

            int rotDivisions = 120;

            AddProfilePnt(polarProfile, px, -capRadius);
            AddProfilePnt(polarProfile, px, capRadius);
            AddProfilePnt(polarProfile, px, capRadius + hubHeight);
            AddProfilePnt(polarProfile, px + hubRadius, capRadius + hubHeight);
            AddProfilePnt(polarProfile, px + hubRadius, capRadius);
            AddProfilePnt(polarProfile, px + upperRimInnerRadius, capRadius);
            AddProfilePnt(polarProfile, px + upperRimInnerRadius, capRadius + rimHeight);
            AddProfilePnt(polarProfile, px + upperRimOuterRadius, capRadius + rimHeight);
            AddProfilePnt(polarProfile, px + lowerRimRadius, capRadius);

            AddProfilePnt(polarProfile, cx, capRadius);
            double theta = 0;
            double dt = Math.PI / 10;
            for (theta = 0; theta <= Math.PI; theta += dt)
            {
                double x = cx + (capRadius * Math.Sin(theta));

                double y = (capRadius * Math.Cos(theta));

                AddProfilePnt(polarProfile, x, y);
            }

            // AddProfilePnt(polarProfile, cx, -capRadius);
            AddProfilePnt(polarProfile, px, -capRadius);
            SweepPolarProfileTheta(polarProfile, px, 0, 360, rotDivisions);
        }
    }
}