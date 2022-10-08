using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class PulleyMaker : MakerBase
    {
        private double axleBoreRadius;
        private double extraRimRadius;
        private double extraRimThickness;
        private double grooveDepth;
        private double mainRadius;
        private double mainThickness;

        public PulleyMaker(double mainRadius, double mainThickness, double extraRimRadius, double extraRimThickness, double grooveDepth, double axleBoreRadius)
        {
            this.mainRadius = mainRadius;
            this.mainThickness = mainThickness;
            this.extraRimRadius = extraRimRadius;
            this.extraRimThickness = extraRimThickness;
            this.grooveDepth = grooveDepth;
            this.axleBoreRadius = axleBoreRadius;
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            if (extraRimThickness > 0 && extraRimRadius > 0)
            {
                GenerateWithRim();
            }
            else
            {
                GenerateWithoutRim();
            }
        }

        private void GenerateWithoutRim()
        {
            if (axleBoreRadius > 0)
            {
                List<PolarCoordinate> polarProfile = new List<PolarCoordinate>();
                if (grooveDepth > mainThickness)
                {
                    grooveDepth = mainThickness;
                }

                double cx = axleBoreRadius;

                int rotDivisions = 120;
                double halfheight = mainThickness / 2;
                Point3D p3d = new Point3D(cx, 0, -halfheight);
                PolarCoordinate pcol = new PolarCoordinate(0, 0, 0);
                pcol.SetPoint3D(p3d);
                polarProfile.Add(pcol);

                p3d = new Point3D(cx, 0, halfheight);
                pcol = new PolarCoordinate(0, 0, 0);
                pcol.SetPoint3D(p3d);
                polarProfile.Add(pcol);

                p3d = new Point3D(cx + mainRadius, 0, halfheight);
                pcol = new PolarCoordinate(0, 0, 0);
                pcol.SetPoint3D(p3d);
                polarProfile.Add(pcol);

                double theta = 0;
                double dt = Math.PI / 10;
                for (theta = Math.PI; theta <= 2 * Math.PI; theta += dt)
                {
                    double x = cx + mainRadius + (grooveDepth * Math.Sin(theta));
                    double y = grooveDepth * Math.Cos(theta);

                    p3d = new Point3D(x, 0, -y);
                    pcol = new PolarCoordinate(0, 0, 0);
                    pcol.SetPoint3D(p3d);
                    polarProfile.Add(pcol);
                }
                p3d = new Point3D(cx + mainRadius, 0, -halfheight);
                pcol = new PolarCoordinate(0, 0, 0);
                pcol.SetPoint3D(p3d);
                polarProfile.Add(pcol);

                SweepPolarProfileTheta(polarProfile, cx, 0, 360, rotDivisions);
            }
        }

        private void GenerateWithRim()
        {
            if (axleBoreRadius > 0)
            {
                List<PolarCoordinate> polarProfile = new List<PolarCoordinate>();
                if (grooveDepth > mainThickness)
                {
                    grooveDepth = mainThickness;
                }

                double cx = axleBoreRadius;

                int rotDivisions = 120;
                double halfheight = mainThickness / 2;
                Point3D p3d = new Point3D(cx, 0, -halfheight);
                PolarCoordinate pcol = new PolarCoordinate(0, 0, 0);
                pcol.SetPoint3D(p3d);
                polarProfile.Add(pcol);

                p3d = new Point3D(cx, 0, halfheight);
                pcol = new PolarCoordinate(0, 0, 0);
                pcol.SetPoint3D(p3d);
                polarProfile.Add(pcol);

                p3d = new Point3D(cx + mainRadius, 0, halfheight);
                pcol = new PolarCoordinate(0, 0, 0);
                pcol.SetPoint3D(p3d);
                polarProfile.Add(pcol);

                p3d = new Point3D(cx + mainRadius, 0, halfheight + extraRimThickness);
                pcol = new PolarCoordinate(0, 0, 0);
                pcol.SetPoint3D(p3d);
                polarProfile.Add(pcol);

                p3d = new Point3D(cx + mainRadius + extraRimRadius, 0, halfheight + extraRimThickness);
                pcol = new PolarCoordinate(0, 0, 0);
                pcol.SetPoint3D(p3d);
                polarProfile.Add(pcol);

                double theta = 0;
                double dt = Math.PI / 10;
                for (theta = Math.PI; theta <= 2 * Math.PI; theta += dt)
                {
                    double x = cx + mainRadius + extraRimRadius + (grooveDepth * Math.Sin(theta));
                    double y = grooveDepth * Math.Cos(theta);

                    p3d = new Point3D(x, 0, -y);
                    pcol = new PolarCoordinate(0, 0, 0);
                    pcol.SetPoint3D(p3d);
                    polarProfile.Add(pcol);
                }

                p3d = new Point3D(cx + mainRadius + extraRimRadius, 0, -halfheight - extraRimThickness);
                pcol = new PolarCoordinate(0, 0, 0);
                pcol.SetPoint3D(p3d);
                polarProfile.Add(pcol);

                p3d = new Point3D(cx + mainRadius, 0, -halfheight - extraRimThickness);
                pcol = new PolarCoordinate(0, 0, 0);
                pcol.SetPoint3D(p3d);
                polarProfile.Add(pcol);

                p3d = new Point3D(cx + mainRadius, 0, -halfheight);
                pcol = new PolarCoordinate(0, 0, 0);
                pcol.SetPoint3D(p3d);
                polarProfile.Add(pcol);

                SweepPolarProfileTheta(polarProfile, cx, 0, 360, rotDivisions);
            }
        }
    }
}