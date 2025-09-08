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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptLanguage
{
    internal class HullNode : SingleParameterFunction
    {
        private string label = "Hull";

        public HullNode()
        {
        }

        public HullNode(ExpressionNode ls)
        {
            this.parameterExpression = ls;
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            try
            {
                int ls = -1;

                if (EvalExpression(parameterExpression, ref ls, "Solid Id", label))
                {
                    Object3D src = Script.ResultArtefacts[ls];

                    if (src != null)
                    {
                        src.CalcScale(false);
                        Object3D clone = src.Clone();
                        clone = clone.ConvertToMesh();
                        clone.ConvertToHull();
                        clone.CalcScale(false);
                        clone.Remesh();

                        int id = Script.NextObjectId;
                        Script.ResultArtefacts[id] = clone;
                        ExecutionStack.Instance().PushSolid(id);
                        result = true;
                    }
                    else
                    {
                        Log.Instance().AddEntry($"Run Time Error: {label} source not found {ls}");
                    }
                }
                else
                {
                    Log.Instance().AddEntry($"Run Time Error : {label} solid name incorrect {parameterExpression.ToString()}");
                }
            }
            catch (Exception ex)
            {
                Log.Instance().AddEntry($"{label} : {ex.Message}");
            }
            return result;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = RichTextFormatter.KeyWord(label) + "( ";

            result += parameterExpression.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = $"{label}( ";
            result += parameterExpression.ToString();
            result += " )";
            return result;
        }
    }
}