using System;

namespace ScriptLanguage
{
    internal class CInEqualityNode : ExpressionNode
    {
        private ExpressionNode _LeftNode;

        private ExpressionNode _RightNode;

        // Instance constructor
        public CInEqualityNode()
        {
            _LeftNode = null;
            _RightNode = null;
        }

        public ExpressionNode LeftNode
        {
            get { return _LeftNode; }
            set { _LeftNode = value; }
        }

        public ExpressionNode RightNode
        {
            get { return _RightNode; }
            set { _RightNode = value; }
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            //
            // Ask the expression on the left to execute
            //
            result = _LeftNode.Execute();
            if (result)
            {
                //
                // Ask the expression on the right to execute
                //
                result = _RightNode.Execute();
                if (result)
                {
                    //
                    // try pulling two items off the stack
                    //
                    StackItem RightVal = ExecutionStack.Instance().Pull();
                    if (RightVal != null)
                    {
                        StackItem LeftVal = ExecutionStack.Instance().Pull();
                        if (LeftVal != null)
                        {
                            //
                            // Are both types the same ie. not adding double to a string
                            //
                            if (LeftVal.MyType == RightVal.MyType)
                            {
                                if (LeftVal.MyType == StackItem.ItemType.ival)
                                {
                                    ExecutionStack.Instance().Push((bool)(LeftVal.IntValue != RightVal.IntValue));
                                }
                                else if (LeftVal.MyType == StackItem.ItemType.dval)
                                {
                                    ExecutionStack.Instance().Push((bool)(LeftVal.DoubleValue != RightVal.DoubleValue));
                                }
                                else if (LeftVal.MyType == StackItem.ItemType.bval)
                                {
                                    ExecutionStack.Instance().Push((bool)(LeftVal.BooleanValue != RightVal.BooleanValue));
                                }
                                else if (LeftVal.MyType == StackItem.ItemType.sval)
                                {
                                    ExecutionStack.Instance().Push((bool)(LeftVal.StringValue != RightVal.StringValue));
                                }
                                else
                                {
                                    Log.Instance().AddEntry("Run Time Error : Comparing  types that can't be Compared");
                                }
                            }
                            else
                            {
                                //
                                // left and right types are not the same
                                // There are a couple of mixed ones we will accect
                                //

                                //
                                // Handle against int
                                //
                                if ((LeftVal.MyType == StackItem.ItemType.hval) &&
                                     (RightVal.MyType == StackItem.ItemType.ival))
                                {
                                    ExecutionStack.Instance().Push(LeftVal.HandleValue != RightVal.IntValue);
                                }
                                else
                                if ((LeftVal.MyType == StackItem.ItemType.ival) &&
                                     (RightVal.MyType == StackItem.ItemType.hval))
                                {
                                    ExecutionStack.Instance().Push(RightVal.HandleValue != LeftVal.IntValue);
                                }
                                else
                                    if ((LeftVal.MyType == StackItem.ItemType.sldval) &&
                                     (RightVal.MyType == StackItem.ItemType.ival))
                                {
                                    ExecutionStack.Instance().Push(LeftVal.SolidValue != RightVal.IntValue);
                                }
                                else
                                if ((LeftVal.MyType == StackItem.ItemType.ival) &&
                                     (RightVal.MyType == StackItem.ItemType.sldval))
                                {
                                    ExecutionStack.Instance().Push(RightVal.SolidValue != LeftVal.IntValue);
                                }
                                else
                                    Log.Instance().AddEntry("Run Time Error : Type Mismatch in Compare");
                            }
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
            String result = "";
            if (_LeftNode != null)
            {
                result = _LeftNode.ToRichText();
                result += RichTextFormatter.Operator(" <> ");
                if (_RightNode != null)
                {
                    result += _RightNode.ToRichText();
                }
            }
            return result;
        }

        public override String ToString()
        {
            String result = "";
            if (_LeftNode != null)
            {
                result = _LeftNode.ToString();
                result += " <> ";
                if (_RightNode != null)
                {
                    result += _RightNode.ToString();
                }
            }
            return result;
        }
    }
}