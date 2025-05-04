using System;

namespace ScriptLanguage
{
    public partial class Interpreter
    {
        private ExpressionNode ParseGeneral(string parentName, ExpressionNode parseNode, int exprCount, string label)
        {
            ExpressionNode exp = null;

            String commaError = $"{label} expected ,";
            bool parsed = true;
            ExpressionCollection coll = new ExpressionCollection();

            for (int i = 0; i < exprCount && parsed; i++)
            {
                ExpressionNode paramExp = ParseExpressionNode(parentName);
                if (paramExp != null)
                {
                    if (i < exprCount - 1)
                    {
                        if (CheckForComma() == false)
                        {
                            ReportSyntaxError(commaError);
                            parsed = false;
                        }
                    }
                    coll.Add(paramExp);
                }
                else
                {
                    String expError = $"{label} error parsing parameter expression number {i + 1} ";
                    ReportSyntaxError(expError);
                    parsed = false;
                }
            }
            if (parsed && coll.Count() == exprCount)
            {
                parseNode.SetExpressions(coll);
                parseNode.IsInLibrary = tokeniser.InIncludeFile();
                exp = parseNode;
            }

            return exp;
        }
    }
}