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
        }
    }
}