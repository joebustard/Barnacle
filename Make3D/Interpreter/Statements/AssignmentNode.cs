using System;

namespace ScriptLanguage
{
    internal class AssignmentNode : StatementNode
    {
        private ExpressionNode _ExpressionNode;
        private String _ExternalName;
        private String _VariableName;

        // Instance constructor
        public AssignmentNode()
        {
            _VariableName = "";
            _ExpressionNode = null;
        }

        public ExpressionNode ExpressionNode
        {
            get
            {
                return _ExpressionNode;
            }
            set
            {
                _ExpressionNode = value;
            }
        }

        public String ExternalName
        {
            get
            {
                return _ExternalName;
            }
            set
            {
                _ExternalName = value;
            }
        }

        public String VariableName
        {
            get
            {
                return _VariableName;
            }
            set
            {
                _VariableName = value;
            }
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
                    result = AssignTopOfStackToVar(_VariableName);
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
                result = Indentor.Indentation() + RichTextFormatter.VariableName(_ExternalName) + RichTextFormatter.Operator(" = ") + _ExpressionNode.ToRichText() + " ;";
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
                result = Indentor.Indentation() + _ExternalName + " = " + _ExpressionNode.ToString() + " ;";
            }
            return result;
        }

        private bool AssignTopOfStackToVar(string VariableName)
        {
            bool result = false;
            //  Log.Instance().AddEntry( $"AssignTopOfStackToVar {VariableName}");
            StackItem sti = ExecutionStack.Instance().Pull();
            //    Log.Instance().AddEntry($"AssignTopOfStackToVar {sti.MyType.ToString()}");
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
                                        Log.Instance().AddEntry($" {VariableName} is set to {(int)sti.IntValue}");
                                        result = true;
                                    }
                                    else
                                    {
                                        ReportStatement();
                                        Log.Instance().AddEntry($"Run Time Error : Type mismatch in assignment {VariableName} is not an int, double or solid, expression is int");
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
                                    ReportStatement();
                                    Log.Instance().AddEntry($"Run Time Error : Type mismatch in assignment. Variable {VariableName} is not an int or double. Expression is double");
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
                                ReportStatement();
                                Log.Instance().AddEntry("Run Time Error : Type mismatch in assignment. Variable {VariableName} is not a bool.");
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
                                ReportStatement($"Run Time Error : Type mismatch in assignment. Variable {VariableName} is not a string.");
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
                                ReportStatement($"Run Time Error : Type mismatch in assignment. Variable {VariableName} is not a handle.");
                            }
                        }
                        break;

                    case StackItem.ItemType.sldval:
                        {
                            Symbol sym = SymbolTable.Instance().FindSymbol(VariableName, SymbolTable.SymbolType.solidvariable);
                            if (sym != null)
                            {
                                sym.SolidValue = sti.SolidValue;
                                if (sti.SolidValue == 53)
                                {
                                    bool stophere = true;
                                }
                                //    Log.Instance().AddEntry($" {VariableName} is set to {(int)sti.SolidValue}");
                                //                                sym.Dump();
                                result = true;
                            }
                            else
                            {
                                ReportStatement($"Run Time Error : Type mismatch in assignment. Variable {VariableName} is not a solid.");
                            }
                        }
                        break;
                }
            }
            return result;
        }
    }
}