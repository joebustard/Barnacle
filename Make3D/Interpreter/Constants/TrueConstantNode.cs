﻿using System;

namespace ScriptLanguage
{
    internal class TrueConstantNode : ExpressionNode
    {
        // Instance constructor
        public TrueConstantNode()
        {
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            ExecutionStack.Instance().Push(true);

            return true;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = RichTextFormatter.KeyWord("True");
            return result;
        }

        public override String ToString()
        {
            return "True";
        }
    }
}