using Make3D.Object3DLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class SolidResizeNode : SolidStatement
    {
        public SolidResizeNode()
        {
            expressions = new ExpressionCollection();
            label = "Resize";
        }

        public override bool Execute()
        {
            bool result = false;
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
                                        Script.ResultArtefacts[objectIndex].CalcScale(false);
                                        Scale3D sc = Script.ResultArtefacts[objectIndex].Scale;
                                        if (xr > 0 && yr > 0 && zr > 0 && sc.X > 0 && sc.Y > 0 && sc.Z > 0)
                                        {
                                            xr = xr / sc.X;
                                            yr = yr / sc.Y;
                                            zr = zr / sc.Z;
                                            Script.ResultArtefacts[objectIndex].ScaleMesh(xr, yr, zr);
                                            Script.ResultArtefacts[objectIndex].Remesh();
                                        }
                                        else
                                        {
                                            Log.Instance().AddEntry($"Run Time Error : {label} value must be > 0");
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            Log.Instance().AddEntry($"Run Time Error : {label} unknown solid");
                        }
                    }
                }
            }
            return result;
        }
    }
}