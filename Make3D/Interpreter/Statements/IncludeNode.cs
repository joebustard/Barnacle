using System;

namespace ScriptLanguage
{
    internal class IncludeNode : CStatementNode
    {
        private String path;

        // Instance constructor
        public IncludeNode()
        {
            path = "";
        }

        // Copy constructor
        public IncludeNode(IncludeNode it)
        {
            path = it.Path;
        }

        public String Path
        {
            get { return path; }
            set { path = value; }
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
            //
            // Need to double up the slashes to stop rtf treating them as escape chars
            //
            String localPath = path.Replace(@"\", @"\\");

            String result = Indentor.Indentation() + RichTextFormatter.KeyWord("Include ") + RichTextFormatter.Normal(localPath + " ;");
            if (HighLight)
            {
                result = RichTextFormatter.Highlight(result);
            }

            return result;
        }

        public override String ToString()
        {
            String result = Indentor.Indentation() + "Include " + path + " ;";

            return result;
        }
    }
}