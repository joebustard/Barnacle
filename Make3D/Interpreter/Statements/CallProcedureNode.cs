using System;

namespace ScriptLanguage
{
    internal class CallProcedureNode : StatementNode
    {
        private ExpressionCollection expressions;
        private String procedureName;
        private ProcedureNode procToCall;

        // Instance constructor
        public CallProcedureNode()
        {
            procedureName = "";
            expressions = new ExpressionCollection();
            procToCall = null;
        }

        public String ProcedureName
        {
            get { return procedureName; }
            set { procedureName = value; }
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
            if (procToCall == null)
            {
                procToCall = CProcedureCache.Instance().FindProcedure(procedureName);
            }

            if (procToCall != null)
            {
                //
                // Evaluate any parameters
                //
                result = expressions.Execute();
                if (!result)
                {
                    ReportStatement();
                    Log.Instance().AddEntry("Failed evaluating parameters for procedure " + ProcedureName);
                }
                else
                {
                    //
                    // Call the procedure
                    //
                    result = procToCall.ExecuteFromCall();
                    if (!result)
                    {
                        Log.Instance().AddEntry("Stopped in  procedure " + ProcedureName);
                    }
                }
            }
            else
            {
                Log.Instance().AddEntry("Cant find procedure " + ProcedureName);
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
                result = Indentor.Indentation();
                result += RichTextFormatter.Procedure(procedureName);
                result += "( ";
                result += expressions.ToRichText();

                result += " ) ;";
                if (HighLight)
                {
                    result = RichTextFormatter.Highlight(result);
                }
            }
            return result;
        }

        public override String ToString()
        {
            String result = "";

            if (!IsInLibrary)
            {
                result = Indentor.Indentation();
                result += procedureName;
                result += "( ";
                result += expressions.ToString();

                result += " ) ;";
            }
            return result;
        }

        internal void AddParameterExpression(ExpressionNode exp)
        {
            if (expressions != null)
            {
                expressions.InsertAtStart(exp);
            }
        }
    }
}