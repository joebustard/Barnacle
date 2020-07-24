using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Make3D.Models
{
    public class Scale3D
    {
     
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Scale3D()
        {
            X = 1;
            Y = 1;
            Z = 1;
        }

        public Scale3D(double v1, double v2, double v3)
        {
            this.X = v1;
            this.Y = v2;
            this.Z = v3;
        }

        internal void Adjust(double v1, double v2, double v3)
        {
            X *= v1;
            Y *= v2;
            Z *= v3;
        }
    }
}
