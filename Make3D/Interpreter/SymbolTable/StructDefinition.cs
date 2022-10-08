using System;
using System.Collections.Generic;

namespace ScriptLanguage
{
    public class StructDefinition
    {
        public StructDefinition()
        {
            StructName = "";
            Fields = new List<StructField>();
        }

        public List<StructField> Fields { get; set; }

        public String StructName { get; set; }

        internal int FieldIndex(string str)
        {
            int result = -1;
            str = str.ToLower();
            for (int i = 0; i < Fields.Count; i++)
            {
                if (Fields[i].FieldName.ToLower() == str)
                {
                    result = i;
                    break;
                }
            }
            return result;
        }

        public struct StructField
        {
            public string FieldName;
            public string SymType;
        }
    }
}