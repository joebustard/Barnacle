using System;

namespace ScriptLanguage
{
    internal class AssignStructToArrayElement : StatementNode
    {
        private String _ExternalName;
        private String _VariableName;
        private ExpressionNode indexExpression;
        private ExpressionNode valueExpression;

        // Instance constructor
        public AssignStructToArrayElement()
        {
            _VariableName = "";
            valueExpression = null;
            indexExpression = null;
        }

        public StructArraySymbol ActualSymbol { get; internal set; }

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
            bool ok = true;
            if (arrayIndex < 0 || arrayIndex >= ActualSymbol.Array.Length)
            {
                Log.Instance().AddEntry("Run Time Error : Array index out of bounds");
            }
            else
            {
                Array entry = (Array)ActualSymbol.Array.Get(arrayIndex);
                // for all fields in the struct
                for (int field = ActualSymbol.Structure.Fields.Count - 1; field >= 0 && ok; field--)
                {
                    ok = false;
                    SymbolTable.SymbolType ty = SymbolTable.Instance().GetFieldType(ActualSymbol.Structure.Fields[field].SymType);
                    StackItem sti = ExecutionStack.Instance().Pull();
                    if (sti != null)
                    {
                        switch (ty)
                        {
                            case SymbolTable.SymbolType.intvariable:
                                {
                                    if (sti.MyType == StackItem.ItemType.ival)
                                    {
                                        entry.Set(field, sti.IntValue);
                                        ok = true;
                                    }
                                    else if (sti.MyType == StackItem.ItemType.dval)
                                    {
                                        entry.Set(field, (int)sti.DoubleValue);
                                        ok = true;
                                    }
                                }
                                break;

                            case SymbolTable.SymbolType.doublevariable:
                                {
                                    if (sti.MyType == StackItem.ItemType.ival)
                                    {
                                        entry.Set(field, (double)sti.IntValue);
                                        ok = true;
                                    }
                                    else if (sti.MyType == StackItem.ItemType.dval)
                                    {
                                        entry.Set(field, sti.DoubleValue);
                                        ok = true;
                                    }
                                }
                                break;

                            case SymbolTable.SymbolType.boolvariable:
                                {
                                    if (sti.MyType == StackItem.ItemType.bval)
                                    {
                                        entry.Set(field, sti.BooleanValue);
                                        ok = true;
                                    }
                                }
                                break;

                            case SymbolTable.SymbolType.stringvariable:
                                {
                                    if (sti.MyType == StackItem.ItemType.sval)
                                    {
                                        entry.Set(field, sti.StringValue);
                                        ok = true;
                                    }
                                }
                                break;

                            case SymbolTable.SymbolType.solidvariable:
                                {
                                    if (sti.MyType == StackItem.ItemType.sldval)
                                    {
                                        entry.Set(field, sti.SolidValue);
                                        ok = true;
                                    }
                                }
                                break;

                            case SymbolTable.SymbolType.handlevariable:
                                {
                                    if (sti.MyType == StackItem.ItemType.hval)
                                    {
                                        entry.Set(field, sti.HandleValue);
                                        ok = true;
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            result = ok;
            return result;
        }
    }
}