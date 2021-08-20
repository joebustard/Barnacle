using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptLanguage
{
    internal class ArrayDeclarationNode : DeclarationNode
    {
        private ExpressionNode dimensions;
        private SymbolTable.SymbolType itemType;

        // Instance constructor
        public ArrayDeclarationNode()
        {
            DeclarationType = "Array";
            dimensions = null;
        }

        public ExpressionNode Dimensions
        {
            get { return dimensions; }
            set { dimensions = value; }
        }

        public SymbolTable.SymbolType ItemType
        {
            get { return itemType; }
            set { itemType = value; }
        }

        public override String ToRichText()
        {
            String result = "";
            if (!IsInLibrary)
            {
                result = Indentor.Indentation() + RichTextFormatter.KeyWord(GetTypeName() + " ") + "[" + dimensions.ToRichText() + "] " + RichTextFormatter.VariableName(VarName) + " ;";
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
                result = Indentor.Indentation() + GetTypeName() + " [" + dimensions.ToString() + "] " + VarName + " ;";
            }
            return result;
        }

        private string GetTypeName()
        {
            string r = "???";
            switch (itemType)
            {
                case SymbolTable.SymbolType.boolarrayvariable: r = "Bool"; break;
            }
            return r;
        }
    }
}