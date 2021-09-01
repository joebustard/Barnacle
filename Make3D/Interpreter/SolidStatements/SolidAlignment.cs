using Make3D.Object3DLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScriptLanguage
{
    internal class SolidAlignmentNode : SolidStatement
    {
        private Alignment orientation;

        private Mode orientationMode;

        public SolidAlignmentNode()
        {
            expressions = new ExpressionCollection();
            orientation = Alignment.Left;
            orientationMode = Mode.Align;
            GenerateLabel();
        }

        public enum Alignment
        {
            Left,
            Right,
            Front,
            Back,
            Top,
            Bottom,
            Centre
        };

        public enum Mode
        {
            Align,
            Stack
        };

        public Alignment Orientation
        {
            get
            {
                return orientation;
            }
            set
            {
                if (orientation != value)
                {
                    orientation = value;
                    GenerateLabel();
                }
            }
        }

        public Mode OrientationMode
        {
            get
            {
                return orientationMode;
            }
            set
            {
                if (orientationMode != value)
                {
                    orientationMode = value;
                    GenerateLabel();
                }
            }
        }

        public override bool Execute()
        {
            bool result = false;
            if (expressions != null)
            {
                result = expressions.Execute();
                if (result)
                {
                    int leftobjectIndex;
                    if (!PullSolid(out leftobjectIndex))
                    {
                        Log.Instance().AddEntry($"Run Time Error : {label} solid name incorrect");
                    }
                    else
                    {
                        if (leftobjectIndex >= 0 && leftobjectIndex <= Script.ResultArtefacts.Count && Script.ResultArtefacts[leftobjectIndex] != null)
                        {
                            int rightobjectIndex;
                            if (!PullSolid(out rightobjectIndex))
                            {
                                Log.Instance().AddEntry($"Run Time Error : {label} solid name incorrect");
                            }
                            else
                            {
                                if (leftobjectIndex >= 0 && leftobjectIndex <= Script.ResultArtefacts.Count && Script.ResultArtefacts[leftobjectIndex] != null)
                                {
                                    Bounds3D bns = new Bounds3D(Script.ResultArtefacts[leftobjectIndex].AbsoluteBounds);
                                    double midX = bns.MidPoint().X;
                                    double midY = bns.MidPoint().Y;
                                    double midZ = bns.MidPoint().Z;
                                    double dAbsX = 0;
                                    double dAbsY = 0;
                                    double dAbsZ = 0;
                                    Object3D ob = Script.ResultArtefacts[rightobjectIndex];
                                    if (orientationMode == Mode.Align)
                                    {
                                        switch (orientation)
                                        {
                                            case Alignment.Left:
                                                {
                                                    dAbsX = ob.Position.X - (ob.AbsoluteBounds.Lower.X - bns.Lower.X);
                                                    ob.Position = new Point3D(dAbsX, ob.Position.Y, ob.Position.Z);
                                                    ob.RelativeToAbsolute();
                                                }
                                                break;

                                            case Alignment.Right:
                                                {
                                                    dAbsX = ob.Position.X + (bns.Upper.X - ob.AbsoluteBounds.Upper.X);
                                                    ob.Position = new Point3D(dAbsX, ob.Position.Y, ob.Position.Z);
                                                    ob.RelativeToAbsolute();
                                                }
                                                break;

                                            case Alignment.Top:
                                                {
                                                    dAbsY = ob.Position.Y + (bns.Upper.Y - ob.AbsoluteBounds.Upper.Y);
                                                    ob.Position = new Point3D(ob.Position.X, dAbsY, ob.Position.Z);
                                                    ob.RelativeToAbsolute();
                                                }
                                                break;

                                            case Alignment.Bottom:
                                                {
                                                    dAbsY = ob.Position.Y - (ob.AbsoluteBounds.Lower.Y - bns.Lower.Y);
                                                    ob.Position = new Point3D(ob.Position.X, dAbsY, ob.Position.Z);
                                                    ob.RelativeToAbsolute();
                                                }
                                                break;

                                            case Alignment.Back:
                                                {
                                                    dAbsZ = ob.Position.Z - (ob.AbsoluteBounds.Lower.Z - bns.Lower.Z);
                                                    ob.Position = new Point3D(ob.Position.X, ob.Position.Y, dAbsZ);
                                                    ob.RelativeToAbsolute();
                                                }
                                                break;

                                            case Alignment.Front:
                                                {
                                                    dAbsZ = ob.Position.Z + (bns.Upper.Z - ob.AbsoluteBounds.Upper.Z);
                                                    ob.Position = new Point3D(ob.Position.X, ob.Position.Y, dAbsZ);
                                                    ob.RelativeToAbsolute();
                                                }
                                                break;

                                            case Alignment.Centre:
                                                {
                                                    dAbsX = ob.Position.X - (ob.AbsoluteBounds.MidPoint().X - bns.MidPoint().X);
                                                    dAbsZ = ob.Position.Z - (ob.AbsoluteBounds.MidPoint().Z - bns.MidPoint().Z);
                                                    ob.Position = new Point3D(dAbsX, ob.Position.Y, dAbsZ);
                                                    ob.RelativeToAbsolute();
                                                }
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        switch (orientation)
                                        {
                                            case Alignment.Top:
                                                {
                                                    dAbsX = ob.Position.X - (ob.AbsoluteBounds.MidPoint().X - midX);
                                                    dAbsY = ob.Position.Y - (ob.AbsoluteBounds.Lower.Y - bns.Upper.Y) - 0.001;
                                                    dAbsZ = ob.Position.Z - (ob.AbsoluteBounds.MidPoint().Z - midZ);
                                                    ob.Position = new Point3D(dAbsX, dAbsY, dAbsZ);
                                                    ob.RelativeToAbsolute();
                                                    bns.Add(ob.AbsoluteBounds);
                                                }
                                                break;

                                            case Alignment.Bottom:
                                                {
                                                    dAbsX = ob.Position.X - (ob.AbsoluteBounds.MidPoint().X - midX);
                                                    dAbsY = bns.Lower.Y - (ob.AbsoluteBounds.Height / 2) + 0.001;
                                                    dAbsZ = ob.Position.Z - (ob.AbsoluteBounds.MidPoint().Z - midZ);
                                                    ob.Position = new Point3D(dAbsX, dAbsY, dAbsZ);
                                                    ob.RelativeToAbsolute();
                                                    bns.Add(ob.AbsoluteBounds);
                                                }
                                                break;

                                            case Alignment.Right:
                                                {
                                                    dAbsX = bns.Upper.X + ob.AbsoluteBounds.Width / 2 - 0.001;
                                                    dAbsY = ob.Position.Y - (ob.AbsoluteBounds.MidPoint().Y - midY);
                                                    dAbsZ = ob.Position.Z - (ob.AbsoluteBounds.MidPoint().Z - midZ);
                                                    ob.Position = new Point3D(dAbsX, dAbsY, dAbsZ);
                                                    ob.RelativeToAbsolute();
                                                    bns.Add(ob.AbsoluteBounds);
                                                }
                                                break;

                                            case Alignment.Left:
                                                {
                                                    dAbsX = bns.Lower.X - ob.AbsoluteBounds.Width / 2 + 0.001;
                                                    dAbsY = ob.Position.Y - (ob.AbsoluteBounds.MidPoint().Y - midY);
                                                    dAbsZ = ob.Position.Z - (ob.AbsoluteBounds.MidPoint().Z - midZ);
                                                    ob.Position = new Point3D(dAbsX, dAbsY, dAbsZ);
                                                    ob.RelativeToAbsolute();
                                                    bns.Add(ob.AbsoluteBounds);
                                                }
                                                break;

                                            case Alignment.Front:
                                                {
                                                    dAbsX = ob.Position.X - (ob.AbsoluteBounds.MidPoint().X - midX);
                                                    dAbsY = ob.Position.Y - (ob.AbsoluteBounds.MidPoint().Y - midY);
                                                    dAbsZ = ob.Position.Z - (ob.AbsoluteBounds.Lower.Z - bns.Upper.Z) - 0.001;
                                                    ob.Position = new Point3D(dAbsX, dAbsY, dAbsZ);
                                                    ob.RelativeToAbsolute();
                                                    bns.Add(ob.AbsoluteBounds);
                                                }
                                                break;

                                            case Alignment.Back:
                                                {
                                                    dAbsX = ob.Position.X - (ob.AbsoluteBounds.MidPoint().X - midX);
                                                    dAbsY = ob.Position.Y - (ob.AbsoluteBounds.MidPoint().Y - midY);

                                                    dAbsZ = bns.Lower.Z - (ob.AbsoluteBounds.Depth / 2) + 0.001;
                                                    ob.Position = new Point3D(dAbsX, dAbsY, dAbsZ);
                                                    ob.RelativeToAbsolute();
                                                    bns.Add(ob.AbsoluteBounds);
                                                }
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            Log.Instance().AddEntry($"Run Time Error : {label} unknown solid");
                        }
                    }
                }
            }
            return result;
        }

        private void GenerateLabel()
        {
            switch (orientationMode)
            {
                case Mode.Align:
                    {
                        label = "Align";
                    }
                    break;

                case Mode.Stack:
                    {
                        label = "Stack";
                    }
                    break;
            }
            switch (orientation)
            {
                case Alignment.Left: label += "Left"; break;
                case Alignment.Right: label += "Right"; break;
                case Alignment.Top:
                    {
                        if (orientationMode == Mode.Align)
                        {
                            label += "Top";
                        }
                        else
                        {
                            label += "Above";
                        }
                    }
                    break;

                case Alignment.Bottom:
                    {
                        if (orientationMode == Mode.Align)
                        {
                            label += "Bottom";
                        }
                        else
                        {
                            label += "Below";
                        }
                    }
                    break;

                case Alignment.Front: label += "Front"; break;
                case Alignment.Back:
                    {
                        if (orientationMode == Mode.Align)
                        {
                            label += "Back";
                        }
                        else
                        {
                            label += "Behind";
                        }
                    }
                    break;

                case Alignment.Centre: label += "Centre"; break;
            }
        }
    }
}