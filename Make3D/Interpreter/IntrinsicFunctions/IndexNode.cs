using System;

namespace ScriptLanguage
{
    internal class IndexNode : ExpressionNode
    {
        private ExpressionNode _Expression;

        private ExpressionNode _SearchExpression;

        // Instance constructor
        public IndexNode()
        {
            _Expression = null;
            _SearchExpression = null;
        }

        public ExpressionNode Expression
        {
            get { return _Expression; }
            set { _Expression = value; }
        }

        public ExpressionNode SearchExpression
        {
            get { return _SearchExpression; }
            set { _SearchExpression = value; }
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
                        if (sti.MyType == StackItem.ItemType.sval)
                        {
                            String Source = sti.StringValue;
                            if (_SearchExpression != null)
                            {
                                if (_SearchExpression.Execute())
                                {
                                    sti = ExecutionStack.Instance().Pull();
                                    if (sti != null)
                                    {
                                        if (sti.MyType == StackItem.ItemType.sval)
                                        {
                                            String SearchFor = sti.StringValue;
                                            int Index = Source.IndexOf(SearchFor);
                                            ExecutionStack.Instance().Push(Index);
                                            result = true;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            Log.Instance().AddEntry("Run Time Error : Index expected text");
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
            String result = RichTextFormatter.KeyWord("Index") + "( " + _Expression.ToRichText() + ", " + _SearchExpression.ToRichText() + " )";
            return result;
        }

        public override String ToString()
        {
            String result = "Index(" + _Expression.ToString() + ", " + _SearchExpression.ToString() + " )";
            return result;
        }
    }
}