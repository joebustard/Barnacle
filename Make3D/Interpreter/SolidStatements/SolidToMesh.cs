using System;

namespace ScriptLanguage
{
    internal class SolidToMeshNode : SolidStatement
    {
        public SolidToMeshNode()
        {
            label = "GroupToMesh";
            expressions = new ExpressionCollection();
        }

        public override bool Execute()
        {
            bool result = false;
            bool more = false;
            try
            {
                if (expressions != null)
                {
                    more = expressions.Execute();
                    if (more)
                    {
                        int objectIndex;
                        if (!PullSolid(out objectIndex))
                        {
                            ReportStatement($"Run Time Error : {label} solid name incorrect");
                        }
                        else
                        {
                            if (CheckSolidExists(label, expressions.Get(0).ToString(), objectIndex))
                            {
                                Script.ResultArtefacts[objectIndex] = Script.ResultArtefacts[objectIndex].ConvertToMesh();
                                GC.Collect();
                                result = true;
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