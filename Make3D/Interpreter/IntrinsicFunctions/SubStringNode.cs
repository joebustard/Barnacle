using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;

namespace ScriptLanguage
{
    internal class SubStringNode : ExpressionNode
    {
        private ExpressionNode _IndexExpression;
        private ExpressionNode _LengthExpression;
        private ExpressionNode _SourceExpression;

        // Instance constructor
        public SubStringNode()
        {
            _SourceExpression = null;
            _LengthExpression = null;
            _IndexExpression = null;
        }

        public ExpressionNode IndexExpression
        {
            get { return _IndexExpression; }
            set { _IndexExpression = value; }
        }

        public ExpressionNode LengthExpression
        {
            get { return _LengthExpression; }
            set { _LengthExpression = value; }
        }

        public ExpressionNode SourceExpression
        {
            get { return _SourceExpression; }
            set { _SourceExpression = value; }
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            if (_SourceExpression != null)
            {
                if (_SourceExpression.Execute())
                {
                    StackItem sti = ExecutionStack.Instance().Pull();
                    if (sti.MyType == StackItem.ItemType.sval)
                    {
                        String Source = sti.StringValue;

                        if (_IndexExpression != null)
                        {
                            if (_IndexExpression.Execute())
                            {
                                sti = ExecutionStack.Instance().Pull();
                                if (sti.MyType == StackItem.ItemType.ival)
                                {
                                    int Index = sti.IntValue;

                                    //
                                    // Length is optional
                                    //
                                    int Length = Source.Length - Index;
                                    if (_LengthExpression != null)
                                    {
                                        if (_LengthExpression.Execute())
                                        {
                                            sti = ExecutionStack.Instance().Pull();
                                            if (sti.MyType == StackItem.ItemType.ival)
                                            {
                                                Length = sti.IntValue;
                                            }
                                        }
                                    }

                                    ExecutionStack.Instance().Push(Source.Substring(Index, Length));
                                    result = true;
                                }
                                else
                                {
                                    Log.Instance().AddEntry("Run Time Error Substring expected integer index");
                                }
                            }
                        }
                    }
                    else
                    {
                        Log.Instance().AddEntry("Run Time Error Substring expected string");
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
            String result = RichTextFormatter.KeyWord("SubString(");
            result += _SourceExpression.ToRichText() + ", ";
            result += _IndexExpression.ToRichText();
            if (_LengthExpression != null)
            {
                result += ", " + _LengthExpression.ToRichText();
            }
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "SubString(";
            result += _SourceExpression.ToString() + ", ";
            result += _IndexExpression.ToString();
            if (_LengthExpression != null)
            {
                result += ", " + _LengthExpression.ToString();
            }
            result += " )";
            return result;
        }
    }
}