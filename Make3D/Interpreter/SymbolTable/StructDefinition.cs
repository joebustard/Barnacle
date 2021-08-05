using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptLanguage
{

    public class StructDefinition
    {
        public struct StructField
        {
            public string FieldName;
            public string SymType;
        }
        public String StructName { get; set; }
        public List<StructField> Fields { get; set; }

        public StructDefinition()
        {
            StructName = "";
            Fields = new List<StructField>();
        }
    }
}
