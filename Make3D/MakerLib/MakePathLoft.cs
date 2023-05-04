using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class PathLoftMaker : MakerBase
    {
        private double loftHeight;
        private double loftThickness;

        public PathLoftMaker(double loftHeight, double loftThickness)
        {
            this.loftHeight = loftHeight;
            this.loftThickness = loftThickness;
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
        }
    }
}