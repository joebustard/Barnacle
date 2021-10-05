using System;

namespace ScriptLanguage
{
    internal class CProgramNode : CStructuralNode
    {
        private String _Name;

        // Instance constructor
        public CProgramNode()
        {
            _Name = "";
        }

        // Copy constructor
        public CProgramNode(CProgramNode it)
        {
            _Name = it.Name;
        }

        public String Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            Log.Instance().AddEntry(_Name);
            bool result = false;
            if (Child != null)
            {
                result = Child.Execute();
            }
            return result;
        }

        public override SingleStepController.SteppingStatus SingleStep()
        {
            Log.Instance().AddEntry(_Name);
            HighLight = false;
            //
            // Single stepping the test just means setting the next statement to the
            // body.
            //
            SingleStepController.Instance().SetCurrentStatement(Child);
            Child.HighLight = true;
            return SingleStepController.SteppingStatus.OK_Not_Complete;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = "";
            Indentor.Reset();
            result = RichTextFormatter.KeyWord("Program ") + " " + _Name + @"\par";
            if (HighLight)
            {
                result = RichTextFormatter.Highlight(result);
            }
            result += Child.ToRichText();
            return result;
        }

        public override String ToString()
        {
            String result = "";
            Indentor.Reset();
            result = "Program " + _Name + "\n";

            result += Child.ToString();
            return result;
        }
    }
}