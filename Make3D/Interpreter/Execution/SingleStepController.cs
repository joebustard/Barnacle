using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptLanguage
{
    public class SingleStepController
    {
        private bool _IsStepping;
        private Script CurrentProgram;
        private ParseTreeNode CurrentStatement;

        public enum SteppingStatus
        {
            Failed,
            OK_Complete,
            OK_Not_Complete
        }

        public bool IsStepping
        {
            get { return _IsStepping; }
            set { _IsStepping = value; }
        }

        private static SingleStepController Singleton = null;

        private SingleStepController()
        {
            CurrentProgram = null;
            CurrentStatement = null;
            _IsStepping = false;
        }

        public static SingleStepController Instance()
        {
            if (Singleton == null)
            {
                Singleton = new SingleStepController();
            }
            return Singleton;
        }

        public ParseTreeNode GetCurrentStatement()
        {
            return CurrentStatement;
        }

        public void SetCurrentStatement(ParseTreeNode st)
        {
            CurrentStatement = st;
        }

        public SteppingStatus SingleStep()
        {
            SteppingStatus Result = SteppingStatus.Failed;
            if (CurrentStatement != null)
            {
                Result = CurrentStatement.SingleStep();
            }
            return Result;
        }

        public void StopStepping()
        {
            _IsStepping = false;
            CurrentStatement = null;
            CurrentProgram = null;
            GC.Collect();
        }

        public void SetProgram(Script vlp)
        {
            CurrentProgram = vlp;
            if (CurrentProgram != null)
            {
                CurrentStatement = CurrentProgram.PrepareForSingleStep();
            }
            else
            {
                StopStepping();
            }
        }

        public override string ToString()
        {
            String Result = "";
            if (CurrentProgram != null)
            {
                Result = CurrentProgram.ToString();
            }
            return Result;
        }
    }
}