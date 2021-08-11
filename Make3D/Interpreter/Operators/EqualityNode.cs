using System;

namespace ScriptLanguage
{
    internal class EqualityNode : ExpressionNode
    {
        private ExpressionNode leftNode;

        private ExpressionNode rightNode;

        // Instance constructor
        public EqualityNode()
        {
            leftNode = null;
            rightNode = null;
        }

        public ExpressionNode LeftNode
        {
            get { return leftNode; }
            set { leftNode = value; }
        }

        public ExpressionNode RightNode
        {
            get { return rightNode; }
            set { rightNode = value; }
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
            result = leftNode.Execute();
            if (result)
            {
                //
                // Ask the expression on the left to execute
                //
                result = rightNode.Execute();
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
                                Log.Instance().AddEntry("Run Time Error : Type Mismatch ==");
                                result = false;
                            }
                            else
                            {
                                ExecutionStack.Instance().Push((bool)(comparisonResult == 0));
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
            String result = leftNode.ToRichText() + RichTextFormatter.Operator(" == ") + rightNode.ToRichText();
            return result;
        }

        public override String ToString()
        {
            String result = leftNode.ToString() + " == " + rightNode.ToString();
            return result;
        }
    }
}