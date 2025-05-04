using System;

namespace ScriptLanguage
{
    internal class SolidDeleteNode : SolidStatement
    {
        public SolidDeleteNode()
        {
            label = "Delete";
            expressions = new ExpressionCollection();
        }

        public override bool Execute()
        {
            bool result = false;
            try
            {
                // SymbolTable.Instance().Dump();
                if (expressions != null)
                {
                    result = expressions.Execute();
                    if (result)
                    {
                        int objectIndex;
                        if (!PullSolid(out objectIndex))
                        {
                            ReportStatement($"Run Time Error : {label} solid name incorrect {expressions.Get(0).ToString()}");
                            result = false;
                        }
                        else
                        {
                            if (CheckSolidExists(label, expressions.Get(0).ToString(), objectIndex))
                            {
                                Script.ResultArtefacts[objectIndex] = null;
                                GC.Collect();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ReportStatement($"{label} : {ex.Message}");
                result = false;
            }
            return result;
        }
    }
}