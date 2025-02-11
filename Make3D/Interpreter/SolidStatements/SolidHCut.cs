// **************************************************************************
// *   Copyright (c) 2024 Joe Bustard <barnacle3d@gmailcom>                  *
// *                                                                         *
// *   This file is part of the Barnacle 3D application.                     *
// *                                                                         *
// *   This application is free software. You can redistribute it and/or     *
// *   modify it under the terms of the GNU Library General Public           *
// *   License as published by the Free Software Foundation. Either          *
// *   version 2 of the License, or (at your option) any later version.      *
// *                                                                         *
// *   This application is distributed in the hope that it will be useful,   *
// *   but WITHOUT ANY WARRANTY. Without even the implied warranty of        *
// *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
// *   GNU Library General Public License for more details.                  *
// *                                                                         *
// *************************************************************************

using MakerLib.PlaneCutter;
using System;

namespace ScriptLanguage
{
    internal class SolidHCutNode : SolidStatement
    {
        public SolidHCutNode()
        {
            label = "HCut";
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
                                double cutLevel;
                                if (PullDouble(out cutLevel))
                                {
                                    
                                    PlaneCutter cutter = new PlaneCutter(
                                        Script.ResultArtefacts[objectIndex].AbsoluteObjectVertices,
                                        Script.ResultArtefacts[objectIndex].TriangleIndices, cutLevel);
                                    cutter.Cut();

                                    Script.ResultArtefacts[objectIndex].AbsoluteToRelative();

                                    Script.ResultArtefacts[objectIndex].Remesh();
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
