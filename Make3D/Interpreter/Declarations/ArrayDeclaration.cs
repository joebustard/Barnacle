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
        private ExpressionCollection initialisers;
        private SymbolTable.SymbolType itemType;

        // Instance constructor
        public ArrayDeclarationNode()
        {
            DeclarationType = "Array";
            dimensions = null;
            ActualSymbol = null;
            initialisers = new ExpressionCollection();
        }

        public Symbol ActualSymbol { get; set; }

        public ExpressionNode Dimensions
        {
            get { return dimensions; }
            set { dimensions = value; }
        }

        public ExpressionCollection Initialisers
        {
            get { return initialisers; }
            set { initialisers = value; }
        }

        public SymbolTable.SymbolType ItemType
        {
            get { return itemType; }
            set { itemType = value; }
        }

        public void AddInitialiserExpression(ExpressionNode nd)
        {
            initialisers.InsertAtStart(nd);
        }

        public override bool Execute()
        {
            bool res = false;
            if (ActualSymbol != null)
            {
                int dim = 0;
                if (dimensions != null && EvalExpression(dimensions, ref dim, "Dimensions"))
                {
                    // tell the symbol how big the array should be
                    ActualSymbol.SetSize(dim);
                    res = true;
                }
                else
                {
                    if (initialisers != null && initialisers.Execute())
                    {
                        ActualSymbol.SetSize(initialisers.Count());
                        for (int i = 0; i < initialisers.Count(); i++)
                        {
                            PullInitialValues(ActualSymbol as ArraySymbol, i);
                        }
                        res = true;
                    }
                }
            }
            return res;
        }

        public override String ToRichText()
        {
            String result = "";
            if (!IsInLibrary)
            {
                result = Indentor.Indentation() + RichTextFormatter.KeyWord(GetTypeName() + " ") + "[";
                if (dimensions != null)
                {
                    result += dimensions.ToRichText();
                }
                result += "] " + RichTextFormatter.VariableName(VarName);
                if (initialisers != null && initialisers.Count() > 0)
                {
                    result += " = " + @"\par";
                    result += Indentor.Indentation() + @"\{\par ";
                    result += Indentor.Indentation() + Indentor.Indentation() + initialisers.ToRichText() + @"\par";
                    result += Indentor.Indentation() + @"\} ;\par";
                }
                else
                {
                    result += " ;";
                }
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
                result = Indentor.Indentation() + GetTypeName() + " [";
                if (dimensions != null)
                {
                    result += dimensions.ToString();
                }
                result += "] " + VarName;
                if (initialisers != null && initialisers.Count() > 0)
                {
                    result += "=\n";
                    result += Indentor.Indentation() + "{\n";
                    result += Indentor.Indentation() + initialisers.ToString();
                    result += "\n" + Indentor.Indentation() + "}";
                }
                result += " ;";
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

        private bool PullInitialValues(ArraySymbol arrSym, int arrayIndex)
        {
            bool result = false;

            if (arrayIndex < 0 || arrayIndex >= arrSym.Array.Length)
            {
                Log.Instance().AddEntry("Run Time Error : Array index out of bounds");
            }
            else
            {
                StackItem sti = ExecutionStack.Instance().Pull();
                if (sti != null)
                {
                    switch (sti.MyType)
                    {
                        case StackItem.ItemType.ival:
                            {
                                if (arrSym.ItemType == SymbolTable.SymbolType.intarrayvariable)
                                {
                                    arrSym.Array.Set(arrayIndex, sti.IntValue);
                                    result = true;
                                }
                                else
                                if (arrSym.ItemType == SymbolTable.SymbolType.doublearrayvariable)
                                {
                                    arrSym.Array.Set(arrayIndex, (double)sti.IntValue);
                                    result = true;
                                }
                            }
                            break;

                        case StackItem.ItemType.dval:
                            {
                                if (arrSym.ItemType == SymbolTable.SymbolType.doublearrayvariable)
                                {
                                    arrSym.Array.Set(arrayIndex, sti.DoubleValue);
                                    result = true;
                                }
                                else
                                 if (arrSym.ItemType == SymbolTable.SymbolType.intarrayvariable)
                                {
                                    arrSym.Array.Set(arrayIndex, (double)sti.IntValue);
                                    result = true;
                                }
                            }
                            break;

                        case StackItem.ItemType.bval:
                            {
                                if (arrSym.ItemType == SymbolTable.SymbolType.boolarrayvariable)
                                {
                                    arrSym.Array.Set(arrayIndex, sti.BooleanValue);
                                    result = true;
                                }
                                else
                                {
                                    Log.Instance().AddEntry("Run Time Error : Type mismatch in assignment");
                                }
                            }
                            break;

                        case StackItem.ItemType.sval:
                            {
                                if (arrSym.ItemType == SymbolTable.SymbolType.stringarrayvariable)
                                {
                                    arrSym.Array.Set(arrayIndex, sti.StringValue);
                                    result = true;
                                }
                                else
                                {
                                    Log.Instance().AddEntry("Run Time Error : Type mismatch in assignment");
                                }
                            }
                            break;

                        case StackItem.ItemType.hval:
                            {
                                if (arrSym.ItemType == SymbolTable.SymbolType.handlearrayvariable)
                                {
                                    arrSym.Array.Set(arrayIndex, sti.HandleValue);
                                    result = true;
                                }
                                else
                                {
                                    Log.Instance().AddEntry("Run Time Error : Type mismatch in assignment");
                                }
                            }
                            break;

                        case StackItem.ItemType.sldval:
                            {
                                if (arrSym.ItemType == SymbolTable.SymbolType.solidarrayvariable)
                                {
                                    arrSym.Array.Set(arrayIndex, sti.SolidValue);
                                    result = true;
                                }
                                else
                                {
                                    Log.Instance().AddEntry("Run Time Error : Type mismatch in assignment");
                                }
                            }
                            break;
                    }
                }
            }
            return result;
        }
    }
}