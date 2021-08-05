using System;

namespace ScriptLanguage
{
    internal class RadNode : SingleParameterFunction
    {
        // Instance constructor
        public RadNode()
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
                            ExecutionStack.Instance().Push(d * Math.PI / 180.0);
                            result = true;
                        }
                        else
                             if (sti.MyType == StackItem.ItemType.ival)
                        {
                            double d = (double)sti.IntValue;
                            ExecutionStack.Instance().Push(d * Math.PI / 180.0);
                            result = true;
                        }
                        else
                        {
                            Log.Instance().AddEntry("Run Time Error : Rad expected double");
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
            String result = RichTextFormatter.KeyWord("Rad(") + _Expression.ToRichText() + " )";
            return result;
        }

        public override String ToString()
        {
            String result = "Rad( " + _Expression.ToString() + " )";
            return result;
        }
    }
}