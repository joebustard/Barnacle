using System;

namespace ScriptLanguage
{
    internal class PrintNode : StatementNode
    {
        private ExpressionNode _Expression;

        private ExpressionCollection expressions;

        // Instance constructor
        public PrintNode()
        {
            _Expression = null;
            expressions = new ExpressionCollection();
        }

        public ExpressionNode Expression
        {
            get { return _Expression; }
            set { _Expression = value; }
        }

        public void AddExpression(ExpressionNode exp)
        {
            expressions.InsertAtStart(exp);
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            //
            // Evaluate any parameters
            //
            String FinalOutput = "";
            if (expressions.Execute())
            {
                result = true;
                for (int i = 0; i < expressions.Count(); i++)
                {
                    StackItem sti = ExecutionStack.Instance().Pull();
                    if (sti != null)
                    {
                        String Line = "";
                        switch (sti.MyType)
                        {
                            case StackItem.ItemType.ival:
                                {
                                    Line = sti.IntValue.ToString();
                                }
                                break;

                            case StackItem.ItemType.dval:
                                {
                                    Line = sti.DoubleValue.ToString();
                                }
                                break;

                            case StackItem.ItemType.bval:
                                {
                                    Line = sti.BooleanValue.ToString();
                                }
                                break;

                            case StackItem.ItemType.sval:
                                {
                                    Line = sti.StringValue;
                                }
                                break;

                            case StackItem.ItemType.hval:
                                {
                                    Line = sti.HandleValue.ToString();
                                }
                                break;

                            case StackItem.ItemType.sldval:
                                {
                                    Line = sti.SolidValue.ToString();
                                }
                                break;
                        }
                        if (FinalOutput != "")
                        {
                            FinalOutput += " ";
                        }
                        FinalOutput += Line;
                    }
                    else
                    {
                        ReportStatement();
                        Log.Instance().AddEntry("Run Time Error : Print expression incorrect");
                        result = false;
                    }
                }
                if (result)
                {
                    Log.Instance().AddEntry(FinalOutput);
                }
            }
            else
            {
                ReportStatement();
                Log.Instance().AddEntry("Run Time Error : Print expression failed");
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
            if (!isInLibrary)
            {
                result = Indentor.Indentation() + RichTextFormatter.KeyWord("Print ") + " ";
                result += expressions.ToRichText();
                result += " ;";
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
            if (!isInLibrary)
            {
                result = Indentor.Indentation() + "Print ";
                result += expressions.ToString();
                result += " ;";
            }
            return result;
        }
    }
}