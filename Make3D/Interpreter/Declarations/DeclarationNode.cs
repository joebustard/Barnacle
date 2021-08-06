using System;

namespace ScriptLanguage
{
    internal class DeclarationNode : CStatementNode
    {
        private string declarationType;
        private String varName;

        // Instance constructor
        public DeclarationNode()
        {
        }

        // Copy constructor
        public DeclarationNode(DeclarationNode it)
        {
            this.declarationType = it.DeclarationType;
            this.varName = it.VarName;
        }

        public string DeclarationType
        {
            get { return declarationType; }
            set
            {
                if (value != declarationType)
                {
                    declarationType = value;
                }
            }
        }

        public String VarName
        {
            get { return varName; }
            set { varName = value; }
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
        public override String ToRichText()
        {
            String result="";
            if (!IsInLibrary)
            {
                result = Indentor.Indentation() + RichTextFormatter.KeyWord(DeclarationType + " ") + RichTextFormatter.VariableName(varName) + " ;";
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
            if (!IsInLibrary)
            {
                result = Indentor.Indentation()+DeclarationType + " "+varName + " ;";
             
            }
            return result;
        }
    }
}