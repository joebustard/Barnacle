using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;

namespace ScriptLanguage
{
    public class StatementNode : ParseTreeNode
    {
        protected bool isInLibrary;

        public bool IsInLibrary
        {
            get { return isInLibrary; }
            set { isInLibrary = value; }
        }

        // Instance constructor
        public StatementNode()
        {
        }

        // Copy constructor
        public StatementNode(StatementNode it)
        {
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