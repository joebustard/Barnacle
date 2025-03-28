using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barnacle.Dialogs.SdfModel.Operations
{
    internal class SdfSmoothIntersectionOp : SdfOperation
    {
        public override String ToString()
        {
            return "Smooth Intersection";
        }

        internal override double Apply(double v, double res)
        {
            double h = Utils.Clamp(0.5 - 0.5 * (res - v) / Blend, 0.0, 1.0);
            return Utils.Mix(res, v, h) + Blend * h * (1.0 - h);
        }
    }
}