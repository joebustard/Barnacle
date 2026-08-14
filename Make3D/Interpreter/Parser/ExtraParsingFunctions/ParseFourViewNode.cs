using System;

namespace ScriptLanguage
{
    public partial class Interpreter
    {
        private ExpressionNode ParseMakeFourViewFunction(string parentName)
        {
            return ParseGeneral(parentName, new MakeFourViewNode(), 7, "MakeFourView");
        }
    }
}
