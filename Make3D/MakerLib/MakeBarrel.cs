using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class BarrelMaker : MakerBase
    {
        private double barrelHeight;
        private double topBottomRadius;
        private double middleRadius;
        private double numberOfStaves;
        private bool shell;
        private double shellThickness;
        private double staveDepth;

        public BarrelMaker(double barrelHeight, double topRadius, double middleRadius, double numberOfStaves, bool shell, double shellThickness, double ribDepth)
        {
            this.barrelHeight = barrelHeight;
            this.topBottomRadius = topRadius;
            this.middleRadius = middleRadius;
            this.numberOfStaves = numberOfStaves;
            this.shell = shell;
            this.shellThickness = shellThickness;
            this.staveDepth = ribDepth;
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            double bulge = middleRadius - topBottomRadius;
            int verticalSteps = 20;
            double bandGap = barrelHeight / verticalSteps;
            double[] bandRadius = new double[verticalSteps];
            for (int i = 0; i < verticalSteps; i++)
            {
                double theta = (Math.PI / (verticalSteps-1)) * i;
                double sn = Math.Sin(theta) * bulge;
                bandRadius[i] = topBottomRadius + sn;
            }
            List<Point> bottomPoints = new List<Point>();
            for (int staveIndex = 0; staveIndex < numberOfStaves; staveIndex++)
            {
                MakeSingleStave(staveIndex, bandRadius, bottomPoints,  verticalSteps);
            }

            // do we want to just close the top and bottom
            if (!shell)
            {
                int lowCentre = AddVertice(new Point3D(0, 0, 0));
                int highCentre = AddVertice(new Point3D(0, barrelHeight, 0));

                int p0;
                int p1;
                int i = 1;
                for (int staveIndex = 0; staveIndex < numberOfStaves; staveIndex++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        p0 = AddVertice(new Point3D(bottomPoints[i].X, 0, bottomPoints[i].Y));
                        p1 = AddVertice(new Point3D(bottomPoints[i + 1].X, 0, bottomPoints[i + 1].Y));
                        Faces.Add(lowCentre);
                        Faces.Add(p0);
                        Faces.Add(p1);

                        p0 = AddVertice(new Point3D(bottomPoints[i].X, barrelHeight, bottomPoints[i].Y));
                        p1 = AddVertice(new Point3D(bottomPoints[i + 1].X, barrelHeight, bottomPoints[i + 1].Y));
                        Faces.Add(highCentre);
                        Faces.Add(p1);
                        Faces.Add(p0);

                        i += 1;
                    }
                    p0 = AddVertice(new Point3D(bottomPoints[i].X, 0, bottomPoints[i].Y));
                    p1 = AddVertice(new Point3D(bottomPoints[i + 1].X, 0, bottomPoints[i + 1].Y));
                    Faces.Add(lowCentre);
                    Faces.Add(p0);
                    Faces.Add(p1);

                    p0 = AddVertice(new Point3D(bottomPoints[i].X, barrelHeight, bottomPoints[i].Y));
                    p1 = AddVertice(new Point3D(bottomPoints[i + 1].X, barrelHeight, bottomPoints[i + 1].Y));
                    Faces.Add(highCentre);
                    Faces.Add(p1);
                    Faces.Add(p0);

                    i += 3;
                }


            }
            else
            {
                // just close the top and bottom of the stave
                int p0;
                int p1;
                int i = 0;
                for (int staveIndex = 0; staveIndex < numberOfStaves; staveIndex++)
                {
                    int lowstartp = AddVertice(new Point3D(bottomPoints[i].X, 0, bottomPoints[i].Y));
                    int highstartp = AddVertice(new Point3D(bottomPoints[i].X, barrelHeight, bottomPoints[i].Y));
                    for (int j = 0; j < 10; j++)
                    {
                        p0 = AddVertice(new Point3D(bottomPoints[i + j + 1].X, 0, bottomPoints[i + j + 1].Y));
                        p1 = AddVertice(new Point3D(bottomPoints[i + j + 2].X, 0, bottomPoints[i + j + 2].Y));
                        Faces.Add(lowstartp);
                        Faces.Add(p0);
                        Faces.Add(p1);

                        p0 = AddVertice(new Point3D(bottomPoints[i + j + 1].X, barrelHeight, bottomPoints[i + j + 1].Y));
                        p1 = AddVertice(new Point3D(bottomPoints[i + j + 2].X, barrelHeight, bottomPoints[i + j + 2].Y));

                        Faces.Add(highstartp);
                        Faces.Add(p1);
                        Faces.Add(p0);
                    }

                    i += 13;
                }

                // build the inside of the staves
                i = 0;
                int p2;
                int p3;
                
                double dTheta = (2 * Math.PI) / numberOfStaves;
                double radius = topBottomRadius - shellThickness;
                List<Point> innerShellLine = new List<Point>();
                GetShellLine(innerShellLine, dTheta, radius);
                for (int index = 0; index < innerShellLine.Count; index++)
                {
                    int i2 = index + 1;
                    if (i2 == innerShellLine.Count)
                    {
                        i2 = 0;
                    }

                    p0 = AddVertice(innerShellLine[index].X, 0, innerShellLine[index].Y);
                    p1 = AddVertice(innerShellLine[index].X, barrelHeight, innerShellLine[index].Y);
                    p2 = AddVertice(innerShellLine[i2].X, barrelHeight, innerShellLine[i2].Y);
                    p3 = AddVertice(innerShellLine[i2].X, 0, innerShellLine[i2].Y);

                    Faces.Add(p0);
                    Faces.Add(p2);
                    Faces.Add(p1);

                    Faces.Add(p0);
                    Faces.Add(p3);
                    Faces.Add(p2);
                }

                radius = topBottomRadius - staveDepth;
                List<Point> outerShellLine = new List<Point>();
                GetShellLine(outerShellLine, dTheta, radius);
                for (int index = 0; index < innerShellLine.Count; index++)
                {
                    int i2 = index + 1;
                    if (i2 == innerShellLine.Count)
                    {
                        i2 = 0;
                    }

                    p0 = AddVertice(innerShellLine[index].X, 0, innerShellLine[index].Y);
                    p1 = AddVertice(outerShellLine[index].X, 0, outerShellLine[index].Y);
                    p2 = AddVertice(outerShellLine[i2].X, 0, outerShellLine[i2].Y);
                    p3 = AddVertice(innerShellLine[i2].X, 0, innerShellLine[i2].Y);

                    Faces.Add(p0);
                    Faces.Add(p1);
                    Faces.Add(p2);

                    Faces.Add(p0);
                    Faces.Add(p2);
                    Faces.Add(p3);

                    p0 = AddVertice(innerShellLine[index].X, barrelHeight, innerShellLine[index].Y);
                    p1 = AddVertice(outerShellLine[index].X, barrelHeight, outerShellLine[index].Y);
                    p2 = AddVertice(outerShellLine[i2].X, barrelHeight, outerShellLine[i2].Y);
                    p3 = AddVertice(innerShellLine[i2].X, barrelHeight, innerShellLine[i2].Y);

                    Faces.Add(p0);
                    Faces.Add(p2);
                    Faces.Add(p1);

                    Faces.Add(p0);
                    Faces.Add(p3);
                    Faces.Add(p2);
                }
            }

            // As generated, the origin of the object
            // will be in the middle of the base.
            // Move all the coordinates down so the origin is in the middle
            double dy = barrelHeight / 2.0;
            for ( int i = 0; i < Vertices.Count; i ++)
            {
                Vertices[i] = new Point3D(Vertices[i].X, Vertices[i].Y - dy, Vertices[i].Z);
            }
        }


        private void MakeSingleStave(int staveIndex, double[] bandRadius, List<Point> bottomPoints,  int verticalSteps)
        {
            double stepHeight = barrelHeight / (verticalSteps - 1);
            double y = 0;
            for (int step = 0; step < verticalSteps - 1; step++)
            {
                List<Point> lowerLine = new List<Point>();
                List<Point> upperLine = new List<Point>();
                double dtheta = (2 * Math.PI) / numberOfStaves;
                double theta = staveIndex * dtheta;
                GetStaveLine(lowerLine, theta, dtheta, bandRadius[step]);
                GetStaveLine(upperLine, theta, dtheta, bandRadius[step + 1]);
                if (step == 0)
                {
                    bottomPoints.AddRange(lowerLine);
                }
                for (int pi = 0; pi < lowerLine.Count - 1; pi++)
                {
                    int p0 = AddVertice(new Point3D(lowerLine[pi].X, y, lowerLine[pi].Y));
                    int p1 = AddVertice(new Point3D(upperLine[pi].X, y + stepHeight, upperLine[pi].Y));
                    int p2 = AddVertice(new Point3D(upperLine[pi + 1].X, y + stepHeight, upperLine[pi + 1].Y));
                    int p3 = AddVertice(new Point3D(lowerLine[pi + 1].X, y, lowerLine[pi + 1].Y));

                    Faces.Add(p0);
                    Faces.Add(p1);
                    Faces.Add(p2);

                    Faces.Add(p0);
                    Faces.Add(p2);
                    Faces.Add(p3);
                }
                y += stepHeight;
            }
        }

        private void GetStaveLine(List<Point> line, double theta, double dtheta, double radius)
        {
            double innerRadius = radius - staveDepth;
            Point p0 = CalcPoint(theta, innerRadius);
            line.Add(p0);

            for (double d = 0; d <= 0.9; d += 0.1)
            {
                Point p1 = CalcPoint(theta + d * dtheta, radius);
                line.Add(p1);
            }

            Point p2 = CalcPoint(theta + 0.9 * dtheta, innerRadius);
            line.Add(p2);
            Point p3 = CalcPoint(theta + dtheta, innerRadius);
            line.Add(p3);
        }

        private void GetShellLine(List<Point> line, double dtheta, double radius)
        {

            for (int staveIndex = 0; staveIndex < numberOfStaves; staveIndex++)
            {
                double theta = staveIndex * dtheta;
                
                for (double d = 0; d < 1.0; d += 0.1)
                {
                    Point p1 = CalcPoint(theta + d * dtheta, radius);
                    line.Add(p1);
                }
            }
        }

    }
}
