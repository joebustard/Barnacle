using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;

namespace ScriptLanguage
{
    internal class TrimLeftNode : SingleParameterFunction
    {
        // Instance constructor
        public TrimLeftNode()
        {
            parameterExpression = null;
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            if (parameterExpression != null)
            {
                result = parameterExpression.Execute();
                if (result)
                {
                    result = false;
                    StackItem sti = ExecutionStack.Instance().Pull();
                    if (sti != null)
                    {
                        if (sti.MyType == StackItem.ItemType.sval)
                        {
                            String s = sti.StringValue.TrimStart();
                            ExecutionStack.Instance().Push(s);
                            result = true;
                        }
                        else
                        {
                            Log.Instance().AddEntry("Run Time Error : TrimLeft expected text");
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
            String result = RichTextFormatter.KeyWord("TrimLeft(");
            result += parameterExpression.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "TrimLeft(";
            result += parameterExpression.ToString();
            result += " )";
            return result;
        }
    }
}