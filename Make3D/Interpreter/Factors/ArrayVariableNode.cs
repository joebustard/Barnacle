using System;

namespace ScriptLanguage
{
    internal class ArrayVariableNode : ExpressionNode
    {
        private String externalName;
        private ExpressionNode indexExpression;
        private String name;

        private ArraySymbol symbol;

        // Instance constructor
        public ArrayVariableNode()
        {
            name = "";
            symbol = null;
            indexExpression = null;
        }

        public String ExternalName
        {
            get { return externalName; }
            set { externalName = value; }
        }

        public ExpressionNode IndexExpression
        {
            get
            {
                return indexExpression;
            }

            set
            {
                indexExpression = value;
            }
        }

        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        public ArraySymbol Symbol
        {
            set { symbol = value; }
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            result = indexExpression.Execute();
            if (result)
            {
                StackItem sti = ExecutionStack.Instance().Pull();
                if (sti != null)
                {
                    if (sti.MyType == StackItem.ItemType.ival)
                    {
                        int arrayIndex = sti.IntValue;
                        if (symbol != null)
                        {
                            if (arrayIndex < 0 || arrayIndex >= symbol.Array.Length)
                            {
                                Log.Instance().AddEntry("Run Time Error : Array index out of bounds");
                            }
                            else
                            {
                                object obj = symbol.Array.Get(arrayIndex);
                                if (obj != null)
                                {
                                    switch (symbol.SymbolType)
                                    {
                                        case SymbolTable.SymbolType.boolarrayvariable:
                                            {
                                                ExecutionStack.Instance().Push((bool)obj);
                                            }
                                            break;

                                        case SymbolTable.SymbolType.intarrayvariable:
                                            {
                                                ExecutionStack.Instance().Push((int)obj);
                                            }
                                            break;

                                        case SymbolTable.SymbolType.doublearrayvariable:
                                            {
                                                ExecutionStack.Instance().Push((double)obj);
                                            }
                                            break;

                                        case SymbolTable.SymbolType.stringarrayvariable:
                                            {
                                                ExecutionStack.Instance().Push((string)obj);
                                            }
                                            break;

                                        case SymbolTable.SymbolType.handlearrayvariable:
                                            {
                                                ExecutionStack.Instance().Push((int)obj);
                                            }
                                            break;

                                        case SymbolTable.SymbolType.solidarrayvariable:
                                            {
                                                ExecutionStack.Instance().PushSolid((int)obj);
                                            }
                                            break;
                                    }
                                    result = true;
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
        ///
        ///
        public override String ToRichText()
        {
            String result = RichTextFormatter.VariableName(externalName) + "[ " + indexExpression.ToRichText() + " ]";
            return result;
        }

        public override String ToString()
        {
            String result = externalName + "[ " + indexExpression.ToString() + " ]"; ;
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
                        ExecutionStack.Instance().PushSolid(symbol.SolidValue);
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