using System;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class SolidRotateNode : SolidStatement
    {
        public SolidRotateNode()
        {
            label = "Rotate";
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
                            ReportStatement($"Run Time Error : {label} solid name incorrect {expressions.Get(0).ToString()}");
                        }
                        else
                        {
                            if (Script.ResultArtefacts.ContainsKey(objectIndex) && Script.ResultArtefacts[objectIndex] != null)
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
                                            Point3D rot = new Point3D(xr, yr, zr);
                                            Script.ResultArtefacts[objectIndex].Rotate(rot);
                                            Script.ResultArtefacts[objectIndex].Remesh();
                                            Script.ResultArtefacts[objectIndex].CalculateAbsoluteBounds();
                                        }
                                        else
                                        {
                                            ReportStatement($"Run Time Error : {label} z incorrect {expressions.Get(3).ToString()}");
                                        }
                                    }
                                    else
                                    {
                                        ReportStatement($"Run Time Error : {label} y incorrect {expressions.Get(2).ToString()}");
                                    }
                                }
                                else
                                {
                                    ReportStatement($"Run Time Error : {label} x incorrect {expressions.Get(1).ToString()}");
                                }
                            }
                            else
                            {
                                ReportStatement($"Run Time Error : {label} unknown solid index {objectIndex}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ReportStatement($"{label} : {ex.Message}");
            }
            return result;
        }
    }
}