﻿/*
The MIT License (MIT)

Copyright (c) 2014 Sebastian Loncar

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

See:
D. H. Laidlaw, W. B. Trumbore, and J. F. Hughes.
"Constructive Solid Geometry for Polyhedral Objects"
SIGGRAPH Proceedings, 1986, p.161.

original author: Danilo Balby Silva Castanheira (danbalby@yahoo.com)

Ported from Java to C# by Sebastian Loncar, Web: http://www.loncar.de
Project: https://github.com/Arakis/CSGLib

Optimized and refactored by: Lars Brubaker (larsbrubaker@matterhackers.com)
Project: https://github.com/MatterHackers/agg-sharp (an included library)
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace CSGLib
{
    public enum CSGState
    {
        Unknown,
        Good,
        Bad,
        Toolarge,
        Interrupted
    };

    /// <summary>
    /// Class used to apply boolean operations on solids.
    /// Two 'Solid' objects are submitted to this class constructor. There is a methods for
    /// each boolean operation. Each of these return a 'Solid' resulting from the application
    /// of its operation into the submitted solids.
    /// </summary>
    public class BooleanModeller
    {
        /** solid where boolean operations will be applied */
        private static CancellationToken cancelToken;
        private static CSGGroupProgress currentProgress = new CSGGroupProgress();
        private Part Object1;
        private Part Object2;

        private Solid resultObject;

        public BooleanModeller(Solid solid1, Solid solid2, bool classifyFaces = true)
        {
            State = CSGState.Bad;
            //representation to apply boolean operations
            Object1 = new Part(solid1);
            Object2 = new Part(solid2);
            if (classifyFaces)
            {
                //split the faces so that none of them intercepts each other
                //Logger.Log("Split Faces ob1 (ob2)\r\n");
                CSGState ps1 = Object1.SplitFaces(Object2, null, "");
                CSGState ps2 = CSGState.Good;
                CSGState ps3 = CSGState.Good;
                CSGState ps4 = CSGState.Good;
                if (ps1 == CSGState.Good)
                {
                    ps2 = Object2.SplitFaces(Object1, null, "");
                    if (ps2 == CSGState.Good)
                    {
                        //classify faces as being inside or outside the other solid
                        ps3 = Object1.ClassifyFaces(Object2, null, "");
                        if (ps3 == CSGState.Good)
                        {
                            ps4 = Object2.ClassifyFaces(Object1, null, "");
                        }
                        State = CSGState.Good;
                    }
                }
                if (ps1 == CSGState.Interrupted ||
                    ps2 == CSGState.Interrupted ||
                    ps3 == CSGState.Interrupted ||
                    ps4 == CSGState.Interrupted)
                {
                    State = CSGState.Interrupted;
                }
            }
            else
            {
                State = CSGState.Good;
            }
        }

        public BooleanModeller()
        {
        }

        public enum OperationType
        {
            Union,
            Difference,
            Intersection,
            ReverseDifference,
            ForceUnion
        }

        public CSGState State
        {
            get; set;
        }

        public static void ReportProgress(string v1, int v2, IProgress<CSGGroupProgress> progress)
        {
            if (progress != null)
            {
                currentProgress.Text = v1;
                currentProgress.Val = v2;
                progress.Report(currentProgress);
            }
        }

        public Solid ComposeSolid(Status faceStatus1, Status faceStatus2, Status faceStatus3, Part Object1, Part Object2)
        {
            Vertex min1;
            Vertex max1;
            Vertex min2;
            Vertex max2;
            Object1.octTree.Bounds(out min1, out max1);
            Object2.octTree.Bounds(out min2, out max2);
            double minx, miny, minz;
            double maxx, maxy, maxz;
            minx = Math.Min(min1.Position.X, min2.Position.X);
            miny = Math.Min(min1.Position.Y, min2.Position.Y);
            minz = Math.Min(min1.Position.Z, min2.Position.Z);

            maxx = Math.Max(max1.Position.X, max2.Position.X);
            maxy = Math.Max(max1.Position.Y, max2.Position.Y);
            maxz = Math.Max(max1.Position.Z, max2.Position.Z);

            Vertex min = new Vertex(minx, miny, minz);
            Vertex max = new Vertex(maxx, maxy, maxz);

            var vertices = new List<Vertex>();
            var indices = new List<int>();
            OctTree octTree = new OctTree(vertices, min, max, 100);

            //group the elements of the two solids whose faces fit with the desired status
            GroupObjectComponents(octTree, Object1, vertices, indices, faceStatus1, faceStatus2);
            // GroupObjectComponents(Object2, vertices, indices, faceStatus3, faceStatus3);
            GroupObjectComponents(octTree, Object2, vertices, indices, faceStatus3, faceStatus3);

            //turn the arrayLists to arrays
            Vector3D[] verticesArray = new Vector3D[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                verticesArray[i] = vertices[i].Position;
            }
            int[] indicesArray = new int[indices.Count];
            for (int i = 0; i < indices.Count; i++)
            {
                indicesArray[i] = indices[i];
            }

            //returns the solid containing the grouped elements
            return new Solid(verticesArray, indicesArray);
        }

        public OpResult DoModelOperation(BlockingCollection<Point3D> blp, BlockingCollection<int> bli,
                                      BlockingCollection<Point3D> brp, BlockingCollection<int> bri,
                                      OperationType op, IProgress<CSGGroupProgress> progress)
        {
            ReportProgress("Initialising", 0, progress);
            OpResult opresult = new OpResult();
            opresult.ResultObject = null;
            Point3DCollection lp = new Point3DCollection();
            bool more = true;
            while (more && !cancelToken.IsCancellationRequested)
            {
                Point3D pn;
                more = blp.TryTake(out pn);
                if (more) lp.Add(pn);
            }
            Point3DCollection rp = new Point3DCollection();
            more = true;
            while (more && !cancelToken.IsCancellationRequested)
            {
                Point3D pn;
                more = brp.TryTake(out pn);
                if (more) rp.Add(pn);
            }
            Int32Collection li = new Int32Collection();
            more = true;
            while (more && !cancelToken.IsCancellationRequested)
            {
                int i;
                more = bli.TryTake(out i);
                if (more) li.Add(i);
            }
            Int32Collection ri = new Int32Collection();
            more = true;
            while (more && !cancelToken.IsCancellationRequested)
            {
                int i;
                more = bri.TryTake(out i);
                if (more) ri.Add(i);
            }

            opresult.OperationStatus = CSGState.Interrupted;
            if (!cancelToken.IsCancellationRequested)
            {
                Solid leftie = new Solid(lp, li, false);
                Object1 = new Part(leftie);
                Object1.SetCancelationToken(cancelToken);
                if (!cancelToken.IsCancellationRequested)
                {
                    Solid rightie = new Solid(rp, ri, false);
                    if (!cancelToken.IsCancellationRequested)
                    {
                        Object2 = new Part(rightie);
                        Object2.SetCancelationToken(cancelToken);
                        if (!cancelToken.IsCancellationRequested)
                        {
                            State = CSGState.Bad;

                            //split the faces so that none of them intercepts each other

                            ReportProgress("Split Left Faces", 0, progress);
                            if (!cancelToken.IsCancellationRequested)
                            {
                                opresult.OperationStatus = Object1.SplitFaces(Object2, progress, "Split Left Faces");
                                if (opresult.OperationStatus == CSGState.Good)
                                {
                                    ReportProgress("Split Right Faces", 0, progress);
                                    if (!cancelToken.IsCancellationRequested)
                                    {
                                        opresult.OperationStatus = Object2.SplitFaces(Object1, progress, "Split Right Faces");
                                        if (opresult.OperationStatus == CSGState.Good)
                                        {
                                            ReportProgress("Classify Left Faces", 0, progress);
                                            if (!cancelToken.IsCancellationRequested)
                                            {
                                                opresult.OperationStatus = Object1.ClassifyFaces(Object2, progress, "Classify Left Faces");
                                                if (opresult.OperationStatus == CSGState.Good)
                                                {
                                                    ReportProgress("Classify Right Faces", 0, progress);
                                                    if (!cancelToken.IsCancellationRequested)
                                                    {
                                                        opresult.OperationStatus = Object2.ClassifyFaces(Object1, progress, "Classify RIght Faces");
                                                        if (opresult.OperationStatus == CSGState.Good)
                                                        {
                                                            State = CSGState.Good;
                                                            switch (op)
                                                            {
                                                                case OperationType.Union:
                                                                    {
                                                                        ReportProgress("Union", 0, progress);
                                                                        resultObject = GetUnion();
                                                                    }
                                                                    break;

                                                                case OperationType.Difference:
                                                                    {
                                                                        ReportProgress("DIfference", 0, progress);
                                                                        resultObject = GetDifference();
                                                                    }
                                                                    break;

                                                                case OperationType.ReverseDifference:
                                                                    {
                                                                        ReportProgress("Reverse Difference", 0, progress);
                                                                        resultObject = GetReverseDifference();
                                                                    }
                                                                    break;

                                                                case OperationType.Intersection:
                                                                    {
                                                                        ReportProgress("Intersection", 0, progress);
                                                                        resultObject = GetIntersection();
                                                                    }
                                                                    break;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (resultObject != null)
                    {
                        opresult.OperationStatus = CSGState.Good;
                        opresult.ResultObject = resultObject;
                    }
                }
            }

            return opresult;
        }

        public async Task<OpResult> DoModelOperationAsync(Point3DCollection lp, Int32Collection li,
                                    Point3DCollection rp, Int32Collection ri,
                                    OperationType op, CancellationTokenSource csgCancelation, IProgress<CSGGroupProgress> progress)
        {
            Point3D minPnt = new Point3D(double.MaxValue, double.MaxValue, double.MaxValue);
            Point3D maxPnt = new Point3D(double.MinValue, double.MinValue, double.MinValue);

            BlockingCollection<Point3D> lpnts = new BlockingCollection<Point3D>();
            foreach (Point3D p in lp)
            {
                minPnt = MinPoint(minPnt, p);
                maxPnt = MaxPoint(maxPnt, p);

                lpnts.Add(new Point3D(p.X, p.Y, p.Z));
            }

            BlockingCollection<int> lint = new BlockingCollection<int>();
            foreach (int i in li)
            {
                lint.Add(i);
            }
            BlockingCollection<Point3D> rpnts = new BlockingCollection<Point3D>();
            foreach (Point3D p in rp)
            {
                minPnt = MinPoint(minPnt, p);
                maxPnt = MaxPoint(maxPnt, p);

                rpnts.Add(new Point3D(p.X, p.Y, p.Z));
            }

            BlockingCollection<int> rint = new BlockingCollection<int>();
            foreach (int i in ri)
            {
                rint.Add(i);
            }
            return await Task.Run(() => {
                BooleanModeller btmp = new BooleanModeller();
                btmp.SetCancelationToken(csgCancelation.Token);
                OpResult res;
                if (op == OperationType.ForceUnion)
                {
                    Point3DCollection vertices = new Point3DCollection();
                    Int32Collection faces = new Int32Collection();
                    OctTreeLib.OctTree octree = new OctTreeLib.OctTree(vertices, minPnt, maxPnt, 200);
                    Point3D[] lps = lpnts.ToArray();
                    Int32[] lfs = lint.ToArray();
                    for (int fi = 0; fi < lfs.GetLength(0); fi++)
                    {
                        Point3D pn = lps[lfs[fi]];
                        int nf = octree.AddPoint(pn);
                        faces.Add(nf);
                    }
                    lps = rpnts.ToArray();
                    lfs = rint.ToArray();
                    for (int fi = 0; fi < lfs.GetLength(0); fi++)
                    {
                        Point3D pn = lps[lfs[fi]];
                        int nf = octree.AddPoint(pn);
                        faces.Add(nf);
                    }
                    res.OperationStatus = CSGState.Good;
                    res.ResultObject = new Solid(vertices, faces, false);
                }
                else
                {
                    res = btmp.DoModelOperation(lpnts, lint, rpnts, rint, op, progress);
                }
                return res;
            }, csgCancelation.Token).ConfigureAwait(true);
        }

        public Solid GetDifference()
        {
            Object2.InvertInsideFaces();
            Solid result = ComposeSolid(Status.OUTSIDE, Status.OPPOSITE, Status.INSIDE);
            Object2.InvertInsideFaces();

            return result;
        }

        public Solid GetForceUnion()
        {
            Vertex min1;
            Vertex max1;
            Vertex min2;
            Vertex max2;
            Object1.octTree.Bounds(out min1, out max1);
            Object2.octTree.Bounds(out min2, out max2);
            double minx, miny, minz;
            double maxx, maxy, maxz;
            minx = Math.Min(min1.Position.X, min2.Position.X);
            miny = Math.Min(min1.Position.Y, min2.Position.Y);
            minz = Math.Min(min1.Position.Z, min2.Position.Z);

            maxx = Math.Max(max1.Position.X, max2.Position.X);
            maxy = Math.Max(max1.Position.Y, max2.Position.Y);
            maxz = Math.Max(max1.Position.Z, max2.Position.Z);

            Point3D min = new Point3D(minx, miny, minz);
            Point3D max = new Point3D(maxx, maxy, maxz);

            Solid result = new Solid();

            Point3DCollection vertices = new Point3DCollection();
            Int32Collection faces = new Int32Collection();
            OctTreeLib.OctTree octree = new OctTreeLib.OctTree(vertices, min, max, 200);

            for (int fi = 0; fi < Object1.NumFaces; fi++)
            {
                Face f = Object1.GetFace(fi);
                Point3D pn = new Point3D(f.V1.Position.X, f.V1.Position.Y, f.V1.Position.Z);
                int nf = octree.AddPoint(pn);
                faces.Add(nf);
                pn = new Point3D(f.V2.Position.X, f.V2.Position.Y, f.V2.Position.Z);
                nf = octree.AddPoint(pn);
                faces.Add(nf);
                pn = new Point3D(f.V3.Position.X, f.V3.Position.Y, f.V3.Position.Z);
                nf = octree.AddPoint(pn);
                faces.Add(nf);
            }
            for (int fi = 0; fi < Object2.NumFaces; fi++)
            {
                Face f = Object2.GetFace(fi);
                Point3D pn = new Point3D(f.V1.Position.X, f.V1.Position.Y, f.V1.Position.Z);
                int nf = octree.AddPoint(pn);
                faces.Add(nf);
                pn = new Point3D(f.V2.Position.X, f.V2.Position.Y, f.V2.Position.Z);
                nf = octree.AddPoint(pn);
                faces.Add(nf);
                pn = new Point3D(f.V3.Position.X, f.V3.Position.Y, f.V3.Position.Z);
                nf = octree.AddPoint(pn);
                faces.Add(nf);
            }
            return new Solid(vertices, faces, false);
        }

        public Solid GetIntersection()
        {
            Solid result = ComposeSolid(Status.INSIDE, Status.SAME, Status.INSIDE);

            return result;
        }

        public Solid GetReverseDifference()
        {
            Object1.InvertInsideFaces();
            Solid result = ComposeSolid(Status.INSIDE, Status.OPPOSITE, Status.OUTSIDE);
            Object1.InvertInsideFaces();

            return result;
        }

        public Solid GetUnion()
        {
            return ComposeSolid(Status.OUTSIDE, Status.SAME, Status.OUTSIDE);
        }

        public void SetCancelationToken(CancellationToken cancellationToken)
        {
            cancelToken = cancellationToken;
        }

        private Solid ComposeSolid(Status faceStatus1, Status faceStatus2, Status faceStatus3)
        {
            Vertex min1;
            Vertex max1;
            Vertex min2;
            Vertex max2;
            Object1.octTree.Bounds(out min1, out max1);
            Object2.octTree.Bounds(out min2, out max2);
            double minx, miny, minz;
            double maxx, maxy, maxz;
            minx = Math.Min(min1.Position.X, min2.Position.X);
            miny = Math.Min(min1.Position.Y, min2.Position.Y);
            minz = Math.Min(min1.Position.Z, min2.Position.Z);

            maxx = Math.Max(max1.Position.X, max2.Position.X);
            maxy = Math.Max(max1.Position.Y, max2.Position.Y);
            maxz = Math.Max(max1.Position.Z, max2.Position.Z);

            Vertex min = new Vertex(minx, miny, minz);
            Vertex max = new Vertex(maxx, maxy, maxz);

            var vertices = new List<Vertex>();
            var indices = new List<int>();
            OctTree octTree = new OctTree(vertices, min, max, 100);

            //group the elements of the two solids whose faces fit with the desired status
            GroupObjectComponents(octTree, Object1, vertices, indices, faceStatus1, faceStatus2);
            // GroupObjectComponents(Object2, vertices, indices, faceStatus3, faceStatus3);
            GroupObjectComponents(octTree, Object2, vertices, indices, faceStatus3, faceStatus3);

            //turn the arrayLists to arrays
            Vector3D[] verticesArray = new Vector3D[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                verticesArray[i] = vertices[i].Position;
            }
            int[] indicesArray = new int[indices.Count];
            for (int i = 0; i < indices.Count; i++)
            {
                indicesArray[i] = indices[i];
            }

            //returns the solid containing the grouped elements
            return new Solid(verticesArray, indicesArray);
        }

        private void GroupObjectComponents(OctTree octTree, Part obj, List<Vertex> vertices, List<int> indices, Status faceStatus1, Status faceStatus2)
        {
            Face face;
            //for each face..
            for (int i = 0; i < obj.NumFaces; i++)
            {
                face = obj.GetFace(i);
                //if the face status fits with the desired status...
                if (face.GetStatus() == faceStatus1 || face.GetStatus() == faceStatus2)
                {
                    //adds the face elements into the arrays
                    Vertex[] faceVerts = { face.V1, face.V2, face.V3 };
                    for (int j = 0; j < faceVerts.Length; j++)
                    {
                        int vi = octTree.PointPresent(faceVerts[j]);
                        if (vi != -1)
                        {
                            indices.Add(vi);
                        }
                        else
                        {
                            indices.Add(vertices.Count);
                            octTree.AddPoint(vertices.Count, faceVerts[j]);
                        }
                    }
                }
            }
        }

        private Point3D MaxPoint(Point3D p1, Point3D p2)
        {
            return new Point3D(
            Math.Max(p1.X, p2.X),
            Math.Max(p1.Y, p2.Y),
            Math.Max(p1.Z, p2.Z)
            );
        }

        private Point3D MinPoint(Point3D p1, Point3D p2)
        {
            return new Point3D(
           Math.Min(p1.X, p2.X),
           Math.Min(p1.Y, p2.Y),
           Math.Min(p1.Z, p2.Z)
           );
        }

        public struct OpResult
        {
            public CSGState OperationStatus;
            public Solid ResultObject;
        }

        //--------------------------------CONSTRUCTORS----------------------------------//

        //-------------------------------BOOLEAN_OPERATIONS-----------------------------//

        /**
        * Gets the solid generated by the union of the two solids submitted to the constructor
        *
        * @return solid generated by the union of the two solids submitted to the constructor
        */
        //--------------------------PRIVATES--------------------------------------------//

        /**
        * Composes a solid based on the faces status of the two operators solids:
        * Status.INSIDE, Status.OUTSIDE, Status.SAME, Status.OPPOSITE
        *
        * @param faceStatus1 status expected for the first solid faces
        * @param faceStatus2 other status expected for the first solid faces
        * (expected a status for the faces coincident with second solid faces)
        * @param faceStatus3 status expected for the second solid faces
        */
        /**
        * Fills solid arrays with data about faces of an object generated whose status
        * is as required
        *
        * @param object3d solid object used to fill the arrays
        * @param vertices vertices array to be filled
        * @param indices indices array to be filled
        * @param faceStatus1 a status expected for the faces used to to fill the data arrays
        * @param faceStatus2 a status expected for the faces used to to fill the data arrays
        */
    }
}