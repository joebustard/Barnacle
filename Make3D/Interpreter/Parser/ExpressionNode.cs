using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;

namespace ScriptLanguage
{
    internal class ExpressionNode : ParseTreeNode
    {
        protected bool isInLibrary;

        // Instance constructor
        public ExpressionNode()
        {
            isInLibrary = false;
        }

        // Copy constructor
        public ExpressionNode(ExpressionNode it)
        {
        }

        public bool IsInLibrary
        {
            get { return isInLibrary; }
            set { isInLibrary = value; }
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