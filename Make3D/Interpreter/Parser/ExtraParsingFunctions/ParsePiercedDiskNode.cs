using System;

namespace ScriptLanguage
{
    public partial class Interpreter
    {
        private ExpressionNode ParseMakePiercedDiskFunction(string parentName)
        {
            return ParseGeneral(parentName, new MakePiercedDiskNode(), 5, "MakePiercedDisk");
        }
    }
}
