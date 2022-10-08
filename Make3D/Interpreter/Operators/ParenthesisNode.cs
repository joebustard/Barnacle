using System;

namespace ScriptLanguage
{
    internal class ParenthesisNode : ExpressionNode
    {
        private ExpressionNode expressionNode;

        public ParenthesisNode()
        {
            expressionNode = null;
        }

        public ExpressionNode Expression
        {
            get { return expressionNode; }
            set { expressionNode = value; }
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            if (expressionNode != null)
            {
                result = expressionNode.Execute();
            }
            return result;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = "";

            result += RichTextFormatter.Operator("( ");
            if (expressionNode != null)
            {
                result += expressionNode.ToRichText();
            }
            result += RichTextFormatter.Operator(" )");
            return result;
        }

        public override String ToString()
        {
            String result = "( ";

            if (expressionNode != null)
            {
                result += expressionNode.ToString();
            }
            result += " )";
            return result;
        }
    }
}