using System;

namespace ScriptLanguage
{
    internal class TanNode : SingleParameterFunction
    {
        // Instance constructor
        public TanNode()
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
                if (_Expression.Execute())
                {
                    StackItem sti = ExecutionStack.Instance().Pull();
                    if (sti != null)
                    {
                        if (sti.MyType == StackItem.ItemType.dval)
                        {
                            double d = sti.DoubleValue;
                            ExecutionStack.Instance().Push(Math.Tan(d));
                            result = true;
                        }
                        else
                             if (sti.MyType == StackItem.ItemType.ival)
                        {
                            double d = (double)sti.IntValue;
                            ExecutionStack.Instance().Push(Math.Tan(d));
                            result = true;
                        }
                        else
                        {
                            Log.Instance().AddEntry("Run Time Error : Tan expected double");
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
            String result = RichTextFormatter.KeyWord("Tan(") + _Expression.ToRichText() + " )";
            return result;
        }

        public override String ToString()
        {
            String result = "Tan( " + _Expression.ToString() + " )";
            return result;
        }
    }
}