using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptLanguage
{
    public class StructArraySymbol : ArraySymbol
    {
        public StructDefinition Structure { get; set; }
    }
}