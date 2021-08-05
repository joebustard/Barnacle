using System;

namespace ScriptLanguage
{
    internal class FalseConstantNode : ExpressionNode
    {
        // Instance constructor
        public FalseConstantNode()
        {
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            ExecutionStack.Instance().Push(false);

            return true;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = RichTextFormatter.KeyWord("False");
            return result;
        }

        public override String ToString()
        {
            return "False";
        }
    }
}