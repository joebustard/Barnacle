using System;
using System.Windows.Media;

namespace ScriptLanguage
{
    internal class SolidColourNode : SolidStatement
    {
        public SolidColourNode()
        {
            label = "SetColour";
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
                        int objectIndex;
                        if (!PullSolid(out objectIndex))
                        {
                            ReportStatement();
                            Log.Instance().AddEntry($"Run Time Error : {label} solid name incorrect");
                        }
                        else
                        {
                            if (Script.ResultArtefacts.ContainsKey(objectIndex))
                            {
                                byte A;
                                byte R;
                                byte G;
                                byte B;
                                result = PullByte(out A);
                                if (result)
                                {
                                    result = PullByte(out R);
                                    if (result)
                                    {
                                        result = PullByte(out G);
                                        if (result)
                                        {
                                            result = PullByte(out B);
                                            if (result)
                                            {
                                                Script.ResultArtefacts[objectIndex].Color = Color.FromArgb(A, R, G, B);
                                            }
                                        }
                                    }
                                }
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
    }
}