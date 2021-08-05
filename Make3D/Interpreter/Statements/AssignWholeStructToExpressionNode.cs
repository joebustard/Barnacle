using System;

namespace ScriptLanguage
{
    internal class AssignExpressionToStructNode : CStatementNode
    {
        private ExpressionNode expressionNode;
        private String externalName;
        private String variableName;

        // Instance constructor
        public AssignExpressionToStructNode()
        {
            variableName = "";

            expressionNode = null;
        }

        public ExpressionNode ExpressionNode
        {
            get { return expressionNode; }
            set { expressionNode = value; }
        }

        public String ExternalName
        {
            get { return externalName; }
            set { externalName = value; }
        }

        public String VariableName
        {
            get { return variableName; }
            set { variableName = value; }
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            if (expressionNode != null)
            {
                result = expressionNode.Execute();
                if (result)
                {
                    // FInd the symbol for the struct
                    Symbol sym = SymbolTable.Instance().FindSymbol(VariableName, SymbolTable.SymbolType.structname);
                    StructSymbol strsym = sym as StructSymbol;
                    if (strsym != null)
                    {
                        int i = strsym.FieldValues.Symbols.Count - 1;
                        while (i >= 0)
                        {
                            result = AssignTopOfStackToField(strsym.FieldValues.Symbols[i]);
                            i--;
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
                result = Indentor.Indentation() + RichTextFormatter.VariableName(externalName) + RichTextFormatter.Operator(" = ") + expressionNode.ToRichText() + " ;";
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
                result = Indentor.Indentation() + externalName + " = " + expressionNode.ToString() + " ;";
            }
            return result;
        }

        private bool AssignTopOfStackToField(Symbol sym)
        {
            bool result = false;

            StackItem sti = ExecutionStack.Instance().Pull();
            if (sti != null)
            {
                switch (sti.MyType)
                {
                    case StackItem.ItemType.ival:
                        {
                            if (sym.SymbolType == SymbolTable.SymbolType.intvariable)
                            {
                                sym.IntValue = sti.IntValue;
                                result = true;
                            }
                            else
                            {
                                if (sym.SymbolType == SymbolTable.SymbolType.doublevariable)
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
                            if (sym.SymbolType == SymbolTable.SymbolType.doublevariable)
                            {
                                sym.DoubleValue = sti.DoubleValue;
                                result = true;
                            }
                            else
                            {
                                if (sym.SymbolType == SymbolTable.SymbolType.intvariable)
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
                            if (sym.SymbolType == SymbolTable.SymbolType.boolvariable)
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
                            if (sym.SymbolType == SymbolTable.SymbolType.stringvariable)
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
                            if (sym.SymbolType == SymbolTable.SymbolType.handlevariable)
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
                            if (sym.SymbolType == SymbolTable.SymbolType.solidvariable)
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