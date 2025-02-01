using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workflow
{
    public class SliceResult
    {
        public int Days
        {
            get; set;
        }

        public int Filament
        {
            get; set;
        }

        public int Hours
        {
            get; set;
        }

        public int Minutes
        {
            get; set;
        }

        public bool Result
        {
            get; set;
        }

        public int Seconds
        {
            get; set;
        }

        public int TotalSeconds
        {
            get; set;
        }
    }
}