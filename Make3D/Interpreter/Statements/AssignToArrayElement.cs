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

                                AssignTopOfStackToVar(_VariableName, arrayIndex);
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
            StackItem sti = ExecutionStack.Instance().Pull();
            if (sti != null)
            {
                switch (sti.MyType)
                {
                    case StackItem.ItemType.ival:
                        {
                            Symbol sym = SymbolTable.Instance().FindSymbol(VariableName, SymbolTable.SymbolType.intvariable);
                            if (sym != null)
                            {
                                sym.IntValue = sti.IntValue;
                                result = true;
                            }
                            else
                            {
                                sym = SymbolTable.Instance().FindSymbol(VariableName, SymbolTable.SymbolType.doublevariable);
                                if (sym != null)
                                {
                                    sym.DoubleValue = (double)sti.IntValue;
                                    result = true;
                                }
                                else
                                {
                                    sym = SymbolTable.Instance().FindSymbol(VariableName, SymbolTable.SymbolType.solidvariable);
                                    if (sym != null)
                                    {
                                        sym.SolidValue = (int)sti.IntValue;
                                        result = true;
                                    }
                                    else
                                    {
                                        Log.Instance().AddEntry("Run Time Error : Type mismatch in assignment");
                                    }
                                }
                            }
                        }
                        break;

                    case StackItem.ItemType.dval:
                        {
                            Symbol sym = SymbolTable.Instance().FindSymbol(VariableName, SymbolTable.SymbolType.doublevariable);
                            if (sym != null)
                            {
                                sym.DoubleValue = sti.DoubleValue;
                                result = true;
                            }
                            else
                            {
                                sym = SymbolTable.Instance().FindSymbol(VariableName, SymbolTable.SymbolType.intvariable);
                                if (sym != null)
                                {
                                    sym.IntValue = (int)Math.Floor(sti.DoubleValue);
                                    result = true;
                                }
                                else
                                {
                                    Log.Instance().AddEntry("Run Time Error : Type mismatch in assignment");
                                }
                            }
                        }
                        break;

                    case StackItem.ItemType.bval:
                        {
                            Symbol sym = SymbolTable.Instance().FindSymbol(VariableName, SymbolTable.SymbolType.boolarrayvariable);
                            if (sym != null)
                            {
                                // ************************************* WRONG *********************/
                                sym.BooleanValue = sti.BooleanValue;
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
                            Symbol sym = SymbolTable.Instance().FindSymbol(VariableName, SymbolTable.SymbolType.stringvariable);
                            if (sym != null)
                            {
                                sym.StringValue = sti.StringValue;
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
                            Symbol sym = SymbolTable.Instance().FindSymbol(VariableName, SymbolTable.SymbolType.handlevariable);
                            if (sym != null)
                            {
                                sym.HandleValue = sti.HandleValue;
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
                            Symbol sym = SymbolTable.Instance().FindSymbol(VariableName, SymbolTable.SymbolType.solidvariable);
                            if (sym != null)
                            {
                                sym.SolidValue = sti.SolidValue;
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
            return result;
        }
    }
}