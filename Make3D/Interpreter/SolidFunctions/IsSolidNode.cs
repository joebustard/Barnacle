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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptLanguage
{
    internal class IsSolidNode : SingleParameterFunction
    {
        private string label = "IsSolid";

        /// <summary>
        /// Takes an int and checks if there is a solid with that number
        /// Similiar to ValidSolid but different parameter type
        /// </summary>
        public IsSolidNode()
        {
            parameterExpression = null;
        }

        public IsSolidNode(ExpressionNode ls)
        {
            this.parameterExpression = ls;
        }

        public override bool Execute()
        {
            bool result = false;
            try
            {
                if (parameterExpression != null)
                {
                    int objectIndex = -1;
                    if (EvalExpression(parameterExpression, ref objectIndex, "Int"))
                    {
                        if (Script.ResultArtefacts.ContainsKey(objectIndex) &&
                            Script.ResultArtefacts[objectIndex] != null &&
                            Script.ResultArtefacts[objectIndex].RelativeObjectVertices.Count > 0
                            )
                        {
                            ExecutionStack.Instance().Push(true);
                            result = true;
                        }
                        else
                        {
                            ExecutionStack.Instance().Push(false);
                            result = true;
                        }
                    }
                    else
                    {
                        Log.Instance().AddEntry($"Run Time Error : {label} unknown solid");
                    }
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
            String result = RichTextFormatter.KeyWord("IsSolid") + "( ";

            result += parameterExpression.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "IsSolid( ";
            result += parameterExpression.ToString();
            result += " )";
            return result;
        }
    }
}