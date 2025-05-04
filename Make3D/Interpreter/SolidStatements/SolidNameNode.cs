using System;
using System.Linq;

namespace ScriptLanguage
{
    internal class SolidNameNode : SolidStatement
    {
        public SolidNameNode()
        {
            label = "SetName";
            expressions = new ExpressionCollection();
        }

        public override bool Execute()
        {
            bool result = false;
            try
            {
                if (expressions != null)
                {
                    if (expressions.Execute())
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
                                string name;
                                if (PullString(out name))
                                {
                                    if (Script.ResultArtefacts.Keys.Contains(objectIndex) && Script.ResultArtefacts[objectIndex] != null)
                                    {
                                        Script.ResultArtefacts[objectIndex].Name = name;
                                        result = true;
                                    }
                                }
                                else
                                {
                                    ReportStatement($"Run Time Error : {label} expected name ");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Instance().AddEntry($"{label}: {ex.Message}");
            }
            return result;
        }
    }
}