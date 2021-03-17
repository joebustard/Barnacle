using System;
using System.Collections.Generic;
using System.Drawing;

namespace Make3D.Dialogs
{
    internal class ImageEdge
    {
        private double brx = double.MinValue;
        private double bry = double.MinValue;
        private List<PointF> edgePoints;
        private double tlx = double.MaxValue;
        private double tly = double.MaxValue;

        internal ImageEdge()
        {
            edgePoints = new List<PointF>();
            Length = 0;
        }

        public int BackEnd { get; set; }
        public int BackStart { get; set; }

        public List<PointF> EdgePoints
        {
            get { return edgePoints; }
        }

        public int FrontEnd { get; set; }
        public int FrontStart { get; set; }
        public double Length { get; set; }

        public double MiddleX
        {
            get { return tlx + (brx - tlx) / 2; }
        }

        public double MiddleY
        {
            get { return tly + (bry - tly) / 2; }
        }

        public int WholeEnd { get; set; }
        public int WholeStart { get; set; }

        public void Clear()
        {
            edgePoints.Clear();
            tlx = double.MaxValue;
            tly = double.MaxValue;
            brx = double.MinValue;
            bry = double.MinValue;
            BackStart = -1;
            BackEnd = -1;
            FrontStart = -1;
            FrontEnd = -1;
            WholeStart = -1;
            WholeEnd = -1;
        }

        internal void Add(PointF p)
        {
            if (!PointExists(p.X, p.Y))
            {
                AdjustBounds(p.X, p.Y);
                edgePoints.Add(p);
            }
        }

        internal void Add(double x, double y)
        {
            if (!PointExists(x, y))
            {
                AdjustBounds(x, y);
                edgePoints.Add(new PointF((float)x, (float)y));
            }
        }

        internal void Analyse()
        {
            // do the first and last points end on the same y but with an x gap e.g.
            //               first---------last
            //               .                  .
            //              .                      .
            // if so , this will cause big problems if only the front or back
            // of the model is wanted so put dummy start and ends very close together
            // e.g.             .   f l  .
            //                 .          .
            //                .            .
            int l = edgePoints.Count - 1;
            if (edgePoints[0].Y == edgePoints[l].Y)
            {
                if (edgePoints[0].X < edgePoints[l].X)
                {
                    double xm = edgePoints[0].X + (edgePoints[l].X - edgePoints[0].X) / 2;
                    edgePoints.Insert(0, new PointF(((float)xm - 0.000001F), edgePoints[0].Y));
                    edgePoints.Add(new PointF(((float)xm + 0.000001F), edgePoints[0].Y));
                }
            }
            else
            {
                // This can only mean that the top "line" of the edge is a single pixel.
                // add a sneaky one to its right
                double xm = edgePoints[0].X;
                edgePoints.Add(new PointF(((float)xm + 0.000001F), edgePoints[0].Y));
            }

            // find the two bottom points
            int bl = -1;
            int br = -1;
            for (int i = 0; i < edgePoints.Count - 1; i++)
            {
                if ((edgePoints[i].Y == bry))
                {
                    bl = i;
                    if (edgePoints[i + 1].Y == bry)
                    {
                        br = i + 1;
                        break;
                    }
                }
            }

            if (bl != -1)
            {
                if (br != -1)
                {
                    // bottom is a line, split it at a mid point with two new dummies
                    double xm = edgePoints[bl].X + (edgePoints[br].X - edgePoints[bl].X) / 2;
                    edgePoints.Insert(br, new PointF(((float)xm + 0.000001F), edgePoints[br].Y));
                    edgePoints.Insert(bl + 1, new PointF(((float)xm - 0.000001F), edgePoints[br].Y));
                }
                else
                {
                    // bottom is a single pixel
                    // append another one just to its right
                    double xm = edgePoints[bl].X;
                    edgePoints.Insert(bl + 1, new PointF(((float)xm + 0.00001F), edgePoints[bl].Y));
                    br = bl + 1;
                }
            }
            for (int i = 0; i < edgePoints.Count; i++)
            {
                System.Diagnostics.Debug.WriteLine($"{i}, {edgePoints[i].X}, {edgePoints[i].Y}");
            }

            WholeStart = 0;
            WholeEnd = edgePoints.Count;

            BackStart = 0;
            BackEnd = bl;

            FrontStart = br;
            FrontEnd = WholeEnd;
            System.Diagnostics.Debug.WriteLine($" WholeStart {WholeStart}, WholeEnd {WholeEnd}");
            System.Diagnostics.Debug.WriteLine($" BackStart {BackStart}, BackEnd {BackEnd}");
            System.Diagnostics.Debug.WriteLine($" FrontStart {FrontStart}, FrontEnd {FrontEnd}");
            Length = CalcLength(0, edgePoints.Count - 1);
        }

        internal double CalcLength(int s, int e)
        {
            double len = 0;
            for (int i = s + 1; i < e; i++)
            {
                if (i < edgePoints.Count)
                {
                    len += Distance(edgePoints[i - 1], edgePoints[i]);
                }
            }

            return len;
        }

        internal ImageEdge Clone()
        {
            ImageEdge cl = new ImageEdge();
            foreach (PointF p in edgePoints)
            {
                cl.Add(p.X, p.Y);
            }
            cl.Length = Length;
            cl.BackStart = BackStart;
            cl.BackEnd = BackEnd;
            cl.FrontStart = FrontStart;
            cl.FrontEnd = FrontEnd;
            cl.WholeStart = WholeStart;
            cl.WholeEnd = WholeEnd;
            return cl;
        }

        private void AdjustBounds(double px, double py)
        {
            if (px < tlx)
            {
                tlx = px;
            }
            if (px > brx)
            {
                brx = px;
            }

            if (py < tly)
            {
                tly = py;
            }
            if (py > bry)
            {
                bry = py;
            }
        }

        private double Distance(PointF point1, PointF point2)
        {
            double diff = ((point2.X - point1.X) * (point2.X - point1.X)) +
            ((point2.Y - point1.Y) * (point2.Y - point1.Y));

            return Math.Sqrt(diff);
        }

        private bool PointExists(double px, double py)
        {
            bool result = false;
            foreach (PointF p in edgePoints)
            {
                if (p.X == px && p.Y == py)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }
    }
}