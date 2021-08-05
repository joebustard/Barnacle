using System;

namespace ScriptLanguage
{
    internal class StrNode : SingleParameterFunction
    {
        // Instance constructor
        public StrNode()
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
                        String strLine = "";
                        result = true;

                        switch (sti.MyType)
                        {
                            case StackItem.ItemType.bval:
                                {
                                    strLine = sti.BooleanValue.ToString();
                                }
                                break;

                            case StackItem.ItemType.ival:
                                {
                                    strLine = sti.IntValue.ToString();
                                }
                                break;

                            case StackItem.ItemType.dval:
                                {
                                    strLine = sti.DoubleValue.ToString();
                                }
                                break;

                            case StackItem.ItemType.hval:
                                {
                                    strLine = sti.HandleValue.ToString();
                                }
                                break;

                            case StackItem.ItemType.sldval:
                                {
                                    strLine = sti.SolidValue.ToString();
                                }
                                break;

                            default:
                                {
                                    Log.Instance().AddEntry("Run Time Error : Str expected value");
                                    result = false;
                                }
                                break;
                        }
                        ExecutionStack.Instance().Push(strLine);
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
            String result = RichTextFormatter.KeyWord("Str(");
            result += _Expression.ToRichText();
            result += " )";

            return result;
        }

        public override String ToString()
        {
            String result = "Str(";
            result += _Expression.ToString();
            result += " )";

            return result;
        }
    }
}