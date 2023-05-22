using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
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

                                px += l;
                            }
                            break;

                        case 5:
                            {
                                l = stripWidth;

                                px += l;
                            }
                            break;

                        case 6:
                            {
                                l = stripWidth;

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
    }
}