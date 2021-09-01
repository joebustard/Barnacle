using System;

namespace ScriptLanguage
{
    internal class DegreesNode : SingleParameterFunction
    {
        // Instance constructor
        public DegreesNode()
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
                            ExecutionStack.Instance().Push(d * 180.0 / Math.PI);
                            result = true;
                        }
                        else
                             if (sti.MyType == StackItem.ItemType.ival)
                        {
                            double d = (double)sti.IntValue;
                            ExecutionStack.Instance().Push(d * 180.0 / Math.PI);
                            result = true;
                        }
                        else
                        {
                            Log.Instance().AddEntry("Run Time Error : Degrees expected double");
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
            String result = RichTextFormatter.KeyWord("Degrees(") + parameterExpression.ToRichText() + " )";
            return result;
        }

        public override String ToString()
        {
            String result = "Degrees(" + parameterExpression.ToString() + " )";
            return result;
        }
    }
}