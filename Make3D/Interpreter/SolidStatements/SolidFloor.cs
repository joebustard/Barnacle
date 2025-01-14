﻿using System;

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
                                Script.ResultArtefacts[objectIndex].MoveToFloor();
                                Script.ResultArtefacts[objectIndex].Remesh();
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