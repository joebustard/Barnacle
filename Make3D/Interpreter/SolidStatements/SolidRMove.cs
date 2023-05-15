using System;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class SolidRMoveNode : SolidStatement
    {
        public SolidRMoveNode()
        {
            label = "RMove";
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
                            if (Script.ResultArtefacts.ContainsKey(objectIndex))
                            {
                                double xr;
                                double yr;
                                double zr;

                                result = PullDouble(out xr);
                                if (result)
                                {
                                    result = PullDouble(out yr);
                                    if (result)
                                    {
                                        result = PullDouble(out zr);
                                        if (result)
                                        {
                                            Vector3D rel = new Vector3D(xr, yr, zr);
                                            Script.ResultArtefacts[objectIndex].Position += rel;
                                            Script.ResultArtefacts[objectIndex].Remesh();
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Log.Instance().AddEntry($"Run Time Error : {label} solid name incorrect");
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