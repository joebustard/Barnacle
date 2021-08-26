namespace ScriptLanguage
{
    internal class FunctionBodyNode : CCompoundNode
    {
        private bool bReturned;

        public FunctionBodyNode() : base()
        {
            bReturned = false;
        }

        public override bool Execute()
        {
            bool result = true;
            bReturned = false;
            if (Statements.Count > 0)
            {
                //
                // execute all the statements  in the compound
                // until one of them fails or the function returns
                //
                int i = 0;
                while ((i < Statements.Count) &&
                         (result == true) &&
                         (bReturned == false))
                {
                    result = Statements[i].Execute();
                    i++;
                }
            }

            return result;
        }

        //
        // Called by a return statement to break the main statement
        //
        public override void SetReturn()
        {
            bReturned = true;
        }
    }
}