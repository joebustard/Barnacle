using System;

namespace ScriptLanguage
{
    internal class CWriteLineNode : StatementNode
    {
        private ExpressionNode _FilePathExpression;
        private ExpressionNode _LineExpression;

        public CWriteLineNode()
        {
            _FilePathExpression = null;
            _LineExpression = null;
        }

        public ExpressionNode FilePathExpression
        {
            get { return _FilePathExpression; }
            set { _FilePathExpression = value; }
        }

        public ExpressionNode LineExpression
        {
            get { return _LineExpression; }
            set { _LineExpression = value; }
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
                            if (System.IO.File.Exists(FilePath) == true)
                            {
                                if (_LineExpression != null)
                                {
                                    if (_LineExpression.Execute())
                                    {
                                        sti = ExecutionStack.Instance().Pull();
                                        if (sti != null)
                                        {
                                            if (sti.MyType == StackItem.ItemType.sval)
                                            {
                                                String Line = sti.StringValue;
                                                System.IO.StreamWriter fout = System.IO.File.AppendText(FilePath);
                                                if (fout != null)
                                                {
                                                    fout.WriteLine(Line);
                                                    fout.Close();
                                                    result = true;
                                                }
                                                else
                                                {
                                                    Log.Instance().AddEntry("Run Time Error : WriteLine file write failed");
                                                }
                                            }
                                            else
                                            {
                                                Log.Instance().AddEntry("Run Time Error : WriteLine expected a string expression");
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Log.Instance().AddEntry("Run Time Error : WriteLine file does not exist");
                            }
                        }
                    }
                }
            }
            return result;
        }

        public override String ToRichText()
        {
            String result = Indentor.Indentation() + RichTextFormatter.KeyWord("WriteLine ") + _FilePathExpression.ToRichText() + ", ";
            result += _LineExpression.ToRichText();
            result += " ;";
            if (HighLight)
            {
                result = RichTextFormatter.Highlight(result);
            }
            return result;
        }

        public override String ToString()
        {
            String result = Indentor.Indentation() + "WriteLine " + _FilePathExpression.ToString() + ", ";
            result += _LineExpression.ToString();
            result += " ;";

            return result;
        }
    }
}