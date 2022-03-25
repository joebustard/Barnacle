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
        private String _VariableName;
        private CompoundNode body;
        private SingleSteppingMode CurrentSingleStepMode;
        private ExpressionNode endExpression;
        private ExpressionNode startExpression;
        private double Step;
        private ExpressionNode stepExpression;
        private Symbol symbol;
        private SymbolTable.SymbolType symType;

        // Instance constructor
        public CForNode()
        {
            startExpression = null;
            endExpression = null;
            stepExpression = null;
            body = null;
            symType = SymbolTable.SymbolType.unknown;
            symbol = null;
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

        public CompoundNode Body
        {
            set { body = value; }
        }

        public ExpressionNode EndExpression
        {
            get { return endExpression; }
            set { endExpression = value; }
        }

        public string LocalName { get; internal set; }

        public ExpressionNode StartExpression
        {
            get { return startExpression; }
            set { startExpression = value; }
        }

        public ExpressionNode StepExpression
        {
            get { return stepExpression; }
            set { stepExpression = value; }
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
            if (body != null)
            {
                if (symbol == null)
                {
                    symType = SymbolTable.Instance().FindSymbol(VariableName);
                    symbol = SymbolTable.Instance().FindSymbol(VariableName, symType);
                }
                if (startExpression != null)
                {
                    result = startExpression.Execute();
                    if (result)
                    {
                        result = AssignTopOfStackToVar(VariableName);
                        if (result)
                        {
                            result = CalculateStep();
                            if (result)
                            {
                                bool bExecuteBody;
                                body.InBreakMode = false;
                                bExecuteBody = ContinueLoop(Step);
                                while (bExecuteBody && result && !body.InBreakMode && ParseTreeNode.continueRunning)
                                {
                                    result = body.Execute();
                                    if (!body.InBreakMode)
                                    {
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
                        if (symbol == null)
                        {
                            symType = SymbolTable.Instance().FindSymbol(VariableName);
                            symbol = SymbolTable.Instance().FindSymbol(VariableName, symType);
                        }

                        if (startExpression.Execute())
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
                                        body.HighLight = true;
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
                            body.HighLight = true;
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
                        SingleStepController.SteppingStatus BodyStatus = body.SingleStep();

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
            String result = "";
            if (!isInLibrary)
            {
                result = Indentor.Indentation() + RichTextFormatter.KeyWord("For ") +
                           RichTextFormatter.VariableName(LocalName) +
                           RichTextFormatter.Operator(" = ") +
                           startExpression.ToRichText() + " " +
                           RichTextFormatter.KeyWord("To ") + " " +
                           endExpression.ToRichText();
                if (stepExpression != null)
                {
                    result += RichTextFormatter.KeyWord(" Step ") +
                               stepExpression.ToRichText();
                }
                result += @" \par";
                if (HighLight)
                {
                    result = RichTextFormatter.Highlight(result);
                }
                result += body.ToRichText();
            }
            return result;
        }

        public override String ToString()
        {
            String result = "";
            if (!isInLibrary)
            {
                result = Indentor.Indentation() + "For " +
                           LocalName +
                           " = " +
                           startExpression.ToString() + " " +
                           "To " + " " +
                           endExpression.ToString();
                if (stepExpression != null)
                {
                    result += " Step " + stepExpression.ToString();
                }
                result += "\n";

                result += body.ToString();
            }
            return result;
        }

        private void AddStep(double Step)
        {
            if (symbol != null)
            {
                switch (symbol.SymbolType)
                {
                    case SymbolTable.SymbolType.intvariable:
                        {
                            symbol.IntValue += Convert.ToInt32(Step); ;
                        }
                        break;

                    case SymbolTable.SymbolType.doublevariable:
                        {
                            symbol.DoubleValue += Step;
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
            if (stepExpression != null)
            {
                result = stepExpression.Execute();
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
            if (symbol != null)
            {
                switch (symbol.SymbolType)
                {
                    case SymbolTable.SymbolType.intvariable:
                        {
                            SymbolValue = (double)(symbol.IntValue);
                        }
                        break;

                    case SymbolTable.SymbolType.doublevariable:
                        {
                            SymbolValue = (symbol.DoubleValue);
                        }
                        break;
                }
            }
            if (endExpression != null)
            {
                bool result = endExpression.Execute();
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