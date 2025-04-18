using asdflibrary;
using Barnacle.Dialogs.SdfModel.Operations;
using Barnacle.Dialogs.SdfModel.Primitives;
using Barnacle.Object3DLib;
using OctTreeLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs.SdfModel
{
    internal class SdfModelMaker : BaseModellerDialog
    {
        public StepManager stepManager;
        private OctTree octTree;

        public SdfModelMaker()
        {
            stepManager = new StepManager();
            Resolution = 1;
        }

        public double Resolution
        {
            get;
            set;
        }

        internal void AddStep(string primitiveType, Point3D position, double sizeX, double sizeY, double sizeZ, double rotX, double rotY, double rotZ, string opType, double blend, double thickness)
        {
            switch (primitiveType.ToLower())
            {
                case "box":
                    {
                        Vector3D size = new Vector3D(sizeX, sizeY, sizeZ);
                        SdfOperation operation = GetStepOperation(opType, blend);
                        SdfStep step = new SdfStep();
                        SdfBox sp = new SdfBox();
                        sp.Size = size;
                        sp.Position = position;
                        if (rotX != 0 || rotY != 0 || rotZ != 0)
                        {
                            sp.Rotation = Rotations.RotateXYZ(rotX, rotY, rotZ);
                        }
                        sp.RotX = rotX;
                        sp.RotY = rotY;
                        sp.RotZ = rotZ;
                        step.Primitive = sp;
                        step.Operation = operation;
                        stepManager.Steps.Add(step);
                    }
                    break;

                case "sphere":
                    {
                        Vector3D size = new Vector3D(sizeX, sizeY, sizeZ);
                        SdfOperation operation = GetStepOperation(opType, blend);
                        SdfStep step = new SdfStep();
                        SdfSphere sp = new SdfSphere();
                        sp.Size = size;
                        sp.Position = position;
                        step.Primitive = sp;
                        step.Operation = operation;
                        stepManager.Steps.Add(step);
                    }
                    break;

                case "torus":
                    {
                        Vector3D size = new Vector3D(sizeX, sizeY, sizeZ);

                        SdfOperation operation = GetStepOperation(opType, blend);
                        SdfStep step = new SdfStep();
                        SdfTorus sp = new SdfTorus();
                        sp.Size = size;
                        sp.Position = position;
                        if (rotX != 0 || rotY != 0 || rotZ != 0)
                        {
                            sp.Rotation = Rotations.RotateXYZ(rotX, rotY, rotZ);
                        }
                        sp.RotX = rotX;
                        sp.RotY = rotY;
                        sp.RotZ = rotZ;
                        sp.Thickness = thickness;
                        step.Primitive = sp;
                        step.Operation = operation;
                        stepManager.Steps.Add(step);
                    }
                    break;

                case "cylinder":
                    {
                        Vector3D size = new Vector3D(sizeX, sizeY, sizeZ);
                        SdfOperation operation = GetStepOperation(opType, blend);
                        SdfStep step = new SdfStep();
                        SdfCylinder sp = new SdfCylinder();
                        sp.Size = size;
                        sp.Position = position;
                        if (rotX != 0 || rotY != 0 || rotZ != 0)
                        {
                            sp.Rotation = Rotations.RotateXYZ(rotX, rotY, rotZ);
                        }
                        sp.RotX = rotX;
                        sp.RotY = rotY;
                        sp.RotZ = rotZ;
                        step.Primitive = sp;
                        step.Operation = operation;
                        stepManager.Steps.Add(step);
                    }
                    break;

                case "triangle":
                    {
                        Vector3D size = new Vector3D(sizeX, sizeY, sizeZ);
                        SdfOperation operation = GetStepOperation(opType, blend);
                        SdfStep step = new SdfStep();
                        SdfTriangle sp = new SdfTriangle();
                        sp.Size = size;
                        sp.Position = position;
                        if (rotX != 0 || rotY != 0 || rotZ != 0)
                        {
                            sp.Rotation = Rotations.RotateXYZ(rotX, rotY, rotZ);
                        }
                        sp.RotX = rotX;
                        sp.RotY = rotY;
                        sp.RotZ = rotZ;
                        step.Primitive = sp;
                        step.Operation = operation;
                        stepManager.Steps.Add(step);
                    }
                    break;
            }
        }

        internal void ClearSteps()
        {
            stepManager.Clear();
        }

        internal double Generate(Point3DCollection vertices, Int32Collection faces)
        {
            LoggerLib.Logger.MarkStart("Sdf Model");
            Bounds3D bounds = stepManager.GetBounds();
            if (stepManager.Steps.Count > 0)
            {
                // first find out how big the sdf field is by calculating the bounds of all the steps

                bounds.Expand(new Point3D(3, 3, 3)); ;
                CubeMarcher cm = new CubeMarcher();
                GridCell gc = new GridCell();
                List<asdflibrary.Triangle> triangles = new List<asdflibrary.Triangle>();
                CreateOctree(bounds.Lower,
                     bounds.Upper, vertices);
                bool bufferPoints = false;
                for (float x = (float)bounds.Lower.X; x <= (float)bounds.Upper.X; x += (float)Resolution)
                {
                    for (float y = (float)bounds.Lower.Y; y <= (float)bounds.Upper.Y; y += (float)Resolution)
                    {
                        bufferPoints = false;
                        for (float z = (float)bounds.Lower.Z; z <= (float)bounds.Upper.Z; z += (float)Resolution)
                        {
                            if (bufferPoints)
                            {
                                // bottom
                                gc.p[0] = gc.p[3];
                                gc.val[0] = gc.val[3];

                                gc.p[1] = gc.p[2];
                                gc.val[1] = gc.val[2];

                                gc.p[2] = new XYZ(x + Resolution, y, z + Resolution);
                                gc.p[3] = new XYZ(x, y, z + Resolution);

                                gc.p[4] = gc.p[7];
                                gc.val[4] = gc.val[7];
                                gc.p[5] = gc.p[6];
                                gc.val[5] = gc.val[6];
                                gc.p[6] = new XYZ(x + Resolution, y + Resolution, z + Resolution);
                                gc.p[7] = new XYZ(x, y + Resolution, z + Resolution);
                                gc.val[2] = stepManager.GetSdfValue(new Point3D(gc.p[2].x,
                                                                        gc.p[2].y,
                                                                        gc.p[2].z));

                                gc.val[3] = stepManager.GetSdfValue(new Point3D(gc.p[3].x,
                                                                                gc.p[3].y,
                                                                                gc.p[3].z));

                                gc.val[6] = stepManager.GetSdfValue(new Point3D(gc.p[6].x,
                                                                                gc.p[6].y,
                                                                                gc.p[6].z));

                                gc.val[7] = stepManager.GetSdfValue(new Point3D(gc.p[7].x,
                                                                                gc.p[7].y,
                                                                                gc.p[7].z));
                            }
                            else
                            {
                                // bottom
                                gc.p[0] = new XYZ(x, y, z);
                                gc.p[1] = new XYZ(x + Resolution, y, z);
                                gc.p[2] = new XYZ(x + Resolution, y, z + Resolution);
                                gc.p[3] = new XYZ(x, y, z + Resolution);

                                // top
                                gc.p[4] = new XYZ(x, y + Resolution, z);
                                gc.p[5] = new XYZ(x + Resolution, y + Resolution, z);
                                gc.p[6] = new XYZ(x + Resolution, y + Resolution, z + Resolution);
                                gc.p[7] = new XYZ(x, y + Resolution, z + Resolution);
                                for (int i = 0; i < 8; i++)
                                {
                                    gc.val[i] = stepManager.GetSdfValue(new Point3D(gc.p[i].x,
                                                                                    gc.p[i].y,
                                                                                    gc.p[i].z));
                                }
                            }
                            triangles.Clear();

                            cm.Polygonise(gc, 0, triangles);

                            foreach (asdflibrary.Triangle t in triangles)
                            {
                                int p0 = octTree.AddPoint(t.p[0].x, t.p[0].y, t.p[0].z);
                                int p1 = octTree.AddPoint(t.p[1].x, t.p[1].y, t.p[1].z);
                                int p2 = octTree.AddPoint(t.p[2].x, t.p[2].y, t.p[2].z);
                                faces.Add(p0);
                                faces.Add(p1);
                                faces.Add(p2);
                            }
                            bufferPoints = true;
                        }
                    }
                }
            }
            else
            {
                bounds.Zero();
            }
            LoggerLib.Logger.MarkEnd("Sdf Model");
            return bounds.Upper.Y;
        }

        internal string GetStepsAsText()
        {
            return stepManager.GetStepsAsText();
        }

        internal void SetSteps(string s)
        {
            stepManager.SetSteps(s);
        }

        private void CreateOctree(Point3D minPoint, Point3D maxPoint, Point3DCollection vertices)
        {
            octTree = new OctTreeLib.OctTree(vertices,
                                  minPoint,
                                  maxPoint,
                                  200);
        }

        private SdfOperation GetStepOperation(string opType, double blend)
        {
            SdfOperation op = null;
            if (!String.IsNullOrEmpty(opType))
                switch (opType.ToLower())
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
                            (op as SdfSmoothUnionOp).Blend = blend;
                        }
                        break;

                    case "smooth subtraction":
                        {
                            op = new SdfSmoothSubtractionOp();

                            (op as SdfSmoothSubtractionOp).Blend = blend;
                        }
                        break;

                    case "smooth intersection":
                        {
                            op = new SdfSmoothIntersectionOp();

                            (op as SdfSmoothIntersectionOp).Blend = blend;
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

                            (op as SdfSmoothXorOp).Blend = blend;
                        }
                        break;
                }

            return op;
        }
    }
}