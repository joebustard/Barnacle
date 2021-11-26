using System;

namespace ScriptLanguage
{
    internal class IncludeNode : StatementNode
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
            String result = "";
            if (!isInLibrary)
            {
                //
                // Need to double up the slashes to stop rtf treating them as escape chars
                //
                String localPath = path.Replace(@"\", @"\\");

                result = Indentor.Indentation() + RichTextFormatter.KeyWord("Include ") + RichTextFormatter.Normal(localPath + " ;");
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
                result = Indentor.Indentation() + "Include " + path + " ;";
            }
            return result;
        }
    }
}