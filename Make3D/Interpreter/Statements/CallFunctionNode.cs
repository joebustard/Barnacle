using System;

namespace ScriptLanguage
{
    internal class CCallFunctionNode : ExpressionNode
    {
        private ExpressionCollection _Expressions;
        private String _FunctionName;
        private CFunctionNode ProcToCall;

        // Instance constructor
        public CCallFunctionNode()
        {
            _FunctionName = "";
            _Expressions = new ExpressionCollection();
            ProcToCall = null;
        }

        public String FunctionName
        {
            get { return _FunctionName; }
            set { _FunctionName = value; }
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;

            //
            // First time through we may need to find the actual function
            //
            if (ProcToCall == null)
            {
                ProcToCall = FunctionCache.Instance().FindFunction(_FunctionName);
            }

            if (ProcToCall != null)
            {
                //
                // Evaluate any parameters
                //
                result = _Expressions.Execute();
                if (result)
                {
                    //
                    // Call the Function
                    //
                    result = ProcToCall.ExecuteFromCall();
                }
                else
                {
                    Log.Instance().AddEntry("Run time error:Call failed");
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
            if (!IsInLibrary)
            {
                result = RichTextFormatter.Procedure(_FunctionName);
                result += "( ";
                result += _Expressions.ToRichText();
                result += ") ";
            }
            return result;
        }

        public override String ToString()
        {
            String result = "";
            if (!IsInLibrary)
            {
                result = _FunctionName;
                result += "( ";

                result += _Expressions.ToString();
                result += ") ";
            }
            return result;
        }

        internal void AddParameterExpression(ExpressionNode exp)
        {
            if (_Expressions != null)
            {
                _Expressions.InsertAtStart(exp);
            }
        }
    }
}