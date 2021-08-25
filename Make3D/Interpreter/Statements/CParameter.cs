using System;

namespace ScriptLanguage
{
    public class CParameter : ParseTreeNode
    {
        // the target symbol that will recieve  a value passed on the stack
        private Symbol associatedSymbol;

        private String name;
        private SymbolTable.SymbolType paramSymbolType;

        // Instance constructor
        public CParameter()
        {
            paramSymbolType = 0;
            name = "";
            StructDefinition = null;
        }

        // Copy constructor
        public CParameter(CParameter it)
        {
            paramSymbolType = it.ParamType;
            name = it.Name;
            associatedSymbol = null;
        }

        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        public SymbolTable.SymbolType ParamType
        {
            get { return paramSymbolType; }
            set { paramSymbolType = value; }
        }

        public StructDefinition StructDefinition
        {
            get; set;
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            if (associatedSymbol == null)
            {
                associatedSymbol = SymbolTable.Instance().FindSymbol(name, paramSymbolType);
            }

            if (associatedSymbol != null)
            {
                result = PullParam(associatedSymbol);
            }
            else
            {
                Log.Instance().AddEntry("Run time error parameter type mismatch in procedure call");
            }

            return result;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public String ToRichText(String ParentName)
        {
            String PType = "";
            switch (paramSymbolType)
            {
                case SymbolTable.SymbolType.boolvariable:
                    {
                        PType = RichTextFormatter.KeyWord("Bool ");
                        break;
                    }

                case SymbolTable.SymbolType.boolarrayvariable:
                    {
                        PType = RichTextFormatter.KeyWord("Bool [] ");
                        break;
                    }

                case SymbolTable.SymbolType.doublevariable:
                    {
                        PType = RichTextFormatter.KeyWord("Double ");
                        break;
                    }
                case SymbolTable.SymbolType.doublearrayvariable:
                    {
                        PType = RichTextFormatter.KeyWord("Double [] ");
                        break;
                    }

                case SymbolTable.SymbolType.handlevariable:
                    {
                        PType = RichTextFormatter.KeyWord("Handle ");
                        break;
                    }
                case SymbolTable.SymbolType.handlearrayvariable:
                    {
                        PType = RichTextFormatter.KeyWord("Handle [] ");
                        break;
                    }

                case SymbolTable.SymbolType.solidvariable:
                    {
                        PType = RichTextFormatter.KeyWord("Solid ");
                        break;
                    }
                case SymbolTable.SymbolType.solidarrayvariable:
                    {
                        PType = RichTextFormatter.KeyWord("Solid [] ");
                        break;
                    }
                case SymbolTable.SymbolType.intvariable:
                    {
                        PType = RichTextFormatter.KeyWord("Int ");
                        break;
                    }
                case SymbolTable.SymbolType.intarrayvariable:
                    {
                        PType = RichTextFormatter.KeyWord("Int [] ");
                        break;
                    }
                case SymbolTable.SymbolType.stringvariable:
                    {
                        PType = RichTextFormatter.KeyWord("String ");
                        break;
                    }
                case SymbolTable.SymbolType.stringarrayvariable:
                    {
                        PType = RichTextFormatter.KeyWord("String []");
                        break;
                    }

                case SymbolTable.SymbolType.structname:
                    {
                        if (StructDefinition != null)
                        {
                            PType = RichTextFormatter.KeyWord(StructDefinition.StructName);
                            break;
                        }
                    }
                    break;
            }
            String ParamName = name.Substring(ParentName.Length);
            String result = PType + " " + RichTextFormatter.VariableName(ParamName);
            return result;
        }

        public String ToText(String ParentName)
        {
            String PType = "";
            switch (paramSymbolType)
            {
                case SymbolTable.SymbolType.boolvariable:
                    {
                        PType = "Bool ";
                        break;
                    }

                case SymbolTable.SymbolType.boolarrayvariable:
                    {
                        PType = "Bool [] ";
                        break;
                    }
                case SymbolTable.SymbolType.doublevariable:
                    {
                        PType = "Double ";
                        break;
                    }

                case SymbolTable.SymbolType.doublearrayvariable:
                    {
                        PType = "Double [] ";
                        break;
                    }
                case SymbolTable.SymbolType.handlevariable:
                    {
                        PType = "Handle ";
                        break;
                    }

                case SymbolTable.SymbolType.handlearrayvariable:
                    {
                        PType = "Handle [] ";
                        break;
                    }
                case SymbolTable.SymbolType.solidvariable:
                    {
                        PType = "Solid ";
                        break;
                    }

                case SymbolTable.SymbolType.solidarrayvariable:
                    {
                        PType = "Solid [] ";
                        break;
                    }
                case SymbolTable.SymbolType.intvariable:
                    {
                        PType = "Int ";
                        break;
                    }
                case SymbolTable.SymbolType.intarrayvariable:
                    {
                        PType = "Int [] ";
                        break;
                    }
                case SymbolTable.SymbolType.stringvariable:
                    {
                        PType = "String ";
                        break;
                    }

                case SymbolTable.SymbolType.stringarrayvariable:
                    {
                        PType = "String []";
                        break;
                    }
                case SymbolTable.SymbolType.structname:
                    {
                        if (StructDefinition != null)
                        {
                            PType = RichTextFormatter.KeyWord(StructDefinition.StructName);
                            break;
                        }
                    }
                    break;
            }
            String ParamName = name.Substring(ParentName.Length);
            String result = PType + " " + ParamName;
            return result;
        }

        private static StackItem GetArrayRef(Symbol sym, ref bool result)
        {
            StackItem sti = ExecutionStack.Instance().Pull();
            if (sti.MyType == StackItem.ItemType.arrayval)
            {
                if ((sti.ObjectValue as ArraySymbol).SymbolType == sym.SymbolType)
                {
                    (sym as ArraySymbol).Array = (sti.ObjectValue as ArraySymbol).Array;
                    result = true;
                }
                else
                {
                    Log.Instance().AddEntry("Run time error parameter type mismatch in procedure call");
                }
            }

            return sti;
        }

        private bool PullParam(Symbol sym)
        {
            bool result = false;

            StackItem sti;

            switch (sym.SymbolType)
            {
                case SymbolTable.SymbolType.boolvariable:
                    {
                        sti = ExecutionStack.Instance().Pull();
                        if (sti.MyType == StackItem.ItemType.bval)
                        {
                            sym.BooleanValue = sti.BooleanValue;
                            result = true;
                        }
                    }
                    break;

                case SymbolTable.SymbolType.intvariable:
                    {
                        sti = ExecutionStack.Instance().Pull();
                        if (sti.MyType == StackItem.ItemType.ival)
                        {
                            sym.IntValue = sti.IntValue;
                            result = true;
                        }
                    }
                    break;

                case SymbolTable.SymbolType.doublevariable:
                    {
                        sti = ExecutionStack.Instance().Pull();
                        if (sti.MyType == StackItem.ItemType.dval)
                        {
                            sym.DoubleValue = sti.DoubleValue;
                            result = true;
                        }
                    }
                    break;

                case SymbolTable.SymbolType.boolarrayvariable:
                case SymbolTable.SymbolType.intarrayvariable:
                case SymbolTable.SymbolType.doublearrayvariable:
                case SymbolTable.SymbolType.stringarrayvariable:
                case SymbolTable.SymbolType.handlearrayvariable:
                case SymbolTable.SymbolType.solidarrayvariable:
                    {
                        sti = GetArrayRef(sym, ref result);
                    }
                    break;

                case SymbolTable.SymbolType.stringvariable:
                    {
                        sti = ExecutionStack.Instance().Pull();
                        if (sti.MyType == StackItem.ItemType.sval)
                        {
                            sym.StringValue = sti.StringValue;
                            result = true;
                        }
                    }
                    break;

                case SymbolTable.SymbolType.handlevariable:
                    {
                        sti = ExecutionStack.Instance().Pull();
                        if (sti.MyType == StackItem.ItemType.hval)
                        {
                            sym.HandleValue = sti.HandleValue;
                            result = true;
                        }
                    }
                    break;

                case SymbolTable.SymbolType.solidvariable:
                    {
                        sti = ExecutionStack.Instance().Pull();
                        if (sti.MyType == StackItem.ItemType.sldval)
                        {
                            sym.SolidValue = sti.SolidValue;
                            result = true;
                        }
                    }
                    break;

                case SymbolTable.SymbolType.structname:
                    {
                        StructSymbol structSym = sym as StructSymbol;
                        result = true;
                        // pull struct fields in reverse order
                        for (int j = structSym.FieldValues.Symbols.Count - 1; j >= 0 && result == true; j--)
                        {
                            Symbol field = structSym.FieldValues.Symbols[j];

                            result = PullParam(field);
                        }
                    }
                    break;
            }

            return result;
        }
    }
}