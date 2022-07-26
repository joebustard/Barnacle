using Barnacle.Object3DLib;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class TiledRoofMaker : MakerBase
    {
        private bool chamfer;
        private double gapBetweenTiles;
        private double overlap;
        private double tileHeight;
        private double tileLength;
        private double tileWidth;
        private double wallHeight;
        private double wallLength;
        private double wallWidth;

        public TiledRoofMaker(double length, double height, double width, double tileLength, double tileHeight, double tileWidth, double overlap, double gapBetweenTiles, bool chamfer = false)
        {
            this.wallLength = length;
            this.wallHeight = height;
            this.wallWidth = width;
            this.tileLength = tileLength;
            this.tileHeight = tileHeight;
            this.tileWidth = tileWidth;
            this.gapBetweenTiles = gapBetweenTiles;
            this.overlap = overlap;
            this.chamfer = chamfer;
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
            double overlappedTileHeight = tileHeight * (1.0 - overlap);
            double y = overlappedTileHeight;
            double x = -hwallLen;

            int wholerows = (int)(wallHeight / overlappedTileHeight);
            double tl;
            bool offsetStart = true;

            // start at the bottom row and work up
            for (int j = 0; j < wholerows; j++)
            {
                x = -hwallLen;
                offsetStart = !offsetStart;
                while (x + tileLength + gapBetweenTiles < hwallLen)
                {
                    if (offsetStart && x == -hwallLen)
                    {
                        MakeTileWithGap(x, y, 0.0, tileLength / 2, overlappedTileHeight, tileWidth);
                        x += tileLength / 2.0;
                        x += gapBetweenTiles;
                    }

                    MakeTileWithGap(x, y, 0.0, tileLength, overlappedTileHeight, tileWidth);
                    x += tileLength + gapBetweenTiles;
                }

                tl = hwallLen - x;

                if (tl > gapBetweenTiles)
                {
                    MakeTile(x, y, 0.0, tl, overlappedTileHeight, tileWidth);
                }
                else
                {
                    MakeGap(x, y, 0.0, overlappedTileHeight, tl);
                }

                y = y + overlappedTileHeight;
            }
            y = y - overlappedTileHeight;

            // May need a smaller row at the top to make up the correct height
            double lastHeight = wallHeight - y;

            if (lastHeight > 0)
            {
                x = -hwallLen;

                y = wallHeight;
                offsetStart = !offsetStart;
                while (x + tileLength + gapBetweenTiles < hwallLen)
                {
                    if (offsetStart && x == -hwallLen)
                    {
                        MakeTileWithGap(x, y, 0.0, tileLength / 2, lastHeight, tileWidth);
                        x += tileLength / 2.0;
                        x += gapBetweenTiles;
                    }

                    MakeTileWithGap(x, y, 0.0, tileLength, lastHeight, tileWidth);
                    x += tileLength + gapBetweenTiles;
                }

                tl = hwallLen - x;

                if (tl > gapBetweenTiles)
                {
                    MakeTile(x, y, 0.0, tl, lastHeight, tileWidth);
                }
                else
                {
                    MakeGap(x, y, 0.0, lastHeight, tl);
                }
            }

            CloseBack(chamfer);

            // make sure 0,0,0 is in the middle
            Point3D min = new Point3D(double.MaxValue, double.MaxValue, double.MaxValue);
            Point3D max = new Point3D(double.MinValue, double.MinValue, double.MinValue);
            PointUtils.MinMax(Vertices, ref min, ref max);

            double scaleX = max.X - min.X;
            double scaleY = max.Y - min.Y;
            double scaleZ = max.Z - min.Z;

            double midx = min.X + (scaleX / 2);
            double midy = min.Y + (scaleY / 2);
            double midz = min.Z + (scaleZ / 2);
            Vector3D offset = new Vector3D(-midx, -min.Y, -midz);

            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertices[i] += offset;
            }
        }

        private void CloseBack(bool chamfer)
        {
            // calculate position of back, but must be at least 1
            double z = tileWidth - wallWidth;
            if (z > -1)
            {
                z = -1;
            }

            // calculate chamfer offsets
            double hl = wallLength / 2.0;
            double ch = 0;
            if (chamfer)
            {
                ch = -z;
            }
            if (ch > hl)
            {
                ch = hl;
            }
            if (ch > wallHeight / 2)
            {
                ch = wallHeight / 2;
            }

            // back
            int p0 = AddVertice(new Point3D(-hl + ch, wallHeight - ch, z));
            int p1 = AddVertice(new Point3D(hl - ch, wallHeight - ch, z));
            int p2 = AddVertice(new Point3D(hl - ch, ch, z));
            int p3 = AddVertice(new Point3D(-hl + ch, ch, z));

            Faces.Add(p0);
            Faces.Add(p1);
            Faces.Add(p2);

            Faces.Add(p3);
            Faces.Add(p0);
            Faces.Add(p2);

            //left
            p0 = AddVertice(new Point3D(-hl + ch, wallHeight - ch, z));
            p1 = AddVertice(new Point3D(-hl, wallHeight, 0));
            p2 = AddVertice(new Point3D(-hl, 0, 0));
            p3 = AddVertice(new Point3D(-hl + ch, ch, z));

            Faces.Add(p0);
            Faces.Add(p2);
            Faces.Add(p1);

            Faces.Add(p3);
            Faces.Add(p2);
            Faces.Add(p0);

            // right
            p0 = AddVertice(new Point3D(hl - ch, wallHeight - ch, z));
            p1 = AddVertice(new Point3D(hl, wallHeight, 0));
            p2 = AddVertice(new Point3D(hl, 0, 0));
            p3 = AddVertice(new Point3D(hl - ch, ch, z));

            Faces.Add(p0);
            Faces.Add(p1);
            Faces.Add(p2);

            Faces.Add(p3);
            Faces.Add(p0);
            Faces.Add(p2);

            // Bottom
            p0 = AddVertice(new Point3D(-hl + ch, ch, z));
            p1 = AddVertice(new Point3D(-hl, 0, 0));
            p2 = AddVertice(new Point3D(hl, 0, 0));
            p3 = AddVertice(new Point3D(hl - ch, ch, z));

            Faces.Add(p0);
            Faces.Add(p2);
            Faces.Add(p1);

            Faces.Add(p3);
            Faces.Add(p2);
            Faces.Add(p0);

            // Top
            p0 = AddVertice(new Point3D(-hl + ch, wallHeight - ch, z));
            p1 = AddVertice(new Point3D(-hl, wallHeight, 0));
            p2 = AddVertice(new Point3D(hl, wallHeight, 0));
            p3 = AddVertice(new Point3D(hl - ch, wallHeight - ch, z));

            Faces.Add(p0);
            Faces.Add(p1);
            Faces.Add(p2);

            Faces.Add(p3);
            Faces.Add(p0);
            Faces.Add(p2);
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

        private void MakeGap(double x, double y, double z, double th, double g)
        {
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
            int p1 = AddVertice(new Point3D(x + tl, y, z));
            int p2 = AddVertice(new Point3D(x + tl, y - th, z));
            int p3 = AddVertice(new Point3D(x, y - th, z));
            int p6 = AddVertice(new Point3D(x, y - th, z + tw));
            int p7 = AddVertice(new Point3D(x + tl, y - th, z + tw));

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

        private void MakeTileWithGap(double x, double y, double z, double tl, double th, double tw)
        {
            MakeTile(x, y, z, tl, th, tw);
            MakeGap(x + tl, y, z, th);
        }
    }
}