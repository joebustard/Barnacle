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
using Object3DLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace ScriptLanguage.SolidStatements
{
    internal class SolidDropNode : SolidStatement
    {
        private string direction;
        private Alignment orientation;

        public SolidDropNode()
        {
            expressions = new ExpressionCollection();
            orientation = Alignment.Above;
            GenerateLabel();
        }

        public enum Alignment
        {
            Left,
            Right,
            Front,
            Back,
            Above,
            Below
        };

        public Alignment Orientation
        {
            get
            {
                return orientation;
            }
            set
            {
                if (orientation != value)
                {
                    orientation = value;
                    GenerateLabel();
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
                        int leftobjectIndex;
                        if (!PullSolid(out leftobjectIndex))
                        {
                            ReportStatement($"Run Time Error : {label} solid name incorrect {expressions.Get(1).ToString()}");
                        }
                        else
                        {
                            if (Script.ResultArtefacts.ContainsKey(leftobjectIndex) && Script.ResultArtefacts[leftobjectIndex] != null)
                            {
                                int rightobjectIndex;
                                if (!PullSolid(out rightobjectIndex))
                                {
                                    ReportStatement($"Run Time Error : {label} solid name incorrect  {expressions.Get(0).ToString()}");
                                }
                                else
                                {
                                    if (Script.ResultArtefacts.ContainsKey(rightobjectIndex) && Script.ResultArtefacts[rightobjectIndex] != null)
                                    {
                                        ObjectDropper dropper = new ObjectDropper();
                                        dropper.DropItems(direction, Script.ResultArtefacts[leftobjectIndex], Script.ResultArtefacts[rightobjectIndex]);
                                        result = true;
                                    }
                                    else
                                    {
                                        ReportStatement($"Run Time Error : {label} unknown solid {rightobjectIndex}");
                                    }
                                }
                            }
                            else
                            {
                                ReportStatement($"Run Time Error : {label} unknown solid {expressions.Get(1).ToString()}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                Log.Instance().AddEntry($" {label} : {ex.Message}");
            }
            return result;
        }

        private void GenerateLabel()
        {
            label = "Drop";
            switch (orientation)
            {
                case Alignment.Left: direction = "Left"; break;
                case Alignment.Right: direction = "Right"; break;
                case Alignment.Above: direction = "Above"; break;
                case Alignment.Below: direction = "Below"; break;
                case Alignment.Front: direction = "Front"; break;
                case Alignment.Back: direction = "Back"; break;
            }
            label += direction;
        }
    }
}