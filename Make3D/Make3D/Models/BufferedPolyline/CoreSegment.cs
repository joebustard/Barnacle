using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Barnacle.Models.BufferedPolyline
{
    public class CoreSegment
    {
        public Point Start { get; set; }
        public Point End { get; set; }

        public CoreSegment(Point s, Point e)
        {
            Start = s;
            End = e;
        }
    }
}