using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;

namespace ScriptLanguage
{
    internal class CReplayNode : ParseTreeNode
    {
        // Instance constructor
        public CReplayNode()
        {
        }

        // Copy constructor
        public CReplayNode(CReplayNode it)
        {
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            throw new NotImplementedException();

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