using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;

namespace ScriptLanguage
{
    internal class CForNode : StatementNode
    {
        private CCompoundNode _Body;

        private ExpressionNode _EndExpression;

        private ExpressionNode _StartExpression;

        private ExpressionNode _StepExpression;

        private Symbol _Symbol;

        private String _VariableName;

        private SingleSteppingMode CurrentSingleStepMode;

        private double Step;

        private SymbolTable.SymbolType symType;

        // Instance constructor
        public CForNode()
        {
            _StartExpression = null;
            _EndExpression = null;
            _StepExpression = null;
            _Body = null;
            symType = SymbolTable.SymbolType.unknown;
            _Symbol = null;
            CurrentSingleStepMode = SingleSteppingMode.NotStarted;
            Step = 1.0;
        }

        private enum SingleSteppingMode
        {
            NotStarted,
            InitialisingVariable,
            TestingCondition,
            SteppingThroughBody
        }

        public CCompoundNode Body
        {
            set { _Body = value; }
        }

        public ExpressionNode EndExpression
        {
            get { return _EndExpression; }
            set { _EndExpression = value; }
        }

        public ExpressionNode StartExpression
        {
            get { return _StartExpression; }
            set { _StartExpression = value; }
        }

        public ExpressionNode StepExpression
        {
            get { return _StepExpression; }
            set { _StepExpression = value; }
        }

