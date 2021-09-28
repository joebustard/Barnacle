using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;

namespace ScriptLanguage
{
    public class ParseTreeNode
    {
        public bool HighLight;
        protected ParseTreeNode Child;

        // Instance constructor
        public ParseTreeNode()
        {
            Child = null;
            HighLight = false;
        }

        public virtual bool Execute()
        {
            bool result = true;
            if (Child != null)
            {
                result = Child.Execute();
            }
            return result;
        }

        public virtual void SetChild(ParseTreeNode ch)
        {
            Child = ch;
        }

        public virtual SingleStepController.SteppingStatus SingleStep()
        {
            SingleStepController.SteppingStatus Status = SingleStepController.SteppingStatus.Failed;
            bool result = Execute();
            if (result)
            {
                Status = SingleStepController.SteppingStatus.OK_Complete;
            }
            return Status;
        }

        public virtual String ToRichText()
        {
            return "";
        }

        public override String ToString()
        {
            return "";
        }
    }
}