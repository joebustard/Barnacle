using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class BrickWallMaker : MakerBase
    {
        private double wallLength;
        private double wallHeight;
        private double wallWidth;
        private double largeBrickLength;
        private double smallBrickLength;
        private double brickHeight;
        private double mortarGap;

        public BrickWallMaker(double wallLength, double wallHeight, double wallWidth, double largeBrickLength, double smallBrickLength, double brickHeight, double mortarGap)
        {
            this.wallLength = wallLength;
            this.wallHeight = wallHeight;
            this.wallWidth = wallWidth;
            this.largeBrickLength = largeBrickLength;
            this.smallBrickLength = smallBrickLength;
            this.brickHeight = brickHeight;
            this.mortarGap = mortarGap;
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            int row = 0;
            double bAndg = (largeBrickLength + mortarGap);
            double lengthWithoutLastBrick = wallLength - largeBrickLength;
            double columns = lengthWithoutLastBrick / bAndg;
            int bricksinRow = (int)columns;
            double lengthOfAllBricks = bricksinRow * bAndg;
            Double leftOver = lengthWithoutLastBrick - lengthOfAllBricks;
            Double gapHAdjustment = leftOver / bricksinRow;
            double gl = mortarGap + gapHAdjustment;
            double y = wallHeight;
            double x = 0;
            while (y > 0)
            {
                x = 0;
                for (int i = 0; i <= bricksinRow; i++)
                {
                    if (row % 2 == 0)
                    {
                        if (i == 0)
                        {
                            Point3D brickOrigin = new Point3D(x, y, 0);
                            OneBrickPoints(brickOrigin, smallBrickLength, brickHeight, mortarGap, gl, mortarGap);
                            x += smallBrickLength + gl;
                        }
                        /*      else
                              if (i == bricksinRow - 1)
                              {
                                  Point3D brickOrigin = new Point3D(x, y, 0);
                                  OneBrickPoints(brickOrigin, smallBrickLength, brickHeight, mortarGap, gl, mortarGap);
                                  x += smallBrickLength + gl;
                              } */
                        else
                        {
                            Point3D brickOrigin = new Point3D(x, y, 0);
                            OneBrickPoints(brickOrigin, largeBrickLength, brickHeight, mortarGap, gl, mortarGap);
                            x += largeBrickLength + gl;
                        }
                    }
                    else
                    {
                        if  (i == bricksinRow - 1)
                        {
                            Point3D brickOrigin = new Point3D(x, y, 0);
                            OneBrickPoints(brickOrigin, smallBrickLength, brickHeight, mortarGap, gl, mortarGap);
                            x += smallBrickLength + gl;
                        }
                        else
                        {
                            Point3D brickOrigin = new Point3D(x, y, 0);
                            OneBrickPoints(brickOrigin, largeBrickLength, brickHeight, mortarGap, gl, mortarGap);
                            x += largeBrickLength + gl;
                        }
                    }
                }
                y = y - brickHeight - mortarGap;
                row++;
            }

            // assume top left point is 0,wallheight,0
            // close the "box"
            Point3D v0 = new Point3D(0, wallHeight, mortarGap - wallWidth);
            Point3D v1 = new Point3D(x, wallHeight, mortarGap - wallWidth);
            Point3D v2 = new Point3D(0, y, mortarGap - wallWidth);
            Point3D v3 = new Point3D(x, y, mortarGap - wallWidth);
            int p0 = AddVertice(v0);
            int p1 = AddVertice(v1);
            int p2 = AddVertice(v2);
            int p3 = AddVertice(v3);

            Point3D v4 = new Point3D(0, wallHeight, 0);
            Point3D v5 = new Point3D(x, wallHeight, 0);
            Point3D v6 = new Point3D(0, y, 0);
            Point3D v7 = new Point3D(x, y, 0);
            int p4 = AddVertice(v4);
            int p5 = AddVertice(v5);
            int p6 = AddVertice(v6);
            int p7 = AddVertice(v7);

            // back
            Faces.Add(p0);
            Faces.Add(p1);
            Faces.Add(p2);

            Faces.Add(p2);
            Faces.Add(p1);
            Faces.Add(p3);

            // top
            Faces.Add(p0);
            Faces.Add(p4);
            Faces.Add(p1);

            Faces.Add(p4);
            Faces.Add(p5);
            Faces.Add(p1);

            //  bottom
            Faces.Add(p2);
            Faces.Add(p3);
            Faces.Add(p6);

            Faces.Add(p6);
            Faces.Add(p3);
            Faces.Add(p7);

            //  left
            Faces.Add(p0);
            Faces.Add(p2);
            Faces.Add(p4);

            Faces.Add(p4);
            Faces.Add(p2);
            Faces.Add(p6);

            //  right
            Faces.Add(p7);
            Faces.Add(p3);
            Faces.Add(p5);

            Faces.Add(p3);
            Faces.Add(p1);
            Faces.Add(p5);
        }

        private void OneBrickPoints(Point3D origin, double bl, double bh, double bw, double gl, double gh)
        {
            int[] vid = new int[13];
            // gap above brick
            vid[0] = AddVertice(new Point3D(origin.X, origin.Y, origin.Z));
            vid[1] = AddVertice(new Point3D(origin.X + bl, origin.Y, origin.Z));
            vid[2] = AddVertice(new Point3D(origin.X + bl + gl, origin.Y, origin.Z));

            vid[3] = AddVertice(new Point3D(origin.X, origin.Y - gh, origin.Z));
            vid[4] = AddVertice(new Point3D(origin.X + bl, origin.Y - gh, origin.Z));
            vid[5] = AddVertice(new Point3D(origin.X + bl + gl, origin.Y - gh, origin.Z));

            vid[6] = AddVertice(new Point3D(origin.X, origin.Y - bh - gh, origin.Z));
            vid[7] = AddVertice(new Point3D(origin.X + bl, origin.Y - bh - gh, origin.Z));
            vid[8] = AddVertice(new Point3D(origin.X + bl + gl, origin.Y - bh - gh, origin.Z));

            // front of brick
            vid[9] = AddVertice(new Point3D(origin.X, origin.Y - gh, origin.Z + bw));
            vid[10] = AddVertice(new Point3D(origin.X + bl, origin.Y - gh, origin.Z + bw));
            vid[11] = AddVertice(new Point3D(origin.X, origin.Y - gh - bh, origin.Z + bw));
            vid[12] = AddVertice(new Point3D(origin.X + bl, origin.Y - gh - bh, origin.Z + bw));

            Faces.Add(vid[0]);
            Faces.Add(vid[3]);
            Faces.Add(vid[1]);

            Faces.Add(vid[3]);
            Faces.Add(vid[4]);
            Faces.Add(vid[1]);

            Faces.Add(vid[1]);
            Faces.Add(vid[4]);
            Faces.Add(vid[2]);

            Faces.Add(vid[4]);
            Faces.Add(vid[5]);
            Faces.Add(vid[2]);

            Faces.Add(vid[4]);
            Faces.Add(vid[7]);
            Faces.Add(vid[5]);

            Faces.Add(vid[7]);
            Faces.Add(vid[8]);
            Faces.Add(vid[5]);

            // top of brick
            Faces.Add(vid[3]);
            Faces.Add(vid[9]);
            Faces.Add(vid[4]);

            Faces.Add(vid[9]);
            Faces.Add(vid[10]);
            Faces.Add(vid[4]);

            // front of brick
            Faces.Add(vid[9]);
            Faces.Add(vid[11]);
            Faces.Add(vid[10]);

            Faces.Add(vid[11]);
            Faces.Add(vid[12]);
            Faces.Add(vid[10]);

            // brick left
            Faces.Add(vid[3]);
            Faces.Add(vid[11]);
            Faces.Add(vid[9]);

            Faces.Add(vid[3]);
            Faces.Add(vid[6]);
            Faces.Add(vid[11]);

            // brick right
            Faces.Add(vid[12]);
            Faces.Add(vid[7]);
            Faces.Add(vid[4]);

            Faces.Add(vid[4]);
            Faces.Add(vid[10]);
            Faces.Add(vid[12]);

            // brick bottom
            Faces.Add(vid[6]);
            Faces.Add(vid[7]);
            Faces.Add(vid[12]);

            Faces.Add(vid[12]);
            Faces.Add(vid[11]);
            Faces.Add(vid[6]);
        }
    }
}