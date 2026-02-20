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

using System;

namespace ScriptLanguage
{
    internal class SolidCleanNode : SolidStatement
    {
        public SolidCleanNode()
        {
            label = "Clean";
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
                        result = false;
                        int objectIndex;
                        if (!PullSolid(out objectIndex))
                        {
                            ReportStatement();
                            ReportStatement($"Run Time Error : {label} solid name incorrect {expressions.Get(0).ToString()}");
                        }
                        else
                        {
                            if (CheckSolidExists(label, expressions.Get(0).ToString(), objectIndex))
                            {
                                Script.ResultArtefacts[objectIndex].RemoveDuplicateAndUnreferencedVertices();
                                Script.ResultArtefacts[objectIndex].Remesh();
                                Script.ResultArtefacts[objectIndex].CalculateAbsoluteBounds();
                                result = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ReportStatement($"{label}: {ex.Message}");
            }
            return result;
        }
    }
}