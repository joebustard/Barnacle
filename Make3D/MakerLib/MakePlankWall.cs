using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class PlankWallMaker : MakerBase
    {

        private double wallLength;
        private double wallHeight;
        private double wallWidth;
        private double plankWidth;
        private double gap;
        private double gapDepth;

 
        public PlankWallMaker(double wallLength, double wallHeight, double wallWidth, double plankWidth, double gap, double gapDepth)
        {
            this.wallLength = wallLength;
            this.wallHeight = wallHeight;
            this.wallWidth = wallWidth;
            this.plankWidth = plankWidth;
            this.gap = gap;
            this.gapDepth = gapDepth;

        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;

            double plankAndGapLength = plankWidth + gap;
            int wholePlanksAndGaps = (int)(wallLength / plankAndGapLength);
            double x = 0;
            for ( int pl = 0; pl < wholePlanksAndGaps; pl ++)
            {

                
                
                PlankAndGap(x, plankWidth,wallHeight,gapDepth,gap);
                x += plankAndGapLength;
            }
            double leftOver = wallLength - x;
            Plank(x, leftOver, wallHeight, gapDepth);
            CloseBox();
        }

        private void CloseBox()
        {
            // assume top left point is 0,wallheight,0
            // close the "box"
            double x = wallLength;
            double backz = gapDepth - wallWidth;
            if ( backz >=0)
            {
                backz = -1.0;
            }
            Point3D v0 = new Point3D(0, wallHeight, backz);
            Point3D v1 = new Point3D(x, wallHeight, backz);
            Point3D v2 = new Point3D(0, 0, backz);
            Point3D v3 = new Point3D(x, 0, backz);
            int p0 = AddVertice(v0);
            int p1 = AddVertice(v1);
            int p2 = AddVertice(v2);
            int p3 = AddVertice(v3);

            Point3D v4 = new Point3D(0, wallHeight, 0);
            Point3D v5 = new Point3D(x, wallHeight, 0);
            Point3D v6 = new Point3D(0, 0, 0);
            Point3D v7 = new Point3D(x, 0, 0);
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
   
        private void PlankAndGap(double x, double plw, double plh, double depth, double gl)
        {
            int[] vid = new int[13];
           
            vid[0] = AddVertice(new Point3D(x, 0.0, 0.0));
            vid[1] = AddVertice(new Point3D(x + plw, 0.0, 0.0));
            vid[2] = AddVertice(new Point3D(x + plw + gl, 0.0, 0.0));

            vid[3] = AddVertice(new Point3D(x, plh, 0.0));
            vid[4] = AddVertice(new Point3D(x + plw, plh, 0.0));
            vid[5] = AddVertice(new Point3D(x + plw + gl, plh, 0.0));

            // front of Plank
            vid[6] = AddVertice(new Point3D(x, 0.0, depth));
            vid[7] = AddVertice(new Point3D(x + plw, 0,  depth));
            vid[8] = AddVertice(new Point3D(x,plh, depth));
            vid[9] = AddVertice(new Point3D(x + plw,plh,depth));

            Faces.Add(vid[0]);
            Faces.Add(vid[8]);
            Faces.Add(vid[3]);
           
            Faces.Add(vid[0]);
            Faces.Add(vid[6]);
            Faces.Add(vid[8]);
            
            Faces.Add(vid[6]);
            Faces.Add(vid[9]);
            Faces.Add(vid[8]);
            
            Faces.Add(vid[6]);
            Faces.Add(vid[7]);
            Faces.Add(vid[9]);
            
            Faces.Add(vid[7]);
            Faces.Add(vid[4]);
            Faces.Add(vid[9]);
            
            Faces.Add(vid[7]);
            Faces.Add(vid[1]);
            Faces.Add(vid[4]);
            
            // top of Plank
            Faces.Add(vid[3]);
            Faces.Add(vid[9]);
            Faces.Add(vid[4]);
            
            Faces.Add(vid[3]);
            Faces.Add(vid[8]);
            Faces.Add(vid[9]);
            
            // bottom of plank
            Faces.Add(vid[0]);
            Faces.Add(vid[7]);
            Faces.Add(vid[6]);
            
            Faces.Add(vid[0]);
            Faces.Add(vid[1]);
            Faces.Add(vid[7]);
            
            
            Faces.Add(vid[1]);
            Faces.Add(vid[5]);
            Faces.Add(vid[4]);
            
            Faces.Add(vid[1]);
            Faces.Add(vid[2]);
            Faces.Add(vid[5]);          
        }

        private void Plank(double x, double plw, double plh, double depth)
        {
            int[] vid = new int[8];

            vid[0] = AddVertice(new Point3D(x, 0.0, 0.0));
            vid[1] = AddVertice(new Point3D(x + plw, 0.0, 0.0));           

            vid[2] = AddVertice(new Point3D(x, plh, 0.0));
            vid[3] = AddVertice(new Point3D(x + plw, plh, 0.0));
           
            vid[4] = AddVertice(new Point3D(x, 0.0, depth));
            vid[5] = AddVertice(new Point3D(x + plw, 0, depth));
            vid[6] = AddVertice(new Point3D(x, plh, depth));
            vid[7] = AddVertice(new Point3D(x + plw, plh, depth));

            Faces.Add(vid[0]);
            Faces.Add(vid[6]);
            Faces.Add(vid[2]);

            Faces.Add(vid[0]);
            Faces.Add(vid[4]);
            Faces.Add(vid[6]);

            Faces.Add(vid[4]);
            Faces.Add(vid[7]);
            Faces.Add(vid[6]);

            Faces.Add(vid[4]);
            Faces.Add(vid[5]);
            Faces.Add(vid[7]);

            Faces.Add(vid[5]);
            Faces.Add(vid[3]);
            Faces.Add(vid[7]);

            Faces.Add(vid[5]);
            Faces.Add(vid[1]);
            Faces.Add(vid[3]);

            // top of Plank
            Faces.Add(vid[2]);
            Faces.Add(vid[7]);
            Faces.Add(vid[3]);

            Faces.Add(vid[2]);
            Faces.Add(vid[6]);
            Faces.Add(vid[7]);

            // bottom of plank
            Faces.Add(vid[0]);
            Faces.Add(vid[5]);
            Faces.Add(vid[4]);

            Faces.Add(vid[0]);
            Faces.Add(vid[1]);
            Faces.Add(vid[5]);




        }
    }
}
