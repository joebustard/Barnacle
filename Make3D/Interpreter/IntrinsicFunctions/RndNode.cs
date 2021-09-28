using System;

namespace ScriptLanguage
{
    internal class RndNode : SingleParameterFunction
    {
        private Random rnd;

        // Instance constructor
        public RndNode()
        {
            parameterExpression = null;
            rnd = new Random();
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            if (parameterExpression != null)
            {
                if (parameterExpression.Execute())
                {
                    double max = 1;
                    if (PullDouble(out max))
                    {
                        double value = rnd.NextDouble() * max;
                        ExecutionStack.Instance().Push(value);
                        result = true;
                    }
                    else
                    {
                        Log.Instance().AddEntry("Run Time Error : Rnd expected double");
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
            String result = RichTextFormatter.KeyWord("Rnd(") + parameterExpression.ToRichText() + " )";
            return result;
        }

        public override String ToString()
        {
            String result = "Rnd( " + parameterExpression.ToString() + " )";
            return result;
        }
    }
}