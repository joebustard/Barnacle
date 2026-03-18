using System;

namespace ScriptLanguage
{
    public partial class Interpreter
    {
        private ExpressionNode ParseMakeUBracketFunction(string parentName)
        {
            return ParseGeneral(parentName, new MakeUBracketNode(), 4, "MakeUBracket");
        }
    }
}
