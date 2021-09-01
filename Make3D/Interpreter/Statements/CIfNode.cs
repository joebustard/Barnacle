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
        private ExpressionNode _Expression;

        private CompoundNode _FalseBody;

        private CompoundNode _TrueBody;

        private SingleSteppingMode CurrentSingleStepMode;

        // Instance constructor
        public CIfNode()
        {
            _Expression = null;
            _TrueBody = null;
            _FalseBody = null;
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
            get { return _Expression; }
            set { _Expression = value; }
        }

        public CompoundNode FalseBody
        {
            set { _FalseBody = value; }
        }

        public CompoundNode TrueBody
        {
            set { _TrueBody = value; }
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            if ((_Expression != null) &&
                 (_TrueBody != null))
            {
                result = true;

                bool bRunTrueBody = false;
                result = _Expression.Execute();
                if (result == true)
                {
                    StackItem sti = ExecutionStack.Instance().Pull();

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
                            result = _TrueBody.Execute();
                        }
                        else
                        {
                            if (_FalseBody != null)
                            {
                                result = _FalseBody.Execute();
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

                        if (_Expression.Execute())
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
                                    _TrueBody.HighLight = true;
                                    CurrentSingleStepMode = SingleSteppingMode.SteppingTrueBody;
                                    Status = SingleStepController.SteppingStatus.OK_Not_Complete;
                                }
                                else
                                {
                                    if (_FalseBody != null)
                                    {
                                        HighLight = false;
                                        _FalseBody.HighLight = true;
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
                        Status = _TrueBody.SingleStep();
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
                        Status = _FalseBody.SingleStep();
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
                result = Indentor.Indentation() + RichTextFormatter.KeyWord("If ") + "( " + _Expression.ToRichText() + @" ) \par";
                if (HighLight)
                {
                    result = RichTextFormatter.Highlight(result);
                }
                result += _TrueBody.ToRichText();
                if (_FalseBody != null)
                {
                    result += @"\par" + Indentor.Indentation() + RichTextFormatter.KeyWord("Else ") + @"\par";
                    result += _FalseBody.ToRichText();
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
                result = Indentor.Indentation() + "If ( " + _Expression.ToString() + " )\n";

                result += _TrueBody.ToString();
                if (_FalseBody != null)
                {
                    result += "\n" + Indentor.Indentation() + "Else\n";
                    result += _FalseBody.ToString();
                }
                result += "\n";
            }
            return result;
        }
    }
}