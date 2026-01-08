using System;

namespace ScriptLanguage
{
    public partial class Interpreter
    {
        private ExpressionNode ParseMakeHoneyCombGrilleFunction(string parentName)
        {
            return ParseGeneral(parentName, new MakeHoneyCombGrilleNode(), 7, "MakeHoneyCombGrille");
        }
    }
}