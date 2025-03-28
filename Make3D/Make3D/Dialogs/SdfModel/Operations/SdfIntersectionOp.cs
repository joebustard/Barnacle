using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barnacle.Dialogs.SdfModel.Operations
{
    internal class SdfIntersectionOp : SdfOperation
    {
        public override String ToString()
        {
            return "Intersection";
        }

        internal override double Apply(double v, double res)
        {
            return Math.Max(v, res);
        }

        internal override string ToParameters()
        {
            return ToString();
        }
    }
}