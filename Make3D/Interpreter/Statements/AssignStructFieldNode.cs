using System;

namespace ScriptLanguage
{
    internal class AssignStructFieldNode : StatementNode
    {
        private ExpressionNode _ExpressionNode;
        private String externalName;
        private String fieldName;
        private String variableName;

        // Instance constructor
        public AssignStructFieldNode()
        {
            variableName = "";
            fieldName = "";
            _ExpressionNode = null;
        }

        public ExpressionNode ExpressionNode
        {
            get { return _ExpressionNode; }
            set { _ExpressionNode = value; }
        }

        public String ExternalName
        {
            get { return externalName; }
            set { externalName = value; }
        }

        public String FieldName
        {
            get { return fieldName; }
            set { fieldName = value; }
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
            if (_ExpressionNode != null)
            {
                result = _ExpressionNode.Execute();
                if (result)
                {
                    // FInd the symbol for the struct
                    Symbol strsym = SymbolTable.Instance().FindSymbol(VariableName, SymbolTable.SymbolType.structname);
                    if (strsym != null)
                    {
                        result = AssignTopOfStackToField((strsym as StructSymbol).FieldValues);
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
                result = Indentor.Indentation() + RichTextFormatter.VariableName(externalName) + "." + RichTextFormatter.VariableName(fieldName) + RichTextFormatter.Operator(" = ") + _ExpressionNode.ToRichText() + " ;";
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
                result = Indentor.Indentation() + externalName + "." + fieldName + " = " + _ExpressionNode.ToString() + " ;";
            }
            return result;
        }

        private bool AssignTopOfStackToField(SymbolTable st)
        {
            bool result = false;
            StackItem sti = ExecutionStack.Instance().Pull();
            if (sti != null)
            {
                switch (sti.MyType)
                {
                    case StackItem.ItemType.ival:
                        {
                            Symbol sym = st.FindSymbol(fieldName, SymbolTable.SymbolType.intvariable);
                            if (sym != null)
                            {
                                sym.IntValue = sti.IntValue;
                                result = true;
                            }
                            else
                            {
                                sym = st.FindSymbol(fieldName, SymbolTable.SymbolType.doublevariable);
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
                            Symbol sym = st.FindSymbol(fieldName, SymbolTable.SymbolType.doublevariable);
                            if (sym != null)
                            {
                                sym.DoubleValue = sti.DoubleValue;
                                result = true;
                            }
                            else
                            {
                                sym = st.FindSymbol(fieldName, SymbolTable.SymbolType.intvariable);
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
                            Symbol sym = st.FindSymbol(fieldName, SymbolTable.SymbolType.boolvariable);
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
                            Symbol sym = st.FindSymbol(fieldName, SymbolTable.SymbolType.stringvariable);
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
                            Symbol sym = st.FindSymbol(fieldName, SymbolTable.SymbolType.handlevariable);
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
                            Symbol sym = st.FindSymbol(fieldName, SymbolTable.SymbolType.solidvariable);
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