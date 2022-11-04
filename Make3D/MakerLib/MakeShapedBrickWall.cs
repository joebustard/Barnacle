using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class ShapedBrickWallMaker : MakerBase
    {
        private String path;
        private double brickLength;
        private double brickHeight;
        private double mortarGap;

        public ShapedBrickWallMaker(string path, double brickLength, double brickHeight, double mortarGap)
        {
            this.path = path;
            this.brickLength = brickLength;
            this.brickHeight = brickHeight;
            this.mortarGap = mortarGap;
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