using System;
using System.Reflection.Emit;

namespace ScriptLanguage
{
    internal class ToLowerNode : SingleParameterFunction
    {
        // Instance constructor
        private string label = "Tolower";

        public ToLowerNode()
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
                            String s = sti.StringValue.Trim();
                            ExecutionStack.Instance().Push(s.ToLower());
                            result = true;
                        }
                        else
                        {
                            Log.Instance().AddEntry($"Run Time Error : {label} expected text");
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
            String result = RichTextFormatter.KeyWord("Tolower(");
            result += parameterExpression.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "Tolower(";
            result += parameterExpression.ToString();
            result += " )";
            return result;
        }
    }
}