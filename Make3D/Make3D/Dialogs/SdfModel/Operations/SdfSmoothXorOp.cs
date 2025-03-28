using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barnacle.Dialogs.SdfModel.Operations
{
    internal class SdfSmoothXorOp : SdfOperation
    {
        public override String ToString()
        {
            return "Smooth Xor";
        }

        internal override double Apply(double a, double b)
        {
            return Math.Max(Utils.Smin(a, b, Blend), -Utils.Smax(a, b, Blend));
        }
    }
}