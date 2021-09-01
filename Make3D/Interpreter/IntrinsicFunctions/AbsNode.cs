using System;

namespace ScriptLanguage
{
    internal class AbsNode : SingleParameterFunction
    {
        // Instance constructor
        public AbsNode()
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
                        if (sti.MyType == StackItem.ItemType.dval)
                        {
                            double d = sti.DoubleValue;
                            ExecutionStack.Instance().Push(Math.Abs(d));
                            result = true;
                        }
                        else
                             if (sti.MyType == StackItem.ItemType.ival)
                        {
                            double d = (double)sti.IntValue;
                            ExecutionStack.Instance().Push(Math.Abs(d));
                            result = true;
                        }
                        else
                        {
                            Log.Instance().AddEntry("Run Time Error : Abs expected double");
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
            String result = RichTextFormatter.KeyWord("Abs(") + parameterExpression.ToRichText() + " )";
            return result;
        }

        public override String ToString()
        {
            String result = "Abs(" + parameterExpression.ToString() + " )";
            return result;
        }
    }
}