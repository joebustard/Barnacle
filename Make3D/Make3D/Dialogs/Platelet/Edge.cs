using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barnacle.Dialogs
{
    public class Edge
    {
        public int P0 { get; set; }
        public int P1 { get; set; }
        public int FaceA { get; set; }
        public int FaceB { get; set; }

        public Edge()
        {
            P0 = P1 = FaceA = FaceB = -1;
        }

        public Edge(int p0, int p1)
        {
            P0 = p0;
            P1 = p1;
        }

        public bool Matches(Edge e)

        {
            bool result = false;
            if (P0 == e.P0 && P1 == e.P1)
            {
                result = true;
            }
            else
           if (P1 == e.P0 && P0 == e.P1)
            {
                result = true;
            }
            return result;
        }

        public bool Matches(int s, int e)

        {
            bool result = false;
            if (P0 == s && P1 == e)
            {
                result = true;
            }
            else
           if (P1 == s && P0 == e)
            {
                result = true;
            }
            return result;
        }
    }
}