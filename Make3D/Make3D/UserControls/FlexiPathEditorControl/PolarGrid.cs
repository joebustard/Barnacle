using System.Windows;

namespace Barnacle.UserControls
{
    public class PolarGrid : PointGrid
    {
        public override Point Snap(Point p)
        {
            return p;
        }

        /// <summary>
        /// Sets the bounds of the grid
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