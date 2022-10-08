using System;

namespace ScriptLanguage
{
    public class StatementNode : ParseTreeNode
    {
        protected bool isInLibrary;

        // Instance constructor
        public StatementNode()
        {
            ParentCompound = null;
        }

        public bool IsInLibrary
        {
            get { return isInLibrary; }
            set { isInLibrary = value; }
        }

        public CompoundNode ParentCompound { get; internal set; }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            return true;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing

        public override String ToRichText()
        {
            String result = "";
            return result;
        }

        public override String ToString()
        {
            String result = "";
            return result;
        }

        public void ReportStatement()
        {
            Log.Instance().AddEntry($"{this.ToString()}");
        }
    }
}