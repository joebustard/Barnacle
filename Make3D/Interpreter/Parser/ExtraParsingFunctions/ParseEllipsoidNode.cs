using System;

namespace ScriptLanguage
{
    public partial class Interpreter
    {
        private ExpressionNode ParseMakeEllipsoidFunction(string parentName)
        {
            return ParseGeneral(parentName, new MakeEllipsoidNode(), 5, "MakeEllipsoid");
        }
    }
}