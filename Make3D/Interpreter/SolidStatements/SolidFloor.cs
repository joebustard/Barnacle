using System;

namespace ScriptLanguage
{
    internal class SolidFloorNode : SolidStatement
    {
        public SolidFloorNode()
        {
            label = "Floor";
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
                        result = false;
                        int objectIndex;
                        if (!PullSolid(out objectIndex))
                        {
                            ReportStatement();
                            ReportStatement($"Run Time Error : {label} solid name incorrect {expressions.Get(0).ToString()}");
                        }
                        else
                        {
                            if (CheckSolidExists(label, expressions.Get(0).ToString(), objectIndex))
                            {
                                Script.ResultArtefacts[objectIndex].MoveToFloor();
                                Script.ResultArtefacts[objectIndex].Remesh();
                                Script.ResultArtefacts[objectIndex].CalculateAbsoluteBounds();
                                result = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ReportStatement($"{label}: {ex.Message}");
            }
            return result;
        }
    }
}