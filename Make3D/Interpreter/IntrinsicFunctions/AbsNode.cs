using System;

namespace ScriptLanguage
{
    internal class AbsNode : SingleParameterFunction
    {
        // Instance constructor
        public AbsNode()
        {
            parameterExpression = null;
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            if (parameterExpression != null)
            {
                double d = 0;
                if (EvalExpression(parameterExpression, ref d, "Value", "Abs"))
                {
                    ExecutionStack.Instance().Push(Math.Abs(d));
                    result = true;
                }
            }
            return result;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = RichTextFormatter.KeyWord("Abs(") + parameterExpression.ToRichText() + " )";
            return result;
        }

        public override String ToString()
        {
            String result = "Abs(" + parameterExpression.ToString() + " )";
            return result;
        }
    }
}