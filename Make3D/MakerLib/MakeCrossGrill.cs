using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class CrossGrillMaker : MakerBase
    {
        private const double EquityTolerance = 0.0000001;
        private double crossBeamWidth;
        private double edgeWidth;
        private double grilleHeight;
        private double grilleLength;
        private double grilleWidth;
        private bool makeEdge;
        private int numberOfCrossBeams;

        public CrossGrillMaker()
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
            if (numberOfCrossBeams == 1)
            {
                if (makeEdge)
                {
                    GenerateSingleWithEdge();
                }
                else
                {
                    GenerateSingleWithoutEdge();
                }
            }
            else
            if (numberOfCrossBeams == 2)
            {
                if (makeEdge)
                {
                    GenerateDoubleWithEdge();
                }
                else
                {
                    GenerateDoubleWithoutEdge();
                }
            }
            else
            if (numberOfCrossBeams >= 3)
            {
                if (makeEdge)
                {
                    GenerateMultiWithEdge();
                }
                else
                {
                    GenerateMultiWithoutEdge();
                }
            }
        }

        public Point? GetIntersectionPoint(Point l1p1, Point l1p2, Point l2p1, Point l2p2)
        {
            double A1 = l1p2.Y - l1p1.Y;
            double B1 = l1p1.X - l1p2.X;
            double C1 = A1 * l1p1.X + B1 * l1p1.Y;
            double A2 = l2p2.Y - l2p1.Y;
            double B2 = l2p1.X - l2p2.X;
            double C2 = A2 * l2p1.X + B2 * l2p1.Y;
            //lines are parallel
            double det = A1 * B2 - A2 * B1;
            if (IsEqual(det, 0d))
            {
                return null; //parallel lines
            }
            else
            {
                double x = (B2 * C1 - B1 * C2) / det;
                double y = (A1 * C2 - A2 * C1) / det;
                bool online1 = ((Math.Min(l1p1.X, l1p2.X) < x || IsEqual(Math.Min(l1p1.X, l1p2.X), x))
                    && (Math.Max(l1p1.X, l1p2.X) > x || IsEqual(Math.Max(l1p1.X, l1p2.X), x))
                    && (Math.Min(l1p1.Y, l1p2.Y) < y || IsEqual(Math.Min(l1p1.Y, l1p2.Y), y))
                    && (Math.Max(l1p1.Y, l1p2.Y) > y || IsEqual(Math.Max(l1p1.Y, l1p2.Y), y))
                    );
                bool online2 = ((Math.Min(l2p1.X, l2p2.X) < x || IsEqual(Math.Min(l2p1.X, l2p2.X), x))
                    && (Math.Max(l2p1.X, l2p2.X) > x || IsEqual(Math.Max(l2p1.X, l2p2.X), x))
                    && (Math.Min(l2p1.Y, l2p2.Y) < y || IsEqual(Math.Min(l2p1.Y, l2p2.Y), y))
                    && (Math.Max(l2p1.Y, l2p2.Y) > y || IsEqual(Math.Max(l2p1.Y, l2p2.Y), y))
                    );
                if (online1 && online2)
                    return new Point(x, y);
            }
            return null; //intersection is at out of at least one segment.
        }

        public void SetValues(double grilleLength,
                                      double grilleHeight,
                              double grilleWidth,
                              int numberOfCrossBeams,
                              double crossBeamWidth,
                              bool showEdge,
                              double edgeWidth)
        {
            this.grilleLength = grilleLength;
            this.grilleHeight = grilleHeight;
            this.grilleWidth = grilleWidth;
            this.numberOfCrossBeams = numberOfCrossBeams;
            this.crossBeamWidth = crossBeamWidth;
            this.makeEdge = showEdge;
            this.edgeWidth = edgeWidth;
        }

        private void AddSpsFace(int[] sps, int v1, int v2, int v3, bool invert)
        {
            if (invert)
            {
                AddFace(sps[v1], sps[v3], sps[v2]);
            }
            else
            {
                AddFace(sps[v1], sps[v2], sps[v3]);
            }
        }

        private void CloseSide(Point p1, Point p2, bool invert = false)
        {
            int v0 = AddVertice(p1.X, p1.Y, 0);
            int v1 = AddVertice(p2.X, p2.Y, 0);
            int v2 = AddVertice(p1.X, p1.Y, grilleWidth);
            int v3 = AddVertice(p2.X, p2.Y, grilleWidth);
            if (invert)
            {
                AddFace(v0, v1, v2);
                AddFace(v1, v3, v2);
            }
            else
            {
                AddFace(v0, v2, v1);
                AddFace(v1, v2, v3);
            }
        }

        private void GenerateDoubleWithEdge()
        {
            GenerateLeftWithEdge(grilleLength / 2);
            GenerateRightWithEdge(grilleLength / 2, grilleLength / 2);
        }

        private void GenerateDoubleWithoutEdge()
        {
        }

        private void GenerateLeftWithEdge(double length)
        {
            Point[] sps = new Point[17];
            double hb = crossBeamWidth / 2;
            double hew = edgeWidth - 2;
            sps[0] = new Point(0, 0);
            sps[1] = new Point(0, grilleHeight);
            Point topRight = new Point(length, grilleHeight);
            Point bottomRight = new Point(length, 0);
            sps[2] = topRight;
            sps[3] = bottomRight;
            Point op1 = new Point(length, grilleHeight - edgeWidth);
            Point op2 = new Point(length, edgeWidth);

            Line2D offsetLine1 = GetOffsetLine(sps[0], op1, hb);
            Line2D offsetLine2 = GetOffsetLine(sps[0], op1, -hb);
            Line2D offsetLine3 = GetOffsetLine(sps[1], op2, hb);
            Line2D offsetLine4 = GetOffsetLine(sps[1], op2, -hb);
            Logger.Log($"{offsetLine1.start.X},{offsetLine1.start.Y} to {offsetLine1.end.X},{offsetLine1.end.Y}");
            Logger.Log($"{offsetLine2.start.X},{offsetLine2.start.Y} to {offsetLine2.end.X},{offsetLine2.end.Y}");
            Logger.Log($"{offsetLine3.start.X},{offsetLine3.start.Y} to {offsetLine3.end.X},{offsetLine3.end.Y}");
            Logger.Log($"{offsetLine4.start.X},{offsetLine4.start.Y} to {offsetLine4.end.X},{offsetLine4.end.Y}");

            double x = GetX(offsetLine2, edgeWidth);
            sps[4] = new Point(x, edgeWidth);
            double y = GetY(offsetLine1, edgeWidth);
            sps[5] = new Point(edgeWidth, y);

            y = GetY(offsetLine4, edgeWidth);
            sps[6] = new Point(edgeWidth, y);
            x = GetX(offsetLine3, grilleHeight - edgeWidth);
            if (x < edgeWidth)
            {
                // extra triangle need top left
            }
            sps[7] = new Point(x, grilleHeight - edgeWidth);

            x = GetX(offsetLine1, grilleHeight - edgeWidth);
            if (x > length - edgeWidth)
            {
                // extra triangle need
            }
            sps[8] = new Point(x, grilleHeight - edgeWidth);
            y = GetY(offsetLine2, length);
            sps[9] = new Point(length, y);

            y = GetY(offsetLine3, length);
            sps[10] = new Point(length, y);
            x = GetX(offsetLine4, edgeWidth);
            sps[11] = new Point(x, edgeWidth);

            sps[12] = Intercept(offsetLine2, offsetLine4);
            sps[13] = Intercept(offsetLine1, offsetLine4);
            sps[14] = Intercept(offsetLine1, offsetLine3);
            sps[15] = Intercept(offsetLine2, offsetLine3);

            MakeSurfaceForSingleWithEdge(sps, 0);
            MakeSurfaceForSingleWithEdge(sps, grilleWidth);

            CloseSide(sps[0], sps[1]);
            CloseSide(sps[1], sps[2]);

            CloseSide(sps[3], sps[0]);

            // left hole
            CloseSide(sps[6], sps[5]);
            CloseSide(sps[13], sps[6]);
            CloseSide(sps[5], sps[13]);

            // Top Hole
            CloseSide(sps[8], sps[7]);
            CloseSide(sps[14], sps[8]);
            CloseSide(sps[7], sps[14]);

            // right
            CloseSide(sps[15], sps[10]);
            CloseSide(sps[9], sps[15]);

            // bottom
            CloseSide(sps[4], sps[11]);
            CloseSide(sps[12], sps[4]);
            CloseSide(sps[11], sps[12]);
        }

        private void GenerateMiddleWithEdge(double position, double length)
        {
            Point[] sps = new Point[16];
            double hb = crossBeamWidth / 2;
            double hew = edgeWidth - 2;
            sps[0] = new Point(0, 0);
            sps[1] = new Point(0, grilleHeight);
            Point topRight = new Point(length, grilleHeight);
            Point bottomRight = new Point(length, 0);
            sps[2] = topRight;
            sps[3] = bottomRight;
            Point v1 = new Point(0, edgeWidth);
            Point v2 = new Point(0, grilleHeight - edgeWidth);

            Point op1 = new Point(length, grilleHeight - edgeWidth);
            Point op2 = new Point(length, edgeWidth);

            Line2D offsetLine1 = GetOffsetLine(v1, op1, hb);
            Line2D offsetLine2 = GetOffsetLine(v1, op1, -hb);
            Line2D offsetLine3 = GetOffsetLine(v2, op2, hb);
            Line2D offsetLine4 = GetOffsetLine(v2, op2, -hb);

            double x = GetX(offsetLine2, edgeWidth);
            sps[4] = new Point(x, edgeWidth);
            double y = GetY(offsetLine1, 0);
            sps[5] = new Point(0, y);

            y = GetY(offsetLine4, 0);
            sps[6] = new Point(0, y);
            x = GetX(offsetLine3, grilleHeight - edgeWidth);
            if (x < edgeWidth)
            {
                // extra triangle need top left
            }
            sps[7] = new Point(x, grilleHeight - edgeWidth);

            x = GetX(offsetLine1, grilleHeight - edgeWidth);
            if (x > length - edgeWidth)
            {
                // extra triangle need
            }
            sps[8] = new Point(x, grilleHeight - edgeWidth);
            y = GetY(offsetLine2, length);
            sps[9] = new Point(length, y);

            y = GetY(offsetLine3, length);
            sps[10] = new Point(length, y);
            x = GetX(offsetLine4, edgeWidth);
            sps[11] = new Point(x, edgeWidth);

            sps[12] = Intercept(offsetLine2, offsetLine4);
            sps[13] = Intercept(offsetLine1, offsetLine4);
            sps[14] = Intercept(offsetLine1, offsetLine3);
            sps[15] = Intercept(offsetLine2, offsetLine3);
            for (int i = 0; i < sps.GetLength(0); i++)
            {
                sps[i].X = sps[i].X + position;
            }
            MakeSurfaceForSingleWithEdge(sps, 0);
            MakeSurfaceForSingleWithEdge(sps, grilleWidth);

            CloseSide(sps[1], sps[2]);
            CloseSide(sps[3], sps[0]);

            // left hole
            CloseSide(sps[13], sps[6]);
            CloseSide(sps[5], sps[13]);

            // Top Hole
            CloseSide(sps[8], sps[7]);
            CloseSide(sps[14], sps[8]);
            CloseSide(sps[7], sps[14]);

            // right
            CloseSide(sps[15], sps[10]);
            CloseSide(sps[9], sps[15]);

            // bottom
            CloseSide(sps[4], sps[11]);
            CloseSide(sps[12], sps[4]);
            CloseSide(sps[11], sps[12]);
        }

        private void GenerateMultiWithEdge()
        {
            int numOfMiddles = numberOfCrossBeams - 2;
            if (numOfMiddles > 0)
            {
                double lenWithoutEdges = (grilleLength - (2 * edgeWidth));
                double crossLen = lenWithoutEdges / numberOfCrossBeams;
                double sideLen = crossLen + edgeWidth;
                GenerateLeftWithEdge(sideLen);
                double position = sideLen + (numOfMiddles * crossLen);
                GenerateRightWithEdge(position, sideLen);

                position = sideLen;
                for (int i = 0; i < numOfMiddles; i++)
                {
                    GenerateMiddleWithEdge(position, crossLen);
                    position += crossLen;
                }
            }
        }

        private void GenerateMultiWithoutEdge()
        {
        }

        private void GenerateRightWithEdge(double position, double length)
        {
            // Calculate the points as if we are making a left
            Point[] sps = new Point[17];
            double hb = crossBeamWidth / 2;
            double hew = edgeWidth - 2;
            sps[0] = new Point(0, 0);
            sps[1] = new Point(0, grilleHeight);
            Point topRight = new Point(length, grilleHeight);
            Point bottomRight = new Point(length, 0);
            sps[2] = topRight;
            sps[3] = bottomRight;
            Point op1 = new Point(length, grilleHeight - edgeWidth);
            Point op2 = new Point(length, edgeWidth);

            Line2D offsetLine1 = GetOffsetLine(sps[0], op1, hb);
            Line2D offsetLine2 = GetOffsetLine(sps[0], op1, -hb);
            Line2D offsetLine3 = GetOffsetLine(sps[1], op2, hb);
            Line2D offsetLine4 = GetOffsetLine(sps[1], op2, -hb);

            double x = GetX(offsetLine2, edgeWidth);
            sps[4] = new Point(x, edgeWidth);
            double y = GetY(offsetLine1, edgeWidth);
            sps[5] = new Point(edgeWidth, y);

            y = GetY(offsetLine4, edgeWidth);
            sps[6] = new Point(edgeWidth, y);
            x = GetX(offsetLine3, grilleHeight - edgeWidth);
            if (x < edgeWidth)
            {
                // extra triangle need top left
            }
            sps[7] = new Point(x, grilleHeight - edgeWidth);

            x = GetX(offsetLine1, grilleHeight - edgeWidth);
            if (x > length - edgeWidth)
            {
                // extra triangle need
            }
            sps[8] = new Point(x, grilleHeight - edgeWidth);
            y = GetY(offsetLine2, length);
            sps[9] = new Point(length, y);

            y = GetY(offsetLine3, length);
            sps[10] = new Point(length, y);
            x = GetX(offsetLine4, edgeWidth);
            sps[11] = new Point(x, edgeWidth);

            sps[12] = Intercept(offsetLine2, offsetLine4);
            sps[13] = Intercept(offsetLine1, offsetLine4);
            sps[14] = Intercept(offsetLine1, offsetLine3);
            sps[15] = Intercept(offsetLine2, offsetLine3);

            // flip the x's and offset
            for (int i = 0; i < sps.GetLength(0); i++)
            {
                sps[i].X = (length - sps[i].X) + position;
            }

            MakeSurfaceForSingleWithEdge(sps, 0, true);
            MakeSurfaceForSingleWithEdge(sps, grilleWidth, true);

            CloseSide(sps[0], sps[1], true);
            CloseSide(sps[1], sps[2], true);
            CloseSide(sps[3], sps[0], true);

            // left hole
            CloseSide(sps[6], sps[5], true);
            CloseSide(sps[13], sps[6], true);
            CloseSide(sps[5], sps[13], true);

            // Top Hole
            CloseSide(sps[8], sps[7], true);
            CloseSide(sps[14], sps[8], true);
            CloseSide(sps[7], sps[14], true);

            // right
            CloseSide(sps[15], sps[10], true);
            CloseSide(sps[9], sps[15], true);

            // bottom
            CloseSide(sps[4], sps[11], true);
            CloseSide(sps[12], sps[4], true);
            CloseSide(sps[11], sps[12], true);
        }

        private void GenerateSingleWithEdge()
        {
            Point[] sps = new Point[16];
            double hb = crossBeamWidth / 2;
            sps[0] = new Point(0, 0);
            sps[1] = new Point(0, grilleHeight);
            sps[2] = new Point(grilleLength, grilleHeight);
            sps[3] = new Point(grilleLength, 0);

            Line2D offsetLine1 = GetOffsetLine(sps[0], sps[2], hb);
            Line2D offsetLine2 = GetOffsetLine(sps[0], sps[2], -hb);
            Line2D offsetLine3 = GetOffsetLine(sps[1], sps[3], hb);
            Line2D offsetLine4 = GetOffsetLine(sps[1], sps[3], -hb);
            Logger.Log($"{offsetLine1.start.X},{offsetLine1.start.Y} to {offsetLine1.end.X},{offsetLine1.end.Y}");
            Logger.Log($"{offsetLine2.start.X},{offsetLine2.start.Y} to {offsetLine2.end.X},{offsetLine2.end.Y}");
            Logger.Log($"{offsetLine3.start.X},{offsetLine3.start.Y} to {offsetLine3.end.X},{offsetLine3.end.Y}");
            Logger.Log($"{offsetLine4.start.X},{offsetLine4.start.Y} to {offsetLine4.end.X},{offsetLine4.end.Y}");

            double x = GetX(offsetLine2, edgeWidth);
            sps[4] = new Point(x, edgeWidth);
            double y = GetY(offsetLine1, edgeWidth);
            sps[5] = new Point(edgeWidth, y);

            y = GetY(offsetLine4, edgeWidth);
            sps[6] = new Point(edgeWidth, y);
            x = GetX(offsetLine3, grilleHeight - edgeWidth);
            if (x < edgeWidth)
            {
                // extra triangle need top left
            }
            sps[7] = new Point(x, grilleHeight - edgeWidth);

            x = GetX(offsetLine1, grilleHeight - edgeWidth);
            if (x > grilleLength - edgeWidth)
            {
                // extra triangle need
            }
            sps[8] = new Point(x, grilleHeight - edgeWidth);
            y = GetY(offsetLine2, grilleLength - edgeWidth);
            sps[9] = new Point(grilleLength - edgeWidth, y);

            y = GetY(offsetLine3, grilleLength - edgeWidth);
            sps[10] = new Point(grilleLength - edgeWidth, y);
            x = GetX(offsetLine4, edgeWidth);
            sps[11] = new Point(x, edgeWidth);

            sps[12] = Intercept(offsetLine2, offsetLine4);
            sps[13] = Intercept(offsetLine1, offsetLine4);
            sps[14] = Intercept(offsetLine1, offsetLine3);
            sps[15] = Intercept(offsetLine2, offsetLine3);

            MakeSurfaceForSingleWithEdge(sps, 0);
            MakeSurfaceForSingleWithEdge(sps, grilleWidth);
            CloseSide(sps[0], sps[1]);
            CloseSide(sps[1], sps[2]);
            CloseSide(sps[2], sps[3]);
            CloseSide(sps[3], sps[0]);

            // left hole
            CloseSide(sps[6], sps[5]);
            CloseSide(sps[13], sps[6]);
            CloseSide(sps[5], sps[13]);

            // Top Hole
            CloseSide(sps[8], sps[7]);
            CloseSide(sps[14], sps[8]);
            CloseSide(sps[7], sps[14]);

            // right
            CloseSide(sps[10], sps[9]);
            CloseSide(sps[15], sps[10]);
            CloseSide(sps[9], sps[15]);

            // bottom
            CloseSide(sps[4], sps[11]);
            CloseSide(sps[12], sps[4]);
            CloseSide(sps[11], sps[12]);
        }

        private void GenerateSingleWithoutEdge()
        {
            Point[] sps = new Point[16];
            double hb = crossBeamWidth / 2;
            sps[0] = new Point(0, 0);
            sps[1] = new Point(0, grilleHeight);
            sps[2] = new Point(grilleLength, grilleHeight);
            sps[3] = new Point(grilleLength, 0);

            Line2D offsetLine1 = GetOffsetLine(sps[0], sps[2], hb);
            Line2D offsetLine2 = GetOffsetLine(sps[0], sps[2], -hb);
            Line2D offsetLine3 = GetOffsetLine(sps[1], sps[3], hb);
            Line2D offsetLine4 = GetOffsetLine(sps[1], sps[3], -hb);

            double x = GetX(offsetLine2, edgeWidth);
            sps[4] = new Point(x, edgeWidth);
            double y = GetY(offsetLine1, edgeWidth);
            sps[5] = new Point(edgeWidth, y);

            y = GetY(offsetLine4, edgeWidth);
            sps[6] = new Point(edgeWidth, y);
            x = GetX(offsetLine3, grilleHeight - edgeWidth);
            if (x < edgeWidth)
            {
                // extra triangle need top left
            }
            sps[7] = new Point(x, grilleHeight - edgeWidth);

            x = GetX(offsetLine1, grilleHeight - edgeWidth);
            if (x > grilleLength - edgeWidth)
            {
                // extra triangle need
            }
            sps[8] = new Point(x, grilleHeight - edgeWidth);
            y = GetY(offsetLine2, grilleLength - edgeWidth);
            sps[9] = new Point(grilleLength - edgeWidth, y);

            y = GetY(offsetLine3, grilleLength - edgeWidth);
            sps[10] = new Point(grilleLength - edgeWidth, y);
            x = GetX(offsetLine4, edgeWidth);
            sps[11] = new Point(x, edgeWidth);

            sps[12] = Intercept(offsetLine2, offsetLine4);
            sps[13] = Intercept(offsetLine1, offsetLine4);
            sps[14] = Intercept(offsetLine1, offsetLine3);
            sps[15] = Intercept(offsetLine2, offsetLine3);

            MakeSurfaceForSingleWithoutEdge(sps, 0);
            MakeSurfaceForSingleWithoutEdge(sps, grilleWidth);
            CloseSide(sps[0], sps[1]);
            CloseSide(sps[1], sps[2]);
            CloseSide(sps[2], sps[3]);
            CloseSide(sps[3], sps[0]);

            // left hole
            CloseSide(sps[6], sps[5]);
            CloseSide(sps[13], sps[6]);
            CloseSide(sps[5], sps[13]);

            // Top Hole
            CloseSide(sps[8], sps[7]);
            CloseSide(sps[14], sps[8]);
            CloseSide(sps[7], sps[14]);

            // right
            CloseSide(sps[10], sps[9]);
            CloseSide(sps[15], sps[10]);
            CloseSide(sps[9], sps[15]);

            // bottom
            CloseSide(sps[4], sps[11]);
            CloseSide(sps[12], sps[4]);
            CloseSide(sps[11], sps[12]);
        }

        private Line2D GetOffsetLine(Point sp, Point se, double offset)
        {
            double dx = se.X - sp.X;
            double dy = se.Y - sp.Y;
            Point p0 = new Point(0, 0);
            Point p1 = new Point(0, 0);

            // is the line horizontal
            if (dy != 0)
            {
                double len = Math.Sqrt((dx * dx) + (dy * dy));

                double mag = offset / len;
                double dx2 = -dy * mag;
                double dy2 = dx * mag;

                p0.X = sp.X + dx2;
                p0.Y = sp.Y + dy2;

                p1.X = se.X + dx2;
                p1.Y = se.Y + dy2;
            }
            else
            {
                // the segment is horizontal
                if (sp.X < se.X)
                {
                    p0.X = sp.X;
                    p0.Y = sp.Y + offset;

                    p1.X = se.X;
                    p1.Y = se.Y + offset;
                }
                else
                {
                    p0.X = sp.X;
                    p0.Y = sp.Y - offset;

                    p1.X = se.X;
                    p1.Y = se.Y - offset;
                }
            }
            Line2D res;
            res.start = p0;
            res.end = p1;
            return res;
        }

        private double GetX(Line2D l, double py)
        {
            double t = (py - l.start.Y) / (l.end.Y - l.start.Y);
            double x = l.start.X + (t * (l.end.X - l.start.X));
            return x;
        }

        private double GetY(Line2D l, double px)
        {
            double t = (px - l.start.X) / (l.end.X - l.start.X);
            double y = l.start.Y + (t * (l.end.Y - l.start.Y));
            return y;
        }

        private Point Intercept(Line2D l1, Line2D l2)
        {
            Point? p = GetIntersectionPoint(l1.start, l1.end, l2.start, l2.end);
            if (p != null)
            {
                return new Point((double)p?.X, (double)p?.Y);
            }
            else
            {
                return new Point(double.NaN, double.NaN);
            }
        }

        private bool IsEqual(double d1, double d2)
        {
            return Math.Abs(d1 - d2) <= EquityTolerance;
        }

        private void MakeSurfaceForSingleWithEdge(Point[] sps, double level, bool flip = false)
        {
            int[] v = new int[sps.GetLength(0)];
            for (int i = 0; i < sps.GetLength(0); i++)
            {
                Logger.Log($"i={i} {sps[i].X},{sps[i].Y}");
                v[i] = AddVertice(sps[i].X, sps[i].Y, level);
            }
            bool invert = false;
            if (level != 0)
            {
                invert = true;
            }
            if (flip)
            {
                invert = !invert;
            }
            AddSpsFace(v, 0, 1, 6, invert);

            AddSpsFace(v, 0, 6, 5, invert);

            AddSpsFace(v, 1, 2, 8, invert);
            AddSpsFace(v, 1, 8, 7, invert);

            AddSpsFace(v, 11, 12, 15, invert);
            AddSpsFace(v, 11, 15, 10, invert);

            AddSpsFace(v, 2, 3, 10, invert);
            AddSpsFace(v, 2, 10, 9, invert);

            AddSpsFace(v, 3, 0, 4, invert);

            AddSpsFace(v, 3, 4, 11, invert);

            AddSpsFace(v, 0, 5, 4, invert);

            AddSpsFace(v, 1, 7, 6, invert);

            AddSpsFace(v, 2, 9, 8, invert);

            AddSpsFace(v, 3, 11, 10, invert);

            // cross
            AddSpsFace(v, 5, 13, 12, invert);
            AddSpsFace(v, 5, 12, 4, invert);

            AddSpsFace(v, 6, 7, 14, invert);
            AddSpsFace(v, 6, 14, 13, invert);

            AddSpsFace(v, 8, 15, 14, invert);
            AddSpsFace(v, 8, 9, 15, invert);

            AddSpsFace(v, 12, 13, 14, invert);
            AddSpsFace(v, 12, 14, 15, invert);
        }

        private void MakeSurfaceForSingleWithoutEdge(Point[] sps, double level)
        {
            int[] v = new int[sps.GetLength(0)];
            for (int i = 0; i < sps.GetLength(0); i++)
            {
                Logger.Log($"i={i} {sps[i].X},{sps[i].Y}");
                v[i] = AddVertice(sps[i].X, sps[i].Y, level);
            }
            bool invert = false;
            if (level != 0)
            {
                invert = true;
            }
            AddSpsFace(v, 0, 1, 6, invert);

            AddSpsFace(v, 0, 6, 5, invert);

            AddSpsFace(v, 1, 2, 8, invert);
            AddSpsFace(v, 1, 8, 7, invert);

            AddSpsFace(v, 11, 12, 15, invert);
            AddSpsFace(v, 11, 15, 10, invert);

            AddSpsFace(v, 2, 3, 10, invert);
            AddSpsFace(v, 2, 10, 9, invert);

            AddSpsFace(v, 3, 0, 4, invert);

            AddSpsFace(v, 3, 4, 11, invert);

            AddSpsFace(v, 0, 5, 4, invert);

            AddSpsFace(v, 1, 7, 6, invert);

            AddSpsFace(v, 2, 9, 8, invert);

            AddSpsFace(v, 3, 11, 10, invert);

            // cross
            AddSpsFace(v, 5, 13, 12, invert);
            AddSpsFace(v, 5, 12, 4, invert);

            AddSpsFace(v, 6, 7, 14, invert);
            AddSpsFace(v, 6, 14, 13, invert);

            AddSpsFace(v, 8, 15, 14, invert);
            AddSpsFace(v, 8, 9, 15, invert);

            AddSpsFace(v, 12, 13, 14, invert);
            AddSpsFace(v, 12, 14, 15, invert);
        }

        private void SetLimits()
        {
            paramLimits.AddLimit("GrilleLength", 1, 200);
            paramLimits.AddLimit("GrilleHeight", 1, 200);
            paramLimits.AddLimit("GrilleWidth", 1, 200);
            paramLimits.AddLimit("NumberOfCrossBeams", 1, 20);
            paramLimits.AddLimit("CrossBeamWidth", 1, 100);
            paramLimits.AddLimit("EdgeWidth", 1, 100);
        }

        internal struct Line2D
        {
            internal Point end;
            internal Point start;
        }
    }
}