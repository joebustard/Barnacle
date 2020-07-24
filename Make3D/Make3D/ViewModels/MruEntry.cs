using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Make3D.ViewModels
{

    public class MruEntry
    {
        public String Name { get; set; }
        public String Path { get; set; }

        public MruEntry(string n, string p)
        {
            Name = n;
            Path = p;
        }
    }
}
