using System;

namespace ScriptLanguage
{
    internal class AssignOpNode : StatementNode
    {
        private ExpressionNode _ExpressionNode;
        private String _ExternalName;
        private String _VariableName;
        public string OpCode { get; set; }

        // Instance constructor
        public AssignOpNode()
        {
            _VariableName = "";
            _ExpressionNode = null;
        }

        public ExpressionNode ExpressionNode
        {
            get { return _ExpressionNode; }
            set { _ExpressionNode = value; }
        }

        public String ExternalName
        {
            get { return _ExternalName; }
            set { _ExternalName = value; }
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
                result = Indentor.Indentation() + RichTextFormatter.VariableName(_ExternalName) + RichTextFormatter.Operator(OpCode) + _ExpressionNode.ToRichText() + " ;";
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
                result = Indentor.Indentation() + _ExternalName + " " + OpCode + " " + _ExpressionNode.ToString() + " ;";
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
                                switch (OpCode)
                                {
                                    case "+=": sym.IntValue += sti.IntValue; break;
                                    case "-=": sym.IntValue -= sti.IntValue; break;
                                    case "*=": sym.IntValue *= sti.IntValue; break;
                                    case "/=": sym.IntValue /= sti.IntValue; break;
                                }

                                result = true;
                            }
                            else
                            {
                                sym = SymbolTable.Instance().FindSymbol(VariableName, SymbolTable.SymbolType.doublevariable);
                                if (sym != null)
                                {
                                    switch (OpCode)
                                    {
                                        case "+=": sym.DoubleValue += (double)sti.IntValue; break;
                                        case "-=": sym.DoubleValue -= (double)sti.IntValue; break;
                                        case "*=": sym.DoubleValue *= (double)sti.IntValue; break;
                                        case "/=": sym.DoubleValue /= (double)sti.IntValue; break;
                                    }
                                    result = true;
                                }
                                else
                                {
                                    ReportStatement();
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
                                switch (OpCode)
                                {
                                    case "+=": sym.DoubleValue += sti.DoubleValue; break;
                                    case "-=": sym.DoubleValue -= sti.DoubleValue; break;
                                    case "*=": sym.DoubleValue *= sti.DoubleValue; break;
                                    case "/=": sym.DoubleValue /= sti.DoubleValue; break;
                                }

                                result = true;
                            }
                            else
                            {
                                sym = SymbolTable.Instance().FindSymbol(VariableName, SymbolTable.SymbolType.intvariable);
                                if (sym != null)
                                {
                                    switch (OpCode)
                                    {
                                        case "+=": sym.IntValue += (int)Math.Floor(sti.DoubleValue); break;
                                        case "-=": sym.IntValue -= (int)Math.Floor(sti.DoubleValue); break;
                                        case "*=": sym.IntValue *= (int)Math.Floor(sti.DoubleValue); break;
                                        case "/=": sym.IntValue /= (int)Math.Floor(sti.DoubleValue); break;
                                    }

                                    result = true;
                                }
                                else
                                {
                                    ReportStatement();
                                    Log.Instance().AddEntry("Run Time Error : Type mismatch in assignment");
                                }
                            }
                        }
                        break;

                    case StackItem.ItemType.sval:
                        {
                            Symbol sym = SymbolTable.Instance().FindSymbol(VariableName, SymbolTable.SymbolType.stringvariable);
                            if (sym != null)
                            {
                                if (OpCode == "+=")
                                {
                                    sym.StringValue += sti.StringValue;
                                    result = true;
                                }
                            }
                            else
                            {
                                ReportStatement();
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