        public String VariableName
        {
            get { return _VariableName; }
            set { _VariableName = value; }
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            if (_Body != null)
            {
                if (_Symbol == null)
                {
                    symType = SymbolTable.Instance().FindSymbol(VariableName);
                    _Symbol = SymbolTable.Instance().FindSymbol(VariableName, symType);
                }
                if (_StartExpression != null)
                {
                    result = _StartExpression.Execute();
                    if (result)
                    {
                        result = AssignTopOfStackToVar(VariableName);
                        if (result)
                        {
                            result = CalculateStep();
                            if (result)
                            {
                                bool bExecuteBody;

                                bExecuteBody = ContinueLoop(Step);
                                while (bExecuteBody && result)
                                {
                                    result = _Body.Execute();
                                    if (result)
                                    {
                                        AddStep(Step);
                                        bExecuteBody = ContinueLoop(Step);
                                    }
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
                case SingleSteppingMode.NotStarted:
                    {
                        HighLight = true;
                        CurrentSingleStepMode = SingleSteppingMode.InitialisingVariable;
                        Status = SingleStepController.SteppingStatus.OK_Not_Complete;
                    }
                    break;

                case SingleSteppingMode.InitialisingVariable:
                    {
                        if (_Symbol == null)
                        {
                            symType = SymbolTable.Instance().FindSymbol(VariableName);
                            _Symbol = SymbolTable.Instance().FindSymbol(VariableName, symType);
                        }

                        if (_StartExpression.Execute())
                        {
                            if (AssignTopOfStackToVar(VariableName))
                            {
                                if (CalculateStep())
                                {
                                    if (ContinueLoop(Step))
                                    {
                                        CurrentSingleStepMode = SingleSteppingMode.SteppingThroughBody;
                                        //
                                        // Make the curly bracket highlight
                                        //
                                        _Body.HighLight = true;
                                        Status = SingleStepController.SteppingStatus.OK_Not_Complete;
                                    }
                                }
                            }
                        }
                    }
                    break;

                case SingleSteppingMode.TestingCondition:
                    {
                        AddStep(Step);
                        if (ContinueLoop(Step))
                        {
                            //
                            // Condition indicataes that we need to go round the loop again
                            //
                            HighLight = false;
                            _Body.HighLight = true;
                            CurrentSingleStepMode = SingleSteppingMode.SteppingThroughBody;
                            Status = SingleStepController.SteppingStatus.OK_Not_Complete;
                        }
                        else
                        {
                            //
                            // End condition has been met
                            //
                            Status = SingleStepController.SteppingStatus.OK_Complete;
                            //
                            // Reset the node in case its run again
                            //
                            CurrentSingleStepMode = SingleSteppingMode.InitialisingVariable;
                        }
                    }
                    break;

                case SingleSteppingMode.SteppingThroughBody:
                    {
                        //
                        // For statement itself should not be highlighted
                        //
                        HighLight = false;
                        SingleStepController.SteppingStatus BodyStatus = _Body.SingleStep();

                        //
                        // If we have reached the end of the body we need to switch to testing the condition
                        //
                        if (BodyStatus == SingleStepController.SteppingStatus.OK_Complete)
                        {
                            CurrentSingleStepMode = SingleSteppingMode.TestingCondition;
                            //
                            // Body is complete but the for loop may not be
                            //
                            Status = SingleStepController.SteppingStatus.OK_Not_Complete;
                            HighLight = true;
                        }
                        else
                        {
                            Status = BodyStatus;
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
            String result = Indentor.Indentation() + RichTextFormatter.KeyWord("For ") +
                            RichTextFormatter.VariableName(_VariableName) +
                            RichTextFormatter.Operator(" = ") +
                            _StartExpression.ToRichText() + " " +
                            RichTextFormatter.KeyWord("To ") + " " +
                            _EndExpression.ToRichText();
            if (_StepExpression != null)
            {
                result += RichTextFormatter.KeyWord(" Step ") +
                           _StepExpression.ToRichText();
            }
            result += @" \par";
            if (HighLight)
            {
                result = RichTextFormatter.Highlight(result);
            }
            result += _Body.ToRichText();

            return result;
        }

        public override String ToString()
        {
            String result = Indentor.Indentation() + "For " +
                            _VariableName +
                            " = " +
                            _StartExpression.ToString() + " " +
                            "To " + " " +
                            _EndExpression.ToString();
            if (_StepExpression != null)
            {
                result += " Step " + _StepExpression.ToString();
            }
            result += "\n";

            result += _Body.ToString();
            return result;
        }

        private void AddStep(double Step)
        {
            if (_Symbol != null)
            {
                switch (_Symbol.SymbolType)
                {
                    case SymbolTable.SymbolType.intvariable:
                        {
                            _Symbol.IntValue += Convert.ToInt32(Step); ;
                        }
                        break;

                    case SymbolTable.SymbolType.doublevariable:
                        {
                            _Symbol.DoubleValue += Step;
                        }
                        break;
                }
            }
        }

        private bool AssignTopOfStackToVar(string VariableName)
        {
            bool result = false;
            StackItem sti = ExecutionStack.Instance().Pull();
            if (sti != null)
            {
                switch (sti.MyType)
                {
                    case StackItem.ItemType.ival:
                        {
                            Symbol sym = SymbolTable.Instance().FindSymbol(VariableName, SymbolTable.SymbolType.intvariable);
                            if (sym != null)
                            {
                                sym.IntValue = sti.IntValue;
                                result = true;
                            }
                            else
                            {
                                sym = SymbolTable.Instance().FindSymbol(VariableName, SymbolTable.SymbolType.doublevariable);
                                if (sym != null)
                                {
                                    sym.DoubleValue = sti.IntValue;
                                    result = true;
                                }
                                else
                                {
                                    Log.Instance().AddEntry("Run Time Error : Type mismatch in for");
                                }
                            }
                        }
                        break;

                    case StackItem.ItemType.dval:
                        {
                            Symbol sym = SymbolTable.Instance().FindSymbol(VariableName, SymbolTable.SymbolType.doublevariable);
                            if (sym != null)
                            {
                                sym.DoubleValue = sti.DoubleValue;
                                result = true;
                            }
                            else
                            {
                                Log.Instance().AddEntry("Run Time Error : Type mismatch in for");
                            }
                        }
                        break;

                    default:
                        {
                            Log.Instance().AddEntry("Run Time Error : Illegal value in for ");
                        }
                        break;
                }
            }
            return result;
        }

        private bool CalculateStep()
        {
            bool result = false; ;
            Step = 1.0;
            if (_StepExpression != null)
            {
                result = _StepExpression.Execute();
                StackItem sti = ExecutionStack.Instance().Pull();
                if (sti.MyType == StackItem.ItemType.ival)
                {
                    Step = (double)sti.IntValue;
                }
                else if (sti.MyType == StackItem.ItemType.dval)
                {
                    Step = sti.DoubleValue;
                }
                else
                {
                    Log.Instance().AddEntry("Run Time Error : Type mismatch in for");
                    result = false;
                }
            }
            else
            {
                result = true;
            }
            return result;
        }

        private bool ContinueLoop(double Step)
        {
            bool bContinue = false;
            double SymbolValue = 0.0;
            if (_Symbol != null)
            {
                switch (_Symbol.SymbolType)
                {
                    case SymbolTable.SymbolType.intvariable:
                        {
                            SymbolValue = (double)(_Symbol.IntValue);
                        }
                        break;

                    case SymbolTable.SymbolType.doublevariable:
                        {
                            SymbolValue = (_Symbol.DoubleValue);
                        }
                        break;
                }
            }
            if (_EndExpression != null)
            {
                bool result = _EndExpression.Execute();
                if (result)
                {
                    StackItem sti = ExecutionStack.Instance().Pull();
                    double dEndVal = 0.0;
                    if (sti.MyType == StackItem.ItemType.ival)
                    {
                        dEndVal = (double)sti.IntValue;
                    }
                    else if (sti.MyType == StackItem.ItemType.dval)
                    {
                        dEndVal = (double)sti.DoubleValue;
                    }
                    else
                    {
                        Log.Instance().AddEntry("Run Time Error : end expression");
                        result = false;
                    }
                    if (result)
                    {
                        // and finally!
                        if (Step > 0.0)
                        {
                            bContinue = (SymbolValue <= dEndVal);
                        }
                        else
                        {
                            bContinue = (SymbolValue >= dEndVal);
                        }
                    }
                }
            }
            return bContinue;
        }
    }
}