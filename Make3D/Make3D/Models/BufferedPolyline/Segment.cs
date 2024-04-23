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

        public Segment(Point s, Point e)
        {
            Start = s;
            End = e;
        }

        public Segment()
        {
        }

        public void Reverse()
        {
            Point tmp = Start;
            Start = End;
            End = tmp;
        }
    }
}