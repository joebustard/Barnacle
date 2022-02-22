using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
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
                            Log.Instance().AddEntry($"Run Time Error : {label} solid name incorrect");
                        }
                        else
                        {
                            if (objectIndex >= 0 && objectIndex <= Script.ResultArtefacts.Count && Script.ResultArtefacts[objectIndex] != null)
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
                                            Point3D pos = new Point3D(xr, yr, zr);
                                            Script.ResultArtefacts[objectIndex].Position = pos;
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