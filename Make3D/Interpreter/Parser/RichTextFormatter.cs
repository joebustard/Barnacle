using System;

namespace ScriptLanguage
{
    public class RichTextFormatter
    {
        public const String StartHighLightMarker = @"\cb1\cf6\b";
        public const String EndHighLightMarker = @"\b0";

        public static String Line(String s)
        {
            return s + @"\par";
        }

        public static String KeyWord(String s)
        {
            if (s.EndsWith(" ") == false)
            {
                s += " ";
            }
            //
            // Switch to Green ( Colour table entry 1) then back
            //
            String result = @"\cf1 " + s + @"\cf2 ";

            return result;
        }

        public static String Normal(String s)
        {
            //
            // Switch to black ( Colour table entry 2)
            //
            String result = @"\cf2 " + s;

            return result;
        }

        internal static string Operator(string s)
        {
            //
            // Switch to blue ( Colour table entry 3)
            //
            String result = @"\cf3 " + s + @"\cf2 ";

            return result;
        }

        internal static string LineComment(string s)
        {
            String result = @"\cf3 " + s + @"\cf2 ";
            return result;
        }

        internal static string VariableName(string _VariableName)
        {
            String Result;
            if (_VariableName.Length > 1)
            {
                Result = _VariableName.Substring(0, 1).ToLower() + _VariableName.Substring(1);
            }
            else
            {
                Result = _VariableName;
            }
            return Result;
        }

        internal static string Procedure(string ProcedureName)
        {
            String Result;
            if (ProcedureName.Length > 1)
            {
                Result = ProcedureName.Substring(0, 1).ToUpper() + ProcedureName.Substring(1);
            }
            else
            {
                Result = ProcedureName;
            }
            return Result;
        }

        internal static string Highlight(string str)
        {
            str = str.Replace("\\cf1", "");
            str = str.Replace("\\cf2", "");
            str = str.Replace("\\cf3", "");
            String result = StartHighLightMarker + str + EndHighLightMarker;
            return result;
        }
    }
}