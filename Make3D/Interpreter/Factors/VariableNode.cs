using System;

namespace ScriptLanguage
{
    internal class VariableNode : ExpressionNode
    {
        private String externalName;
        private String name;

        private Symbol symbol;

        // Instance constructor
        public VariableNode()
        {
            name = "";
            symbol = null;
        }

        // Copy constructor
        public VariableNode(VariableNode it)
        {
            name = it.Name;
        }

        public String ExternalName
        {
            get { return externalName; }
            set { externalName = value; }
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

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            if (symbol != null)
            {
                result = PushSymbol(symbol);
            }
            return result;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = RichTextFormatter.VariableName(externalName);
            return result;
        }

        public override String ToString()
        {
            String result = externalName;
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