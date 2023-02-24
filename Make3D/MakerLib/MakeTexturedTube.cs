using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class TexturedTubeMaker : MakerBase
    {

        private double tubeHeight;
        private double innerRadius;
        private double thickness;

        private double sweep;
        private string texture;
        private double textureDepth;
        private double textureResolution;

        public TexturedTubeMaker(double tubeHeight, double innerRadius, double thickness, double sweep, string texture, double textureDepth, double textureResolution)
        {
            this.tubeHeight = tubeHeight;
            this.innerRadius = innerRadius;
            this.thickness = thickness;

            this.sweep = sweep;
            this.texture = texture;
            this.textureDepth = textureDepth;
            this.textureResolution = textureResolution;

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
