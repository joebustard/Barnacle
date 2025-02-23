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
using MakerLib.Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class SolidMirrorNode : SolidStatement
    {


        private Alignment orientation;

        public SolidMirrorNode()
        {
            expressions = new ExpressionCollection();
            orientation = Alignment.Left;
            GenerateLabel();
        }

        public enum Alignment
        {
            Left,
            Right,
            Front,
            Back,
            Up,
            Down

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
                            Log.Instance().AddEntry($"Run Time Error : {label} solid name incorrect");
                        }
                        else
                        {
                            if (Script.ResultArtefacts.ContainsKey(objectIndex))
                            {
                                Object3D ob = Script.ResultArtefacts[objectIndex];
                                string direction = "left";
                                switch (orientation)
                                {

                                    case Alignment.Left: direction = "left"; break;
                                    case Alignment.Right: direction = "right"; break;
                                    case Alignment.Up: direction = "up"; break;
                                    case Alignment.Down: direction = "down"; break;
                                    case Alignment.Front: direction = "front"; break;
                                    case Alignment.Back: direction = "back"; break;
                                   
                                }
                                Mirror.Reflect(ob, direction);
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
                result = false;
                Log.Instance().AddEntry($" {label} : {ex.Message}");
            }
            return result;
        }

        private void GenerateLabel()
        {

            switch (orientation)
            {
                case Alignment.Left: label = "MirrorLeft"; break;
                case Alignment.Right: label = "MirrorRight"; break;
                case Alignment.Up: label = "MirrorUp"; break;
                case Alignment.Down: label = "MirrorDown"; break;
                case Alignment.Front: label = "MirrorFront"; break;
                case Alignment.Back: label = "MirrorBack"; break;
            }
        }
    }
}
