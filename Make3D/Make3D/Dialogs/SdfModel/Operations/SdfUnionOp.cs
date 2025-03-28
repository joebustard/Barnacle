using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barnacle.Dialogs.SdfModel.Operations
{
    internal class SdfUnionOp : SdfOperation
    {
        public override String ToString()
        {
            return "Union";
        }

        internal override double Apply(double v, double res)
        {
            return Math.Min(v, res);
        }

        internal override string ToParameters()
        {
            return ToString();
        }
    }
}