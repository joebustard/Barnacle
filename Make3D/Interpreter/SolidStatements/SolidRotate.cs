﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
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
                                        Point3D rot = new Point3D(xr, yr, zr);
                                        Script.ResultArtefacts[objectIndex].Rotate(rot);
                                        Script.ResultArtefacts[objectIndex].Remesh();
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