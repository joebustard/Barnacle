using System;

namespace ScriptLanguage
{
    internal class SingleParameterFunction : ExpressionNode
    {
        protected ExpressionNode _Expression;

        // Instance constructor
        public SingleParameterFunction()
        {
            _Expression = null;
        }

        public ExpressionNode Expression
        {
            get { return _Expression; }
            set { _Expression = value; }
        }
    }
}