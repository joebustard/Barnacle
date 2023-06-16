using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barnacle.Dialogs.RibbedFuselage.Models
{
    internal class MarkerModel
    {
        public String Name
        {
            get;
            set;
        }

        public double Position
        {
            get;
            set;
        }

        public double LowValue
        {
            get;
            set;
        }

        public String HighValue
        {
            get;
            set;
        }
    }
}