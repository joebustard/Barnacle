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
        private double trayThickness;
        private int shape;
        private int edgeDivs;
        private const double halfPi = Math.PI / 2.0;
        private Point[] trigPoints;

        public PillMaker(double flatLength, double flatHeight, double edge, double pillWidth,double trayThickness,  int shape)
        {
            this.flatLength = flatLength;
            this.flatHeight = flatHeight;
            this.edge = edge;
            this.pillWidth = pillWidth;
            this.shape = shape;
            halfW = pillWidth / 2.0;
            frontX = edge;
            frontY = edge;
            frontZ = halfW;
            edgeDivs = 10;
           this.trayThickness = trayThickness;
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            trigPoints = new Point[edgeDivs + 1];
            double theta;
            double dt = halfPi / (edgeDivs - 1);

            for (int i = 0; i < edgeDivs; i++)
            {
                theta = (dt * i);
                trigPoints[i] = new Point(Math.Sin(theta), Math.Cos(theta));
            }
            switch (shape)
            {
                case 0:
                    {
                        MakeFrontFlat();
                        MakeFrontRight();
                        MakeFrontLeft();
                        MakeFrontTop();
                        MakeFrontBottom();
                        MakeFrontTopRightCorner();
                        MakeFrontTopLeftCorner();
                        MakeFrontBottomRightCorner();
                        MakeFrontBottomLeftCorner();

                        MakeBackFlat();
                        MakeBackRight();
                        MakeBackLeft();
                        MakeBackTop();
                        MakeBackBottom();
                        MakeBackTopRightCorner();
                        MakeBackTopLeftCorner();
                        MakeBackBottomRightCorner();
                        MakeBackBottomLeftCorner();
                    }
                    break;

                case 1:
                    {
                        MakeFrontFlat();
                        MakeFrontRight();
                        MakeFrontLeft();
                        MakeFrontTop();
                        MakeFrontBottom();
                        MakeFrontTopRightCorner();
                        MakeFrontTopLeftCorner();
                        MakeFrontBottomRightCorner();
                        MakeFrontBottomLeftCorner();

                        MakeBackHalf();

                        MakeHalfCorners();
                        MakeBackHalfGaps();
                    }
                    break;

                case 2:
                    {
                    // make outside shell
                    
                        MakeFrontFlat();
                        MakeFrontRight();
                        MakeFrontLeft();
                        MakeFrontTop();
                        MakeFrontBottom();
                        MakeFrontTopRightCorner();
                        MakeFrontTopLeftCorner();
                        MakeFrontBottomRightCorner();
                        MakeFrontBottomLeftCorner();
                        
                        // inside shell
                        MakeFlat(frontZ-trayThickness);
                        MakeFrontTrayRight();
                        MakeFrontTrayLeft();
                        MakeFrontTrayTop();
                        MakeFrontTrayBottom();

                        MakeFrontTopRightTrayCorner();
                        MakeFrontTopLeftTrayCorner();
                        MakeFrontBottomRightTrayCorner();
                        MakeFrontBottomLeftTrayCorner();
                        ConnectInnerTrayToOuter();
                    }
                    break;
            }
        }

        private void ConnectInnerTrayToOuter()
        {
            MakeRectFace(frontX, frontY-edge, frontX + flatLength, frontY + trayThickness-edge,0);
            MakeRectFace(frontX, frontY +flatHeight+edge-trayThickness, frontX + flatLength, frontY  +flatHeight+edge, 0);
            MakeRectFace(0, edge, trayThickness, frontY+flatHeight, 0);
            MakeRectFace(frontX+flatLength+edge-trayThickness, edge, frontX + flatLength + edge , frontY + flatHeight, 0);
            MakeArcFace(frontX, frontY, 0,edge - trayThickness, edge, 270, 180);
            MakeArcFace(frontX+flatLength, frontY, 0, edge - trayThickness, edge, 360, 270);
            MakeArcFace(frontX + flatLength, frontY+flatHeight, 0, edge - trayThickness, edge, 90, 0);
            MakeArcFace(frontX , frontY + flatHeight, 0, edge - trayThickness, edge, 180, 90);
        }

        private void MakeArcFace(double cx, double cy, double cz,double innerR, double outterR, double st, double et)
        {
            double theta;
            double theta2;
            
            double str = DegToRad(st);
            double etr = DegToRad(et);
            double dt = (etr - str) / edgeDivs;
            for (int i = 0; i < edgeDivs; i ++)
            {
                theta = str + (i * dt);
                theta2 = theta + dt;
                Point pn0 = CalcPoint(theta,innerR);
                int p0 = AddVertice(cx + pn0.X, cy + pn0.Y, cz);
                Point pn1 = CalcPoint(theta, outterR);
                int p1 = AddVertice(cx + pn1.X, cy + pn1.Y, cz);
                Point pn2 = CalcPoint(theta2, outterR);
                int p2 = AddVertice(cx + pn2.X, cy + pn2.Y, cz);
                Point pn3 = CalcPoint(theta2, innerR);
                int p3 = AddVertice(cx + pn3.X, cy + pn3.Y, cz);
                AddFace(p0, p1, p2);
                AddFace(p0, p2, p3);
            }
        }

        private void MakeRectFace(double lx, double ly, double rx, double ry,double z)
        {
            int p0 = AddVertice(lx, ly, z);
            int p1 = AddVertice(rx, ly, z);
            int p2 = AddVertice(rx, ry, z);
            int p3 = AddVertice(lx, ry, z);
            AddFace(p0, p2, p1);
            AddFace(p0, p3, p2);
        }

        private void MakeBackBottomLeftCorner()
        {
            double cx = frontX;
            double cy = frontY;
            double cz = 0;

            MakeCorner(Math.PI, 0, cx, cy, cz, true);
        }



        private void MakeFrontBottomLeftCorner()
        {
            double cx = frontX;
            double cy = frontY;
            double cz = 0;

            MakeCorner(-halfPi, 0, cx, cy, cz, true);
        }

        private void MakeFrontBottomLeftTrayCorner()
        {
            double cx = frontX;
            double cy = frontY;
            double cz = 0;

            MakeTrayCorner(-halfPi, 0, cx, cy, cz);
        }

        private void MakeFrontBottomRightCorner()
        {
            double cx = frontX + flatLength;
            double cy = frontY;
            double cz = 0;

            MakeCorner(0, -halfPi, cx, cy, cz);
        }

        private void MakeFrontBottomRightTrayCorner()
        {
            double cx = frontX + flatLength;
            double cy = frontY;
            double cz = 0;

            MakeTrayCorner(0, -halfPi, cx, cy, cz,true);
        }
        private void MakeBackBottomRightCorner()
        {
            double cx = frontX + flatLength;
            double cy = frontY;
            double cz = 0;

            MakeCorner(halfPi, -halfPi, cx, cy, cz);
        }

        private Point3D SpherePoint(double cx, double cy, double cz, double theta, double phi, double r1, double r2)
        {
            double px = cx + r1 * Math.Sin(theta) * Math.Cos(phi);
            double py = cy + r1 * Math.Sin(theta) * Math.Sin(phi);
            double pz = cz + r2 * Math.Cos(theta);
            return new Point3D(px, py, pz);
        }

        private void MakeFrontTopLeftCorner()
        {
            double cx = frontX;
            double cy = frontY + flatHeight;
            double cz = 0;

            MakeCorner(0, halfPi, cx, cy, cz);
        }

        private void MakeFrontTopLeftTrayCorner()
        {
            double cx = frontX;
            double cy = frontY + flatHeight;
            double cz = 0;

            MakeTrayCorner(0, halfPi, cx, cy, cz, true);
        }
        private void MakeBackTopLeftCorner()
        {
            double cx = frontX;
            double cy = frontY + flatHeight;
            double cz = 0;

            MakeCorner(halfPi, halfPi, cx, cy, cz);
        }

        private void MakeFrontTopRightTrayCorner()
        {
            double cx = frontX + flatLength;
            double cy = frontY + flatHeight;
            double cz = 0;

            MakeTrayCorner(0, 0, cx, cy, cz,true);
        }


        private void MakeFrontTopRightCorner()
        {
            double cx = frontX + flatLength;
            double cy = frontY + flatHeight;
            double cz = 0;

            MakeCorner(0, 0, cx, cy, cz);
        }
        private void MakeBackTopRightCorner()
        {
            double cx = frontX + flatLength;
            double cy = frontY + flatHeight;
            double cz = 0;

            MakeCorner(halfPi, 0, cx, cy, cz);
        }

        private void MakeCorner(double toff, double poff, double cx, double cy, double cz, bool invert = false)
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
                theta = toff + (d * i);
                for (int j = 0; j < edgeDivs - 1; j++)
                {
                    phi = poff + (d * j);
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

        private void MakeTrayCorner(double toff, double poff, double cx, double cy, double cz, bool invert = false)
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
                theta = toff + (d * i);
                for (int j = 0; j < edgeDivs - 1; j++)
                {
                    phi = poff + (d * j);
                    p0 = SpherePoint(cx, cy, cz, theta, phi, edge-trayThickness, halfW - trayThickness);
                    p1 = SpherePoint(cx, cy, cz, theta + d, phi, edge - trayThickness, halfW - trayThickness);
                    p2 = SpherePoint(cx, cy, cz, theta + d, phi + d, edge - trayThickness, halfW - trayThickness);
                    p3 = SpherePoint(cx, cy, cz, theta, phi + d, edge - trayThickness, halfW - trayThickness);

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
            double ly = frontY;
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
                AddFace(p0, p3, p2);
            }
        }

        private void MakeFrontTrayBottom()
        {
            double lx = frontX;
            double rx = frontX + flatLength;
            double ly = frontY;
            double tz = frontZ - trayThickness;
            for (int i = 0; i < edgeDivs - 1; i++)
            {
                double py1 = ly - trigPoints[i].X * (edge - trayThickness);
                double pz1 = trigPoints[i].Y * (halfW - trayThickness);

                double py2 = ly - trigPoints[i + 1].X * (edge - trayThickness);
                double pz2 = trigPoints[i + 1].Y * (halfW - trayThickness);

                int p0 = AddVertice(lx, py1, pz1);
                int p1 = AddVertice(rx, py1, pz1);
                int p2 = AddVertice(rx, py2, pz2);
                int p3 = AddVertice(lx, py2, pz2);
                AddFace(p0, p1, p2);
                AddFace(p0, p2, p3);
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
        private void MakeFrontTrayTop()
        {
            double lx = frontX;
            double rx = frontX + flatLength;
            double ly = frontY + flatHeight;
            double tz = frontZ- trayThickness;
            for (int i = 0; i < edgeDivs - 1; i++)
            {
                double py1 = ly + trigPoints[i].X * (edge-trayThickness);
                double pz1 = trigPoints[i].Y * (halfW - trayThickness);

                double py2 = ly + trigPoints[i + 1].X * (edge - trayThickness);
                double pz2 = trigPoints[i + 1].Y * (halfW - trayThickness);

                int p0 = AddVertice(lx, py1, pz1);
                int p1 = AddVertice(rx, py1, pz1);
                int p2 = AddVertice(rx, py2, pz2);
                int p3 = AddVertice(lx, py2, pz2);
                AddFace(p0, p2, p1);
                AddFace(p0, p3, p2);
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

                double px2 = frontX - trigPoints[i + 1].X * edge;
                double pz2 = trigPoints[i + 1].Y * halfW;

                int p0 = AddVertice(px1, frontY, pz1);
                int p1 = AddVertice(px2, frontY, pz2);
                int p2 = AddVertice(px2, ty, pz2);
                int p3 = AddVertice(px1, ty, pz1);
                AddFace(p0, p2, p1);
                AddFace(p0, p3, p2);
            }
        }

        private void MakeFrontTrayLeft()
        {
            double ty = frontY + flatHeight;
            double tz = frontZ-trayThickness;
            for (int i = 0; i < edgeDivs - 1; i++)
            {
                double px1 = frontX - trigPoints[i].X * (edge - trayThickness);
                double pz1 = trigPoints[i].Y * (halfW - trayThickness);

                double px2 = frontX - trigPoints[i + 1].X * (edge - trayThickness);
                double pz2 = trigPoints[i + 1].Y * (halfW - trayThickness);

                int p0 = AddVertice(px1, frontY, pz1);
                int p1 = AddVertice(px2, frontY, pz2);
                int p2 = AddVertice(px2, ty, pz2);
                int p3 = AddVertice(px1, ty, pz1);
                AddFace(p0, p1, p2);
                AddFace(p0, p2, p3);
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
            for (int i = 0; i < edgeDivs - 1; i++)
            {
                double px1 = rx + trigPoints[i].X * edge;
                double pz1 = trigPoints[i].Y * halfW;

                double px2 = rx + trigPoints[i + 1].X * edge;
                double pz2 = trigPoints[i + 1].Y * halfW;

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
                double pz1 = -trigPoints[i].Y * halfW;

                double px2 = rx + trigPoints[i + 1].X * edge;
                double pz2 = -trigPoints[i + 1].Y * halfW;

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
            int p2 = AddVertice(frontX + flatLength, frontY + flatHeight, frontZ);
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
            MakeFlat(-frontZ);
        }

        private void MakeFlat(double backZ)
        {
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

        private void MakeBackHalf()
        {
            double px1 = frontX;
            double py1 = frontY - edge;

            double px2 = px1 + flatLength;
            double py2 = frontY + flatHeight + edge;

            // fill the gap between the back square and the right edge of the pill
            int p0 = AddVertice(px1, py1, 0);
            int p1 = AddVertice(px1, py2, 0);
            int p2 = AddVertice(px2, py2, 0);
            int p3 = AddVertice(px2, py1, 0);
            AddFace(p0, p1, p2);
            AddFace(p0, p2, p3);
        }

        private void MakeBackHalfGaps()
        {
            double px1 = frontX + flatLength;
            double ty = frontY + flatHeight;
            double px2 = px1 + edge;

            // fill the gap between the back square and the right edge of the pill
            int p0 = AddVertice(px1, ty, 0);
            int p1 = AddVertice(px1, frontY, 0);
            int p2 = AddVertice(px2, frontY, 0);
            int p3 = AddVertice(px2, ty, 0);
            AddFace(p0, p2, p1);
            AddFace(p0, p3, p2);
            // fill the gap between the back square and the left edge of the pill
            px1 = frontX - edge;
            ty = frontY + flatHeight;
            px2 = frontX;
            p0 = AddVertice(px1, ty, 0);
            p1 = AddVertice(px1, frontY, 0);
            p2 = AddVertice(px2, frontY, 0);
            p3 = AddVertice(px2, ty, 0);
            AddFace(p0, p2, p1);
            AddFace(p0, p3, p2);
        }

        private void MakeHalfCorners()
        {
            MakeFlatPie(frontX + flatLength, frontY + flatHeight, 90, 0, edge);
            MakeFlatPie(frontX, frontY + flatHeight, 180, 90, edge);
            MakeFlatPie(frontX, frontY, 270, 180, edge);
            MakeFlatPie(frontX + flatLength, frontY, 360, 270, edge);
        }

        private void MakeFlatPie(double cx, double cy, double start, double stop, double radius)
        {
            double theta = DegToRad(start);
            double dt = DegToRad((stop - start) / edgeDivs);
            int v0 = AddVertice(cx, cy, 0);
            for (int i = 0; i < edgeDivs; i++)
            {
                double t = theta + (dt * i);
                double t2 = t + dt;

                Point p1 = CalcPoint(t, radius);
                Point p2 = CalcPoint(t2, radius);
                int v1 = AddVertice(cx + p1.X, cy + p1.Y, 0);
                int v2 = AddVertice(cx + p2.X, cy + p2.Y, 0);
                AddFace(v0, v1, v2);
            }
        }


        private void MakeFrontTrayRight()
        {
            double rx = frontX + flatLength;
            double ty = frontY + flatHeight;
            double tz = frontZ - trayThickness;
            for (int i = 0; i < edgeDivs - 1; i++)
            {
                double px1 = rx + trigPoints[i].X * (edge - trayThickness);
                double pz1 = trigPoints[i].Y * (halfW - trayThickness);

                double px2 = rx + trigPoints[i + 1].X * (edge - trayThickness);
                double pz2 = trigPoints[i + 1].Y * (halfW - trayThickness); 

                int p0 = AddVertice(px1, frontY, pz1);
                int p1 = AddVertice(px2, frontY, pz2);
                int p2 = AddVertice(px2, ty, pz2);
                int p3 = AddVertice(px1, ty, pz1);
                AddFace(p0, p2, p1);
                AddFace(p0, p3, p2);
            }
        }
    }
}