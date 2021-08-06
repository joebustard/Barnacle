using System;

namespace ScriptLanguage
{
    internal class StringConstantNode : ExpressionNode
    {
        private String _Value;

        // Instance constructor
        public StringConstantNode()
        {
            _Value = "";
        }

        // Copy constructor
        public StringConstantNode(StringConstantNode it)
        {
            _Value = it.Value;
        }

        public String Value
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
        public override String ToRichText()
        {
            string EscapedValue = _Value.Replace(@"\", @"\\");
            String result = "\"" + EscapedValue + "\"";
            return result;
        }

        public override String ToString()
        {
            return ToRichText();
        }
    }
}