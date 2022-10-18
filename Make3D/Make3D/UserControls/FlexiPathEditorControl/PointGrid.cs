using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace Barnacle.UserControls
{
    public class PointGrid
    {

        protected List<Shape> gridMarkers;
        public List<Shape> GridMarkers
        {
            get { return gridMarkers; }
            set
            {
                gridMarkers = value;
            }
        }

        public virtual void CreateMarkers()
        {
            gridMarkers.Clear();
        }
        public virtual Point Snap( Point p)
        {
            return p;
        }

        /// <summary>
        /// Sets the bounds of the grid
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        public virtual void SetSize(double v1,double v2)
        {
           
        }

        public virtual void SetGridIntervals(double v1, double v2)
        {

        }
        public PointGrid()
        {
            gridMarkers = new List<Shape>();
        }
    }
}
