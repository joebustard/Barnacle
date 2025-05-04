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
            bool more = false;
            try
            {
                if (expressions != null)
                {
                    more = expressions.Execute();
                    if (more)
                    {
                        int objectIndex;
                        if (!PullSolid(out objectIndex))
                        {
                            ReportStatement($"Run Time Error : {label} solid name incorrect");
                        }
                        else
                        {
                            if (CheckSolidExists(label, expressions.Get(0).ToString(), objectIndex))
                            {
                                byte A;
                                byte R;
                                byte G;
                                byte B;
                                more = PullByte(out A);
                                if (more)
                                {
                                    more = PullByte(out R);
                                    if (more)
                                    {
                                        more = PullByte(out G);
                                        if (more)
                                        {
                                            more = PullByte(out B);
                                            if (more)
                                            {
                                                Script.ResultArtefacts[objectIndex].Color = Color.FromArgb(A, R, G, B);
                                                result = true;
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
                ReportStatement($"{label} : {ex.Message}");
            }
            return result;
        }
    }
}