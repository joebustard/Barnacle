using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;

namespace ScriptLanguage
{
    internal class CIfNode : StatementNode
    {
        private SingleSteppingMode CurrentSingleStepMode;
        private ExpressionNode expression;
        private CompoundNode falseBody;
        private CompoundNode trueBody;

        // Instance constructor
        public CIfNode()
        {
            expression = null;
            trueBody = null;
            falseBody = null;
            CurrentSingleStepMode = SingleSteppingMode.TestingCondition;
        }

        private enum SingleSteppingMode
        {
            TestingCondition,
            SteppingTrueBody,
            SteppingFalseBody
        }

        public ExpressionNode Expression
        {
            get { return expression; }
            set { expression = value; }
        }

        public CompoundNode FalseBody
        {
            set { falseBody = value; }
        }

        public CompoundNode TrueBody
        {
            set { trueBody = value; }
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            if ((expression != null) &&
                 (trueBody != null))
            {
                result = true;

                bool bRunTrueBody = false;
                result = expression.Execute();
                if (result == true)
                {
                    StackItem sti = ExecutionStack.Instance().Pull();

                    switch (sti?.MyType)
                    {
                        case StackItem.ItemType.bval:
                            {
                                bRunTrueBody = sti.BooleanValue;
                            }
                            break;

                        case StackItem.ItemType.ival:
                            {
                                bRunTrueBody = Convert.ToBoolean(sti.IntValue);
                            }
                            break;

                        case StackItem.ItemType.dval:
                            {
                                Log.Instance().AddEntry("Run Time Error : Expected boolean in while");
                                result = false;
                            }
                            break;

                        case StackItem.ItemType.sval:
                            {
                                Log.Instance().AddEntry("Run Time Error : Expected boolean in while");
                                result = false;
                            }
                            break;
                    }
                    if (result)
                    {
                        if (bRunTrueBody)
                        {
                            result = trueBody.Execute();
                            if ( result )
                            {
                                ParentCompound.InBreakMode = trueBody.InBreakMode;
                            }
                        }
                        else
                        {
                            if (falseBody != null)
                            {
                                result = falseBody.Execute();
                                if (result)
                                {
                                    ParentCompound.InBreakMode = falseBody.InBreakMode;
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        public override SingleStepController.SteppingStatus SingleStep()
        {
            SingleStepController.SteppingStatus Status = SingleStepController.SteppingStatus.Failed;

            switch (CurrentSingleStepMode)
            {
                case SingleSteppingMode.TestingCondition:
                    {
                        bool bRunTrueBody = false;

                        if (expression.Execute())
                        {
                            StackItem sti = ExecutionStack.Instance().Pull();
                            bool result = true;
                            switch (sti.MyType)
                            {
                                case StackItem.ItemType.bval:
                                    {
                                        bRunTrueBody = sti.BooleanValue;
                                    }
                                    break;

                                case StackItem.ItemType.ival:
                                    {
                                        bRunTrueBody = Convert.ToBoolean(sti.IntValue);
                                    }
                                    break;

                                case StackItem.ItemType.dval:
                                    {
                                        Log.Instance().AddEntry("Run Time Error : Expected boolean in if");
                                        result = false;
                                    }
                                    break;

                                case StackItem.ItemType.sval:
                                    {
                                        Log.Instance().AddEntry("Run Time Error : Expected boolean in if");
                                        result = false;
                                    }
                                    break;
                            }
                            if (result)
                            {
                                if (bRunTrueBody)
                                {
                                    HighLight = false;
                                    trueBody.HighLight = true;
                                    CurrentSingleStepMode = SingleSteppingMode.SteppingTrueBody;
                                    Status = SingleStepController.SteppingStatus.OK_Not_Complete;
                                }
                                else
                                {
                                    if (falseBody != null)
                                    {
                                        HighLight = false;
                                        falseBody.HighLight = true;
                                        CurrentSingleStepMode = SingleSteppingMode.SteppingFalseBody;
                                        Status = SingleStepController.SteppingStatus.OK_Not_Complete;
                                    }
                                    else
                                    {
                                        //
                                        // No false body so stepping is complete
                                        //
                                        Status = SingleStepController.SteppingStatus.OK_Complete;
                                        //
                                        // Reset if for next time
                                        //
                                        CurrentSingleStepMode = SingleSteppingMode.TestingCondition;
                                    }
                                }
                            }
                        }
                    }
                    break;

                case SingleSteppingMode.SteppingTrueBody:
                    {
                        Status = trueBody.SingleStep();
                        //
                        // when the body is done, reset the if
                        //
                        if (Status != SingleStepController.SteppingStatus.OK_Not_Complete)
                        {
                            CurrentSingleStepMode = SingleSteppingMode.TestingCondition;
                        }
                    }
                    break;

                case SingleSteppingMode.SteppingFalseBody:
                    {
                        Status = falseBody.SingleStep();
                        if (Status != SingleStepController.SteppingStatus.OK_Not_Complete)
                        {
                            CurrentSingleStepMode = SingleSteppingMode.TestingCondition;
                        }
                    }
                    break;
            }

            return Status;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = "";
            if (!isInLibrary)
            {
                result = Indentor.Indentation() + RichTextFormatter.KeyWord("If ") + "( " + expression.ToRichText() + @" ) \par";
                if (HighLight)
                {
                    result = RichTextFormatter.Highlight(result);
                }
                result += trueBody.ToRichText();
                if (falseBody != null)
                {
                    result += @"\par" + Indentor.Indentation() + RichTextFormatter.KeyWord("Else ") + @"\par";
                    result += falseBody.ToRichText();
                }
                result += @"\par";
            }
            return result;
        }

        public override String ToString()
        {
            String result = "";
            if (!isInLibrary)
            {
                result = Indentor.Indentation() + "If ( " + expression.ToString() + " )\n";

                result += trueBody.ToString();
                if (falseBody != null)
                {
                    result += "\n" + Indentor.Indentation() + "Else\n";
                    result += falseBody.ToString();
                }
                result += "\n";
            }
            return result;
        }
    }
}