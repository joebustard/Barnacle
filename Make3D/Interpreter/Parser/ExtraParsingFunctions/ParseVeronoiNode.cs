using System;

namespace ScriptLanguage
{
    public partial class Interpreter
    {
        private ExpressionNode ParseMakeVeronoiFunction(string parentName)
        {
            ExpressionNode exp = null;
            VeronoiNode asn = new VeronoiNode();
            exp = ParseSolidFunction(parentName, asn.Label, 3, asn);
            return exp;
        }
    }
}