using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barnacle.Dialogs.MeshEditor
{
    internal class SculptingTool
    {
        private double piByTwo = Math.PI / 2.0;

        public SculptingTool()
        {
            Radius = 5;
            Scaler = 0.5;
        }

        public double Radius { get; set; }
        public double Scaler { get; set; }

        public virtual double Force(double v)
        {
            double res = 0;
            if (v > 0 && v <= Radius)
            {
                res = Math.Cos((v / Radius) * piByTwo) * Scaler;
            }
            return res;
        }
    }
}