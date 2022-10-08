using System;

namespace ScriptLanguage
{
    internal class ReturnNode : StatementNode
    {
        public CompoundNode Parent;

        private ExpressionNode expression;

        // Instance constructor
        public ReturnNode()
        {
            expression = null;
        }

        // Copy constructor
        public ReturnNode(ReturnNode it)
        {
        }

        public ExpressionNode Expression
        {
            get { return expression; }
            set { expression = value; }
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            if (expression != null)
            {
                result = expression.Execute();
                Parent.SetReturn();
            }

            return result;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = Indentor.Indentation() + RichTextFormatter.KeyWord("Return ") + expression.ToRichText() + " ;";
            if (HighLight)
            {
                result = RichTextFormatter.Highlight(result);
            }
            return result;
        }

        public override String ToString()
        {
            String result = Indentor.Indentation() + "Return " + expression.ToString() + " ;";
            return result;
        }
    }
}