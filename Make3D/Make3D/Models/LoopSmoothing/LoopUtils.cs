using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barnacle.Models.LoopSmoothing
{
    class LoopUtils
    {
        private static double TwoPi = Math.PI * 2.0;
        private static double fiveEigths = 5.0 / 8.0;

        public static double Beta( double n)
    {
            double q1 = (3.0 + 2.0 * Math.Cos(TwoPi / n));
            double res = (1.0 / n) * (fiveEigths - (q1 * q1)/64.0);
            return res;
    }
    }
}
