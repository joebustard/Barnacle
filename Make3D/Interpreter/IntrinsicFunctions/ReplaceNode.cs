using System;

namespace ScriptLanguage
{
    internal class ReplaceNode : ParseTreeNode
    {
        // Instance constructor
        public ReplaceNode()
        {
        }

        // Copy constructor
        public ReplaceNode(ReplaceNode it)
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
    }
}