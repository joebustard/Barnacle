using System;

namespace ScriptLanguage
{
    internal class GetPCNameNode : ExpressionNode
    {
        public override bool Execute()
        {
            bool result = true;
            String Name;
            Name = System.Environment.MachineName;
            ExecutionStack.Instance().Push(Name);
            return result;
        }

        public override String ToRichText()
        {
            String result = RichTextFormatter.KeyWord("PCName") + "( )";

            return result;
        }

        public override String ToString()
        {
            String result = "PCName( )";
            return result;
        }
    }
}