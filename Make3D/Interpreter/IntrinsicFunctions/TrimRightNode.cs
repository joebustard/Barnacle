using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;

namespace ScriptLanguage
{
    internal class TrimRightNode : SingleParameterFunction
    {
        // Instance constructor
        public TrimRightNode()
        {
            _Expression = null;
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            if (_Expression != null)
            {
                result = _Expression.Execute();
                if (result)
                {
                    result = false;
                    StackItem sti = ExecutionStack.Instance().Pull();
                    if (sti != null)
                    {
                        if (sti.MyType == StackItem.ItemType.sval)
                        {
                            String s = sti.StringValue.TrimEnd();
                            ExecutionStack.Instance().Push(s);
                            result = true;
                        }
                        else
                        {
                            Log.Instance().AddEntry("Run Time Error : TrimRight expected text");
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
            String result = RichTextFormatter.KeyWord("TrimRight") + "( ";
            result += _Expression.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "TrimRight(";
            result += _Expression.ToString();
            result += " )";
            return result;
        }
    }
}