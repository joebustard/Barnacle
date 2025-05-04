using System;

namespace ScriptLanguage
{
    internal class SolidStatement : StatementNode
    {
        public ExpressionCollection expressions;
        public string label;

        public int ExpressionCount
        {
            get
            {
                return expressions.Count();
            }
        }

        public void AddExpression(ExpressionNode exp)
        {
            expressions.InsertAtStart(exp);
        }

        public bool CheckSolidExists(string label, string expr, int objectIndex)
        {
            bool res = false;
            if (Script.ResultArtefacts.ContainsKey(objectIndex))
            {
                if (Script.ResultArtefacts[objectIndex] != null)
                {
                    res = true;
                }
                else
                {
                    ReportStatement($"Run Time Error : {label} (5) solid name incorrect {expr} index = {objectIndex} is null. Has it been deleted?");
                }
            }
            else
            {
                ReportStatement($"Run Time Error : {label} (5) solid name incorrect {expr} index = {objectIndex} doesn't exist");
            }
            return res;
        }

        public override String ToRichText()
        {
            String result = "";
            if (!IsInLibrary)
            {
                result = Indentor.Indentation() + RichTextFormatter.KeyWord($"{label} ");
                result += expressions.ToRichText();
                result += " ;";
                if (HighLight)
                {
                    result = RichTextFormatter.Highlight(result);
                }
            }
            return result;
        }

        public override String ToString()
        {
            String result = "";
            if (!IsInLibrary)
            {
                result = Indentor.Indentation() + $"{label} " + expressions.ToString();
                result += " ;";
            }
            return result;
        }
    }
}