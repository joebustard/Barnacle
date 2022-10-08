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
                if (expressions != null)
                {
                    result = expressions.Execute();
                    if (result)
                    {
                        int objectIndex;
                        if (!PullSolid(out objectIndex))
                        {
                            ReportStatement();
                            Log.Instance().AddEntry($"Run Time Error : {label} solid name incorrect");
                        }
                        else
                        {
                            if (objectIndex >= 0 && objectIndex <= Script.ResultArtefacts.Count && Script.ResultArtefacts[objectIndex] != null)
                            {
                                Script.ResultArtefacts[objectIndex] = null;
                                GC.Collect();
                            }
                            else
                            {
                                Log.Instance().AddEntry($"Run Time Error : {label} unknown solid");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Instance().AddEntry($"{label} : {ex.Message}");
            }
            return result;
        }
    }
}