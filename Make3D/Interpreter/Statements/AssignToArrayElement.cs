using System;

namespace ScriptLanguage
{
    internal class AssignToArrayElement : StatementNode
    {
        private String _ExternalName;
        private String _VariableName;
        private ExpressionNode indexExpression;
        private ExpressionNode valueExpression;

        // Instance constructor
        public AssignToArrayElement()
        {
            _VariableName = "";
            valueExpression = null;
            indexExpression = null;
        }

        public String ExternalName
        {
            get { return _ExternalName; }
            set { _ExternalName = value; }
        }

        public ExpressionNode IndexExpression
        {
            get { return indexExpression; }
            set { indexExpression = value; }
        }

        public ExpressionNode ValueExpression
        {
            get { return valueExpression; }
            set { valueExpression = value; }
        }

        public String VariableName
        {
            get { return _VariableName; }
            set { _VariableName = value; }
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            if (valueExpression != null)
            {
                result = valueExpression.Execute();
                if (result)
                {
                    result = indexExpression.Execute();
                    if (result)
                    {
                        StackItem sti = ExecutionStack.Instance().Pull();
                        if (sti != null)
                        {
                            if (sti.MyType == StackItem.ItemType.ival)
                            {
                                int arrayIndex = sti.IntValue;

                                result = AssignTopOfStackToVar(_VariableName, arrayIndex);
                            }
                        }
                    }
                }
            }
            return result;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = "";
            if (!IsInLibrary)
            {
                result = Indentor.Indentation() + RichTextFormatter.VariableName(_ExternalName) + "[" + IndexExpression.ToRichText() + "]" + RichTextFormatter.Operator(" = ") + valueExpression.ToRichText() + " ;";
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
                result = Indentor.Indentation() + _ExternalName + "[" + IndexExpression.ToString() + "] = " + valueExpression.ToString() + " ;";
            }
            return result;
        }

        private bool AssignTopOfStackToVar(string VariableName, int arrayIndex)
        {
            bool result = false;
            ArraySymbol sym = SymbolTable.Instance().FindArraySymbol(VariableName);
            if (arrayIndex < 0 || arrayIndex >= sym.Array.Length)
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
                                if (sym.ItemType == SymbolTable.SymbolType.intarrayvariable)
                                {
                                    sym.Array.Set(arrayIndex, sti.IntValue);
                                    result = true;
                                }
                                else
                                if (sym.ItemType == SymbolTable.SymbolType.doublearrayvariable)
                                {
                                    sym.Array.Set(arrayIndex, (double)sti.IntValue);
                                    result = true;
                                }
                            }
                            break;

                        case StackItem.ItemType.dval:
                            {
                                if (sym.ItemType == SymbolTable.SymbolType.doublearrayvariable)
                                {
                                    sym.Array.Set(arrayIndex, sti.DoubleValue);
                                    result = true;
                                }
                                else
                                 if (sym.ItemType == SymbolTable.SymbolType.intarrayvariable)
                                {
                                    sym.Array.Set(arrayIndex, (double)sti.IntValue);
                                    result = true;
                                }
                            }
                            break;

                        case StackItem.ItemType.bval:
                            {
                                if (sym.ItemType == SymbolTable.SymbolType.boolarrayvariable)
                                {
                                    sym.Array.Set(arrayIndex, sti.BooleanValue);
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
                                if (sym.ItemType == SymbolTable.SymbolType.stringarrayvariable)
                                {
                                    sym.Array.Set(arrayIndex, sti.StringValue);
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
                                if (sym.ItemType == SymbolTable.SymbolType.handlearrayvariable)
                                {
                                    sym.Array.Set(arrayIndex, sti.HandleValue);
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
                                if (sym.ItemType == SymbolTable.SymbolType.solidarrayvariable)
                                {
                                    sym.Array.Set(arrayIndex, sti.SolidValue);
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