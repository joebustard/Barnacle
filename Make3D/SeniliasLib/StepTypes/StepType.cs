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

using SeniliasLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Senilias.StepTypes
{
    public class StepType
    {
        private int level;

        public StepType()
        {
        }

        public enum TypeOfStep
        {
            None = 0,
            Script,
            Slice,
            Touch
        }

        /// <summary>
        /// Only used for indentation
        /// </summary>
        public int Level
        {
            get
            {
                return level;
            }
        }

        public virtual bool Compare(StepType other)
        {
            return false;
        }

        public virtual bool Execute()
        {
            return true;
        }

        public virtual void Load(XmlElement ele)
        {
        }

        public string RelativeToProject(string s)
        {
            string res = Utils.RelativeToProject(s);
            return res;
        }

        public virtual void Save(XmlDocument doc, XmlNode node)
        {
        }

        public virtual void SetLevel(int l)
        {
            level = l;
        }

        public virtual void ToDo(List<string> res)
        {
        }

        public override string ToString()
        {
            return "";
        }

        internal virtual StepType Clone()
        {
            StepType res = new StepType();
            return res;
        }

        internal virtual void MakePathsAbsolute()
        {
        }

        internal virtual void ReplaceWildcard(string sub)
        {
        }
    }
}