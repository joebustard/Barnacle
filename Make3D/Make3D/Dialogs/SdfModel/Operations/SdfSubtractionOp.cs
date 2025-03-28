using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barnacle.Dialogs.SdfModel.Operations
{
    internal class SdfSubtractionOp : SdfOperation
    {
        public override String ToString()
        {
            return "Subtraction";
        }

        internal override double Apply(double v, double res)
        {
            return Math.Max(-v, res);
        }

        internal override string ToParameters()
        {
            return ToString();
        }
    }
}