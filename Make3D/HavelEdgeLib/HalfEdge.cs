using LoggerLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalfEdgeLib
{
    public class HalfEdge
    {
        public int StartVertex { get; set; }
        public int Face { get; set; }

        public int Twin { get; set; }

        public int Previous { get; set; }
        public int Next { get; set; }
        public bool OnBoundary { get; set; }
        public int Id { get; set; }

        public HalfEdge()
        {
            StartVertex = -1;
            Face = -1;
            Twin = -1;
            Previous = -1;
            Next = -1;
            OnBoundary = false;
            Id = -1;
        }

        public int EndVertex { get; set; }

        public void Dump()
        {
            Logger.LogLine($"Id {Id:D6} : Next {Next:D6}, Previous {Previous:D6}, Twin {Twin:D6}, StartVertex {StartVertex:D6}, Face {Face:D6} Boundary {OnBoundary}");
        }
    }
}