using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class ImagePlaqueMaker : MakerBase
    {
        private double plagueThickness;
        private string plaqueImagePath;

        public ImagePlaqueMaker(double plagueThickness, string plaqueImagePath)
        {
            this.plagueThickness = plagueThickness;
            this.plaqueImagePath = plaqueImagePath;
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            MakeSymbolUtils msu = new MakeSymbolUtils();
            msu.Faces = Faces;
            msu.Vertices = Vertices;
            if (!String.IsNullOrEmpty(plaqueImagePath))
            {
                msu.GenerateImage(plaqueImagePath);
            }
        }
    }
}