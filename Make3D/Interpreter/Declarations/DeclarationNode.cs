using System;

namespace ScriptLanguage
{
    internal class DeclarationNode : StatementNode
    {
        private string declarationType;
        private String externalName;
        private ExpressionNode initialiser;
        private String name;
        private Symbol symbol;
        private String varName;

        // Instance constructor
        public DeclarationNode()
        {
            initialiser = null;
        }

        // Copy constructor
        public DeclarationNode(DeclarationNode it)
        {
            this.declarationType = it.DeclarationType;
            this.varName = it.VarName;
        }

        public string DeclarationType
        {
            get
            {
                return declarationType;
            }
            set
            {
                if (value != declarationType)
                {
                    declarationType = value;
                }
            }
        }

        public String ExternalName
        {
            get { return externalName; }
            set { externalName = value; }
        }

        public ExpressionNode Initialiser
        {
            get { return initialiser; }
            set { initialiser = value; }
        }

        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        public Symbol Symbol
        {
            set { symbol = value; }
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
            bool result = true;
            if (initialiser != null)
            {
                result = false;
                if (initialiser.Execute())
                {
                    result = PullStackTopToVar();
                }
            }
            return result;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        public override String ToRichText()
        {
            String result = "";
            if (!IsInLibrary)
            {
                result = Indentor.Indentation() + RichTextFormatter.KeyWord(DeclarationType + " ") + RichTextFormatter.VariableName(varName);
                if (initialiser != null)
                {
                    result += " = " + initialiser.ToRichText();
                }
                result += " ;";
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
                result = Indentor.Indentation() + DeclarationType + " " + varName;
                if (initialiser != null)
                {
                    result += " = " + initialiser.ToString();
                }
                result += " ;";
            }
            return result;
        }

        private bool PullStackTopToVar()
        {
            bool res = false;
            if (symbol != null)
            {
                res = ExecutionStack.Instance().PullSymbol(symbol);
            }
            return res;
        }
    }
}