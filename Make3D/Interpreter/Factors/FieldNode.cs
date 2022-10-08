using System;

namespace ScriptLanguage
{
    internal class FieldNode : ExpressionNode
    {
        private String externalObjectName;

        private String fieldName;

        private Symbol objectSymbol;

        // externalObjectName.FieldName
        private String objectSymbolName;

        // Instance constructor
        public FieldNode()
        {
            externalObjectName = "";
            objectSymbolName = "";
            fieldName = "";
            objectSymbol = null;
        }

        // Copy constructor
        public FieldNode(FieldNode it)
        {
            objectSymbolName = it.ObjectSymbolName;
            externalObjectName = it.ExternalObjectName;
            fieldName = it.FieldName;
        }

        public String ExternalObjectName
        {
            get { return externalObjectName; }
            set { externalObjectName = value; }
        }

        public String FieldName
        {
            get { return fieldName; }
            set { fieldName = value; }
        }

        public String ObjectSymbolName
        {
            get { return objectSymbolName; }
            set { objectSymbolName = value; }
        }

        public Symbol Symbol
        {
            set { objectSymbol = value; }
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            if (objectSymbol != null)
            {
                result = PushField((objectSymbol as StructSymbol).FieldValues);
            }

            return result;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = RichTextFormatter.VariableName(externalObjectName) + ".";
            result += RichTextFormatter.VariableName(fieldName);
            return result;
        }

        public override String ToString()
        {
            String result = externalObjectName + "." + fieldName;
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