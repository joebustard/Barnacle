using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptLanguage
{
    internal class CReadLineNode : CStatementNode
    {
        private ExpressionNode _FilePathExpression;
        private String _VariableName;

        public CReadLineNode()
        {
            _FilePathExpression = null;
            _VariableName = "";
        }

        public ExpressionNode FilePathExpression
        {
            get { return _FilePathExpression; }
            set { _FilePathExpression = value; }
        }

        public String VariableName
        {
            get { return _VariableName; }
            set { _VariableName = value; }
        }

        public override bool Execute()
        {
            bool result = false;
            if (_FilePathExpression != null)
            {
                if (_FilePathExpression.Execute())
                {
                    StackItem sti = ExecutionStack.Instance().Pull();
                    if (sti != null)
                    {
                        if (sti.MyType == StackItem.ItemType.sval)
                        {
                            String FilePath = sti.StringValue;
                            String Line = "";
                            if (ReadLineFromFile(FilePath, out Line))
                            {
                                Symbol sym = SymbolTable.Instance().FindSymbol(VariableName, SymbolTable.SymbolType.stringvariable);
                                if (sym != null)
                                {
                                    sym.StringValue = Line;
                                    result = true;
                                }
                                else
                                {
                                    Log.Instance().AddEntry("Run Time Error : Type mismatch in ReadLine");
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
            String result = Indentor.Indentation() + RichTextFormatter.KeyWord("ReadLine ") + _FilePathExpression.ToRichText() + ", ";
            result += RichTextFormatter.VariableName(_VariableName) + " ;";
            if (HighLight)
            {
                result = RichTextFormatter.Highlight(result);
            }

            return result;
        }

        public override String ToString()
        {
            String result = Indentor.Indentation() + "ReadLine " + _FilePathExpression.ToString() + ", ";
            result += _VariableName + " ;";

            return result;
        }

        private bool ReadLineFromFile(string FilePath, out string Line)
        {
            bool result = false;
            Line = "";
            result = COpenedInputFiles.Instance().ReadLine(FilePath, out Line);
            return result;
        }
    }
}