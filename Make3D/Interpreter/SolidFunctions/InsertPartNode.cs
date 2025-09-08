using Barnacle.Object3DLib;
using System;
using System.Xml;

namespace ScriptLanguage
{
    internal class InsertPartNode : ExpressionNode
    {
        private ExpressionNode partFileName;
        private ExpressionNode partName;

        public InsertPartNode
               (ExpressionCollection coll)
        {
            this.partFileName = coll.Get(0);
            this.partName = coll.Get(1);
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
                string part = "";

                if (base.EvalExpression(this.partFileName, ref container, "FileName", "InsertPart") &&
                    base.EvalExpression(this.partName, ref part, "PartName", "InsertPart")
                )
                {
                    if (part != "" && Script.ProjectPath != null && Script.ProjectPath != "")
                    {
                        string fName = System.IO.Path.Combine(Script.ProjectPath, container);
                        if (!fName.EndsWith(".txt"))
                        {
                            fName += ".txt";
                        }
                        if (!System.IO.File.Exists(fName))
                        {
                            Log.Instance().AddEntry($"InsertPart : couldn't find {fName}");
                        }
                        else
                        {
                            Object3D clone = Read(fName, part);
                            if (clone != null)
                            {
                                clone.CalcScale(false);
                                clone.Remesh();
                                int id = Script.NextObjectId;
                                Script.ResultArtefacts[id] = clone;
                                Script.ResultArtefacts[id].CalculateAbsoluteBounds();
                                ExecutionStack.Instance().PushSolid(id);
                                result = true;
                            }
                            else
                            {
                                Log.Instance().AddEntry($"InsertPart : failed to read {part} from {partFileName}");
                            }
                        }
                    }
                    else
                    {
                        Log.Instance().AddEntry("InsertPart : expected part name");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Instance().AddEntry($"InsertPart : {ex.Message}");
            }
            return result;
        }

        // cut down version of read function in main document
        // only loads limited object types
        // and only the named partName one from the file
        public Object3D Read(string file, string partName)
        {
            partName = partName.ToLower();
            Object3D res = null;
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
                        Object3D obj = new Object3D();
                        obj.Read(nd);
                        if (obj.Name.ToLower() == partName)
                        {
                            obj.SetMesh();
                            if (obj.PrimType != "Mesh")
                            {
                                obj = obj.ConvertToMesh();
                            }
                            if (!(double.IsNegativeInfinity(obj.Position.X)))
                            {
                                res = obj;
                                break;
                            }
                        }
                    }

                    if (ndname == "groupobj")
                    {
                        Group3D gobj = new Group3D();
                        gobj.Read(nd);
                        if (gobj.Name.ToLower() == partName)
                        {
                            Object3D obj = gobj.ConvertToMesh();

                            obj.SetMesh();
                            if (!(double.IsNegativeInfinity(obj.Position.X)))
                            {
                                res = obj;
                                break;
                            }
                        }
                    }
                }
                doc = null;
                GC.Collect();
            }
            catch (Exception ex)
            {
                Log.Instance().AddEntry($"InsertPart : failed to load part: " + ex.Message);
            }
            return res;
        }

        /// Returns a String representation of this node that can be used for
        /// Pretty Printing
        ///
        ///
        public override String ToRichText()
        {
            String result = RichTextFormatter.KeyWord("InsertPart( ");
            result += partFileName.ToRichText();
            result += ", ";
            result += partName.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "InsertPart( ";
            result += partFileName.ToString();
            result += ", ";
            result += partName.ToString();
            result += " )";
            return result;
        }
    }
}