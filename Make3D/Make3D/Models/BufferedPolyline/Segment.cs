using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Barnacle.Models.BufferedPolyline
{
    public class Segment
    {
        public Point Start { get; set; }
        public Point End { get; set; }
        public List<Segment> Extensions { get; set; }
        public Segment(Point s, Point e)
        {
            Start = s;
            End = e;
            Extensions = null;
        }

        public Segment()
        {
            Extensions = null;
        }

    }
}