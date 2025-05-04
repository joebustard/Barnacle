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
    internal class SolidFlipNode : SolidStatement
    {
        private Directions direction;

        public SolidFlipNode()
        {
            label = "FlipHorizontal";
            Direction = Directions.H;
            expressions = new ExpressionCollection();
        }

        public enum Directions
        {
            H, V, D
        }

        public Directions Direction
        {
            set
            {
                direction = value;
                switch (direction)
                {
                    case Directions.H:
                        {
                            label = "FlipHorizontal";
                        }
                        break;

                    case Directions.V:
                        {
                            label = "FlipVertical";
                        }
                        break;

                    case Directions.D:
                        {
                            label = "FlipDistal";
                        }
                        break;
                }
            }
        }

        public override bool Execute()
        {
            bool result = false;
            bool more = false;
            try
            {
                if (expressions != null)
                {
                    more = expressions.Execute();
                    if (more)
                    {
                        int objectIndex;
                        if (!PullSolid(out objectIndex))
                        {
                            ReportStatement($"Run Time Error : {label} solid name incorrect");
                        }
                        else
                        {
                            if (CheckSolidExists(label, expressions.Get(0).ToString(), objectIndex))
                            {
                                switch (direction)
                                {
                                    case Directions.H:
                                        {
                                            Script.ResultArtefacts[objectIndex].FlipX();
                                        }
                                        break;

                                    case Directions.V:
                                        {
                                            Script.ResultArtefacts[objectIndex].FlipY();
                                        }
                                        break;

                                    case Directions.D:
                                        {
                                            Script.ResultArtefacts[objectIndex].FlipZ();
                                        }
                                        break;
                                }
                                Script.ResultArtefacts[objectIndex].FlipInside();
                                GC.Collect();
                                result = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ReportStatement($"{label} : {ex.Message}");
                result = false;
            }
            return result;
        }
    }
}