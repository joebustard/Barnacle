using System;

namespace ScriptLanguage
{
    internal class SolidStatement : StatementNode
    {
        public ExpressionCollection expressions;
        public string label;
        public int ExpressionCount
        { get { return expressions.Count(); } }

        public void AddExpression(ExpressionNode exp)
        {
            expressions.InsertAtStart(exp);
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