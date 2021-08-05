using System;
using System.Collections.Generic;

namespace ScriptLanguage
{
    public class Script
    {
        private ParseTreeNode parseTree;
        //   public static MdiChildForm theForm;

        // Instance constructor
        public Script()
        {
            parseTree = null;
            ResultArtefacts = new List<object>();
        }

        // This is used a generic way of returning objects or data created when the script is
        // run.
        public static List<object> ResultArtefacts
        {
            get;
            set;
        }

        public bool Execute()
        {
            ExecutionStack.Instance().Clear();
            ResultArtefacts.Clear();

            bool result = false;
            if (parseTree != null)
            {
                result = parseTree.Execute();
            }

            return result;
        }

        public ParseTreeNode PrepareForSingleStep()
        {
            parseTree.HighLight = true;
            return parseTree;
        }

        public String ToRichText()
        {
            //
            // Rich Text document block
            //
            String Result = @"{\rtf1\ansi\ansicpg1252\deff0\deflang1033{\fonttbl{\f0\fswiss\fcharset0 Arial;}}{\colortbl ;\red0\green128\blue0;\red0\green0\blue0;\red0\green128\blue128;\red128\green128\blue128;\red128\green0\blue0}\viewkind4\uc1\pard\f0\fs20 ";

            if (parseTree != null)
            {
                Result += parseTree.ToRichText();
            }
            Result += "}";
            return Result;
        }

        public string ToString()
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