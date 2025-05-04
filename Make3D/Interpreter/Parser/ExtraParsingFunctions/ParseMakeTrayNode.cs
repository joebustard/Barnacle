using System;

namespace ScriptLanguage
{
    public partial class Interpreter
    {
        private ExpressionNode ParseMakeTrayFunction(string parentName)
        {
            return ParseGeneral(parentName, new MakeDripTrayNode(), 6, "MakeTray");
        }
    }
}