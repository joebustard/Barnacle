using System;

namespace ScriptLanguage
{
    public partial class Interpreter
    {
        private ExpressionNode ParseMakeBevelledGearFunction(string parentName)
        {
            return ParseGeneral(parentName, new MakeBevelledGearNode(), 7, "MakeBevelledGear");
        }
    }
}
