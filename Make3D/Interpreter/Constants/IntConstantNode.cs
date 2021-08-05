using System;

namespace ScriptLanguage
{
    internal class IntConstantNode : ExpressionNode
    {
        private int _Value;

        // Instance constructor
        public IntConstantNode()
        {
            _Value = 0;
        }

        // Copy constructor
        public IntConstantNode(IntConstantNode it)
        {
            _Value = it.Value;
        }

        public int Value
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
            return result;
        }

        public override String ToString()
        {
            return ToRichText();
        }
    }
}