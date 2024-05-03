using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalfEdgeLib
{
    public class HalfEdge
    {
        public int StartVertex;
        public int Face;

        public int Twin;

        public int Previous;
        public int Next;
        public bool OnBoundary;

        public HalfEdge()
        {
            StartVertex = -1;
            Face = -1;
            Twin = -1;
            Previous = -1;
            Next = -1;
            OnBoundary = false;
        }
    }
}