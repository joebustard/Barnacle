using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplateLib
{
    internal class Substitution
    {

        public string Original { get; set;} 
        public string Replacement { get; set; }

        public Substitution()
        {
            Original = String.Empty;
            Replacement = String.Empty;
        }
    }
}
