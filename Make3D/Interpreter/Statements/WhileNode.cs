using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;

namespace ScriptLanguage
{
    internal class WhileNode : CStatementNode
    {
        private CCompoundNode _Body;

        private ExpressionNode _Expression;

        private SingleSteppingMode CurrentSingleStepMode;

        // Instance constructor
        public WhileNode()
        {
            _Expression = null;
            _Body = null;
            CurrentSingleStepMode = SingleSteppingMode.NotStarted;
        }

        private enum SingleSteppingMode
        {
            NotStarted,
            TestingCondition,
            SteppingThroughBody
        }

        public CCompoundNode Body
        {
            set { _Body = value; }
        }

        public ExpressionNode Expression
        {
            get { return _Expression; }
            set { _Expression = value; }
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            if ((_Expression != null) &&
                 (_Body != null))
            {
                result = true;

                bool bRunBody = false;
                do
                {
                    bRunBody = false;
                    result = _Expression.Execute();
                    if (result == true)
                    {
                        StackItem sti = ExecutionStack.Instance().Pull();

                        switch (sti.MyType)
                        {
                            case StackItem.ItemType.bval:
                                {
                                    bRunBody = sti.BooleanValue;
                                }
                                break;

                            case StackItem.ItemType.ival:
                                {
                                    bRunBody = Convert.ToBoolean(sti.IntValue);
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

                        if (bRunBody)
                        {
                            result = _Body.Execute();
                        }
                    }
                } while (bRunBody &&
                           result);
            }
            return result;
        }

        public override SingleStepController.SteppingStatus SingleStep()
        {
            SingleStepController.SteppingStatus Status = SingleStepController.SteppingStatus.Failed;
            switch (CurrentSingleStepMode)
            {
                case SingleSteppingMode.NotStarted:
                    {
                        Status = SingleStepController.SteppingStatus.OK_Not_Complete;
                        CurrentSingleStepMode = SingleSteppingMode.TestingCondition;
                        HighLight = true;
                    }
                    break;

                case SingleSteppingMode.TestingCondition:
                    {
                        bool result = _Expression.Execute();
                        if (result == true)
                        {
                            StackItem sti = ExecutionStack.Instance().Pull();
                            bool bRunBody = false;
                            switch (sti.MyType)
                            {
                                case StackItem.ItemType.bval:
                                    {
                                        bRunBody = sti.BooleanValue;
                                    }
                                    break;

                                case StackItem.ItemType.ival:
                                    {
                                        bRunBody = Convert.ToBoolean(sti.IntValue);
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

                            if (bRunBody)
                            {
                                Status = SingleStepController.SteppingStatus.OK_Not_Complete;
                                CurrentSingleStepMode = SingleSteppingMode.SteppingThroughBody;
                                _Body.HighLight = true;
                                HighLight = false;
                            }
                            else
                            {
                                Status = SingleStepController.SteppingStatus.OK_Complete;
                            }
                        }
                        else
                        {
                            Status = SingleStepController.SteppingStatus.OK_Complete;
                        }
                    }
                    break;

                case SingleSteppingMode.SteppingThroughBody:
                    {
                        SingleStepController.SteppingStatus BodyStatus = _Body.SingleStep();
                        if (BodyStatus == SingleStepController.SteppingStatus.OK_Complete)
                        {
                            CurrentSingleStepMode = SingleSteppingMode.TestingCondition;
                            Status = SingleStepController.SteppingStatus.OK_Not_Complete;
                            HighLight = true;
                        }
                        else
                        {
                            if (BodyStatus == SingleStepController.SteppingStatus.Failed)
                            {
                                Status = SingleStepController.SteppingStatus.Failed;
                            }
                            else
                            {
                                Status = SingleStepController.SteppingStatus.OK_Not_Complete;
                            }
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
            String result = Indentor.Indentation() + RichTextFormatter.KeyWord("While") + "( ";
            result += _Expression.ToRichText();
            result += @" )";
            if ((HighLight) && (CurrentSingleStepMode != SingleSteppingMode.SteppingThroughBody))
            {
                result = RichTextFormatter.Highlight(result);
            }
            result += @"\par ";
            result += _Body.ToRichText();
            result += @" \par";
            return result;
        }

        public override String ToString()
        {
            String result = Indentor.Indentation() + "While (";
            result += _Expression.ToString();
            result += @" )";

            result += "\n";
            result += _Body.ToString();
            result += "\n";
            return result;
        }
    }
}