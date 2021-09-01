using System;

namespace ScriptLanguage
{
    internal class SingleParameterFunction : ExpressionNode
    {
        protected ExpressionNode parameterExpression;

        // Instance constructor
        public SingleParameterFunction()
        {
            parameterExpression = null;
        }

        public ExpressionNode Expression
        {
            get { return parameterExpression; }
            set { parameterExpression = value; }
        }
    }
}