using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Make3D.Object3DLib;
using Object3DLib;

namespace MakerLib
{
    public class TubeMaker : MakerBase
    {
        private double innerRadius;
        private double lowerBevel;
        private double sweepDegrees;
        private double thickness;
        private double tubeHeight;
        private double upperBevel;

        public TubeMaker(double r1, double th, double lb, double ub, double h, double swp)
        {
            innerRadius = r1;
            thickness = th;
            tubeHeight = h;
            lowerBevel = lb;
            upperBevel = ub;
            sweepDegrees = swp;
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            List<PolarCoordinate> polarProfile = new List<PolarCoordinate>();

            double cx = innerRadius;

            int rotDivisions = 36;
            double halfheight = tubeHeight / 2;
            Point3D p3d = new Point3D(cx, 0, -halfheight);
            PolarCoordinate pcol = new PolarCoordinate(0, 0, 0);
            pcol.SetPoint3D(p3d);
            polarProfile.Add(pcol);

            p3d = new Point3D(cx, 0, halfheight);
            pcol = new PolarCoordinate(0, 0, 0);
            pcol.SetPoint3D(p3d);
            polarProfile.Add(pcol);

            p3d = new Point3D(cx + thickness, 0, halfheight - upperBevel);
            pcol = new PolarCoordinate(0, 0, 0);
            pcol.SetPoint3D(p3d);
            polarProfile.Add(pcol);

            p3d = new Point3D(cx + thickness, 0, -halfheight + lowerBevel);
            pcol = new PolarCoordinate(0, 0, 0);
            pcol.SetPoint3D(p3d);
            polarProfile.Add(pcol);

            SweepPolarProfileTheta(polarProfile, cx, 0, sweepDegrees, rotDivisions);
        }
    }
}