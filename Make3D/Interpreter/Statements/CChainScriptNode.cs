using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;

namespace ScriptLanguage
{
    internal class CChainScriptNode : StatementNode
    {
        private ExpressionNode _Expression;

        // Instance constructor
        public CChainScriptNode()
        {
            _Expression = null;
        }

        public ExpressionNode Expression
        {
            get { return _Expression; }
            set { _Expression = value; }
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            if (Expression != null)
            {
                if (Expression.Execute())
                {
                    StackItem sti = ExecutionStack.Instance().Pull();
                    if (sti != null)
                    {
                        if (sti.MyType == StackItem.ItemType.sval)
                        {
                            String Path = sti.StringValue;

                            try
                            {
                                if (System.IO.File.Exists(Path))
                                {
                                    ExecutionResult.Instance().ChainPath = Path;
                                    ExecutionResult.Instance().Passed = ExecutionResult.CompletionStatus.Chained;

                                    //
                                    // Force exit
                                    //
                                    result = false;
                                }
                                else
                                {
                                    Log.Instance().AddEntry("Runtime Error ChainScript " + Path + " not found");
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Instance().AddEntry(e.Message);
                            }
                        }
                        else
                        {
                            Log.Instance().AddEntry("Run Time Error ChainScript expected string");
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
            String result = Indentor.Indentation() + RichTextFormatter.KeyWord("ChainScript ") + _Expression.ToRichText() + " ;";
            if (HighLight)
            {
                result = RichTextFormatter.Highlight(result);
            }

            return result;
        }

        public override String ToString()
        {
            String result = Indentor.Indentation() + "ChainScript " + _Expression.ToString() + " ;";

            return result;
        }
    }
}