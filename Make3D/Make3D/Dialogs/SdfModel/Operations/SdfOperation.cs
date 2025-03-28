using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barnacle.Dialogs.SdfModel.Operations
{
    internal class SdfOperation
    {
        public double Blend { get; set; } = 3;

        public override String ToString()
        {
            return "";
        }

        internal virtual double Apply(double v, double res)
        {
            return res;
        }

        internal virtual string ToParameters()
        {
            string s = ToString() + ",";
            s += Blend.ToString();
            return s;
        }
    }
}