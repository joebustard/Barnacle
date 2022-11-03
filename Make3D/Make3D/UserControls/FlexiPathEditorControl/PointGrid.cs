using System.Collections.Generic;
using System.Windows;
using System.Windows.Shapes;

namespace Barnacle.UserControls
{
    public class PointGrid
    {
        protected double actualHeight;
        protected double actualWidth;
        protected List<Shape> gridMarkers;
        protected double gridXMM;
        protected double gridXPixels;
        protected double gridYMM;
        protected double gridYPixels;
        protected double pixelsPerInchX;
        protected double pixelsPerInchY;

        public PointGrid()
        {
            gridMarkers = new List<Shape>();
        }

        public List<Shape> GridMarkers
        {
            get { return gridMarkers; }
            set
            {
                gridMarkers = value;
            }
        }

        public virtual void CreateMarkers(DpiScale sc)
        {
            gridMarkers.Clear();
        }

        protected DpiScale screenDpi;

        /// <summary>
        /// Sets the bounds of the gr
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        public void SetActualSize(DpiScale dpi, double aw, double ah)
        {
            actualWidth = aw;
            actualHeight = ah;
            screenDpi = dpi;
            pixelsPerInchX = dpi.PixelsPerInchX;
            pixelsPerInchY = dpi.PixelsPerInchY;
        }

        public virtual void SetGridIntervals(double v1, double v2)
        {
        }

        /// <summary>
        /// Sets the bounds of the grid
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        public virtual void SetSize(double v1, double v2)
        {
        }

        public virtual Point SnapPositionToMM(Point p)
        {
            return p;
        }

        protected double ToMMX(double x)
        {
            double res = 25.4 * x / pixelsPerInchX;
            return res;
        }

        protected double ToMMY(double y)
        {
            double res = 25.4 * y / pixelsPerInchY;
            return res;
        }

        protected double ToPixelX(double x)
        {
            double res = pixelsPerInchX * x / 25.4;
            return res;
        }

        protected double ToPixelY(double y)
        {
            double res = pixelsPerInchY * y / 25.4;
            return res;
        }
    }
}