using System;

namespace ScriptLanguage
{
    internal class AssignWholeStructNode : CStatementNode
    {
        private String leftExternalName;
        private String leftVariableName;
        private String rightExternalName;
        private String rightVariableName;

        // Instance constructor
        public AssignWholeStructNode()
        {
            leftVariableName = "";
            rightVariableName = "";
        }

        public String LeftExternalName
        {
            get { return leftExternalName; }
            set { leftExternalName = value; }
        }

        public String LeftVariableName
        {
            get { return leftVariableName; }
            set { leftVariableName = value; }
        }

        public String RightExternalName
        {
            get { return rightExternalName; }
            set { rightExternalName = value; }
        }

        public String RightVariableName
        {
            get { return rightVariableName; }
            set { rightVariableName = value; }
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            StructSymbol leftsym = SymbolTable.Instance().FindSymbol(leftVariableName, SymbolTable.SymbolType.structname) as StructSymbol;
            if (leftsym != null)
            {
                StructSymbol rightsym = SymbolTable.Instance().FindSymbol(rightVariableName, SymbolTable.SymbolType.structname) as StructSymbol;
                if (rightsym != null)
                {
                    if (leftsym.Definition == rightsym.Definition)
                    {
                        for (int i = 0; i < rightsym.FieldValues.Symbols.Count; i++)
                        {
                            Symbol sym = rightsym.FieldValues.Symbols[i];
                            switch (sym.SymbolType)
                            {
                                case SymbolTable.SymbolType.boolvariable:
                                    {
                                        leftsym.FieldValues.Symbols[i].BooleanValue = sym.BooleanValue;
                                    }
                                    break;

                                case SymbolTable.SymbolType.intvariable:
                                    {
                                        leftsym.FieldValues.Symbols[i].IntValue = sym.IntValue;
                                    }
                                    break;

                                case SymbolTable.SymbolType.doublevariable:
                                    {
                                        leftsym.FieldValues.Symbols[i].DoubleValue = sym.DoubleValue;
                                    }
                                    break;

                                case SymbolTable.SymbolType.stringvariable:
                                    {
                                        leftsym.FieldValues.Symbols[i].StringValue = sym.StringValue;
                                    }
                                    break;

                                case SymbolTable.SymbolType.handlevariable:
                                    {
                                        leftsym.FieldValues.Symbols[i].HandleValue = sym.HandleValue;
                                    }
                                    break;

                                case SymbolTable.SymbolType.solidvariable:
                                    {
                                        leftsym.FieldValues.Symbols[i].HandleValue = sym.HandleValue;
                                    }
                                    break;
                            }
                        }
                        result = true;
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
                result = Indentor.Indentation() + leftExternalName + RichTextFormatter.Operator(" = ") + rightExternalName + " ;";
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
                result = Indentor.Indentation() + leftExternalName + " = " + rightExternalName + " ;";
            }
            return result;
        }

        private bool AssignTopOfStackToVar(string VariableName)
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
                                    Log.Instance().AddEntry("Run Time Error : Type mismatch in assignment");
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
                            Symbol sym = SymbolTable.Instance().FindSymbol(VariableName, SymbolTable.SymbolType.boolvariable);
                            if (sym != null)
                            {
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