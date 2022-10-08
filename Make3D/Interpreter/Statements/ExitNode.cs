using System;

namespace ScriptLanguage
{
    internal class ExitNode : StatementNode
    {
        // Instance constructor
        public ExitNode()
        {
        }

        // Copy constructor
        public ExitNode(ExitNode it)
        {
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            //
            // returning false will terminate the application
            //
            ParseTreeNode.continueRunning = false;
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
                result = Indentor.Indentation() + RichTextFormatter.KeyWord("Exit") + " ;";
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
                result = Indentor.Indentation() + "Exit ;";
            }
            return result;
        }
    }
}