using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barnacle.Dialogs
{
    public class EdgeRecord
    {


        public EdgeRecord(int s, int e)
        {
            this.Start = s;
            this.End = e;
        }

        public int Start { get; set; }
        public int End { get; set; }
    }
}
