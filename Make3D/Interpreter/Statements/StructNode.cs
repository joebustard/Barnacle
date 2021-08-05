using System;

namespace ScriptLanguage
{
    internal class StructNode : CStatementNode
    {
        protected CCompoundNode body;
        protected String name;

        // Instance constructor
        public StructNode()
        {
            name = "";
            body = null;
        }

        public CCompoundNode Body
        {
            get { return body; }
            set { body = value; }
        }

        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = true;

            return result;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        public override String ToRichText()
        {
            String result = "";
            if (!isInLibrary)
            {
                result = Indentor.Indentation() + RichTextFormatter.KeyWord("Struct ");

                result += RichTextFormatter.Procedure(name);
                result += @"\par";
                result += body.ToRichText();
                result += @" \par";
            }
            return result;
        }

        public override String ToString()
        {
            String result = "";
            if (!isInLibrary)
            {
                result = Indentor.Indentation() + "Struct ";

                result += name;
                result += "\n";
                result += body.ToString();
                result += "\n";
            }
            return result;
        }
    }
}