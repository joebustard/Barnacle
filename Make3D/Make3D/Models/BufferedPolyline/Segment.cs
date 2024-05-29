using System.Collections.Generic;
using System.Windows;

namespace Barnacle.Models.BufferedPolyline
{
    public class Segment
    {
        public Segment(Point s, Point e, Point c, bool dir)
        {
            Start = s;
            End = e;
            ExtensionCentre = c;
            Extensions = null;
            Outbound = dir;
        }

        public Point End { get; set; }
        public Point ExtensionCentre { get; set; }
        public List<Segment> Extensions { get; set; }
        public bool Outbound { get; set; }
        public Point Start { get; set; }
        public int Twin { get; internal set; }
    }
}