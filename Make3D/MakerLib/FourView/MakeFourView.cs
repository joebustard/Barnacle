using Barnacle.LineLib;
using Barnacle.Object3DLib;
using MakerLib.FourView;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using static Barnacle.LineLib.FlexiPath;

namespace MakerLib
{
    public class FourViewMaker : MakerBase
    {
        private int distalSteps;
        private string frontView;
        private int horizontalSteps;
        private string leftView;
        private string rightView;
        private string topView;

        public FourViewMaker()
        {
            paramLimits = new ParamLimits();
            SetLimits();
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            FlexiPath leftflexiPath = new FlexiPath();
            FlexiPath rightflexiPath = new FlexiPath();
            FlexiPath frontflexiPath = new FlexiPath();
            FlexiPath topflexiPath = new FlexiPath();

            frontflexiPath.FromString(frontView);
            List<System.Windows.Point> frontPnts = frontflexiPath.DisplayPoints();
            FlipY(frontPnts);
            frontflexiPath.CalculatePathBounds();
            Bounds2D frontBounds = new Bounds2D(frontPnts);

            leftflexiPath.FromString(leftView);
            List<System.Windows.Point> leftPnts = leftflexiPath.DisplayPoints();

            rightflexiPath.FromString(rightView);
            List<System.Windows.Point> rightPnts = rightflexiPath.DisplayPoints();

            topflexiPath.FromString(topView);
            topflexiPath.CalculatePathBounds();
            List<System.Windows.Point> topPnts = topflexiPath.DisplayPoints();
            FlipY(topPnts);
            Bounds2D topBounds = new Bounds2D(topPnts);

            // calculate final size object should be
            double aspectRatio = topBounds.Width() / frontBounds.Width();
            double finalLength = frontBounds.Width();
            double finalHeight = frontBounds.Height();
            double finalWidth = topBounds.Height() / aspectRatio;

            // calculate where each pseudo rib will be and what it will be scaled to
            double[] xPositions = new double[horizontalSteps];
            Dimension[] topDims = new Dimension[horizontalSteps];
            Dimension[] frontDims = new Dimension[horizontalSteps];
            double dt = 1.0 / (double)horizontalSteps;
            double dx = frontBounds.Width() / (horizontalSteps + 1);
            for (int ribIndex = 0; ribIndex < horizontalSteps; ribIndex++)
            {
                double x = ribIndex * dx;
                double t = ribIndex * dt;
                xPositions[ribIndex] = x;

                topDims[ribIndex] = topflexiPath.GetUpperAndLowerPoints(t);

                frontDims[ribIndex] = frontflexiPath.GetUpperAndLowerPoints(t);
            }

            List<System.Windows.Point>[] profiles = new List<System.Windows.Point>[horizontalSteps];
            // get the leftmost profile
            profiles[0] = Rib.GenerateProfilePoints(distalSteps, leftPnts);

            // get the rightmost profile
            profiles[horizontalSteps - 1] = Rib.GenerateProfilePoints(distalSteps, rightPnts);

            // interpolate the intermediate ribs

            for (int ribIndex = 1; ribIndex < horizontalSteps - 1; ribIndex++)
            {
                double t = ribIndex * dt;
                profiles[ribIndex] = new List<System.Windows.Point>();
                for (int j = 0; j < profiles[0].Count; j++)
                {
                    System.Windows.Point np = new System.Windows.Point();
                    double xdiff = profiles[horizontalSteps - 1][j].X - profiles[0][j].X;
                    double ydiff = profiles[horizontalSteps - 1][j].Y - profiles[0][j].Y;

                    np.X = profiles[0][j].X + t * xdiff;
                    np.Y = profiles[0][j].Y + t * ydiff;

                    profiles[ribIndex].Add(np);
                }
            }

            // scale each of these profiles by the dimensions we measured on the side and top views
            for (int ribIndex = 0; ribIndex < horizontalSteps; ribIndex++)
            {
                ScaleProfile(profiles[ribIndex], topDims[ribIndex], frontDims[ribIndex]);
            }

            // now convert the ribs into 3d points;
            for (int ribIndex = 0; ribIndex < horizontalSteps - 1; ribIndex++)
            {
                for (int j = 0; j < profiles[0].Count; j++)
                {
                    int k = j + 1;
                    if (k == profiles[0].Count)
                    {
                        k = 0;
                    }
                    System.Windows.Point p1_2D = profiles[ribIndex][j];
                    System.Windows.Point p2_2D = profiles[ribIndex + 1][j];
                    System.Windows.Point p3_2D = profiles[ribIndex][k];
                    System.Windows.Point p4_2D = profiles[ribIndex + 1][k];

                    int V1 = AddVertice(new Point3D(xPositions[ribIndex], p1_2D.Y, p1_2D.X));
                    int V2 = AddVertice(new Point3D(xPositions[ribIndex + 1], p2_2D.Y, p2_2D.X));
                    int V3 = AddVertice(new Point3D(xPositions[ribIndex], p3_2D.Y, p3_2D.X));
                    int V4 = AddVertice(new Point3D(xPositions[ribIndex + 1], p4_2D.Y, p4_2D.X));

                    Faces.Add(V1);
                    Faces.Add(V3);
                    Faces.Add(V2);

                    Faces.Add(V3);
                    Faces.Add(V4);
                    Faces.Add(V2);
                }
            }
            // close left edge
            TriangulateSide(profiles[0].ToArray(), xPositions[0], false);

            TriangulateSide(profiles[horizontalSteps - 1].ToArray(), xPositions[horizontalSteps - 1], true);
        }

        public void SetValues(string frontView,
                              string leftView,
                              string rightView,
                              string topView,
                              int horizontalSteps,
                              int distalSteps)
        {
            this.frontView = frontView;
            this.leftView = leftView;
            this.rightView = rightView;
            this.topView = topView;
            this.horizontalSteps = horizontalSteps;
            this.distalSteps = distalSteps;
        }

        private void FlipY(List<System.Windows.Point> pnts)
        {
            double maxY = double.MinValue;
            List<System.Windows.Point> tmp = new List<System.Windows.Point>();
            foreach (System.Windows.Point p in pnts)
            {
                maxY = Math.Max(maxY, p.Y);
            }

            for (int i = 0; i < pnts.Count; i++)
            {
                System.Windows.Point p = pnts[i];
                tmp.Add(new System.Windows.Point(p.X, maxY - p.Y));
            }
            pnts.Clear();
            for (int i = 0; i < tmp.Count; i++)
            {
                pnts.Add(tmp[i]);
            }
        }

        private void ScaleProfile(List<System.Windows.Point> points, Dimension topDim, Dimension sideDim)
        {
            double td = topDim.Upper - topDim.Lower;
            double sd = sideDim.Upper - sideDim.Lower;
            for (int i = 0; i < points.Count; i++)
            {
                System.Windows.Point p = points[i];
                p.X = topDim.Lower + (p.X * td);
                p.Y = -(sideDim.Lower + (p.Y * sd));
                points[i] = p;
            }
        }

        private void SetLimits()
        {
            paramLimits.AddLimit("HorizontalSteps", 10, 200);
            paramLimits.AddLimit("DistalSteps", 10, 200);
        }
    }
}