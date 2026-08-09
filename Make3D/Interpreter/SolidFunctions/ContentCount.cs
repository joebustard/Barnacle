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
    internal class ContentCountNode : SolidFunctionNode
    {
        public ContentCountNode()
        {
            Label = "ContentCount";
        }

        // cut down version of read function in main document
        // only loads limited object types
        // and only the named partName one from the file
        public int CountContents(string file)
        {
            int res = 0;
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.XmlResolver = null;
                doc.Load(file);
                XmlNode docNode = doc.SelectSingleNode("Document");

                XmlNodeList nodes = docNode.ChildNodes;
                foreach (XmlNode nd in nodes)
                {
                    string ndname = nd.Name.ToLower();

                    if (ndname == "obj")
                    {
                        res++;
                    }

                    if (ndname == "groupobj")
                    {
                        res++;
                    }
                }
                doc = null;
                GC.Collect();
            }
            catch (Exception ex)
            {
                ReportExpression($"{Label} : failed to load part: " + ex.Message);
            }
            return res;
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            try
            {
                string container = "";

                if (expressions != null)
                {
                    result = expressions.Execute();

                    if (result)
                    {
                        if (!PullString(out container))
                        {
                            ReportExpression($"Run Time Error : {Label} file name incorrect");
                        }
                        else
                        {
                            result = false;
                            if (Script.ProjectPath != null && Script.ProjectPath != "" && container.Length > 1)
                            {
                                string fName = "";
                                if (container[1] == ':')
                                {
                                    fName = container;
                                }
                                else
                                {
                                    string pth = Script.ProjectPath;

                                    if (pth.EndsWith("\\"))
                                    {
                                        pth = pth.Substring(0, pth.Length - 1);
                                    }
                                    if (container.StartsWith("\\"))
                                    {
                                        container = container.Substring(1);
                                    }
                                    fName = Script.ProjectPath + "\\" + container;
                                }
                                if (!fName.EndsWith(".txt"))
                                {
                                    fName += ".txt";
                                }
                                if (!System.IO.File.Exists(fName))
                                {
                                    ReportExpression($"{Label} : couldn't find {fName}");
                                }
                                else
                                {
                                    int count = CountContents(fName);
                                    ExecutionStack.Instance().Push(count);
                                    result = true;
                                }
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