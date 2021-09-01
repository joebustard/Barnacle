using System;

namespace ScriptLanguage
{
    internal class LenNode : SingleParameterFunction
    {
        // Instance constructor
        public LenNode()
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
                if (parameterExpression.Execute())
                {
                    StackItem sti = ExecutionStack.Instance().Pull();
                    if (sti != null)
                    {
                        if (sti.MyType == StackItem.ItemType.sval)
                        {
                            int Len = sti.StringValue.Length;
                            ExecutionStack.Instance().Push(Len);
                            result = true;
                        }
                        else
                        {
                            Log.Instance().AddEntry("Run Time Error : Len expected text");
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
            String result = RichTextFormatter.KeyWord("Len(") + parameterExpression.ToRichText() + " )";
            return result;
        }

        public override String ToString()
        {
            String result = "Len(" + parameterExpression.ToString() + " )";
            return result;
        }
    }
}