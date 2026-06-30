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

using Barnacle.Object3DLib;
using System;
using System.Xml;

namespace ScriptLanguage
{
    internal class SplitFrontBackNode : SolidFunctionNode
    {
        public SplitFrontBackNode()
        {
            Label = "SplitFrontBack";
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            try
            {
                if (expressions != null)
                {
                    result = expressions.Execute();
                    int solidId = -1;
                    if (result)
                    {
                        if (!PullSolid(out solidId))
                        {
                            ReportExpression($"Run Time Error : {Label} solid id error");
                        }
                        else
                        {
                            result = false;
                            Object3D src = Script.ResultArtefacts[solidId];
                            if (src != null)
                            {
                                Object3D part2 = src.SplitFrontBack(src.AbsoluteBounds.MidPoint().Z);
                                int id = Script.NextObjectId;
                                Script.ResultArtefacts[id] = part2;
                                ExecutionStack.Instance().PushSolid(id);
                                result = true;
                            }
                            else
                            {
                                ReportExpression($"Run Time Error : {Label} solid {solidId} is not valid");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ReportExpression($"{Label} : {ex.Message}");
            }
            return result;
        }
    }
}