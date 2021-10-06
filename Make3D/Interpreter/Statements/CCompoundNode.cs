using System;
using System.Collections.Generic;

namespace ScriptLanguage
{
    public class CompoundNode : ParseTreeNode
    {
        public bool InBreakMode;
        public bool IsTestBody;
        public List<StatementNode> Statements;
        protected List<StatementNode> UseStatements;
        private int CurrentSingleStepStatement;

        // Instance constructor
        public CompoundNode()
        {
            Statements = new List<StatementNode>();
            UseStatements = new List<StatementNode>();
            IsTestBody = false;
            InBreakMode = false;
            CurrentSingleStepStatement = 0;
        }

        public void AddStatement(StatementNode st)
        {
            if (Statements != null)
            {
                Statements.Add(st);
                st.ParentCompound = this;
            }
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = true;
            InBreakMode = false;
            if (Statements.Count > 0)
            {
                //
                // execute all the statements  in the compound
                // until one of them fails
                //
                int i = 0;
                while ((i < Statements.Count) &&
                        (result == true) && !InBreakMode)
                {
                    result = Statements[i].Execute();
                    i++;
                }
            }

            return result;
        }

        public virtual void SetReturn()
        {
        }

        public override SingleStepController.SteppingStatus SingleStep()
        {
            SingleStepController.SteppingStatus Status = SingleStepController.SteppingStatus.Failed;
            //
            // Single stepping a compound node means single stepping the
            // next statement within the compound node
            // As long as there is another statement return ok_not_comple
            //
            HighLight = false;
            if (CurrentSingleStepStatement < Statements.Count)
            {
                Statements[CurrentSingleStepStatement].HighLight = false;
                Status = Statements[CurrentSingleStepStatement].SingleStep();
                if (Status == SingleStepController.SteppingStatus.OK_Complete)
                {
                    CurrentSingleStepStatement++;
                    if (CurrentSingleStepStatement < Statements.Count)
                    {
                        Statements[CurrentSingleStepStatement].HighLight = true;
                        Status = SingleStepController.SteppingStatus.OK_Not_Complete;
                    }
                    else
                    {
                        CurrentSingleStepStatement = 0;
                    }
                }
            }
            else
            {
                Status = SingleStepController.SteppingStatus.OK_Complete;
                //
                // reset for next time
                CurrentSingleStepStatement = 0;
                //
            }

            return Status;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = Indentor.Indentation() + @"\{\par";
            if (HighLight)
            {
                result = RichTextFormatter.Highlight(result);
                //
                // Force first statement to be highlighted too
                // just looks better
                //
                if (Statements.Count > 0)
                {
                    Statements[0].HighLight = true;
                }
            }

            Indentor.Indent();

            if (IsTestBody)
            {
                foreach (StatementNode ust in UseStatements)
                {
                    result += ust.ToRichText();
                    result += @"\par";
                }

                // result += CFunctionCache.Instance().ToString();
                // result += CProcedureCache.Instance().ToString();
            }

            bool lastWasComment = false;
            for (int i = 0; i < Statements.Count; i++)
            {
                StatementNode st = Statements[i];
                // add a blank line before first line of comment
                if (!lastWasComment && st is CommentNode && st.IsInLibrary == false)
                {
                    result += @"\par";
                }
                lastWasComment = (st is CommentNode);
                result += st.ToRichText();
                if (!st.IsInLibrary)
                {
                    result += @"\par";
                }
            }
            Indentor.Outdent();
            result += Indentor.Indentation() + @"\}";
            result = result.Replace(@"\par\par\par", @"\par\par");
            return result;
        }

        public override String ToString()
        {
            String result = Indentor.Indentation() + "{\n";

            Indentor.Indent();

            if (IsTestBody)
            {
                foreach (StatementNode ust in UseStatements)
                {
                    result += ust.ToString();
                    result += "\n";
                }
            }

            bool lastWasComment = false;
            for (int i = 0; i < Statements.Count; i++)
            {
                StatementNode st = Statements[i];

                if (!lastWasComment && st is CommentNode && st.IsInLibrary == false)
                {
                    result += "\n";
                }
                lastWasComment = (st is CommentNode);

                result += st.ToString();
                if (!st.IsInLibrary)
                {
                    result += "\n";
                }
            }
            Indentor.Outdent();
            result += Indentor.Indentation() + "}";
            result = result.Replace("\n\n", "\n");
            return result;
        }

        internal Symbol AddArraySymbol(string strName, SymbolTable.SymbolType symbolType)
        {
            return SymbolTable.Instance().AddArraySymbol(strName, symbolType);
        }

        internal Symbol AddStructArraySymbol(string strName, StructDefinition def)
        {
            return SymbolTable.Instance().AddStructArraySymbol(strName, def);
        }

        internal Symbol AddSymbol(string strName, SymbolTable.SymbolType symbolType)
        {
            return SymbolTable.Instance().AddSymbol(strName, symbolType);
        }

        // Base version does nothing
        internal void AddUseStatement(IncludeNode node)
        {
            if (UseStatements != null)
            {
                UseStatements.Add(node);
            }
        }

        internal SymbolTable.SymbolType FindSymbol(string strName)
        {
            SymbolTable.SymbolType result = SymbolTable.Instance().FindSymbol(strName);
            return result;
        }
    }
}