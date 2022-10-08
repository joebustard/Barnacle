using System;

namespace ScriptLanguage
{
    internal class CReadFileNode : StatementNode
    {
        private ExpressionNode _Expression;

        // Instance constructor
        public CReadFileNode()
        {
            _Expression = null;
        }

        public ExpressionNode Expression
        {
            get { return _Expression; }
            set { _Expression = value; }
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            if (Expression != null)
            {
                if (Expression.Execute())
                {
                    StackItem sti = ExecutionStack.Instance().Pull();
                    if (sti != null)
                    {
                        if (sti.MyType == StackItem.ItemType.sval)
                        {
                            String Path = sti.StringValue;

                            try
                            {
                                result = COpenedInputFiles.Instance().OpenFile(Path);
                                if (result == false)
                                {
                                    Log.Instance().AddEntry("Run Time Error ReadFile failed to open " + Path);
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Instance().AddEntry(e.Message);
                            }
                        }
                        else
                        {
                            Log.Instance().AddEntry("Run Time Error ReadFile expected string");
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
            String result = Indentor.Indentation() + RichTextFormatter.KeyWord("ReadFile ") + _Expression.ToRichText() + " ;";
            if (HighLight)
            {
                result = RichTextFormatter.Highlight(result);
            }
            return result;
        }

        public override String ToString()
        {
            String result = Indentor.Indentation() + "ReadFile " + _Expression.ToString() + " ;";
            return result;
        }
    }
}