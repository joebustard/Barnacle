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

namespace ScriptLanguage
{
    internal class GetSolidPosNode : ExpressionNode
    {
        private ExpressionNode solid;

        public GetSolidPosNode()
        {
        }

        public GetSolidPosNode(ExpressionNode ls, string prim) : base(ls)
        {
            this.solid = ls;

            this.PrimType = prim;
        }

        public string PrimType
        {
            get; set;
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

                if (EvalExpression(solid, ref ls, "Solid", Id()) && PrimType != null && PrimType != "")
                {
                    Object3D leftie = Script.ResultArtefacts[ls];

                    if (leftie != null)
                    {
                        double d = 0;
                        switch (PrimType.ToLower())
                        {
                            case "posx": d = leftie.Position.X; break;
                            case "posy": d = leftie.Position.Y; break;
                            case "posz": d = leftie.Position.Z; break;
                        }
                        ExecutionStack.Instance().Push(d);
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Instance().AddEntry($"Id() : {ex.Message}");
            }
            return result;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = RichTextFormatter.KeyWord(Id()) + "( ";

            result += solid.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = Id() + "( ";
            result += solid.ToString();
            result += " )";
            return result;
        }

        private string Id()
        {
            string res = "PosX";
            if (PrimType == "posy")
            {
                res = "PosY";
            }
            else if (PrimType == "posz")
            {
                res = "PosZ";
            }

            return res;
        }
    }
}