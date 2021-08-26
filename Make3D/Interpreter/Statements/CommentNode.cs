using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;

namespace ScriptLanguage
{
    internal class CommentNode : StatementNode
    {
        private String _Text;

        // Instance constructor
        public CommentNode()
        {
            Text = "";
        }

        // Copy constructor
        public CommentNode(CommentNode it)
        {
        }

        public String Text
        {
            get { return _Text; }
            set { _Text = value; }
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
            if (!isInLibrary)
            {
                result = Indentor.Indentation() + RichTextFormatter.LineComment("// " + Text);
                if (HighLight)
                {
                    result = RichTextFormatter.Highlight(result);
                }
            }
            return result;
        }

        public override String ToString()
        {
            String result = "";
            if (!isInLibrary)
            {
                result = Indentor.Indentation() + "// " + Text;
            }
            return result;
        }
    }
}