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
            ActualSymbol = null;
        }

        public Symbol ActualSymbol { get; set; }

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

        public override bool Execute()
        {
            bool res = false;
            if (ActualSymbol != null)
            {
                int dim = 0;
                if (EvalExpression(dimensions, ref dim, "Dimensions"))
                {
                    // tell the symbol how big the array should be
                    ActualSymbol.SetSize(dim);
                    res = true;
                }
                else
                {
                }
            }
            return res;
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

        private bool EvalExpression(ExpressionNode exp, ref int x, string v)
        {
            bool result = exp.Execute();
            if (result)
            {
                result = false;
                StackItem sti = ExecutionStack.Instance().Pull();
                if (sti != null)
                {
                    if (sti.MyType == StackItem.ItemType.ival)
                    {
                        x = sti.IntValue;
                        result = true;
                    }
                    else if (sti.MyType == StackItem.ItemType.dval)
                    {
                        x = (int)sti.DoubleValue;
                        result = true;
                    }
                }
            }
            if (!result)
            {
                Log.Instance().AddEntry("Array : " + v + " expression error");
            }
            return result;
        }

        private string GetTypeName()
        {
            string r = "???";
            switch (itemType)
            {
                case SymbolTable.SymbolType.boolarrayvariable: r = "Bool"; break;
                case SymbolTable.SymbolType.intarrayvariable: r = "Int"; break;
                case SymbolTable.SymbolType.doublearrayvariable: r = "Double"; break;
                case SymbolTable.SymbolType.stringarrayvariable: r = "String"; break;
                case SymbolTable.SymbolType.handlearrayvariable: r = "Handle"; break;
                case SymbolTable.SymbolType.solidarrayvariable: r = "Solid"; break;
            }
            return r;
        }
    }
}