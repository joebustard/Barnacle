using System;

namespace ScriptLanguage
{
    internal class ValidSolidNode : SingleParameterFunction
    {
        private string label = "ValidSolid";


        public ValidSolidNode()
        {
            parameterExpression = null;
        }

        public ValidSolidNode(ExpressionNode ls) 
        {
            this.parameterExpression = ls;
        }

        public override bool Execute()
        {
            bool result = false;
            try
            {
                if (parameterExpression != null)
                {
                    int objectIndex = -1;
                    if (EvalExpression(parameterExpression, ref objectIndex, "Solid"))
                    {
                        if (Script.ResultArtefacts.ContainsKey(objectIndex))
                        {
                            ExecutionStack.Instance().Push(true);
                            result = true;
                        }
                        else
                        {
                            ExecutionStack.Instance().Push(false);
                            result = true;
                        }
                    }
                    else
                    {
                        Log.Instance().AddEntry($"Run Time Error : {label} unknown solid");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Instance().AddEntry($"{label} : {ex.Message}");
            }
            return result;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = RichTextFormatter.KeyWord("ValidSolid") + "( ";

            result += parameterExpression.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "ValidSolid( ";
            result += parameterExpression.ToString();
            result += " )";
            return result;
        }
    }
}