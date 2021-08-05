using System;

namespace ScriptLanguage
{
    internal class CInputStringNode : ExpressionNode
    {
        private ExpressionNode _Expression;

        // Instance constructor
        public CInputStringNode()
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
            if (_Expression != null)
            {
                result = _Expression.Execute();
                if (result)
                {
                    result = false;
                    StackItem sti = ExecutionStack.Instance().Pull();
                    if (sti != null)
                    {
                        if (sti.MyType == StackItem.ItemType.sval)
                        {
                            /* dont have this diaog i this version
                            InputTextForm dlg = new InputTextForm();
                            dlg.label1.Text = sti.StringValue;
                            dlg.textBox1.Text = "";
                            dlg.ShowDialog( );

                            CExecutionStack.Instance( ).Push( dlg.textBox1.Text );
                            */
                            result = true;
                        }
                        else
                        {
                            Log.Instance().AddEntry("Run Time Error : InputString expected text as prompt");
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
            String result = RichTextFormatter.KeyWord("InputString") + "( ";
            result += _Expression.ToRichText();
            result += " )";
            return result;
        }
    }
}