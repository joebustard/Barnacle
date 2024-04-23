using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Barnacle.Models.BufferedPolyline
{
    public class CoreSegment : Segment
    {
        // does side A or B need to have curve points added to link it to the next one
        public bool FillA { get; set; }

        public bool FillB { get; set; }

        public CoreSegment(Point s, Point e)
        {
            Start = s;
            End = e;
        }
    }
}