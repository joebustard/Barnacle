using System;

namespace ScriptLanguage
{
    internal class ModNode : ExpressionNode
    {
        private ExpressionNode _LeftNode;

        private ExpressionNode _RightNode;

        // Instance constructor
        public ModNode()
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
                // Ask the expression on the left to execute
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
                            // Left can be double or int
                            //
                            if ((LeftVal.MyType == StackItem.ItemType.dval) &&
                                 (RightVal.MyType == StackItem.ItemType.ival))
                            {
                                ExecutionStack.Instance().Push((int)(LeftVal.DoubleValue % RightVal.IntValue));
                            }
                            else
                            if ((LeftVal.MyType == StackItem.ItemType.ival) &&
                                (RightVal.MyType == StackItem.ItemType.ival))
                            {
                                ExecutionStack.Instance().Push((int)(LeftVal.IntValue % RightVal.IntValue));
                            }
                            else
                            {
                                Log.Instance().AddEntry("Run Time Error : Moding  types that can't be moded");
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
                result += RichTextFormatter.Operator(" % ");
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
                result += " % ";
                if (_RightNode != null)
                {
                    result += _RightNode.ToString();
                }
            }
            return result;
        }
    }
}