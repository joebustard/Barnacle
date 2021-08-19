using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Make3D.Dialogs
{
    public class PolyPoint
    {
 public bool Selected { get; set; }

        public PolyPoint(double x, double y)
        {
            Point = new System.Windows.Point(x, y);
            Selected = false;
        }

        public PolyPoint(System.Windows.Point p)
        {
            Point = p;
            Selected = false;
        }
        public System.Windows.Point Point { get; set; }
    }
}
