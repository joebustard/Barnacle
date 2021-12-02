private ExpressionNode ParseMake//TOOLNAMEFunction(string parentName)
{
    ExpressionNode exp = null;
    String label = "Make//TOOLNAME";
    String commaError = $"{label} expected ,";
    bool parsed = true;
    ExpressionCollection coll = new ExpressionCollection();
int exprCount =//PARAMCOUNT;

    for (int i = 0; i<exprCount && parsed; i++)
    {
        ExpressionNode paramExp = ParseExpressionNode(parentName);
        if (paramExp != null)
        {
            if (i<exprCount - 1)
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
        Make//TOOLNAMENode mn = new Make//TOOLNAMENode(coll);
        mn.IsInLibrary = tokeniser.InIncludeFile();
exp = mn;
    }

    return exp;
}