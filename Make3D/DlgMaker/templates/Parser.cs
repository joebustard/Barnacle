using System;

namespace ScriptLanguage
{
    public partial class Interpreter
    {
        private ExpressionNode ParseMakeTOOLNAMEFunction(string parentName)
        {
            return ParseGeneral(parentName, new MakeTOOLNAMENode(), //PARAMCOUNT, "MakeTOOLNAME");
        }
    }
}