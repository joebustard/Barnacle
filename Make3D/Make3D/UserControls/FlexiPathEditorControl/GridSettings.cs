using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barnacle.UserControls
{
    public class GridSettings
    {
        public enum GridStyle
        {
            Hidden,
            Rectangular,
            Polar
        }

        private double rectangularGridSize = 10;
        public double RectangularGridSize
        {
            get { return rectangularGridSize; }
            set
            {
                if (value != rectangularGridSize)
                {
                    rectangularGridSize = value;
                }
            }
        }

        private double polarGridRadius = 10;
        public double PolarGridRadius
        {
            get { return polarGridRadius; }
            set
            {
                if (value != polarGridRadius)
                {
                    polarGridRadius = value;
                }
            }
        }

        private double polarGridAngle = 10;
        public double PolarGridAngle
        {
            get { return polarGridAngle; }
            set
            {
                if (value != polarGridAngle)
                {
                    polarGridAngle = value;
                }
            }
        }
    }
}
