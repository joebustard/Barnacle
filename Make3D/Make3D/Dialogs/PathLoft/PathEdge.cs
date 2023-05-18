using asdflibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barnacle.Dialogs
{
    public class PathEdge
    {
        public enum Match
        {
            No,
            M1,
            M2,
            M3,
            M4
        };

        public PathEdge(XYZ a, XYZ b)
        {
            P1 = new System.Drawing.PointF((float)a.x, (float)a.z);
            P2 = new System.Drawing.PointF((float)b.x, (float)b.z);
        }

        public System.Drawing.PointF P1 { get; set; }
        public System.Drawing.PointF P2 { get; set; }

        public Match Compare(PathEdge e)
        {
            Match res = Match.No;
            if (equals(P2, e.P1))
            {
                res = Match.M1;
            }
            else
                if (equals(P1, e.P1))
            {
                res = Match.M2;
            }
            else
                if (equals(P2, e.P2))
            {
                res = Match.M3;
            }
            else
                if (equals(P2, e.P2))
            {
                res = Match.M3;
            }
            return res;
        }

        private float tolerance = 0.0000001F;

        public bool equals(System.Drawing.PointF a, System.Drawing.PointF b)
        {
            bool res = false;

            if (Math.Abs(a.X - b.X) < tolerance)
            {
                if (Math.Abs(a.Y - b.Y) < tolerance)
                {
                    res = true;
                }
            }
            return res;
        }

        internal bool EndEquals(PointF pointF)
        {
            return equals(P2, pointF);
        }

        internal bool StartEquals(PointF pointF)
        {
            return equals(P1, pointF);
        }
    }
}