using System;

namespace ScriptLanguage
{
    internal class OperatorNode : ParseTreeNode
    {
        // Instance constructor
        public OperatorNode()
        {
        }

        // Copy constructor
        public OperatorNode(OperatorNode it)
        {
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            return false;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = "";
            return result;
        }

        public override String ToString()
        {
            String result = "";
            return result;
        }
    }
}