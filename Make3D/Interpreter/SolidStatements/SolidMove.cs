using System;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class SolidMoveNode : SolidStatement
    {
        public SolidMoveNode()
        {
            label = "Move";
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
                            ReportStatement($"Run Time Error : {label} (1) solid name incorrect {expressions.Get(0).ToString()}");
                        }
                        else
                        {
                            if (CheckSolidExists(label, expressions.Get(0).ToString(), objectIndex))
                            {
                                double xr;
                                double yr;
                                double zr;

                                more = PullDouble(out xr);
                                if (more)
                                {
                                    more = PullDouble(out yr);
                                    if (more)
                                    {
                                        more = PullDouble(out zr);
                                        if (more)
                                        {
                                            Point3D pos = new Point3D(xr, yr, zr);
                                            Script.ResultArtefacts[objectIndex].Position = pos;
                                            Script.ResultArtefacts[objectIndex].Remesh();
                                            Script.ResultArtefacts[objectIndex].CalculateAbsoluteBounds();
                                            result = true;
                                        }
                                        else
                                        {
                                            ReportStatement($"Run Time Error : {label} (2) z incorrect {expressions.Get(1).ToString()}");
                                        }
                                    }
                                    else
                                    {
                                        ReportStatement($"Run Time Error : {label} (3) y incorrect {expressions.Get(2).ToString()}");
                                    }
                                }
                                else
                                {
                                    ReportStatement($"Run Time Error : {label} (4) x incorrect {expressions.Get(3).ToString()}");
                                }
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