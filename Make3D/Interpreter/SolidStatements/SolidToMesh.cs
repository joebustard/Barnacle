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
                            result = false;
                        }
                        else
                        {
                            if (Script.ResultArtefacts.ContainsKey(objectIndex))
                            {
                                Script.ResultArtefacts[objectIndex] = Script.ResultArtefacts[objectIndex].ConvertToMesh();
                                GC.Collect();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Instance().AddEntry($"{label} : {ex.Message}");
                result = false;
            }
            return result;
        }
    }
}