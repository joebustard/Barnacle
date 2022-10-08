using System;

namespace ScriptLanguage
{
    internal class CUnaryMinusNode : ExpressionNode
    {
        private ExpressionNode leftNode;

        // Instance constructor
        public CUnaryMinusNode()
        {
            leftNode = null;
        }

        public ExpressionNode LeftNode
        {
            get { return leftNode; }
            set { leftNode = value; }
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
                // try pulling an items off the stack
                //
                StackItem LeftVal = ExecutionStack.Instance().Pull();
                if (LeftVal != null)
                {
                    if (LeftVal.MyType == StackItem.ItemType.ival)
                    {
                        ExecutionStack.Instance().Push((int)(-LeftVal.IntValue));
                    }
                    else if (LeftVal.MyType == StackItem.ItemType.dval)
                    {
                        ExecutionStack.Instance().Push((double)(-LeftVal.DoubleValue));
                    }
                    else
                    {
                        Log.Instance().AddEntry("Run Time Error : Negating  types that can't be subtracted");
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
            String result = RichTextFormatter.Operator("-") + leftNode.ToRichText();
            return result;
        }

        public override String ToString()
        {
            String result = "-" + leftNode.ToString();
            return result;
        }
    }
}