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
    }
}