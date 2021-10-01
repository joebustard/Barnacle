using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barnacle.Models.LoopSmoothing
{
    class LoopEdge
    {

        public int F1 { get; set; }
        public int OppositePoint1 { get; set; }
        public int F2 { get; set; }
        public int OppositePoint2 { get; set; }
        public int Start { get; set; }
        public int End { get; set; }

        public LoopCoord Ep
        {
            get; set;
        }

        public LoopEdge()
        {
            F1 = -1;
            F2 = -1;
        }

        internal void MakeEdgePoint(List<LoopFace> catFaces, List<LoopPoint> catPoints)
        {
            LoopCoord cc = new LoopCoord();

            cc.X = (3.0 * (catPoints[Start].X + catPoints[End].X)/ 8.0) + ((catPoints[OppositePoint1].X + catPoints[OppositePoint2].X) / 8.0);
            cc.Y = (3.0 * (catPoints[Start].Y + catPoints[End].Y) / 8.0) + ((catPoints[OppositePoint1].Y + catPoints[OppositePoint2].Y) / 8.0);
            cc.Z = (3.0 * (catPoints[Start].Z + catPoints[End].Z) / 8.0) + ((catPoints[OppositePoint1].Z + catPoints[OppositePoint2].Z) / 8.0);
            Ep = cc;
        }

 
    }
}
