using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakerLib
{
    public class TextPolygon
    {
        private TriangulationPolygon triPoly;

        public TextPolygon()
        {
            Points = new List<PointF>();
            Holes = new List<TextPolygon>();
            triPoly = null;
        }

        public List<TextPolygon> Holes { get; set; }
        public List<PointF> Points { get; set; }
        public string SPath { get; set; }

        public void SetTriPoly()
        {
            if (Points != null && Points.Count > 0 && triPoly == null)
            {
                triPoly = new TriangulationPolygon(Points.ToArray());
            }
        }

        internal bool ContainsPoly(TextPolygon tp)
        {
            bool result = false;

            if (Points.Count > 0)
            {
                SetTriPoly();
                if (tp != null && tp.Points.Count > 0)
                {
                    tp.SetTriPoly();
                    foreach (PointF p2 in tp.Points)
                    {
                        if (triPoly.PointInPolygon(p2.X, p2.Y))
                        {
                            // assume that if even one point of tp is in our own poly then they all are
                            // This should always be the case withholes in text characters
                            result = true;
                            break;
                        }
                    }
                }
            }
            return result;
        }

        internal void RemoveHoles()
        {
            if (Holes != null && Holes.Count > 0)
            {
                foreach (TextPolygon hole in Holes)
                {
                    int perimeter;
                    int holePoint;
                    FindClosestPoints(hole, out perimeter, out holePoint);
                    if (perimeter != -1 && holePoint != -1)
                    {
                        List<PointF> combined = CombineHole(hole, perimeter, holePoint);

                        // replace the outter by the new combined poly
                        Points = combined;
                    }
                }
            }
        }

        private List<PointF> CombineHole(TextPolygon hole, int perimeter, int holePoint)
        {
            List<PointF> combined = new List<PointF>();
            // get all perimeter points upto AND including the one closet to the hole

            int i;
            for (i = 0; i <= perimeter; i++)
            {
                combined.Add(new PointF(Points[i].X, Points[i].Y));
            }

            int j = holePoint;
            while (j < hole.Points.Count)
            {
                combined.Add(new PointF(hole.Points[j].X, hole.Points[j].Y));
                j++;
            }
            if (holePoint > 0)
            {
                j = 0;
                // the holepoint itself goes in twice
                while (j <= holePoint)
                {
                    combined.Add(new PointF(hole.Points[j].X, hole.Points[j].Y));
                    j++;
                }
            }

            // get all the points that come after the join point
            // yes the same point is going in twice
            for (i = perimeter; i < Points.Count; i++)
            {
                combined.Add(new PointF(Points[i].X, Points[i].Y));
            }

            return combined;
        }

        private List<PointF> CombineHoleReversing(TextPolygon hole, int perimeter, int holePoint)
        {
            List<PointF> combined = new List<PointF>();
            // get all perimeter points upto AND including the one closet to the hole
            int i;
            for (i = 0; i <= perimeter; i++)
            {
                combined.Add(new PointF(Points[i].X, Points[i].Y));
            }
            int j = holePoint;
            while (j >= 0)
            {
                combined.Add(new PointF(hole.Points[j].X, hole.Points[j].Y));
                j--;
            }
            if (holePoint > 0)
            {
                j = hole.Points.Count - 1;
                // tes holpoint it self goes in twice
                while (j >= holePoint)
                {
                    combined.Add(new PointF(hole.Points[j].X, hole.Points[j].Y));
                    j--;
                }
            }
            // get all the points that come after the join point
            // yes the same point is going in twice
            for (i = perimeter; i < Points.Count; i++)
            {
                combined.Add(new PointF(Points[i].X, Points[i].Y));
            }

            return combined;
        }

        private double Distance(PointF p1, PointF p2)
        {
            double res = ((p2.X - p1.X) * (p2.X - p1.X)) +
                           ((p2.Y - p1.Y) * (p2.Y - p1.Y));

            res = Math.Sqrt(res);
            return res;
        }

        private void FindClosestPoints(TextPolygon hole, out int perimeter, out int holePoint)
        {
            perimeter = -1;
            holePoint = -1;
            double minDist = double.MaxValue;
            for (int i = 0; i < Points.Count; i++)
            {
                for (int j = 0; j < hole.Points.Count; j++)
                {
                    double d = Distance(Points[i], hole.Points[j]);
                    if (d < minDist)
                    {
                        minDist = d;
                        perimeter = i;
                        holePoint = j;
                    }
                }
            }
        }
    }
}