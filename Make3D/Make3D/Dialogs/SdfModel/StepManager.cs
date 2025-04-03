using asdflibrary;
using Barnacle.Dialogs.SdfModel.Operations;
using Barnacle.Dialogs.SdfModel.Primitives;
using Barnacle.Object3DLib;
using MeshDecimator.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs.SdfModel
{
    internal class StepManager
    {
        public StepManager()
        {
            Steps = new List<SdfStep>();
        }

        public delegate void StepsChanged();

        public StepsChanged OnStepsChanged
        {
            get; set;
        }

        public List<SdfStep> Steps
        {
            get;
            set;
        }

        public void Clear()
        {
            Steps?.Clear();
        }

        public void OnUpDateOpType(int id, string changeTo)
        {
            if (id >= 0 && id < Steps.Count)
            {
                SdfStep s = Steps[id];
                switch (changeTo.ToLower())
                {
                    case "union":
                        {
                            s.Operation = new SdfUnionOp();
                        }
                        break;

                    case "subtraction":
                        {
                            s.Operation = new SdfSubtractionOp();
                        }
                        break;

                    case "intersection":
                        {
                            s.Operation = new SdfIntersectionOp();
                        }
                        break;

                    case "smooth union":
                        {
                            s.Operation = new SdfSmoothUnionOp();
                        }
                        break;

                    case "smooth subtraction":
                        {
                            s.Operation = new SdfSmoothSubtractionOp();
                        }
                        break;

                    case "smooth intersection":
                        {
                            s.Operation = new SdfSmoothIntersectionOp();
                        }
                        break;

                    case "xor":
                        {
                            s.Operation = new SdfXorOp();
                        }
                        break;

                    case "smooth xor":
                        {
                            s.Operation = new SdfSmoothXorOp();
                        }
                        break;
                }
            }
        }

        public void OnUpDatePrimitiveType(int id, string changeTo)
        {
            LoggerLib.Logger.LogLine("OnUpdatePrimitiveType");
            if (id >= 0 && id < Steps.Count)
            {
                SdfStep s = Steps[id];
                SdfPrimitive sdfPrimitive = null;
                switch (changeTo.ToLower())
                {
                    case "box":
                        {
                            sdfPrimitive = new SdfBox();
                        }
                        break;

                    case "sphere":
                        {
                            sdfPrimitive = new SdfSphere();
                        }
                        break;

                    case "torus":
                        {
                            sdfPrimitive = new SdfTorus();
                        }
                        break;

                    case "cylinder":
                        {
                            sdfPrimitive = new SdfCylinder();
                        }
                        break;

                    case "triangle":
                        {
                            sdfPrimitive = new SdfTriangle();
                        }
                        break;
                }
                if (sdfPrimitive != null)
                {
                    sdfPrimitive.Position = s.Primitive.Position;
                    sdfPrimitive.Size = s.Primitive.Size;
                    s.Primitive = sdfPrimitive;
                }
                if (OnStepsChanged != null)
                {
                    LoggerLib.Logger.LogLine("OnStepsChanged");
                    OnStepsChanged();
                }
            }
        }

        internal Bounds3D GetBounds()
        {
            Bounds3D bounds = new Bounds3D();
            bounds.Zero();
            foreach (SdfStep step in Steps)
            {
                step.AdjustBounds(bounds);
            }
            return bounds;
        }

        internal double GetSdfValue(Point3D xYZ)
        {
            double res = double.MaxValue;
            if (Steps.Count > 0)
            {
                // first step does not have an operation
                res = Steps[0].GetSdfValue(xYZ);
                for (int i = 1; i < Steps.Count; i++)
                {
                    res = Steps[i].DoOperation(xYZ, res);
                }
            }
            return res;
        }

        internal string GetStepsAsText()
        {
            string s = "";
            for (int i = 0; i < Steps.Count; i++)
            {
                s += Steps[i].ToString();
                if (i < Steps.Count - 1)
                {
                    s += ",";
                }
            }
            return s;
        }

        internal void SetSteps(string s)
        {
            string[] words = s.Split(',');
            if (words != null && words.GetLength(0) > 1)
            {
                int i = 0;
                while (i < words.GetLength(0))
                {
                    switch (words[i].ToLower())
                    {
                        case "box":
                            {
                                i++;
                                Point3D pos = GetStepPosition(words, ref i);
                                Vector3D size = GetStepSize(words, ref i);
                                SdfOperation operation = GetStepOperation(words, ref i);
                                SdfStep step = new SdfStep();
                                SdfBox sp = new SdfBox();
                                sp.Size = size;
                                sp.Position = pos;
                                step.Primitive = sp;
                                step.Operation = operation;
                                Steps.Add(step);
                            }
                            break;

                        case "sphere":
                            {
                                i++;
                                Point3D pos = GetStepPosition(words, ref i);
                                Vector3D size = GetStepSize(words, ref i);
                                SdfOperation operation = GetStepOperation(words, ref i);
                                SdfStep step = new SdfStep();
                                SdfSphere sp = new SdfSphere();
                                sp.Size = size;
                                sp.Position = pos;
                                step.Primitive = sp;
                                step.Operation = operation;
                                Steps.Add(step);
                            }
                            break;

                        case "torus":
                            {
                                i++;
                                Point3D pos = GetStepPosition(words, ref i);
                                Vector3D size = GetStepSize(words, ref i);
                                double thickness = Convert.ToDouble(words[i++]);
                                SdfOperation operation = GetStepOperation(words, ref i);
                                SdfStep step = new SdfStep();
                                SdfTorus sp = new SdfTorus();
                                sp.Size = size;
                                sp.Position = pos;
                                sp.Thickness = thickness;
                                step.Primitive = sp;
                                step.Operation = operation;
                                Steps.Add(step);
                            }
                            break;

                        case "cylinder":
                            {
                                i++;
                                Point3D pos = GetStepPosition(words, ref i);
                                Vector3D size = GetStepSize(words, ref i);
                                SdfOperation operation = GetStepOperation(words, ref i);
                                SdfStep step = new SdfStep();
                                SdfSphere sp = new SdfSphere();
                                sp.Size = size;
                                sp.Position = pos;
                                step.Primitive = sp;
                                step.Operation = operation;
                                Steps.Add(step);
                            }
                            break;
                    }
                }
            }
        }

        private SdfOperation GetStepOperation(string[] words, ref int i)
        {
            SdfOperation op = null;
            switch (words[i++].ToLower())
            {
                case "union":
                    {
                        op = new SdfUnionOp();
                    }
                    break;

                case "subtraction":
                    {
                        op = new SdfSubtractionOp();
                    }
                    break;

                case "intersection":
                    {
                        op = new SdfIntersectionOp();
                    }
                    break;

                case "smooth union":
                    {
                        op = new SdfSmoothUnionOp();
                        double b = Convert.ToDouble(words[i++]);
                        (op as SdfSmoothUnionOp).Blend = b;
                    }
                    break;

                case "smooth subtraction":
                    {
                        op = new SdfSmoothSubtractionOp();
                        double b = Convert.ToDouble(words[i++]);
                        (op as SdfSmoothSubtractionOp).Blend = b;
                    }
                    break;

                case "smooth intersection":
                    {
                        op = new SdfSmoothIntersectionOp();
                        double b = Convert.ToDouble(words[i++]);
                        (op as SdfSmoothIntersectionOp).Blend = b;
                    }
                    break;

                case "xor":
                    {
                        op = new SdfXorOp();
                    }
                    break;

                case "smooth xor":
                    {
                        op = new SdfSmoothXorOp();
                        double b = Convert.ToDouble(words[i++]);
                        (op as SdfSmoothXorOp).Blend = b;
                    }
                    break;
            }

            return op;
        }

        private Point3D GetStepPosition(string[] words, ref int i)
        {
            Point3D pos = new Point3D(0, 0, 0);
            try
            {
                pos.X = Convert.ToDouble(words[i++]);
                pos.Y = Convert.ToDouble(words[i++]);
                pos.Z = Convert.ToDouble(words[i++]);
            }
            catch (Exception ex)
            {
                LoggerLib.Logger.LogException(ex);
            }
            return pos;
        }

        private Vector3D GetStepSize(string[] words, ref int i)
        {
            Vector3D vec = new Vector3D(0, 0, 0);
            try
            {
                vec.X = Convert.ToDouble(words[i++]);
                vec.Y = Convert.ToDouble(words[i++]);
                vec.Z = Convert.ToDouble(words[i++]);
            }
            catch (Exception ex)
            {
                LoggerLib.Logger.LogException(ex);
            }
            return vec;
        }
    }
}