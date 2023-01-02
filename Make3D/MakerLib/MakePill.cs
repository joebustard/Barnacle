using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class PillMaker : MakerBase
    {

        private double flatLength;
        private double flatHeight;
        private double edge;
        private double pillWidth;
        private double frontX;
        private double frontY;
        private double frontZ;
        private double halfW;
        private int edgeDivs;
        private const double halfPi = Math.PI / 2.0;
        private Point[] trigPoints; 
        public PillMaker(double flatLength, double flatHeight, double edge, double pillWidth)
        {
            this.flatLength = flatLength;
            this.flatHeight = flatHeight;
            this.edge = edge;
            this.pillWidth = pillWidth;
            halfW = pillWidth / 2.0;
            frontX = edge;
            frontY = edge;
            frontZ = halfW; 
            edgeDivs = 10;
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            trigPoints = new Point[edgeDivs+1];
            double theta = 0;
            double dt = halfPi / (edgeDivs-1);
            double px;
            double py;
            for (int i = 0; i < edgeDivs; i++)
            {
                theta = (dt * i);
                trigPoints[i] = new Point(Math.Sin(theta), Math.Cos(theta));
            }
          
            MakeFrontFlat();
            MakeBackFlat();

            MakeFrontRight();
            MakeBackRight();

            MakeFrontLeft();
            MakeBackLeft();

            MakeFrontTop();
            MakeBackTop();

            MakeFrontBottom();
            MakeBackBottom();
            
            MakeFrontTopRightCorner();            
            MakeBackTopRightCorner();
            
            MakeFrontTopLeftCorner();            
            MakeBackTopLeftCorner();
            
            MakeFrontBottomRightCorner();
            MakeBackBottomRightCorner();


            MakeFrontBottomLeftCorner();
            MakeBackBottomLeftCorner();
        }
        private void MakeBackBottomLeftCorner()
        {
            double cx = frontX;
            double cy = frontY;
            double cz = 0;

            MakeCorner(Math.PI, 0, cx, cy, cz,true);
        }
        private void MakeFrontBottomLeftCorner()
        {
            double cx = frontX ;
            double cy = frontY;
            double cz = 0;

            MakeCorner(-halfPi, 0, cx, cy, cz,true);
        }
        private void MakeFrontBottomRightCorner()
        {
            double cx = frontX + flatLength;
            double cy = frontY ;
            double cz = 0;

            MakeCorner(0, -halfPi, cx, cy, cz);
        }

        private void MakeBackBottomRightCorner()
        {
            double cx = frontX + flatLength;
            double cy = frontY;
            double cz = 0;

            MakeCorner(halfPi, -halfPi, cx, cy, cz);
        }
        private Point3D SpherePoint(double cx,double cy, double cz, double theta, double phi, double r1, double r2)
        {

            double px = cx + r1 * Math.Sin(theta) * Math.Cos(phi);
            double py = cy + r1 * Math.Sin(theta) * Math.Sin(phi);
            double pz = cz + r2 * Math.Cos(theta);
            return new Point3D(px, py, pz);
        }

        private void MakeFrontTopLeftCorner()
        {
            double cx = frontX;
            double cy = frontY+flatHeight;
            double cz = 0;

            MakeCorner(0, halfPi, cx, cy, cz);
        }

        private void MakeBackTopLeftCorner()
        {
            double cx = frontX;
            double cy = frontY + flatHeight;
            double cz = 0;

            MakeCorner(halfPi, halfPi, cx, cy, cz);
        }
        private void MakeFrontTopRightCorner()
        {
            double cx = frontX + flatLength;
            double cy = frontY + flatHeight;
            double cz = 0;

            MakeCorner(0,0,cx,cy,cz);  
        }

        private void MakeBackTopRightCorner()
        {
            double cx = frontX + flatLength;
            double cy = frontY + flatHeight;
            double cz = 0;

            MakeCorner(halfPi, 0, cx, cy, cz);
        }

        private void MakeCorner(double toff, double poff, double cx, double cy, double cz, bool invert= false)
        {
            double d = halfPi / (edgeDivs - 1);
            double theta;
            double phi;
            Point3D p0;
            Point3D p1;
            Point3D p2;
            Point3D p3;
            for (int i = 0; i < edgeDivs - 1; i++)
            {
                theta = toff+(d * i);
                for (int j = 0; j < edgeDivs - 1; j++)
                {
                    phi = poff+(d * j);
                    p0 = SpherePoint(cx, cy, cz, theta, phi, edge, halfW);
                    p1 = SpherePoint(cx, cy, cz, theta + d, phi, edge, halfW);
                    p2 = SpherePoint(cx, cy, cz, theta + d, phi + d, edge, halfW);
                    p3 = SpherePoint(cx, cy, cz, theta, phi + d, edge, halfW);

                    int v0 = AddVertice(p0);
                    int v1 = AddVertice(p1);
                    int v2 = AddVertice(p2);
                    int v3 = AddVertice(p3);
                    if (invert)
                    {
                        AddFace(v0, v2, v1);
                        AddFace(v0, v3, v2);
                    }
                    else
                    {
                        AddFace(v0, v1, v2);
                        AddFace(v0, v2, v3);
                    }
                }

            }
        }

        private void MakeFrontBottom()
        {
            double lx = frontX;
            double rx = frontX + flatLength;
            double ly = frontY ;
            double tz = frontZ;
            for (int i = 0; i < edgeDivs - 1; i++)
            {
                double py1 = ly - trigPoints[i].X * edge;
                double pz1 = trigPoints[i].Y * halfW;

                double py2 = ly - trigPoints[i + 1].X * edge;
                double pz2 = trigPoints[i + 1].Y * halfW;

                int p0 = AddVertice(lx, py1, pz1);
                int p1 = AddVertice(rx, py1, pz1);
                int p2 = AddVertice(rx, py2, pz2);
                int p3 = AddVertice(lx, py2, pz2);
                AddFace(p0, p2, p1);
                AddFace(p0, p3  , p2);
            }
        }

        private void MakeBackBottom()
        {
            double lx = frontX;
            double rx = frontX + flatLength;
            double ly = frontY;
            double tz = frontZ;
            for (int i = 0; i < edgeDivs - 1; i++)
            {
                double py1 = ly - trigPoints[i].X * edge;
                double pz1 = -trigPoints[i].Y * halfW;

                double py2 = ly - trigPoints[i + 1].X * edge;
                double pz2 = -trigPoints[i + 1].Y * halfW;

                int p0 = AddVertice(lx, py1, pz1);
                int p1 = AddVertice(rx, py1, pz1);
                int p2 = AddVertice(rx, py2, pz2);
                int p3 = AddVertice(lx, py2, pz2);
                AddFace(p0, p1, p2);
                AddFace(p0, p2, p3);
            }
        }

        private void MakeFrontTop()
        {
            double lx = frontX;
            double rx = frontX + flatLength;
            double ly = frontY + flatHeight;
            double tz = frontZ;
            for (int i = 0; i < edgeDivs - 1; i++)
            {
                double py1 = ly + trigPoints[i].X * edge;
                double pz1 = trigPoints[i].Y * halfW;

                double py2 = ly + trigPoints[i + 1].X * edge;
                double pz2 = trigPoints[i + 1].Y * halfW;

                int p0 = AddVertice(lx, py1, pz1);
                int p1 = AddVertice(rx, py1, pz1);
                int p2 = AddVertice(rx, py2, pz2);
                int p3 = AddVertice(lx, py2, pz2);
                AddFace(p0, p1, p2);
                AddFace(p0, p2, p3);
            }
        }
        private void MakeBackTop()
        {
            double lx = frontX;
            double rx = frontX + flatLength;
            double ly = frontY + flatHeight;
            double tz = frontZ;
            for (int i = 0; i < edgeDivs - 1; i++)
            {
                double py1 = ly + trigPoints[i].X * edge;
                double pz1 = -trigPoints[i].Y * halfW;

                double py2 = ly + trigPoints[i + 1].X * edge;
                double pz2 = -trigPoints[i + 1].Y * halfW;

                int p0 = AddVertice(lx, py1, pz1);
                int p1 = AddVertice(rx, py1, pz1);
                int p2 = AddVertice(rx, py2, pz2);
                int p3 = AddVertice(lx, py2, pz2);
                AddFace(p0, p2, p1);
                AddFace(p0, p3, p2);
            }
        }
        private void MakeFrontLeft()
        {
           
            double ty = frontY + flatHeight;
            double tz = frontZ;
            for (int i = 0; i < edgeDivs - 1; i++)
            {
                double px1 = frontX - trigPoints[i].X * edge;
                double pz1 = trigPoints[i].Y * halfW;

                double px2 = frontX -  trigPoints[i + 1].X * edge;
                double pz2 = trigPoints[i + 1].Y * halfW;

                int p0 = AddVertice(px1, frontY, pz1);
                int p1 = AddVertice(px2, frontY, pz2);
                int p2 = AddVertice(px2, ty, pz2);
                int p3 = AddVertice(px1, ty, pz1);
                AddFace(p0, p2, p1);
                AddFace(p0, p3, p2);
            }
        }

        private void MakeBackLeft()
        {

            double ty = frontY + flatHeight;
            double tz = -frontZ;
            for (int i = 0; i < edgeDivs - 1; i++)
            {
                double px1 = frontX - trigPoints[i].X * edge;
                double pz1 = -trigPoints[i].Y * halfW;

                double px2 = frontX - trigPoints[i + 1].X * edge;
                double pz2 = -trigPoints[i + 1].Y * halfW;

                int p0 = AddVertice(px1, frontY, pz1);
                int p1 = AddVertice(px2, frontY, pz2);
                int p2 = AddVertice(px2, ty, pz2);
                int p3 = AddVertice(px1, ty, pz1);
                AddFace(p0, p1, p2);
                AddFace(p0, p2, p3);
            }
        }
        private void MakeFrontRight()
        {
            double rx = frontX + flatLength;
            double ty = frontY + flatHeight;
            double tz = frontZ;
            for ( int i = 0; i < edgeDivs-1; i ++)
           {
                double px1 = rx + trigPoints[i].X * edge;
                double pz1 = trigPoints[i].Y * halfW;

                double px2 = rx + trigPoints[i+1].X * edge;
                double pz2 =  trigPoints[i+1].Y * halfW;

                int p0 = AddVertice(px1, frontY, pz1);
                int p1 = AddVertice(px2, frontY, pz2);
                int p2 = AddVertice(px2, ty, pz2);
                int p3 = AddVertice(px1, ty, pz1);
                AddFace(p0, p1, p2);
                AddFace(p0, p2, p3);
            }
        }

        private void MakeBackRight()
        {
            double rx = frontX + flatLength;
            double ty = frontY + flatHeight;
            double tz = -frontZ;
            for (int i = 0; i < edgeDivs - 1; i++)
            {
                double px1 = rx + trigPoints[i].X * edge;
                double pz1 = -trigPoints[i].Y * halfW ;

                double px2 = rx + trigPoints[i + 1].X * edge ;
                double pz2 = -trigPoints[i + 1].Y * halfW ;

                int p0 = AddVertice(px1, frontY, pz1);
                int p1 = AddVertice(px2, frontY, pz2);
                int p2 = AddVertice(px2, ty, pz2);
                int p3 = AddVertice(px1, ty, pz1);
                AddFace(p0, p2, p1);
                AddFace(p0, p3, p2);
            }
        }
        private void MakeFrontFlat()
        {
            int p0 = AddVertice(frontX, frontY, frontZ);
            int p1 = AddVertice(frontX + flatLength, frontY, frontZ);
            int p2 = AddVertice(frontX + flatLength, frontY+flatHeight, frontZ);
            int p3 = AddVertice(frontX, frontY + flatHeight, frontZ);

            Faces.Add(p0);
            Faces.Add(p1);
            Faces.Add(p2);

            Faces.Add(p0);
            Faces.Add(p2);
            Faces.Add(p3);
        }

        private void MakeBackFlat()
        {
            double backZ = -frontZ;
            int p0 = AddVertice(frontX, frontY, backZ);
            int p1 = AddVertice(frontX + flatLength, frontY, backZ);
            int p2 = AddVertice(frontX + flatLength, frontY + flatHeight, backZ);
            int p3 = AddVertice(frontX, frontY + flatHeight, backZ);

            Faces.Add(p0);
            Faces.Add(p2);
            Faces.Add(p1);

            Faces.Add(p0);
            Faces.Add(p3);
            Faces.Add(p2);
        }
    }
}
