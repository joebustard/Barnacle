/*
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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;


namespace CSGLib
{
    /// <summary>
    /// Class used to apply boolean operations on solids.
    /// Two 'Solid' objects are submitted to this class constructor. There is a methods for
    /// each boolean operation. Each of these return a 'Solid' resulting from the application
    /// of its operation into the submitted solids.
    /// </summary>
    public class BooleanModeller
    {
        /** solid where boolean operations will be applied */
        private Part Object1;
        private Part Object2;

        private Solid resultObject;
        public enum OperationType
        {
            Union,
            Difference,
            Intersection,
            ReverseDifference
        }
        public BooleanModeller(Solid solid1, Solid solid2)
        {
            State = ModellerState.Bad;
            //representation to apply boolean operations
            Object1 = new Part(solid1);
            Object2 = new Part(solid2);

            //split the faces so that none of them intercepts each other
            //Logger.Log("Split Faces ob1 (ob2)\r\n");
            Part.PartState ps1 = Object1.SplitFaces(Object2);
            Part.PartState ps2 = Part.PartState.Good;
            Part.PartState ps3 = Part.PartState.Good;
            Part.PartState ps4 = Part.PartState.Good;
            if (ps1 == Part.PartState.Good)
            {

                ps2 = Object2.SplitFaces(Object1);
                if (ps2 == Part.PartState.Good)
                {
                    //classify faces as being inside or outside the other solid
                    ps3 = Object1.ClassifyFaces(Object2);
                    if (ps3 == Part.PartState.Good)
                    {
                        ps4 = Object2.ClassifyFaces(Object1);
                    }
                    State = ModellerState.Good;
                }
            }
            if (ps1 == Part.PartState.Interupted ||
                ps2 == Part.PartState.Interupted ||
                ps3 == Part.PartState.Interupted ||
                ps4 == Part.PartState.Interupted)
            {
                State = ModellerState.Interrupted;
            }
        }

        public BooleanModeller()
        {
        }
        public struct OpResult
        {
            public ModellerState OperationStatus;
            public Solid ResultObject;
        }
        public async Task<OpResult> DoModelOperationAsync(Point3DCollection lp, Int32Collection li,
                                    Point3DCollection rp, Int32Collection ri,
                                    OperationType op, CancellationTokenSource csgCancelation)
        {
            BlockingCollection<Point3D> lpnts = new BlockingCollection<Point3D>();
            foreach (Point3D p in lp)
            {
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
                rpnts.Add(new Point3D(p.X, p.Y, p.Z));
            }

            BlockingCollection<int> rint = new BlockingCollection<int>();
            foreach (int i in ri)
            {
                rint.Add(i);
            }
            return await Task.Run(() =>
            {
                BooleanModeller btmp = new BooleanModeller();
                btmp.SetCancelationToken(csgCancelation.Token);
                OpResult res = btmp.DoModelOperation(lpnts, lint, rpnts, rint, op);
                return res;
            }, csgCancelation.Token).ConfigureAwait(false);

        }
        private static CancellationToken cancelToken;
        public void SetCancelationToken(CancellationToken cancellationToken)
        {
            cancelToken = cancellationToken;
        }


        public OpResult DoModelOperation(BlockingCollection<Point3D> blp, BlockingCollection<int> bli,
                                      BlockingCollection<Point3D> brp, BlockingCollection<int> bri,
                                      OperationType op)
        {
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

            opresult.OperationStatus = ModellerState.Interrupted;
            if (!cancelToken.IsCancellationRequested)
            {
                Solid leftie = new Solid(lp, li, false);
                Object1 = new Part(leftie);
                if (!cancelToken.IsCancellationRequested)
                {

                    Solid rightie = new Solid(rp, ri, false);
                    if (!cancelToken.IsCancellationRequested)
                    {

                        Object2 = new Part(rightie);
                        if (!cancelToken.IsCancellationRequested)
                        {

                            State = ModellerState.Bad;


                            //split the faces so that none of them intercepts each other
                            //Logger.Log("Split Faces ob1 (ob2)\r\n");

                            if (!cancelToken.IsCancellationRequested && Object1.SplitFaces(Object2) == Part.PartState.Good)
                            {

                                if (!cancelToken.IsCancellationRequested && Object2.SplitFaces(Object1) == Part.PartState.Good)
                                {

                                    if (!cancelToken.IsCancellationRequested && Object1.ClassifyFaces(Object2) == Part.PartState.Good)
                                    {
                                        if (!cancelToken.IsCancellationRequested && Object2.ClassifyFaces(Object1) == Part.PartState.Good)
                                        {
                                            State = ModellerState.Good;
                                            switch (op)
                                            {
                                                case OperationType.Union:
                                                    {
                                                        resultObject = GetUnion();
                                                    }
                                                    break;
                                                case OperationType.Difference:
                                                    {
                                                        resultObject = GetDifference();
                                                    }
                                                    break;
                                                case OperationType.ReverseDifference:
                                                    {
                                                        resultObject = GetReverseDifference();
                                                    }
                                                    break;
                                                case OperationType.Intersection:
                                                    {
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
                    if (resultObject != null)
                    {
                        opresult.OperationStatus = ModellerState.Good;
                        opresult.ResultObject = resultObject;
                    }
                }
            }

            return opresult;
        }
        public enum ModellerState
        {
            Bad,
            Good,
            Interrupted
        }
        public ModellerState State { get; set; }
        //--------------------------------CONSTRUCTORS----------------------------------//



        //-------------------------------BOOLEAN_OPERATIONS-----------------------------//

        /**
        * Gets the solid generated by the union of the two solids submitted to the constructor
        *
        * @return solid generated by the union of the two solids submitted to the constructor
        */

        public Solid GetDifference()
        {
            Object2.InvertInsideFaces();
            Solid result = ComposeSolid(Status.OUTSIDE, Status.OPPOSITE, Status.INSIDE);
            Object2.InvertInsideFaces();

            return result;
        }

        public Solid GetIntersection()
        {
            //   Object2.InvertInsideFaces();
            Solid result = ComposeSolid(Status.INSIDE, Status.SAME, Status.INSIDE);
            //Object2.InvertInsideFaces();
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

        private Solid ComposeSolid(Status faceStatus1, Status faceStatus2, Status faceStatus3)
        {
            var vertices = new List<Vertex>();
            var indices = new List<int>();

            //group the elements of the two solids whose faces fit with the desired status
            GroupObjectComponents(Object1, vertices, indices, faceStatus1, faceStatus2);
            // GroupObjectComponents(Object2, vertices, indices, faceStatus3, faceStatus3);
            GroupObjectComponents(Object2, vertices, indices, faceStatus3, faceStatus3);

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

        private void GroupObjectComponents(Part obj, List<Vertex> vertices, List<int> indices, Status faceStatus1, Status faceStatus2)
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
                        if (vertices.Contains(faceVerts[j]))
                        {
                            indices.Add(vertices.IndexOf(faceVerts[j]));
                        }
                        else
                        {
                            indices.Add(vertices.Count);
                            vertices.Add(faceVerts[j]);
                        }
                    }
                }
            }
        }
    }
}