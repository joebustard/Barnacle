using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;

namespace ScriptLanguage
{
    internal class ReplaceAllNode : ExpressionNode
    {
        private ExpressionNode _NewExpression;
        private ExpressionNode _OldExpression;
        private ExpressionNode _TextExpression;

        // Instance constructor
        public ReplaceAllNode()
        {
            _TextExpression = null;
            _OldExpression = null;
            _NewExpression = null;
        }

        public ExpressionNode NewExpression
        {
            get { return _NewExpression; }
            set { _NewExpression = value; }
        }

        public ExpressionNode OldExpression
        {
            get { return _OldExpression; }
            set { _OldExpression = value; }
        }

        public ExpressionNode TextExpression
        {
            get { return _TextExpression; }
            set { _TextExpression = value; }
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            if (_TextExpression != null)
            {
                if (_TextExpression.Execute())
                {
                    result = false;
                    StackItem sti = ExecutionStack.Instance().Pull();
                    if (sti != null)
                    {
                        if (sti.MyType == StackItem.ItemType.sval)
                        {
                            String OriginalText = sti.StringValue;

                            if (_OldExpression != null)
                            {
                                if (_OldExpression.Execute())
                                {
                                    sti = ExecutionStack.Instance().Pull();
                                    if (sti != null)
                                    {
                                        if (sti.MyType == StackItem.ItemType.sval)
                                        {
                                            String OldChars = sti.StringValue;
                                            if (OldChars == "")
                                            {
                                                Log.Instance().AddEntry("Run Time Error :ReplaceAll old chars are blank");
                                            }
                                            else
                                            {
                                                if (_NewExpression != null)
                                                {
                                                    if (_NewExpression.Execute())
                                                    {
                                                        sti = ExecutionStack.Instance().Pull();
                                                        if (sti != null)
                                                        {
                                                            if (sti.MyType == StackItem.ItemType.sval)
                                                            {
                                                                String NewChars = sti.StringValue;

                                                                String Latest = OriginalText.Replace(OldChars, NewChars);
                                                                ExecutionStack.Instance().Push(Latest);
                                                                result = true;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            Log.Instance().AddEntry("Run Time Error : ReplacAll expected text");
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
            String result = RichTextFormatter.KeyWord("ReplaceAll") + "( ";
            result += _TextExpression.ToRichText();
            result += ", ";
            result += _OldExpression.ToRichText();
            result += ", ";
            result += _NewExpression.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "ReplaceAll( ";
            result += _TextExpression.ToString();
            result += ", ";
            result += _OldExpression.ToString();
            result += ", ";
            result += _NewExpression.ToString();
            result += " )";
            return result;
        }
    }
}