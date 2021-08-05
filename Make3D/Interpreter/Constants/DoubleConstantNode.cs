using System;

namespace ScriptLanguage
{
    internal class DoubleConstantNode : ExpressionNode
    {
        private double _Value;

        // Instance constructor
        public DoubleConstantNode()
        {
            _Value = 0.0;
        }

        // Copy constructor
        public DoubleConstantNode(DoubleConstantNode it)
        {
            _Value = it.Value;
        }

        public double Value
        {
            get { return _Value; }
            set { _Value = value; }
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            ExecutionStack.Instance().Push(_Value);

            return true;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = _Value.ToString();
            if (result.IndexOf(".") == -1)
            {
                result += ".0";
            }
            return result;
        }

        public override String ToString()
        {
            return ToRichText();
        }
    }
}