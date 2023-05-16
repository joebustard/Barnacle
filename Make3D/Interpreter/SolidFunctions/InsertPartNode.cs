using Barnacle.Object3DLib;
using System;
using System.Xml;

namespace ScriptLanguage
{
    internal class InsertPartNode : ExpressionNode
    {
        private ExpressionNode solid;

        public InsertPartNode()
        {
        }

        public InsertPartNode(ExpressionNode ls) : base(ls)
        {
            this.solid = ls;
        }

        /// Execute this node
        /// returning false terminates the application
        ///
        public override bool Execute()
        {
            bool result = false;
            try
            {
                string partName = "";

                if (EvalExpression(solid, ref partName, "PartName", "InsertPart"))
                {
                    if (partName != "" && Script.PartsLibraryPath != null && Script.PartsLibraryPath != "")
                    {
                        string fName = Script.PartsLibraryPath;
                        fName = System.IO.Path.Combine(Script.PartsLibraryPath, partName);
                        if (!System.IO.File.Exists(fName))
                        {
                            Log.Instance().AddEntry($"InsertPart : couldn't find {fName}");
                        }
                        else
                        {
                            Object3D clone = Read(fName);
                            if (clone != null)
                            {
                                clone.CalcScale(false);
                                clone.Remesh();
                                int id = Script.NextObjectId;
                                Script.ResultArtefacts[id] = clone;
                                ExecutionStack.Instance().PushSolid(id);
                                result = true;
                            }
                            else
                            {
                                Log.Instance().AddEntry("InsertPart : read failed");
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
        // and only the first one from the file
        public Object3D Read(string file)
        {
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

                    if (ndname == "groupobj")
                    {
                        Group3D obj = new Group3D();
                        obj.Read(nd);

                        obj.SetMesh();
                        if (!(double.IsNegativeInfinity(obj.Position.X)))
                        {
                            res = obj;
                            break;
                        }
                    }
                }
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

            result += solid.ToRichText();
            result += " )";
            return result;
        }

        public override String ToString()
        {
            String result = "Insert( ";
            result += solid.ToString();
            result += " )";
            return result;
        }
    }
}