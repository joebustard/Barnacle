using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class TexturedDiskMaker : MakerBase
    {

        private double tubeHeight;
        private double radius;


        private double sweep;
        private string texture;
        private double textureDepth;
        private double textureResolution;

        public TexturedDiskMaker(double tubeHeight, double radius, double sweep, string texture, double textureDepth, double textureResolution)
        {
            this.tubeHeight = tubeHeight;
            this.radius = radius;


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
