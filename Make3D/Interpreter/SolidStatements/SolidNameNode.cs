using System;

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
                            ReportStatement();
                            Log.Instance().AddEntry($"Run Time Error : {label} solid name incorrect");                    
                        }
                        else
                        {
                            if (Script.ResultArtefacts.ContainsKey(objectIndex))
                            {
                                string name;                              
                                if (PullString(out name))
                                {
                                    Script.ResultArtefacts[objectIndex].Name = name;
                                    result = true;
                                }
                                else
                                {
                                    Log.Instance().AddEntry($"Run Time Error : {label} unknown name");
                                }                                
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
                Log.Instance().AddEntry($"{label}: {ex.Message}");
            }
            return result;
        }
    }
}