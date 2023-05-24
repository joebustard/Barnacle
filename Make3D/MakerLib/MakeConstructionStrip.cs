using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class ConstructionStripMaker : MakerBase
    {
        private double holeRadius;
        private int numberOfHoles;
        private double stripHeight;
        private int stripRepeats;
        private double stripWidth;

        public ConstructionStripMaker(double stripHeight, double stripWidth, int stripRepeats, double holeRadius, int numberOfHoles)
        {
            this.stripHeight = stripHeight;
            this.stripWidth = stripWidth;
            this.stripRepeats = stripRepeats;
            this.holeRadius = holeRadius;
            this.numberOfHoles = numberOfHoles;
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            if (stripRepeats == 1)
            {
                GenerateStrip();
            }
            else
            {
                GeneratePlate();
            }
        }

        private void Dump(byte[,] pattern)
        {
            for (int r = 0; r < pattern.GetLength(0); r++)
            {
                for (int c = 0; c < pattern.GetLength(1); c++)
                {
                    System.Diagnostics.Debug.Write($"{pattern[r, c]}, ");
                }
                System.Diagnostics.Debug.WriteLine("");
            }
        }

        private void GeneratePlate()
        {
            if (numberOfHoles > 2)
            {
                byte[,] pattern = new byte[numberOfHoles + 1, stripRepeats + 1];
                // sorry, you need the documentation to understand these numbers
                pattern[0, 0] = 1;
                for (int i = 1; i < numberOfHoles; i++)
                {
                    pattern[i, 0] = 2;
                }
                pattern[numberOfHoles, 0] = 3;

                for (int j = 1; j < stripRepeats; j++)
                {
                    pattern[0, j] = 4;
                    for (int i = 1; i < numberOfHoles; i++)
                    {
                        pattern[i, j] = 5;
                    }

                    pattern[numberOfHoles, j] = 6;
                }

                pattern[0, stripRepeats] = 7;
                for (int i = 1; i < numberOfHoles; i++)
                {
                    pattern[i, stripRepeats] = 8;
                }
                pattern[numberOfHoles, stripRepeats] = 9;
                //Dump(pattern);
                GeneratePattern(pattern);
            }
        }

        private void GenerateStrip()
        {
            if (numberOfHoles > 2)
            {
                byte[,] pattern = new byte[numberOfHoles + 1, 2];
                // sorry, you need the documentation to understand these numbers
                pattern[0, 0] = 1;
                for (int i = 1; i < numberOfHoles; i++)
                {
                    pattern[i, 0] = 2;
                }
                pattern[numberOfHoles, 0] = 3;

                pattern[0, 1] = 7;
                for (int i = 1; i < numberOfHoles; i++)
                {
                    pattern[i, 1] = 8;
                }
                pattern[numberOfHoles, 1] = 9;
                //Dump(pattern);
                GeneratePattern(pattern);
            }
        }

        private void GeneratePattern(byte[,] pattern)
        {
            double px;
            double py;
            double stripLength = numberOfHoles * stripWidth;
            double hw = stripWidth / 2.0;
            py = 0;
            for (int r = 0; r < pattern.GetLength(0); r++)
            {
                px = 0;
                for (int c = 0; c < pattern.GetLength(1); c++)
                {
                    double l = 10;
                    switch (pattern[r, c])
                    {
                        case 1:
                            {
                                l = hw;
                                MakeShapeOne(px, py, hw);
                                px += l;
                            }
                            break;

                        case 2:
                            {
                                l = hw;
                                MakeShapeTwo(px, py, hw);
                                px += l;
                            }
                            break;

                        case 3:
                            {
                                l = hw;
                                MakeShapeThree(px, py, hw);
                                px += l;
                            }
                            break;

                        case 4:
                            {
                                l = stripWidth;
                                MakeShapeFour(px, py, hw);
                                px += l;
                            }
                            break;

                        case 5:
                            {
                                l = stripWidth;
                                MakeShapeFive(px, py, hw);
                                px += l;
                            }
                            break;

                        case 6:
                            {
                                l = stripWidth;
                                MakeShapeSix(px, py, hw);
                                px += l;
                            }
                            break;

                        case 7:
                            {
                                l = hw;
                                MakeShapeSeven(px, py, hw);
                                px += l;
                            }
                            break;

                        case 8:
                            {
                                l = hw;
                                MakeShapeEight(px, py, hw);
                                px += l;
                            }
                            break;

                        case 9:
                            {
                                l = hw;
                                MakeShapeNine(px, py, hw);
                                px += l;
                            }
                            break;
                    }
                }
                // The change in Y is only half for the first and last rows.
                if ((r == 0) || (r == numberOfHoles + 1))
                {
                    py += hw;
                }
                else
                {
                    py += stripWidth;
                }
            }
        }

        private void MakeShapeOne(double px, double py, double hw)
        {
            // back left corner
            double cx = px + hw;
            double cy = py + hw;

            double st = DegToRad(180);
            double et = DegToRad(270);


            MakeCorner(hw, cx, cy, st, et);
        }

        private void MakeShapeTwo(double px, double py, double hw)
        {
            // shape 2 is Left side

            Point e0 = new Point(px, py);
            Point e1 = new Point(px + hw - holeRadius, py);
            Point e2 = new Point(px + hw, py + holeRadius);
            Point e3 = new Point(px + hw, py + (stripWidth - holeRadius));
            Point e4 = new Point(px + hw - holeRadius, py + stripWidth);
            Point e5 = new Point(px, py + stripWidth);
            Point e6 = new Point(px + hw - holeRadius - 0.1, py + hw);

            double cx = px + hw;
            double cy = py;

            double st = DegToRad(90);
            double et = DegToRad(180);
            int pn0 = AddVertice(e0.X, 0, e0.Y);
            int pn1 = AddVertice(e1.X, 0, e1.Y);
            int pn2 = AddVertice(e2.X, 0, e2.Y);
            int pn3 = AddVertice(e3.X, 0, e3.Y);
            int pn4 = AddVertice(e4.X, 0, e4.Y);
            int pn5 = AddVertice(e5.X, 0, e5.Y);
            int pn6 = AddVertice(e6.X, 0, e6.Y);

            int pn7 = AddVertice(e0.X, stripHeight, e0.Y);
            int pn8 = AddVertice(e1.X, stripHeight, e1.Y);
            int pn9 = AddVertice(e2.X, stripHeight, e2.Y);
            int pn10 = AddVertice(e3.X, stripHeight, e3.Y);
            int pn11 = AddVertice(e4.X, stripHeight, e4.Y);
            int pn12 = AddVertice(e5.X, stripHeight, e5.Y);
            int pn13 = AddVertice(e6.X, stripHeight, e6.Y);
            double dt = DegToRad(3);
            double theta = 0;
            for (theta = st; theta < et; theta += dt)
            {
                System.Windows.Point pin0 = CalcPoint(theta, holeRadius);
                System.Windows.Point pin1 = CalcPoint(theta + dt, holeRadius);

                int p0 = AddVertice(cx + pin0.X, 0, cy + pin0.Y);
                int p1 = AddVertice(cx + pin0.X, stripHeight, cy + pin0.Y);
                int p2 = AddVertice(cx + pin1.X, stripHeight, cy + pin1.Y);
                int p3 = AddVertice(cx + pin1.X, 0, cy + pin1.Y);

                Faces.Add(p0);
                Faces.Add(p2);
                Faces.Add(p1);

                Faces.Add(p0);
                Faces.Add(p3);
                Faces.Add(p2);

                Faces.Add(p0);
                Faces.Add(pn6);
                Faces.Add(p3);

                Faces.Add(p1);
                Faces.Add(p2);
                Faces.Add(pn13);
            }

            cx = px + hw;
            cy = py + stripWidth;
            st = DegToRad(180);
            et = DegToRad(270);

            theta = 0;
            for (theta = st; theta < et; theta += dt)
            {
                System.Windows.Point pin0 = CalcPoint(theta, holeRadius);
                System.Windows.Point pin1 = CalcPoint(theta + dt, holeRadius);

                int p0 = AddVertice(cx + pin0.X, 0, cy + pin0.Y);
                int p1 = AddVertice(cx + pin0.X, stripHeight, cy + pin0.Y);
                int p2 = AddVertice(cx + pin1.X, stripHeight, cy + pin1.Y);
                int p3 = AddVertice(cx + pin1.X, 0, cy + pin1.Y);

                Faces.Add(p0);
                Faces.Add(p2);
                Faces.Add(p1);

                Faces.Add(p0);
                Faces.Add(p3);
                Faces.Add(p2);

                Faces.Add(p0);
                Faces.Add(pn6);
                Faces.Add(p3);

                Faces.Add(p1);
                Faces.Add(p2);
                Faces.Add(pn13);
            }

            // bottom
            Faces.Add(pn2);
            Faces.Add(pn3);
            Faces.Add(pn6);

            Faces.Add(pn0);
            Faces.Add(pn1);
            Faces.Add(pn6);

            Faces.Add(pn6);
            Faces.Add(pn4);
            Faces.Add(pn5);

            Faces.Add(pn6);
            Faces.Add(pn5);
            Faces.Add(pn0);

            //top
            Faces.Add(pn9);
            Faces.Add(pn13);
            Faces.Add(pn10);

            Faces.Add(pn7);
            Faces.Add(pn13);
            Faces.Add(pn8);

            Faces.Add(pn13);
            Faces.Add(pn12);
            Faces.Add(pn11);

            Faces.Add(pn13);
            Faces.Add(pn7);
            Faces.Add(pn12);

            //Close left
            Faces.Add(pn0);
            Faces.Add(pn5);
            Faces.Add(pn7);

            Faces.Add(pn7);
            Faces.Add(pn5);
            Faces.Add(pn12);
        }

        private void MakeShapeFour(double px, double py, double hw)
        {
            // 

            Point e0 = new Point(px+stripWidth, py);
            Point e1 = new Point(px + stripWidth, py - holeRadius + hw);
            Point e2 = new Point(px + stripWidth-holeRadius, py + hw);
            Point e3 = new Point(px + holeRadius, py + hw);
            Point e4 = new Point(px, py + hw-holeRadius);
            Point e5 = new Point(px, py);
            Point e6 = new Point(px + stripWidth/2, py + hw/2 -0.1);

            double cx = px + stripWidth;
            double cy = py+hw;

            double st = DegToRad(180);
            double et = DegToRad(270);
            int pn0 = AddVertice(e0.X, 0, e0.Y);
            int pn1 = AddVertice(e1.X, 0, e1.Y);
            int pn2 = AddVertice(e2.X, 0, e2.Y);
            int pn3 = AddVertice(e3.X, 0, e3.Y);
            int pn4 = AddVertice(e4.X, 0, e4.Y);
            int pn5 = AddVertice(e5.X, 0, e5.Y);
            int pn6 = AddVertice(e6.X, 0, e6.Y);

            int pn7 = AddVertice(e0.X, stripHeight, e0.Y);
            int pn8 = AddVertice(e1.X, stripHeight, e1.Y);
            int pn9 = AddVertice(e2.X, stripHeight, e2.Y);
            int pn10 = AddVertice(e3.X, stripHeight, e3.Y);
            int pn11 = AddVertice(e4.X, stripHeight, e4.Y);
            int pn12 = AddVertice(e5.X, stripHeight, e5.Y);
            int pn13 = AddVertice(e6.X, stripHeight, e6.Y);
            double dt = DegToRad(3);
            double theta = 0;
            for (theta = st; theta < et; theta += dt)
            {
                System.Windows.Point pin0 = CalcPoint(theta, holeRadius);
                System.Windows.Point pin1 = CalcPoint(theta + dt, holeRadius);

                int p0 = AddVertice(cx + pin0.X, 0, cy + pin0.Y);
                int p1 = AddVertice(cx + pin0.X, stripHeight, cy + pin0.Y);
                int p2 = AddVertice(cx + pin1.X, stripHeight, cy + pin1.Y);
                int p3 = AddVertice(cx + pin1.X, 0, cy + pin1.Y);

                Faces.Add(p0);
                Faces.Add(p2);
                Faces.Add(p1);

                Faces.Add(p0);
                Faces.Add(p3);
                Faces.Add(p2);

                Faces.Add(p0);
                Faces.Add(pn6);
                Faces.Add(p3);

                Faces.Add(p1);
                Faces.Add(p2);
                Faces.Add(pn13);
            }

            cx = px;
            cy = py + hw;
            st = DegToRad(270);
            et = DegToRad(360);

            theta = 0;
            for (theta = st; theta < et; theta += dt)
            {
                System.Windows.Point pin0 = CalcPoint(theta, holeRadius);
                System.Windows.Point pin1 = CalcPoint(theta + dt, holeRadius);

                int p0 = AddVertice(cx + pin0.X, 0, cy + pin0.Y);
                int p1 = AddVertice(cx + pin0.X, stripHeight, cy + pin0.Y);
                int p2 = AddVertice(cx + pin1.X, stripHeight, cy + pin1.Y);
                int p3 = AddVertice(cx + pin1.X, 0, cy + pin1.Y);

                Faces.Add(p0);
                Faces.Add(p2);
                Faces.Add(p1);

                Faces.Add(p0);
                Faces.Add(p3);
                Faces.Add(p2);

                Faces.Add(p0);
                Faces.Add(pn6);
                Faces.Add(p3);

                Faces.Add(p1);
                Faces.Add(p2);
                Faces.Add(pn13);
            }

            // bottom
            Faces.Add(pn2);
            Faces.Add(pn3);
            Faces.Add(pn6);

            Faces.Add(pn0);
            Faces.Add(pn1);
            Faces.Add(pn6);

            Faces.Add(pn6);
            Faces.Add(pn4);
            Faces.Add(pn5);

            Faces.Add(pn6);
            Faces.Add(pn5);
            Faces.Add(pn0);

            //top
            Faces.Add(pn9);
            Faces.Add(pn13);
            Faces.Add(pn10);

            Faces.Add(pn7);
            Faces.Add(pn13);
            Faces.Add(pn8);

            Faces.Add(pn13);
            Faces.Add(pn12);
            Faces.Add(pn11);

            Faces.Add(pn13);
            Faces.Add(pn7);
            Faces.Add(pn12);

            //Close left
            Faces.Add(pn0);
            Faces.Add(pn5);
            Faces.Add(pn7);

            Faces.Add(pn7);
            Faces.Add(pn5);
            Faces.Add(pn12);
        }

        private void MakeShapeFive( double px, double py, double hw)
        {
            
            Point e0 = new Point(px + holeRadius, py);
            Point e1 = new Point(px + stripWidth - holeRadius, py);
            Point e2 = new Point(px + stripWidth, py + holeRadius);
            Point e3 = new Point(px + stripWidth, py + stripWidth - holeRadius);
            Point e4 = new Point(px + stripWidth - holeRadius, py + stripWidth);
            Point e5 = new Point(px + holeRadius, py + stripWidth);
            Point e6 = new Point(px, py + stripWidth - holeRadius);
            Point e7 = new Point(px , py + holeRadius);
            Point e8 = new Point(px + stripWidth / 2, py + stripWidth / 2);

            int pn0 = AddVertice(e0.X, 0, e0.Y);
            int pn1 = AddVertice(e1.X, 0, e1.Y);
            int pn2 = AddVertice(e2.X, 0, e2.Y);
            int pn3 = AddVertice(e3.X, 0, e3.Y);
            int pn4 = AddVertice(e4.X, 0, e4.Y);
            int pn5 = AddVertice(e5.X, 0, e5.Y);
            int pn6 = AddVertice(e6.X, 0, e6.Y);
            int pn7 = AddVertice(e7.X, 0, e7.Y);
            int pn8 = AddVertice(e8.X, 0, e8.Y);

            int pn9 = AddVertice(e0.X, stripHeight, e0.Y);
            int pn10 = AddVertice(e1.X, stripHeight, e1.Y);
            int pn11 = AddVertice(e2.X, stripHeight, e2.Y);
            int pn12 = AddVertice(e3.X, stripHeight, e3.Y);
            int pn13 = AddVertice(e4.X, stripHeight, e4.Y);
            int pn14 = AddVertice(e5.X, stripHeight, e5.Y);
            int pn15 = AddVertice(e6.X, stripHeight, e6.Y);
            int pn16 = AddVertice(e7.X, stripHeight, e7.Y);
            int pn17 = AddVertice(e8.X, stripHeight, e8.Y);

            double cx = px;
            double cy = py;

            double st = DegToRad(0);
            double et = DegToRad(90);
            double dt = DegToRad(3);

            double theta = 0;
            for (theta = st; theta < et; theta += dt)
            {
                System.Windows.Point pin0 = CalcPoint(theta, holeRadius);
                System.Windows.Point pin1 = CalcPoint(theta + dt, holeRadius);

                int p0 = AddVertice(cx + pin0.X, 0, cy + pin0.Y);
                int p1 = AddVertice(cx + pin0.X, stripHeight, cy + pin0.Y);
                int p2 = AddVertice(cx + pin1.X, stripHeight, cy + pin1.Y);
                int p3 = AddVertice(cx + pin1.X, 0, cy + pin1.Y);

                Faces.Add(p0);
                Faces.Add(p2);
                Faces.Add(p1);

                Faces.Add(p0);
                Faces.Add(p3);
                Faces.Add(p2);

                Faces.Add(p0);
                Faces.Add(pn8);
                Faces.Add(p3);

                Faces.Add(p1);
                Faces.Add(p2);
                Faces.Add(pn17);
            }

            cx = px + stripWidth;
            cy = py;
            st = DegToRad(90);
            et = DegToRad(180);

            theta = 0;
            for (theta = st; theta < et; theta += dt)
            {
                System.Windows.Point pin0 = CalcPoint(theta, holeRadius);
                System.Windows.Point pin1 = CalcPoint(theta + dt, holeRadius);

                int p0 = AddVertice(cx + pin0.X, 0, cy + pin0.Y);
                int p1 = AddVertice(cx + pin0.X, stripHeight, cy + pin0.Y);
                int p2 = AddVertice(cx + pin1.X, stripHeight, cy + pin1.Y);
                int p3 = AddVertice(cx + pin1.X, 0, cy + pin1.Y);

                Faces.Add(p0);
                Faces.Add(p2);
                Faces.Add(p1);

                Faces.Add(p0);
                Faces.Add(p3);
                Faces.Add(p2);

                Faces.Add(p0);
                Faces.Add(pn8);
                Faces.Add(p3);

                Faces.Add(p1);
                Faces.Add(p2);
                Faces.Add(pn17);
            }

            cx = px + stripWidth;
            cy = py + stripWidth;
            st = DegToRad(180);
            et = DegToRad(270);

            theta = 0;
            for (theta = st; theta < et; theta += dt)
            {
                System.Windows.Point pin0 = CalcPoint(theta, holeRadius);
                System.Windows.Point pin1 = CalcPoint(theta + dt, holeRadius);

                int p0 = AddVertice(cx + pin0.X, 0, cy + pin0.Y);
                int p1 = AddVertice(cx + pin0.X, stripHeight, cy + pin0.Y);
                int p2 = AddVertice(cx + pin1.X, stripHeight, cy + pin1.Y);
                int p3 = AddVertice(cx + pin1.X, 0, cy + pin1.Y);

                Faces.Add(p0);
                Faces.Add(p2);
                Faces.Add(p1);

                Faces.Add(p0);
                Faces.Add(p3);
                Faces.Add(p2);

                Faces.Add(p0);
                Faces.Add(pn8);
                Faces.Add(p3);

                Faces.Add(p1);
                Faces.Add(p2);
                Faces.Add(pn17);
            }


            cx = px;
            cy = py + stripWidth;
            st = DegToRad(270);
            et = DegToRad(360);

            theta = 0;
            for (theta = st; theta < et; theta += dt)
            {
                System.Windows.Point pin0 = CalcPoint(theta, holeRadius);
                System.Windows.Point pin1 = CalcPoint(theta + dt, holeRadius);

                int p0 = AddVertice(cx + pin0.X, 0, cy + pin0.Y);
                int p1 = AddVertice(cx + pin0.X, stripHeight, cy + pin0.Y);
                int p2 = AddVertice(cx + pin1.X, stripHeight, cy + pin1.Y);
                int p3 = AddVertice(cx + pin1.X, 0, cy + pin1.Y);

                Faces.Add(p0);
                Faces.Add(p2);
                Faces.Add(p1);

                Faces.Add(p0);
                Faces.Add(p3);
                Faces.Add(p2);

                Faces.Add(p0);
                Faces.Add(pn8);
                Faces.Add(p3);

                Faces.Add(p1);
                Faces.Add(p2);
                Faces.Add(pn17);
            }
            
            // bottom
            Faces.Add(pn0);
            Faces.Add(pn1);
            Faces.Add(pn8);

            Faces.Add(pn9);
            Faces.Add(pn17);
            Faces.Add(pn10);

            Faces.Add(pn2);
            Faces.Add(pn3);
            Faces.Add(pn8);

            Faces.Add(pn11);
            Faces.Add(pn17);
            Faces.Add(pn12);


            Faces.Add(pn4);
            Faces.Add(pn5);
            Faces.Add(pn8);

            Faces.Add(pn13);
            Faces.Add(pn17);
            Faces.Add(pn14);

            Faces.Add(pn6);
            Faces.Add(pn7);
            Faces.Add(pn8);

            Faces.Add(pn15);
            Faces.Add(pn17);
            Faces.Add(pn16);

 
          
        }
        private void MakeShapeSix(double px, double py, double hw)
        {
            // 

            Point e0 = new Point(px , py+hw);
            Point e1 = new Point(px , py + holeRadius );
            Point e2 = new Point(px + holeRadius, py );
            Point e3 = new Point(px + stripWidth - holeRadius, py);
            Point e4 = new Point(px + stripWidth, py +holeRadius);
            Point e5 = new Point(px +stripWidth, py+hw);
            Point e6 = new Point(px + stripWidth / 2, py + hw / 2 + 0.1);

            double cx = px;
            double cy = py;

            double st = DegToRad(0);
            double et = DegToRad(90);
            int pn0 = AddVertice(e0.X, 0, e0.Y);
            int pn1 = AddVertice(e1.X, 0, e1.Y);
            int pn2 = AddVertice(e2.X, 0, e2.Y);
            int pn3 = AddVertice(e3.X, 0, e3.Y);
            int pn4 = AddVertice(e4.X, 0, e4.Y);
            int pn5 = AddVertice(e5.X, 0, e5.Y);
            int pn6 = AddVertice(e6.X, 0, e6.Y);

            int pn7 = AddVertice(e0.X, stripHeight, e0.Y);
            int pn8 = AddVertice(e1.X, stripHeight, e1.Y);
            int pn9 = AddVertice(e2.X, stripHeight, e2.Y);
            int pn10 = AddVertice(e3.X, stripHeight, e3.Y);
            int pn11 = AddVertice(e4.X, stripHeight, e4.Y);
            int pn12 = AddVertice(e5.X, stripHeight, e5.Y);
            int pn13 = AddVertice(e6.X, stripHeight, e6.Y);
            double dt = DegToRad(3);
            double theta = 0;
            for (theta = st; theta < et; theta += dt)
            {
                System.Windows.Point pin0 = CalcPoint(theta, holeRadius);
                System.Windows.Point pin1 = CalcPoint(theta + dt, holeRadius);

                int p0 = AddVertice(cx + pin0.X, 0, cy + pin0.Y);
                int p1 = AddVertice(cx + pin0.X, stripHeight, cy + pin0.Y);
                int p2 = AddVertice(cx + pin1.X, stripHeight, cy + pin1.Y);
                int p3 = AddVertice(cx + pin1.X, 0, cy + pin1.Y);

                Faces.Add(p0);
                Faces.Add(p2);
                Faces.Add(p1);

                Faces.Add(p0);
                Faces.Add(p3);
                Faces.Add(p2);

                Faces.Add(p0);
                Faces.Add(pn6);
                Faces.Add(p3);

                Faces.Add(p1);
                Faces.Add(p2);
                Faces.Add(pn13);
            }

            cx = px+stripWidth;
            cy = py ;
            st = DegToRad(90);
            et = DegToRad(180);

            theta = 0;
            for (theta = st; theta < et; theta += dt)
            {
                System.Windows.Point pin0 = CalcPoint(theta, holeRadius);
                System.Windows.Point pin1 = CalcPoint(theta + dt, holeRadius);

                int p0 = AddVertice(cx + pin0.X, 0, cy + pin0.Y);
                int p1 = AddVertice(cx + pin0.X, stripHeight, cy + pin0.Y);
                int p2 = AddVertice(cx + pin1.X, stripHeight, cy + pin1.Y);
                int p3 = AddVertice(cx + pin1.X, 0, cy + pin1.Y);

                Faces.Add(p0);
                Faces.Add(p2);
                Faces.Add(p1);

                Faces.Add(p0);
                Faces.Add(p3);
                Faces.Add(p2);

                Faces.Add(p0);
                Faces.Add(pn6);
                Faces.Add(p3);

                Faces.Add(p1);
                Faces.Add(p2);
                Faces.Add(pn13);
            }

            // bottom
            Faces.Add(pn2);
            Faces.Add(pn3);
            Faces.Add(pn6);

            Faces.Add(pn0);
            Faces.Add(pn1);
            Faces.Add(pn6);

            Faces.Add(pn6);
            Faces.Add(pn4);
            Faces.Add(pn5);

            Faces.Add(pn6);
            Faces.Add(pn5);
            Faces.Add(pn0);

            //top
            Faces.Add(pn9);
            Faces.Add(pn13);
            Faces.Add(pn10);

            Faces.Add(pn7);
            Faces.Add(pn13);
            Faces.Add(pn8);

            Faces.Add(pn13);
            Faces.Add(pn12);
            Faces.Add(pn11);

            Faces.Add(pn13);
            Faces.Add(pn7);
            Faces.Add(pn12);

            //Close left
            Faces.Add(pn0);
            Faces.Add(pn5);
            Faces.Add(pn7);

            Faces.Add(pn7);
            Faces.Add(pn5);
            Faces.Add(pn12);
        }
        private void MakeCorner(double hw, double cx, double cy, double st, double et)
        {
            double dt = DegToRad(3);
            double theta = 0;
            for (theta = st; theta < et; theta += dt)
            {
                System.Windows.Point pin0 = CalcPoint(theta, holeRadius);
                System.Windows.Point pin1 = CalcPoint(theta + dt, holeRadius);

                System.Windows.Point pout0 = CalcPoint(theta, hw);
                System.Windows.Point pout1 = CalcPoint(theta + dt, hw);

                // bottom
                int p0 = AddVertice(cx + pin0.X, 0, cy + pin0.Y);
                int p1 = AddVertice(cx + pout0.X, 0, cy + pout0.Y);
                int p2 = AddVertice(cx + pout1.X, 0, cy + pout1.Y);
                int p3 = AddVertice(cx + pin1.X, 0, cy + pin1.Y);

                Faces.Add(p0);
                Faces.Add(p1);
                Faces.Add(p2);

                Faces.Add(p0);
                Faces.Add(p2);
                Faces.Add(p3);

                // Top
                int p4 = AddVertice(cx + pin0.X, stripHeight, cy + pin0.Y);
                int p5 = AddVertice(cx + pout0.X, stripHeight, cy + pout0.Y);
                int p6 = AddVertice(cx + pout1.X, stripHeight, cy + pout1.Y);
                int p7 = AddVertice(cx + pin1.X, stripHeight, cy + pin1.Y);

                Faces.Add(p4);
                Faces.Add(p6);
                Faces.Add(p5);

                Faces.Add(p4);
                Faces.Add(p7);
                Faces.Add(p6);

                // outside curve
                Faces.Add(p1);
                Faces.Add(p5);
                Faces.Add(p2);

                Faces.Add(p2);
                Faces.Add(p5);
                Faces.Add(p6);

                // inside curve
                Faces.Add(p0);
                Faces.Add(p7);
                Faces.Add(p4);

                Faces.Add(p0);
                Faces.Add(p3);
                Faces.Add(p7);
            }
        }

        private void MakeShapeThree(double px, double py, double hw)
        {
            // front left corner
            double cx = px + hw;
            double cy = py;

            double st = DegToRad(90);
            double et = DegToRad(180);
            MakeCorner(hw, cx, cy, st, et);
        }

        private void MakeShapeSeven(double px, double py, double hw)
        {
            // front left corner
            double cx = px;
            double cy = py + hw;

            double st = DegToRad(270);
            double et = DegToRad(360);
            MakeCorner(hw, cx, cy, st, et);
        }

        private void MakeShapeNine(double px, double py, double hw)
        {
            // front left corner
            double cx = px;
            double cy = py;

            double st = DegToRad(0);
            double et = DegToRad(90);
            MakeCorner(hw, cx, cy, st, et);
        }
        private void MakeShapeEight(double px, double py, double hw)
        {
            // shape 8 is right side

            Point e0 = new Point(px+hw, py+stripWidth);
            Point e1 = new Point(px +  holeRadius, py+stripWidth);
            Point e2 = new Point(px, py + stripWidth - holeRadius);
            Point e3 = new Point(px , py +  holeRadius);
            Point e4 = new Point(px +  holeRadius, py);
            Point e5 = new Point(px+hw, py );
            Point e6 = new Point(px + holeRadius + 0.1, py + hw);

            double cx = px;
            double cy = py;

            double st = DegToRad(0);
            double et = DegToRad(90);
            int pn0 = AddVertice(e0.X, 0, e0.Y);
            int pn1 = AddVertice(e1.X, 0, e1.Y);
            int pn2 = AddVertice(e2.X, 0, e2.Y);
            int pn3 = AddVertice(e3.X, 0, e3.Y);
            int pn4 = AddVertice(e4.X, 0, e4.Y);
            int pn5 = AddVertice(e5.X, 0, e5.Y);
            int pn6 = AddVertice(e6.X, 0, e6.Y);

            int pn7 = AddVertice(e0.X, stripHeight, e0.Y);
            int pn8 = AddVertice(e1.X, stripHeight, e1.Y);
            int pn9 = AddVertice(e2.X, stripHeight, e2.Y);
            int pn10 = AddVertice(e3.X, stripHeight, e3.Y);
            int pn11 = AddVertice(e4.X, stripHeight, e4.Y);
            int pn12 = AddVertice(e5.X, stripHeight, e5.Y);
            int pn13 = AddVertice(e6.X, stripHeight, e6.Y);
            double dt = DegToRad(3);
            double theta = 0;
            for (theta = st; theta < et; theta += dt)
            {
                System.Windows.Point pin0 = CalcPoint(theta, holeRadius);
                System.Windows.Point pin1 = CalcPoint(theta + dt, holeRadius);

                int p0 = AddVertice(cx + pin0.X, 0, cy + pin0.Y);
                int p1 = AddVertice(cx + pin0.X, stripHeight, cy + pin0.Y);
                int p2 = AddVertice(cx + pin1.X, stripHeight, cy + pin1.Y);
                int p3 = AddVertice(cx + pin1.X, 0, cy + pin1.Y);

                Faces.Add(p0);
                Faces.Add(p2);
                Faces.Add(p1);

                Faces.Add(p0);
                Faces.Add(p3);
                Faces.Add(p2);

                Faces.Add(p0);
                Faces.Add(pn6);
                Faces.Add(p3);

                Faces.Add(p1);
                Faces.Add(p2);
                Faces.Add(pn13);
            }
           
            cx = px ;
            cy = py + stripWidth;
            st = DegToRad(270);
            et = DegToRad(360);

            theta = 0;
            for (theta = st; theta < et; theta += dt)
            {
                System.Windows.Point pin0 = CalcPoint(theta, holeRadius);
                System.Windows.Point pin1 = CalcPoint(theta + dt, holeRadius);

                int p0 = AddVertice(cx + pin0.X, 0, cy + pin0.Y);
                int p1 = AddVertice(cx + pin0.X, stripHeight, cy + pin0.Y);
                int p2 = AddVertice(cx + pin1.X, stripHeight, cy + pin1.Y);
                int p3 = AddVertice(cx + pin1.X, 0, cy + pin1.Y);

                Faces.Add(p0);
                Faces.Add(p2);
                Faces.Add(p1);

                Faces.Add(p0);
                Faces.Add(p3);
                Faces.Add(p2);

                Faces.Add(p0);
                Faces.Add(pn6);
                Faces.Add(p3);

                Faces.Add(p1);
                Faces.Add(p2);
                Faces.Add(pn13);
            }
            
            // bottom
            Faces.Add(pn2);
            Faces.Add(pn3);
            Faces.Add(pn6);
           
            Faces.Add(pn0);
            Faces.Add(pn1);
            Faces.Add(pn6);
            
            Faces.Add(pn6);
            Faces.Add(pn4);
            Faces.Add(pn5);

            Faces.Add(pn6);
            Faces.Add(pn5);
            Faces.Add(pn0);

            //top
            Faces.Add(pn9);
            Faces.Add(pn13);
            Faces.Add(pn10);

            Faces.Add(pn7);
            Faces.Add(pn13);
            Faces.Add(pn8);

            Faces.Add(pn13);
            Faces.Add(pn12);
            Faces.Add(pn11);

            Faces.Add(pn13);
            Faces.Add(pn7);
            Faces.Add(pn12);

            //Close left
            Faces.Add(pn0);
            Faces.Add(pn5);
            Faces.Add(pn7);

            Faces.Add(pn7);
            Faces.Add(pn5);
            Faces.Add(pn12);
        }
    }
}