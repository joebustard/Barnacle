using System;

namespace ScriptLanguage
{
    internal class ValNode : SingleParameterFunction
    {
        // Instance constructor
        public ValNode()
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
                            //
                            // Does it look like a floating point
                            // By the way dont worry about these conversions failing and throwing an exception
                            // Harness will just report it to the user
                            //
                            if (sti.StringValue.IndexOf(".") > -1)
                            {
                                Double d = Convert.ToDouble(sti.StringValue);
                                ExecutionStack.Instance().Push(d);
                                result = true;
                            }
                            else
                            {
                                int i = Convert.ToInt32(sti.StringValue);
                                ExecutionStack.Instance().Push(i);
                                result = true;
                            }
                            result = true;
                        }
                        else
                        {
                            Log.Instance().AddEntry("Run Time Error : Val expected text");
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
            String result = RichTextFormatter.KeyWord("Val") + "( ";
            result += parameterExpression.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "Val(";
            result += parameterExpression.ToString();
            result += " )";
            return result;
        }
    }
}