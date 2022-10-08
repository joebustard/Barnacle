using System;

namespace ScriptLanguage
{
    public class CStructuralNode : ParseTreeNode
    {
        private CompoundNode compoundNode;

        // Instance constructor
        public CStructuralNode()
        {
            compoundNode = new CompoundNode();
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            return true;
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