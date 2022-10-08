using System;

namespace ScriptLanguage
{
    internal class StructArrayFieldNode : StructArrayVariableNode
    {
        private String fieldName;
        private int fieldNumber;

        // Instance constructor
        public StructArrayFieldNode()
        {
            fieldName = "";
        }

        public String FieldName
        {
            get { return fieldName; }
            set { fieldName = value; }
        }

        public int FieldNumber
        {
            get { return fieldNumber; }
            set { fieldNumber = value; }
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            result = IndexExpression.Execute();
            if (result)
            {
                StackItem sti = ExecutionStack.Instance().Pull();
                if (sti != null)
                {
                    if (sti.MyType == StackItem.ItemType.ival)
                    {
                        int arrayIndex = sti.IntValue;
                        if (Symbol != null)
                        {
                            if (arrayIndex < 0 || arrayIndex >= Symbol.Array.Length)
                            {
                                Log.Instance().AddEntry("Run Time Error : Array index out of bounds");
                            }
                            else
                            {
                                Array obj = (Array)Symbol.Array.Get(arrayIndex);
                                if (obj != null)
                                {
                                    SymbolTable.SymbolType ty = SymbolTable.Instance().GetFieldType(Symbol.Structure.Fields[fieldNumber].SymType);
                                    switch (ty)
                                    {
                                        case SymbolTable.SymbolType.boolvariable:
                                            {
                                                ExecutionStack.Instance().Push((bool)obj.Get(fieldNumber));
                                                result = true;
                                            }
                                            break;

                                        case SymbolTable.SymbolType.intvariable:
                                            {
                                                ExecutionStack.Instance().Push((int)obj.Get(fieldNumber));
                                                result = true;
                                            }
                                            break;

                                        case SymbolTable.SymbolType.doublevariable:
                                            {
                                                ExecutionStack.Instance().Push((double)obj.Get(fieldNumber));
                                                result = true;
                                            }
                                            break;

                                        case SymbolTable.SymbolType.stringvariable:
                                            {
                                                ExecutionStack.Instance().Push((string)obj.Get(fieldNumber));
                                                result = true;
                                            }
                                            break;

                                        case SymbolTable.SymbolType.handlevariable:
                                            {
                                                ExecutionStack.Instance().Push((int)obj.Get(fieldNumber));
                                                result = true;
                                            }
                                            break;

                                        case SymbolTable.SymbolType.solidvariable:
                                            {
                                                ExecutionStack.Instance().PushSolid((int)obj.Get(fieldNumber));
                                                result = true;
                                            }
                                            break;

                                        default:
                                            {
                                                Log.Instance().AddEntry("Run Time Error : Stuct Array Field type error");
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        public override String ToRichText()
        {
            String result = RichTextFormatter.VariableName(ExternalName) + "[ " + IndexExpression.ToRichText() + " ]." + FieldName; ;
            return result;
        }

        public override String ToString()
        {
            String result = ExternalName + "[ " + IndexExpression.ToString() + " ]." + FieldName; ;
            return result;
        }

        private bool PushField(SymbolTable fieldValues)
        {
            bool result = false;
            foreach (Symbol sym in fieldValues.Symbols)
            {
                if (sym.Name == FieldName)
                {
                    PushSymbol(sym);
                    result = true;
                    break;
                }
            }

            return result;
        }

        private bool PushSymbol(Symbol symbol)
        {
            bool result = false;
            switch (symbol.SymbolType)
            {
                case SymbolTable.SymbolType.boolvariable:
                    {
                        ExecutionStack.Instance().Push(symbol.BooleanValue);
                        result = true;
                    }
                    break;

                case SymbolTable.SymbolType.intvariable:
                    {
                        ExecutionStack.Instance().Push(symbol.IntValue);
                        result = true;
                    }
                    break;

                case SymbolTable.SymbolType.doublevariable:
                    {
                        ExecutionStack.Instance().Push(symbol.DoubleValue);
                        result = true;
                    }
                    break;

                case SymbolTable.SymbolType.stringvariable:
                    {
                        ExecutionStack.Instance().Push(symbol.StringValue);
                        result = true;
                    }
                    break;

                case SymbolTable.SymbolType.handlevariable:
                    {
                        ExecutionStack.Instance().Push(symbol.HandleValue);
                        result = true;
                    }
                    break;

                case SymbolTable.SymbolType.solidvariable:
                    {
                        ExecutionStack.Instance().Push(symbol.SolidValue);
                        result = true;
                    }
                    break;

                case SymbolTable.SymbolType.structname:
                    {
                        StructSymbol structSym = symbol as StructSymbol;
                        foreach (Symbol sym in structSym.FieldValues.Symbols)
                        {
                            PushSymbol(sym);
                        }
                        result = true;
                    }
                    break;
            }
            return result;
        }
    }
}