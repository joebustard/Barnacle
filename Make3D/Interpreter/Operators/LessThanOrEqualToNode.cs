using System;

namespace ScriptLanguage
{
    internal class LessThanOrEqualToNode : ExpressionNode
    {
        private ExpressionNode _LeftNode;
        private ExpressionNode _RightNode;

        // Instance constructor
        public LessThanOrEqualToNode()
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
                    StackItem rightVal = ExecutionStack.Instance().Pull();
                    if (rightVal != null)
                    {
                        StackItem leftVal = ExecutionStack.Instance().Pull();
                        if (leftVal != null)
                        {
                            int comparisonResult;
                            bool valid = StackComparator.Compare(leftVal, rightVal, out comparisonResult);
                            if (!valid)
                            {
                                Log.Instance().AddEntry("Run Time Error : Type Mismatch <=");
                                result = false;
                            }
                            else
                            {
                                ExecutionStack.Instance().Push((bool)(comparisonResult == 0) || (bool)(comparisonResult < 0));
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
                result += RichTextFormatter.Operator(" <= ");
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
                result += " <= ";
                if (_RightNode != null)
                {
                    result += _RightNode.ToString();
                }
            }
            return result;
        }
    }
}