using System;

namespace ScriptLanguage
{
    internal class SinNode : SingleParameterFunction
    {
        // Instance constructor
        public SinNode()
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
                            ExecutionStack.Instance().Push(Math.Sin(d));
                            result = true;
                        }
                        else
                             if (sti.MyType == StackItem.ItemType.ival)
                        {
                            double d = (double)sti.IntValue;
                            ExecutionStack.Instance().Push(Math.Sin(d));
                            result = true;
                        }
                        else
                        {
                            Log.Instance().AddEntry("Run Time Error : Sin expected double");
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
            String result = RichTextFormatter.KeyWord("Sin") + "( " + _Expression.ToRichText() + " )";
            return result;
        }

        public override String ToString()
        {
            String result = "Sin( " + _Expression.ToString() + " )";
            return result;
        }
    }
}