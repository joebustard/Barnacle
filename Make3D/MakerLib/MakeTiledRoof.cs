using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class TiledRoofMaker : MakerBase
    {

        private double wallLength;
        private double wallHeight;
        private double wallWidth;
        private double tileLength;
        private double tileHeight;
        private double tileWidth;
        private double gapBetweenTiles;
        private double overlap = 0.5;
        public TiledRoofMaker(double length, double height, double width, double tileLength, double tileHeight, double tileWidth, double gapBetweenTiles)
        {
            this.wallLength = length;
            this.wallHeight = height;
            this.wallWidth = width;
            this.tileLength = tileLength;
            this.tileHeight = tileHeight;
            this.tileWidth = tileWidth;
            this.gapBetweenTiles = gapBetweenTiles;

        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;

            // top row of tiles are always full length and always start with a full tile.
            double tlg = tileLength + gapBetweenTiles;
            double hwallLen = wallLength / 2.0;
            int numFullTilesInLength = (int)(wallLength / tlg);
            double remainingTile = (wallLength - (tlg * numFullTilesInLength));
            double y = wallHeight;
            double x;
            for ( int i = 0; i < numFullTilesInLength; i ++)
            {
                x = -hwallLen + (i * tlg);
                MakeTileWithGap(x, y, 0.0,tileLength,tileHeight,tileWidth);
            }
            x = -hwallLen + (numFullTilesInLength * tlg);
            MakeTile(x, y, 0.0, remainingTile, tileHeight, tileWidth);

            double overlappedTileHeight = tileHeight * overlap;
            double whBeneathTopRow = wallHeight - tileHeight;
            y = y - tileHeight;
            int wholerows = (int)(whBeneathTopRow / overlappedTileHeight);
            for ( int j = 0; j < wholerows; j ++)
            {
                x = -hwallLen;
                bool offsetStart = (j % 2) == 0;
                for (int i = 0; i < numFullTilesInLength-1; i++)
                {
                    
                    if (offsetStart)
                    {
                        MakeTileWithGap(x, y, 0.0, tileLength/2, overlappedTileHeight, tileWidth);
                        x += tileLength / 2.0;
                        x += gapBetweenTiles;
                        offsetStart = false;
                    }

                        MakeTileWithGap(x, y, 0.0, tileLength, overlappedTileHeight, tileWidth);
                        x += tileLength + gapBetweenTiles;
                    
                   
                }
                
                MakeTile(x, y, 0.0, (wallLength/2) - x, overlappedTileHeight, tileWidth);
               
                y = y - overlappedTileHeight;
            }
            double lastHeight = y;
            if (lastHeight > 0)
            {
                for (int i = 0; i < numFullTilesInLength; i++)
                {
                    x = -hwallLen + (i * tlg);
                    MakeTileWithGap(x, y, 0.0, tileLength, lastHeight, tileWidth);
                }
                x = -hwallLen + (numFullTilesInLength * tlg);
                MakeTile(x, y, 0.0, remainingTile, lastHeight, tileWidth);
            }

            CloseBack();
        }

        private void CloseBack()
        {
            double z = tileWidth - wallWidth;
            double hl = wallLength / 2.0;
            // back
            int p0 = AddVertice(new Point3D(-hl, wallHeight, z));
            int p1 = AddVertice(new Point3D(hl, wallHeight, z));
            int p2 = AddVertice(new Point3D(hl, 0, z));
            int p3 = AddVertice(new Point3D(-hl,0, z));

            Faces.Add(p0);
            Faces.Add(p1);
            Faces.Add(p2);

            Faces.Add(p3);
            Faces.Add(p0);
            Faces.Add(p2);

            //left
            p0 = AddVertice(new Point3D(-hl, wallHeight, z));
            p1 = AddVertice(new Point3D(-hl, wallHeight, 0));
            p2 = AddVertice(new Point3D(-hl, 0, 0));
            p3 = AddVertice(new Point3D(-hl, 0, z));

            Faces.Add(p0);
            Faces.Add(p2);
            Faces.Add(p1);

            Faces.Add(p3);
            Faces.Add(p2);
            Faces.Add(p0);

            // right
            p0 = AddVertice(new Point3D(hl, wallHeight, z));
            p1 = AddVertice(new Point3D(hl, wallHeight, 0));
            p2 = AddVertice(new Point3D(hl, 0, 0));
            p3 = AddVertice(new Point3D(hl, 0, z));

            Faces.Add(p0);
            Faces.Add(p1);
            Faces.Add(p2);

            Faces.Add(p3);
            Faces.Add(p0);
            Faces.Add(p2);

        }

        private void MakeTileWithGap(double x, double y, double z, double tl, double th, double tw)
        {
            MakeTile(x, y, z,tl,th,tw);
            MakeGap(x+tl, y, z,th);
        }

        private void MakeGap(double x, double y, double z, double th)
        {
            double g = gapBetweenTiles;
            int p0 = AddVertice(new Point3D(x, y, z));
            int p1 = AddVertice(new Point3D(x + g, y, z));
            int p2 = AddVertice(new Point3D(x + g, y - th, z));
            int p3 = AddVertice(new Point3D(x, y - th, z));

            Faces.Add(p0);
            Faces.Add(p2);
            Faces.Add(p1);
            
            Faces.Add(p3);
            Faces.Add(p2);
            Faces.Add(p0);
        }

        private void MakeTile(double x, double y, double z, double tl, double th, double tw)
        {
            int p0 = AddVertice(new Point3D(x, y, z));
            int p1 = AddVertice(new Point3D(x+tl, y, z));
            int p2 = AddVertice(new Point3D(x+tl, y-th, z));
            int p3 = AddVertice(new Point3D(x, y-th, z));
            int p6 = AddVertice(new Point3D(x, y-th, z+tw));
            int p7 = AddVertice(new Point3D(x+tl, y-th, z+tw));

            Faces.Add(p0);
            Faces.Add(p3);
            Faces.Add(p6);

            Faces.Add(p1);
            Faces.Add(p7);
            Faces.Add(p2);

            Faces.Add(p0);
            Faces.Add(p6);
            Faces.Add(p1);

            Faces.Add(p1);
            Faces.Add(p6);
            Faces.Add(p7);

            Faces.Add(p3);
            Faces.Add(p7);
            Faces.Add(p6);

            Faces.Add(p3);
            Faces.Add(p2);
            Faces.Add(p7);


        }
    }
}
