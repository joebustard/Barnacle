using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class RectGrilleMaker : MakerBase
    {
        private double grillLength;
        private double grillHeight;
        private bool makeEdge;
        private double edgeThickness;
        private double verticalBars;
        private double verticalBarThickness;
        private double horizontalBars;
        private double horizontalBarThickness;

        public RectGrilleMaker(double grillLength, double grillHeight, bool makeEdge, double edgeThickness, double verticalBars, double verticalBarThickness, double horizontalBars, double horizontalBarThickness)
        {
            this.grillLength = grillLength;
            this.grillHeight = grillHeight;
            this.makeEdge = makeEdge;
            this.edgeThickness = edgeThickness;
            this.verticalBars = verticalBars;
            this.verticalBarThickness = verticalBarThickness;
            this.horizontalBars = horizontalBars;
            this.horizontalBarThickness = horizontalBarThickness;
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