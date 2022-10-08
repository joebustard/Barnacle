using System;

namespace ScriptLanguage
{
    internal class NowNode : ExpressionNode
    {
        public override bool Execute()
        {
            bool result = true;
            System.DateTime dt = System.DateTime.Now;
            String now = dt.ToString();
            now = now.Replace('/', '_');
            now = now.Replace(':', '_');
            ExecutionStack.Instance().Push(now);
            return result;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = RichTextFormatter.KeyWord("Now") + "( )";
            return result;
        }

        public override String ToString()
        {
            String result = "Now( )";
            return result;
        }
    }
}