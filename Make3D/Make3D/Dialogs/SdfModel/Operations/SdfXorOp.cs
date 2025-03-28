using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barnacle.Dialogs.SdfModel.Operations
{
    internal class SdfXorOp : SdfOperation
    {
        public override String ToString()
        {
            return "Xor";
        }

        internal override double Apply(double a, double b)
        {
            return Math.Max(Math.Min(a, b), -Math.Max(a, b));
        }

        internal override string ToParameters()
        {
            return ToString();
        }
    }
}