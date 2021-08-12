using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptLanguage
{
    class StructDefinitiontTable
    {
        private List<StructDefinition> Symbols;
        static private StructDefinitiontTable Singleton;

        private StructDefinitiontTable()
        {
            Symbols = new List<StructDefinition>();
        }

        public static StructDefinitiontTable Instance()
        {
            if (Singleton == null)
            {
                Singleton = new StructDefinitiontTable();
            }
            return Singleton;
        }

        public void Clear()
        {
            Symbols.Clear();
        }
        public StructDefinition FindStruct(string strName)
        {
            StructDefinition result = null;
            foreach (StructDefinition sym in Symbols)
            {
                if (sym.StructName == strName)
                {
                    result = sym;
                    break;
                }
            }
            return result;
        }

        internal void AddStruct(StructNode stNode)
        {
            StructDefinition def = new StructDefinition();
            def.StructName = stNode.Name;
            foreach ( StatementNode nd in stNode.Body.Statements)
            {
                if ( nd is DeclarationNode)
                {
                    StructDefinition.StructField fld = new StructDefinition.StructField();
                    fld.FieldName = (nd as DeclarationNode).VarName;
                    fld.SymType = (nd as DeclarationNode).DeclarationType;
                    def.Fields.Add(fld);
                }
            }
            Symbols.Add(def);
        }
    }
}
