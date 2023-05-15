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
                            result = false;
                        }
                        else
                        {
                            if (Script.ResultArtefacts.ContainsKey(objectIndex))
                            {
                                Script.ResultArtefacts.Remove(objectIndex);
                                GC.Collect();
                            }
                            /* NOT an error to delet something which doesn't exist
                            else
                            {
                                Log.Instance().AddEntry($"Run Time Error : {label} unknown solid");
                            }
                            */
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