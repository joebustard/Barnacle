using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptLanguage
{
    public class ExecutionResult
    {
        private static ExecutionResult Singleton = null;
        public enum CompletionStatus
        {
            Ignore,
            Failed,
            Passed,
            Unknown, 
            Stopped,
            Chained,
            ChainSuite
        };

        private bool _zombie;
        public bool Zombie
        {
            get { return _zombie; }
            set { _zombie = value; }
        }


        private CompletionStatus _passed;
        public CompletionStatus Passed
        {
            get { return _passed; }
            set { _passed = value; }
        }

        private String _ChainPath;
        public String ChainPath
        {
            get { return _ChainPath; }
            set { _ChainPath = value; }
        }

        // Instance constructor
        private ExecutionResult()
        {
            _passed = CompletionStatus.Failed;
            _zombie = false;
        }
        public static ExecutionResult Instance()
        {
            if (Singleton == null)
            {
                Singleton = new ExecutionResult();
            }
            return Singleton;
        }

    }
}
