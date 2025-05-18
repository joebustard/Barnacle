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
using CSGLib;
using System;
using System.ComponentModel;
using System.Windows.Automation.Peers;
using System.Windows.Media;
using System.Xml;

namespace ScriptLanguage.SolidStatements
{
    internal class SaveSolidsNode : SolidStatement
    {
        public SaveSolidsNode()
        {
            label = "SaveSolids";
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
                        string filePath;
                        if (!PullString(out filePath))
                        {
                            ReportStatement($"Run Time Error : {label} file name incorrect");
                        }
                        else
                        {
                            result = false;
                            if (Script.ProjectPath != null && Script.ProjectPath != "")
                            {
                                string fName = System.IO.Path.Combine(Script.ProjectPath, filePath);
                                if (!fName.EndsWith(".txt"))
                                {
                                    fName += ".txt";
                                }
                                if (!System.IO.File.Exists(fName))
                                {
                                    ReportStatement($"{label} : couldn't find {fName}");
                                }
                                else
                                {
                                    try
                                    {
                                        Write(fName);
                                        result = true;
                                        Script.RanSaveParts = true;
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.Instance().AddEntry($"Run Time Error : {label} file save failed : {ex.Message}");
                                    }
                                }
                            }
                            else
                            {
                                Log.Instance().AddEntry($"Run Time Error : {label} file name incorrect");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Instance().AddEntry($"{label} : {ex.Message}");
            }
            return result;
        }

        public void Write(string file)
        {
            GC.Collect();
            XmlDocument doc = new XmlDocument();
            doc.XmlResolver = null;
            XmlElement docNode = doc.CreateElement("Document");
            docNode.SetAttribute("NextId", Script.ResultArtefacts.Count.ToString());

            docNode.SetAttribute("Revision", "1");
            docNode.SetAttribute("Guid", Guid.NewGuid().ToString());

            foreach (Object3D ob in Script.ResultArtefacts.Values)
            {
                if (ob != null)
                {
                    ob.Write(doc, docNode);
                }
            }
            doc.AppendChild(docNode);
            doc.Save(file);
        }
    }
}