using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ScriptLanguage
{
    public class Script
    {
        public static string PartsLibraryPath = "";
        public static string ProjectPath = "";

        //   public static MdiChildForm theForm;
        private static int nextObjectId;

        private ParseTreeNode parseTree;

        // Instance constructor
        public Script()
        {
            parseTree = null;

            ResultArtefacts = new Dictionary<int, Object3D>();
            nextObjectId = 0;
        }

        public static int NextObjectId
        {
            get
            {
                System.Diagnostics.Debug.WriteLine($"nextObjectid = {nextObjectId}");
                return nextObjectId++;
            }
            set
            {
                nextObjectId = value;
            }
        }

        public static bool RanSaveParts
        {
            get;
            set;
        }

        // This is used a generic way of returning objects or data created when the script is
        // run.
        //public static List<Object3D> ResultArtefacts
        public static Dictionary<int, Object3D> ResultArtefacts
        {
            get;
            set;
        }

        public bool Execute()
        {
            bool result = false;
            try
            {
                ExecutionStack.Instance().Clear();
                ResultArtefacts.Clear();
                RanSaveParts = false;

                NextObjectId = 0;

                if (parseTree != null)
                {
                    ParseTreeNode.continueRunning = true;
                    result = parseTree.Execute();
                    foreach (Object3D ob in ResultArtefacts.Values)
                    {
                        if (ob != null)
                        {
                            ob.DeThread();
                        }
                    }
                }
                GC.Collect();
            }
            catch (Exception ex)
            {
                Log.Instance().AddEntry($"Exception:  {ex.Message} ");
            }
            return result;
        }

        public ParseTreeNode PrepareForSingleStep()
        {
            parseTree.HighLight = true;
            return parseTree;
        }

        public void SetCancelationToken(CancellationToken cancellationToken)
        {
            ParseTreeNode.CancellationToken = cancellationToken;
        }

        public void SetPartsLibraryRoot(string v)
        {
            PartsLibraryPath = v;
        }

        public void SetProjectPathRoot(string v)
        {
            ProjectPath = v;
        }

        public void SetResultsContent(Dictionary<int, Object3D> content)
        {
            ResultArtefacts = content;
        }

        public String ToErrorRichText(String s)
        {
            //
            // Rich Text document block
            //
            String Result = @"{\rtf1\ansi\ansicpg1252\deff0\deflang1033{\fonttbl{\f0\fswiss\fcharset0 Arial;}}{\colortbl ;\red0\green128\blue0;\red0\green0\blue0;\red0\green128\blue128;\red128\green128\blue128;\red128\green0\blue0}\viewkind4\uc1\pard\f0\fs20 ";
            s = s.Replace(@"\", @"\\");
            s = s.Replace(System.Environment.NewLine, @"\par ");
            s = s.Replace(@"{", @"\{");
            s = s.Replace(@"}", @"\}");
            Result += s;
            Result += "}";
            return Result;
        }

        public String ToRichText()
        {
            //
            // Rich Text document block
            //
            String Result = @"{\rtf1\ansi\ansicpg1252\deff0\deflang1033{\fonttbl{\f0\fswiss\fcharset0 Arial;}}{\colortbl ;\red0\green128\blue0;\red0\green0\blue0;\red0\green128\blue128;\red128\green128\blue128;\red128\green0\blue0}\viewkind4\uc1\pard\f0\fs30 ";

            if (parseTree != null)
            {
                Result += parseTree.ToRichText();
            }
            Result += "}";
            return Result;
        }

        public override string ToString()
        {
            String Result = "";

            if (parseTree != null)
            {
                Result += parseTree.ToString();
            }

            return Result;
        }

        internal void AddNode(ParseTreeNode tn)
        {
            parseTree = tn;
        }
    }
}