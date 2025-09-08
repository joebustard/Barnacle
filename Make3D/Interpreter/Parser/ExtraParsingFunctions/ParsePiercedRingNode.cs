using System;

namespace ScriptLanguage
{
    public partial class Interpreter
    {
        private ExpressionNode ParseMakePiercedRingFunction(string parentName)
        {
            return ParseGeneral(parentName, new MakePiercedRingNode(), 6, "MakePiercedRing");
        }
    }
}
