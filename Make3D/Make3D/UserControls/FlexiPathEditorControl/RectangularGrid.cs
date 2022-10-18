using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Barnacle.UserControls
{
    public class RectangularGrid : PointGrid
    {

        public override Point Snap(Point p)
        {
            return p;
        }

        /// <summary>
        /// Sets the bounds of the gr
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        public override void SetSize(double v1, double v2)
        {

        }

        public override void SetGridIntervals(double v1, double v2)
        {

        }
    }
}
