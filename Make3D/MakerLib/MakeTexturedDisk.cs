using Barnacle.Object3DLib;
using MakerLib.TextureUtils;
using OctTreeLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class TexturedDiskMaker : MakerBase
    {
        private double diskHeight;
        private double radius;

        private double sweep;
        private string texture;
        private double textureDepth;
        private double textureResolution;
        private TextureManager textureManager;

        public TexturedDiskMaker(double diskHeight, double radius, double sweep, string texture, double textureDepth, double textureResolution)
        {
            this.diskHeight = diskHeight;
            this.radius = radius;

            this.sweep = sweep;
            this.texture = texture;
            this.textureDepth = textureDepth;
            this.textureResolution = textureResolution;
            textureManager = TextureManager.Instance();
            textureManager.LoadTextureImage(texture);
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            double twoPI = Math.PI * 2.0;
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            double xExtent = 1.5 * (textureDepth + radius);
            double yExtent = 1.5 * diskHeight;
            OctTree octTree = CreateOctree(new Point3D(-xExtent, -yExtent, -xExtent),
                          new Point3D(+xExtent, +yExtent, +xExtent));
            Faces = faces;
            if (textureResolution <= 0)
            {
                textureResolution = 0.1;
            }
            // Whats the inner sweep angle in degrees
            double inswe = (textureResolution * 360.0) / (twoPI * radius);

            // outer sweep is going to be close but just takes into account the depth
            double outswe = (textureResolution * 360.0) / (twoPI * (radius + textureDepth));
            inswe = DegToRad(inswe);
            outswe = DegToRad(outswe);

            // so a "FacePIXEL" corresponds to a sweep of inswe
            // In practice how many X-pixels does that mean we have

            int maxFacePixel = (int)((Math.PI * 2.0) / inswe);
            double facePixelHeight = diskHeight / textureResolution;
            double x = 0;
            double y = 0;
            while (y < diskHeight)
            {
                double theta = 0;
                while (theta < twoPI)
                {
                    theta += inswe;

                    Point p = CalcPoint(theta, radius);
                    Point p2 = CalcPoint(theta + inswe, radius);

                    MakeVSquareFace(p.X, y, p.Y, p2.X, y + textureDepth, p2.Y);
                }
                y += textureResolution;
            }
        }
    }
}