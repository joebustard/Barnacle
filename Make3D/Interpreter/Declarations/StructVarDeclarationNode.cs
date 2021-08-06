using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptLanguage
{
    internal class StructVarDeclarationNode : DeclarationNode
    {
        // Instance constructor
        public StructVarDeclarationNode()
        {
            DeclarationType = "struct";
        }
    }
}
