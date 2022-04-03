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

using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media.Media3D;
namespace CSGLib
{
    /// <summary>
    /// Data structure about a 3d solid to apply boolean operations in it.
    /// Tipically, two 'Object3d' objects are created to apply boolean operation. The
    /// methods splitFaces() and classifyFaces() are called in this sequence for both objects,
    /// always using the other one as parameter.Then the faces from both objects are collected
    /// according their status.
    /// </summary>
    public class Part
    {
        private int maxSplitLevel = 300;

        /// <summary>
        /// tolerance value to test equalities
        /// </summary>
        private static readonly double EqualityTolerance = 1e-8f;

        /// <summary>
        /// object representing the solid extremes
        /// </summary>
        private Bound _Bound;

        /// <summary>
        /// solid faces
        /// </summary>
        private List<Face> Faces;

        /// <summary>
        /// solid vertices
        /// </summary>
        private List<Vertex> Vertices;

        private OctTree octTree;
        public enum PartState
        {
            Good,
            Bad,
            Interupted
        }
        /// <summary>
        /// Constructs a Object3d object based on a solid file.
        /// </summary>
        /// <param name="solid">solid used to construct the Object3d object</param>
        public Part(Solid solid)
        {
            Vertex v1, v2, v3, vertex;
            Vector3D[] verticesPoints = solid.GetVertices();
            int[] indices = solid.GetIndices();
            var verticesTemp = new List<Vertex>();

            //create vertices
   
            Vertices = new List<Vertex>();
            octTree = new OctTree(Vertices,solid.Minimum,solid.Maximum,100);
            for (int i = 0; i < verticesPoints.Length; i++)
            {
                vertex = AddVertex(octTree,verticesPoints[i], Status.UNKNOWN);
                verticesTemp.Add(vertex);
            }

            //create faces
            Faces = new List<Face>();
            for (int i = 0; i < indices.Length; i = i + 3)
            {
                v1 = verticesTemp[indices[i]];
                v2 = verticesTemp[indices[i + 1]];
                v3 = verticesTemp[indices[i + 2]];
                AddFace(v1, v2, v3, 0);
            }

            //create bound
            _Bound = new Bound(verticesPoints);
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        private Part()
        {
        }

        /// <summary>
        /// Classify faces as being inside, outside or on boundary of other object
        /// </summary>
        /// <param name="otherObject">object 3d used for the comparison</param>
        public PartState ClassifyFaces(Part otherObject)
        {
            PartState result = PartState.Good;
            //calculate adjacency information
            Face face;
            for (int i = 0; i < NumFaces; i++)
            {
               
                face = GetFace(i);
                face.V1.AddAdjacentVertex(face.V2);
                face.V1.AddAdjacentVertex(face.V3);
                face.V2.AddAdjacentVertex(face.V1);
                face.V2.AddAdjacentVertex(face.V3);
                face.V3.AddAdjacentVertex(face.V1);
                face.V3.AddAdjacentVertex(face.V2);
            }

            //for each face
            for (int i = 0; i < NumFaces; i++)
            {
                face = GetFace(i);
               
                //if the face vertices aren't classified to make the simple classify
                if (face.SimpleClassify() == false)
                {
                    //makes the ray trace classification
                    if (face.RayTraceClassify(otherObject))
                    {

                        //mark the vertices
                        if (face.V1.Status == Status.UNKNOWN)
                        {
                            face.V1.Mark(face.GetStatus());
                        }
                        if (face.V2.Status == Status.UNKNOWN)
                        {
                            face.V2.Mark(face.GetStatus());
                        }
                        if (face.V3.Status == Status.UNKNOWN)
                        {
                            face.V3.Mark(face.GetStatus());
                        }
                    }
                    else
                    {
                        result = PartState.Interupted;
                        break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Clones the Object3D object
        /// </summary>
        /// <returns>cloned object</returns>
        public Part Clone()
        {
            Part clone = new Part();
            clone.Vertices = new List<Vertex>();
            for (int i = 0; i < Vertices.Count; i++)
            {
                clone.Vertices.Add(Vertices[i].Clone());
            }
            clone.Faces = new List<Face>();
            for (int i = 0; i < Vertices.Count; i++)
            {
                clone.Faces.Add(Faces[i].Clone());
            }
            clone._Bound = _Bound;

            return clone;
        }

        /// <summary>
        /// Gets the solid bound
        /// </summary>
        /// <returns>solid bound</returns>
        public Bound Bound => _Bound;

        /// <summary>
        /// Gets a face reference for a given position
        /// </summary>
        /// <param name="index">required face position</param>
        /// <returns>face reference , null if the position is invalid</returns>
        public Face GetFace(int index)
        {
            if (index < 0 || index >= Faces.Count)
                return null;
            else
                return Faces[index];
        }

        /// <summary>
        /// Gets the number of faces
        /// </summary>
        /// <returns>number of faces</returns>
        public int NumFaces => Faces.Count;

        /// <summary>
        /// Inverts faces classified as INSIDE, making its normals point outside. Usually used into the second solid when the difference is applied.
        /// </summary>
        public void InvertInsideFaces()
        {
            Face face;
            for (int i = 0; i < NumFaces; i++)
            {
                face = GetFace(i);
                if (face.GetStatus() == Status.INSIDE)
                {
                    face.Invert();
                }
            }
        }

        /// <summary>
        /// Split faces so that none face is intercepted by a face of other object
        /// </summary>
        /// <param name="obj">the other object 3d used to make the split</param>
        public PartState SplitFaces(Part obj)
        {
            PartState result = PartState.Good;
            Line line;
            Face face1, face2;
            Segment segment1;
            Segment segment2;
            double distFace1Vert1, distFace1Vert2, distFace1Vert3, distFace2Vert1, distFace2Vert2, distFace2Vert3;
            int signFace1Vert1, signFace1Vert2, signFace1Vert3, signFace2Vert1, signFace2Vert2, signFace2Vert3;
            int numFacesBefore = NumFaces;
            int numFacesStart = NumFaces + obj.NumFaces;
            int facesLimit = 50 * numFacesStart;
            if ( facesLimit > 1000000)
            {
                facesLimit = 1000000;
            }
            //if the objects bounds overlap...
            if (_Bound.Overlap(obj._Bound))
            {
                //for each object1 face...
                for (int i = 0; i < NumFaces; i++)
                {

                    //if object1 face bound and object2 bound overlap ...
                    face1 = GetFace(i);

                    if (face1.SplitLevel <= maxSplitLevel && face1.GetBound().Overlap(obj._Bound))
                    {
                        //for each object2 face...
                        for (int j = 0; j < obj.NumFaces; j++)
                        {
                           
                            //if object1 face bound and object2 face bound overlap...
                            face2 = obj.GetFace(j);
                            if (face2.SplitLevel <= maxSplitLevel && face1.GetBound().Overlap(face2.GetBound()))
                            {
                                //PART I - DO TWO POLIGONS INTERSECT?
                                //POSSIBLE RESULTS: INTERSECT, NOT_INTERSECT, COPLANAR

                                //distance from the face1 vertices to the face2 plane
                                distFace1Vert1 = ComputeDistance(face1.V1, face2);
                                distFace1Vert2 = ComputeDistance(face1.V2, face2);
                                distFace1Vert3 = ComputeDistance(face1.V3, face2);

                                //distances signs from the face1 vertices to the face2 plane
                                signFace1Vert1 = distFace1Vert1 > EqualityTolerance ? 1 : (distFace1Vert1 < -EqualityTolerance ? -1 : 0);
                                signFace1Vert2 = distFace1Vert2 > EqualityTolerance ? 1 : (distFace1Vert2 < -EqualityTolerance ? -1 : 0);
                                signFace1Vert3 = distFace1Vert3 > EqualityTolerance ? 1 : (distFace1Vert3 < -EqualityTolerance ? -1 : 0);

                                //if all the signs are zero, the planes are coplanar
                                //if all the signs are positive or negative, the planes do not intersect
                                //if the signs are not equal...
                                //if ((signFace1Vert1 == signFace1Vert2 && signFace1Vert2 == signFace1Vert3) && signFace1Vert1 == 0)
                                //{
                                //    Logger.Log("  Coplanar\r\n");
                                //}
                                //if ((signFace1Vert1 == signFace1Vert2 && signFace1Vert2 == signFace1Vert3))
                                //{
                                //    Logger.Log("  No Interception\r\n");
                                //}
                                if (!(signFace1Vert1 == signFace1Vert2 && signFace1Vert2 == signFace1Vert3))
                                {
                                    //distance from the face2 vertices to the face1 plane
                                    distFace2Vert1 = ComputeDistance(face2.V1, face1);
                                    distFace2Vert2 = ComputeDistance(face2.V2, face1);
                                    distFace2Vert3 = ComputeDistance(face2.V3, face1);

                                    //distances signs from the face2 vertices to the face1 plane
                                    signFace2Vert1 = distFace2Vert1 > EqualityTolerance ? 1 : (distFace2Vert1 < -EqualityTolerance ? -1 : 0);
                                    signFace2Vert2 = distFace2Vert2 > EqualityTolerance ? 1 : (distFace2Vert2 < -EqualityTolerance ? -1 : 0);
                                    signFace2Vert3 = distFace2Vert3 > EqualityTolerance ? 1 : (distFace2Vert3 < -EqualityTolerance ? -1 : 0);

                                    //if the signs are not equal...
                                    if (!(signFace2Vert1 == signFace2Vert2 && signFace2Vert2 == signFace2Vert3))
                                    {
                                        line = new Line(face1, face2);

                                        //intersection of the face1 and the plane of face2
                                        segment1 = new Segment(line, face1, signFace1Vert1, signFace1Vert2, signFace1Vert3);

                                        //intersection of the face2 and the plane of face1
                                        segment2 = new Segment(line, face2, signFace2Vert1, signFace2Vert2, signFace2Vert3);

                                        //if the two segments intersect...
                                        if (segment1.Intersect(segment2))
                                        {
                                            //PART II - SUBDIVIDING NON-COPLANAR POLYGONS
                                            int lastNumFaces = NumFaces;
                                            SplitFace(i, segment1, segment2);

                                            //prevent from infinite loop (with a loss of faces...)
                                            //                                            if (numFacesStart * 100 < NumFaces)
                                            if (NumFaces > facesLimit)
                                            {
                                                //Logger.Log("possible infinite loop situation: terminating faces split");
                                                return PartState.Bad;
                                            }

                                            //if the face in the position isn't the same, there was a break
                                            if (face1 != GetFace(i))
                                            {
                                                //if the generated solid is equal the origin...
                                                if (face1.Equals(GetFace(NumFaces - 1)))
                                                {
                                                    //return it to its position and jump it
                                                    if (i != (NumFaces - 1))
                                                    {
                                                        Faces.RemoveAt(NumFaces - 1);
                                                        Faces.Insert(i, face1);
                                                    }
                                                    else
                                                    {
                                                        continue;
                                                    }
                                                }
                                                //else: test next face
                                                else
                                                {
                                                    i--;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                            }
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Method used to add a face properly for internal methods
        /// </summary>
        /// <param name="v1">a face vertex</param>
        /// <param name="v2">a face vertex</param>
        /// <param name="v3">a face vertex</param>
        /// <returns></returns>
        private Face AddFace(Vertex v1, Vertex v2, Vertex v3, int splitLevel)
        {
            //  Logger.Log("AddFace\r\n");
            //    Logger.Log(v1.ToString() + "\r\n");
            //    Logger.Log(v2.ToString() + "\r\n");
            //Logger.Log(v3.ToString() + "\r\n");

            if (!(v1.Equals(v2) || v1.Equals(v3) || v2.Equals(v3)))
            {
                Face face = new Face(v1, v2, v3);
                face.SplitLevel = splitLevel;
                //if (face.GetArea() > EqualityTolerance)
                if (face.GetArea() > 0.00000001 && splitLevel < maxSplitLevel)
                {
                    Faces.Add(face);
                    return face;
                }
                else
                {
                    //Logger.Log("Reject Adding face with invalid area\r\n");
                    return null;
                }
            }
            else
            {
                //Logger.Log("Reject Adding invalid face\r\n");
                return null;
            }
        }

        /// <summary>
        /// Method used to add a vertex properly for internal methods
        /// </summary>
        /// <param name="pos">vertex position</param>
        /// <param name="status">vertex status</param>
        /// <returns>The vertex inserted (if a similar vertex already exists, this is returned).</returns>
        private Vertex AddVertex(OctTree octTree, Vector3D pos, Status status)
        {
        
            int i;
            //if already there is an equal vertex, it is not inserted
            Vertex vertex = new Vertex(pos, status);
            /*
            int index = octTree.PointPresent(vertex);
            if (index != -1 && index <Vertices.Count )
            {
                vertex = Vertices[index];
                vertex.SetStatus(status);
                return vertex;
            }
            else
            {
                octTree.AddPoint(Vertices.Count, vertex);
                //Vertices.Add(vertex);
                return vertex;
            }

*/
            for (i = 0; i < Vertices.Count; i++)
            {
                if (vertex.Equals(Vertices[i]))
                    break;
            }
            if (i == Vertices.Count)
            {
                Vertices.Add(vertex);
                return vertex;
            }
            else
            {
                vertex = Vertices[i];
                vertex.SetStatus(status);
                return vertex;
            }
            
        }

        /// <summary>
        /// Face breaker for FACE-FACE-FACE
        /// </summary>
        /// <param name="facePos">face position on the faces array</param>
        /// <param name="newPos1">new vertex position</param>
        /// <param name="newPos2">new vertex position</param>
        /// <param name="linedVertex">linedVertex what vertex is more lined with the interersection found</param>
        private void BreakFaceInFive(int facePos, Vector3D newPos1, Vector3D newPos2, int linedVertex)
        {
            // Logger.Log("BreakFaceInFive\r\n");
            Face face = Faces[facePos];
            Faces.RemoveAt(facePos);

            Vertex vertex1 = AddVertex(octTree,newPos1, Status.BOUNDARY);
            Vertex vertex2 = AddVertex(octTree, newPos2, Status.BOUNDARY);

            if (linedVertex == 1)
            {
                AddFace(face.V2, face.V3, vertex1, face.SplitLevel + 1);
                AddFace(face.V2, vertex1, vertex2, face.SplitLevel + 1);
                AddFace(face.V3, vertex2, vertex1, face.SplitLevel + 1);
                AddFace(face.V2, vertex2, face.V1, face.SplitLevel + 1);
                AddFace(face.V3, face.V1, vertex2, face.SplitLevel + 1);
            }
            else if (linedVertex == 2)
            {
                AddFace(face.V3, face.V1, vertex1, face.SplitLevel + 1);
                AddFace(face.V3, vertex1, vertex2, face.SplitLevel + 1);
                AddFace(face.V1, vertex2, vertex1, face.SplitLevel + 1);
                AddFace(face.V3, vertex2, face.V2, face.SplitLevel + 1);
                AddFace(face.V1, face.V2, vertex2, face.SplitLevel + 1);
            }
            else
            {
                AddFace(face.V1, face.V2, vertex1, face.SplitLevel + 1);
                AddFace(face.V1, vertex1, vertex2, face.SplitLevel + 1);
                AddFace(face.V2, vertex2, vertex1, face.SplitLevel + 1);
                AddFace(face.V1, vertex2, face.V3, face.SplitLevel + 1);
                AddFace(face.V2, face.V3, vertex2, face.SplitLevel + 1);
            }
        }

        /// <summary>
        /// Face breaker for EDGE-FACE-FACE / FACE-FACE-EDGE
        /// </summary>
        /// <param name="facePos">face position on the faces array</param>
        /// <param name="newPos1">new vertex position</param>
        /// <param name="newPos2">new vertex position</param>
        /// <param name="endVertex">vertex used for the split</param>
        private void BreakFaceInFour(int facePos, Vector3D newPos1, Vector3D newPos2, Vertex endVertex)
        {
            //  Logger.Log("BreakFaceInFour\r\n");
            Face face = Faces[facePos];
            Faces.RemoveAt(facePos);

            Vertex vertex1 = AddVertex(octTree, newPos1, Status.BOUNDARY);
            Vertex vertex2 = AddVertex(octTree, newPos2, Status.BOUNDARY);

            if (endVertex.Equals(face.V1))
            {
                AddFace(face.V1, vertex1, vertex2, face.SplitLevel + 1);
                AddFace(vertex1, face.V2, vertex2, face.SplitLevel + 1);
                AddFace(face.V2, face.V3, vertex2, face.SplitLevel + 1);
                AddFace(face.V3, face.V1, vertex2, face.SplitLevel + 1);
            }
            else if (endVertex.Equals(face.V2))
            {
                AddFace(face.V2, vertex1, vertex2, face.SplitLevel + 1);
                AddFace(vertex1, face.V3, vertex2, face.SplitLevel + 1);
                AddFace(face.V3, face.V1, vertex2, face.SplitLevel + 1);
                AddFace(face.V1, face.V2, vertex2, face.SplitLevel + 1);
            }
            else
            {
                AddFace(face.V3, vertex1, vertex2, face.SplitLevel + 1);
                AddFace(vertex1, face.V1, vertex2, face.SplitLevel + 1);
                AddFace(face.V1, face.V2, vertex2, face.SplitLevel + 1);
                AddFace(face.V2, face.V3, vertex2, face.SplitLevel + 1);
            }
        }

        /// <summary>
        /// Face breaker for EDGE-EDGE-EDGE
        /// </summary>
        /// <param name="facePos">face position on the faces array</param>
        /// <param name="newPos1">new vertex position</param>
        /// <param name="newPos2">new vertex position</param>
        /// <param name="splitEdge">edge that will be split</param>
        private void BreakFaceInThree(int facePos, Vector3D newPos1, Vector3D newPos2, int splitEdge)
        {
            // Logger.Log("BreakFaceInThree EDGE EDGE EDGE\r\n");
            Face face = Faces[facePos];
            Faces.RemoveAt(facePos);

            Vertex vertex1 = AddVertex(octTree, newPos1, Status.BOUNDARY);
            Vertex vertex2 = AddVertex(octTree, newPos2, Status.BOUNDARY);

            if (splitEdge == 1)
            {
                AddFace(face.V1, vertex1, face.V3, face.SplitLevel + 1);
                AddFace(vertex1, vertex2, face.V3, face.SplitLevel + 1);
                AddFace(vertex2, face.V2, face.V3, face.SplitLevel + 1);
            }
            else if (splitEdge == 2)
            {
                AddFace(face.V2, vertex1, face.V1, face.SplitLevel + 1);
                AddFace(vertex1, vertex2, face.V1, face.SplitLevel + 1);
                AddFace(vertex2, face.V3, face.V1, face.SplitLevel + 1);
            }
            else
            {
                AddFace(face.V3, vertex1, face.V2, face.SplitLevel + 1);
                AddFace(vertex1, vertex2, face.V2, face.SplitLevel + 1);
                AddFace(vertex2, face.V1, face.V2, face.SplitLevel + 1);
            }
        }

        /// <summary>
        /// Face breaker for VERTEX-FACE-FACE / FACE-FACE-VERTEX
        /// </summary>
        /// <param name="facePos">face position on the faces array</param>
        /// <param name="newPos">new vertex position</param>
        /// <param name="endVertex">vertex used for the split</param>
        private void BreakFaceInThree(int facePos, Vector3D newPos, Vertex endVertex)
        {
            // Logger.Log("BreakFaceInThree VERTEX-FACE-FACE\r\n");
            Face face = Faces[facePos];
            Faces.RemoveAt(facePos);

            Vertex vertex = AddVertex(octTree, newPos, Status.BOUNDARY);

            if (endVertex.Equals(face.V1))
            {
                AddFace(face.V1, face.V2, vertex, face.SplitLevel + 1);
                AddFace(face.V2, face.V3, vertex, face.SplitLevel + 1);
                AddFace(face.V3, face.V1, vertex, face.SplitLevel + 1);
            }
            else if (endVertex.Equals(face.V2))
            {
                AddFace(face.V2, face.V3, vertex, face.SplitLevel + 1);
                AddFace(face.V3, face.V1, vertex, face.SplitLevel + 1);
                AddFace(face.V1, face.V2, vertex, face.SplitLevel + 1);
            }
            else
            {
                AddFace(face.V3, face.V1, vertex, face.SplitLevel + 1);
                AddFace(face.V1, face.V2, vertex, face.SplitLevel + 1);
                AddFace(face.V2, face.V3, vertex, face.SplitLevel + 1);
            }
        }

        /// <summary>
        /// Face breaker for EDGE-FACE-EDGE
        /// </summary>
        /// <param name="facePos">face position on the faces array</param>
        /// <param name="newPos1">new vertex position</param>
        /// <param name="newPos2">new vertex position</param>
        /// <param name="startVertex">vertex used for the new faces creation</param>
        /// <param name="endVertex">vertex used for the new faces creation</param>
        private void BreakFaceInThree(int facePos, Vector3D newPos1, Vector3D newPos2, Vertex startVertex, Vertex endVertex)
        {
            //  Logger.Log("BreakFaceInThree EDGE FACE EDGE\r\n");
            Face face = Faces[facePos];
            Faces.RemoveAt(facePos);

            Vertex vertex1 = AddVertex(octTree, newPos1, Status.BOUNDARY);
            Vertex vertex2 = AddVertex(octTree, newPos2, Status.BOUNDARY);

            if (startVertex.Equals(face.V1) && endVertex.Equals(face.V2))
            {
                AddFace(face.V1, vertex1, vertex2, face.SplitLevel + 1);
                AddFace(face.V1, vertex2, face.V3, face.SplitLevel + 1);
                AddFace(vertex1, face.V2, vertex2, face.SplitLevel + 1);
            }
            else if (startVertex.Equals(face.V2) && endVertex.Equals(face.V1))
            {
                AddFace(face.V1, vertex2, vertex1, face.SplitLevel + 1);
                AddFace(face.V1, vertex1, face.V3, face.SplitLevel + 1);
                AddFace(vertex2, face.V2, vertex1, face.SplitLevel + 1);
            }
            else if (startVertex.Equals(face.V2) && endVertex.Equals(face.V3))
            {
                AddFace(face.V2, vertex1, vertex2, face.SplitLevel + 1);
                AddFace(face.V2, vertex2, face.V1, face.SplitLevel + 1);
                AddFace(vertex1, face.V3, vertex2, face.SplitLevel + 1);
            }
            else if (startVertex.Equals(face.V3) && endVertex.Equals(face.V2))
            {
                AddFace(face.V2, vertex2, vertex1, face.SplitLevel + 1);
                AddFace(face.V2, vertex1, face.V1, face.SplitLevel + 1);
                AddFace(vertex2, face.V3, vertex1, face.SplitLevel + 1);
            }
            else if (startVertex.Equals(face.V3) && endVertex.Equals(face.V1))
            {
                AddFace(face.V3, vertex1, vertex2, face.SplitLevel + 1);
                AddFace(face.V3, vertex2, face.V2, face.SplitLevel + 1);
                AddFace(vertex1, face.V1, vertex2, face.SplitLevel + 1);
            }
            else
            {
                AddFace(face.V3, vertex2, vertex1, face.SplitLevel + 1);
                AddFace(face.V3, vertex1, face.V2, face.SplitLevel + 1);
                AddFace(vertex2, face.V1, vertex1, face.SplitLevel + 1);
            }
        }

        /// <summary>
        /// Face breaker for FACE-FACE-FACE (a point only)
        /// </summary>
        /// <param name="facePos">face position on the faces array</param>
        /// <param name="newPos">new vertex position</param>
        private void BreakFaceInThree(int facePos, Vector3D newPos)
        {
            //Logger.Log("BreakFaceInThree FACE FACE FACE\r\n");
            Face face = Faces[facePos];
            Faces.RemoveAt(facePos);

            Vertex vertex = AddVertex(octTree, newPos, Status.BOUNDARY);

            AddFace(face.V1, face.V2, vertex, face.SplitLevel + 1);
            AddFace(face.V2, face.V3, vertex, face.SplitLevel + 1);
            AddFace(face.V3, face.V1, vertex, face.SplitLevel + 1);
        }

        /// <summary>
        /// Face breaker for VERTEX-EDGE-EDGE / EDGE-EDGE-VERTEX
        /// </summary>
        /// <param name="facePos">face position on the faces array</param>
        /// <param name="newPos">new vertex position</param>
        /// <param name="splitEdge">edge that will be split</param>
        private void BreakFaceInTwo(int facePos, Vector3D newPos, int splitEdge)
        {
            // Logger.Log("BreakFaceInTwo VERTEX EDGE EDGE\r\n");
            Face face = Faces[facePos];
            Faces.RemoveAt(facePos);

            Vertex vertex = AddVertex(octTree, newPos, Status.BOUNDARY);

            if (splitEdge == 1)
            {
                AddFace(face.V1, vertex, face.V3, face.SplitLevel + 1);
                AddFace(vertex, face.V2, face.V3, face.SplitLevel + 1);
            }
            else if (splitEdge == 2)
            {
                AddFace(face.V2, vertex, face.V1, face.SplitLevel + 1);
                AddFace(vertex, face.V3, face.V1, face.SplitLevel + 1);
            }
            else
            {
                AddFace(face.V3, vertex, face.V2, face.SplitLevel + 1);
                AddFace(vertex, face.V1, face.V2, face.SplitLevel + 1);
            }
        }

        /// <summary>
        /// Face breaker for VERTEX-FACE-EDGE / EDGE-FACE-VERTEX
        /// </summary>
        /// <param name="facePos">face position on the faces array</param>
        /// <param name="newPos">new vertex position</param>
        /// <param name="endVertex">vertex used for splitting</param>
        private void BreakFaceInTwo(int facePos, Vector3D newPos, Vertex endVertex)
        {
            //  Logger.Log("BreakFaceInThree VERTEX FACE EDGE\r\n");
            Face face = Faces[facePos];
            Faces.RemoveAt(facePos);

            Vertex vertex = AddVertex(octTree, newPos, Status.BOUNDARY);

            if (endVertex.Equals(face.V1))
            {
                AddFace(face.V1, vertex, face.V3, face.SplitLevel + 1);
                AddFace(vertex, face.V2, face.V3, face.SplitLevel + 1);
            }
            else if (endVertex.Equals(face.V2))
            {
                AddFace(face.V2, vertex, face.V1, face.SplitLevel + 1);
                AddFace(vertex, face.V3, face.V1, face.SplitLevel + 1);
            }
            else
            {
                AddFace(face.V3, vertex, face.V2, face.SplitLevel + 1);
                AddFace(vertex, face.V1, face.V2, face.SplitLevel + 1);
            }
        }

        /// <summary>
        /// Computes closest distance from a vertex to a plane
        /// </summary>
        /// <param name="vertex">vertex used to compute the distance</param>
        /// <param name="face">face representing the plane where it is contained</param>
        /// <returns>the closest distance from the vertex to the plane</returns>
        private double ComputeDistance(Vertex vertex, Face face)
        {
            Vector3D normal = face.GetNormal();
            double distToV1 = Vector3D.DotProduct(normal, face.V1._Position);
            double distToPositionMinusDistToV1 = Vector3D.DotProduct(normal, vertex._Position) - distToV1;
            return distToPositionMinusDistToV1;
        }

        /// <summary>
        /// Split an individual face
        /// </summary>
        /// <param name="facePos">face position on the array of faces</param>
        /// <param name="segment1">segment representing the intersection of the face with the plane</param>
        /// <param name="segment2">segment representing the intersection of other face with the plane of the current face plane</param>
        private void SplitFace(int facePos, Segment segment1, Segment segment2)
        {
            // Logger.Log("SplitFace\r\n");
            Vector3D startPos, endPos;
            int startType, endType, middleType;
            double startDist, endDist;

            Face face = GetFace(facePos);
            if (face.SplitLevel <= maxSplitLevel)
            {
                Vertex startVertex = segment1.StartVertex;
                Vertex endVertex = segment1.EndVertex;

                //starting point: deeper starting point
                if (segment2.StartDist > segment1.StartDist + EqualityTolerance)
                {
                    startDist = segment2.StartDist;
                    startType = segment1.IntermediateType;
                    startPos = segment2.StartPosition;
                }
                else
                {
                    startDist = segment1.StartDist;
                    startType = segment1.StartType;
                    startPos = segment1.StartPosition;
                }

                //ending point: deepest ending point
                if (segment2.EndDistance < segment1.EndDistance - EqualityTolerance)
                {
                    endDist = segment2.EndDistance;
                    endType = segment1.IntermediateType;
                    endPos = segment2.EndPosition;
                }
                else
                {
                    endDist = segment1.EndDistance;
                    endType = segment1.EndType;
                    endPos = segment1.EndPosition;
                }
                middleType = segment1.IntermediateType;

                //set vertex to BOUNDARY if it is start type
                if (startType == Segment.VERTEX)
                {
                    startVertex.SetStatus(Status.BOUNDARY);
                }

                //set vertex to BOUNDARY if it is end type
                if (endType == Segment.VERTEX)
                {
                    endVertex.SetStatus(Status.BOUNDARY);
                }

                //VERTEX-_______-VERTEX
                if (startType == Segment.VERTEX && endType == Segment.VERTEX)
                {
                    return;
                }

                //______-EDGE-______
                else if (middleType == Segment.EDGE)
                {
                    //gets the edge
                    int splitEdge;
                    if ((startVertex == face.V1 && endVertex == face.V2) || (startVertex == face.V2 && endVertex == face.V1))
                    {
                        splitEdge = 1;
                    }
                    else if ((startVertex == face.V2 && endVertex == face.V3) || (startVertex == face.V3 && endVertex == face.V2))
                    {
                        splitEdge = 2;
                    }
                    else
                    {
                        splitEdge = 3;
                    }

                    //VERTEX-EDGE-EDGE
                    if (startType == Segment.VERTEX)
                    {
                        BreakFaceInTwo(facePos, endPos, splitEdge);
                        return;
                    }

                    //EDGE-EDGE-VERTEX
                    else if (endType == Segment.VERTEX)
                    {
                        BreakFaceInTwo(facePos, startPos, splitEdge);
                        return;
                    }

                    // EDGE-EDGE-EDGE
                    else if (startDist == endDist)
                    {
                        BreakFaceInTwo(facePos, endPos, splitEdge);
                    }
                    else
                    {
                        if ((startVertex == face.V1 && endVertex == face.V2) || (startVertex == face.V2 && endVertex == face.V3) || (startVertex == face.V3 && endVertex == face.V1))
                        {
                            BreakFaceInThree(facePos, startPos, endPos, splitEdge);
                        }
                        else
                        {
                            BreakFaceInThree(facePos, endPos, startPos, splitEdge);
                        }
                    }
                    return;
                }

                //______-FACE-______

                //VERTEX-FACE-EDGE
                else if (startType == Segment.VERTEX && endType == Segment.EDGE)
                {
                    BreakFaceInTwo(facePos, endPos, endVertex);
                }
                //EDGE-FACE-VERTEX
                else if (startType == Segment.EDGE && endType == Segment.VERTEX)
                {
                    BreakFaceInTwo(facePos, startPos, startVertex);
                }
                //VERTEX-FACE-FACE
                else if (startType == Segment.VERTEX && endType == Segment.FACE)
                {
                    BreakFaceInThree(facePos, endPos, startVertex);
                }
                //FACE-FACE-VERTEX
                else if (startType == Segment.FACE && endType == Segment.VERTEX)
                {
                    BreakFaceInThree(facePos, startPos, endVertex);
                }
                //EDGE-FACE-EDGE
                else if (startType == Segment.EDGE && endType == Segment.EDGE)
                {
                    BreakFaceInThree(facePos, startPos, endPos, startVertex, endVertex);
                }
                //EDGE-FACE-FACE
                else if (startType == Segment.EDGE && endType == Segment.FACE)
                {
                    BreakFaceInFour(facePos, startPos, endPos, startVertex);
                }
                //FACE-FACE-EDGE
                else if (startType == Segment.FACE && endType == Segment.EDGE)
                {
                    BreakFaceInFour(facePos, endPos, startPos, endVertex);
                }
                //FACE-FACE-FACE
                else if (startType == Segment.FACE && endType == Segment.FACE)
                {
                    //  Logger.Log("FACE FACE FACE\r\n");
                    Vector3D segmentVector = new Vector3D(startPos.X - endPos.X, startPos.Y - endPos.Y, startPos.Z - endPos.Z);

                    //if the intersection segment is a point only...
                    if (Math.Abs(segmentVector.X) < EqualityTolerance && Math.Abs(segmentVector.Y) < EqualityTolerance && Math.Abs(segmentVector.Z) < EqualityTolerance)
                    {
                        BreakFaceInThree(facePos, startPos);
                        return;
                    }

                    //gets the vertex more lined with the intersection segment
                    int linedVertex;
                    Vector3D linedVertexPos;
                    Vector3D vertexVector = new Vector3D(endPos.X - face.V1._Position.X, endPos.Y - face.V1._Position.Y, endPos.Z - face.V1._Position.Z);
                    vertexVector.Normalize();
                    double dot1 = Math.Abs(Vector3D.DotProduct(segmentVector, vertexVector));
                    vertexVector = new Vector3D(endPos.X - face.V2._Position.X, endPos.Y - face.V2._Position.Y, endPos.Z - face.V2._Position.Z);
                    vertexVector.Normalize();
                    double dot2 = Math.Abs(Vector3D.DotProduct(segmentVector, vertexVector));
                    vertexVector = new Vector3D(endPos.X - face.V3._Position.X, endPos.Y - face.V3._Position.Y, endPos.Z - face.V3._Position.Z);
                    vertexVector.Normalize();
                    double dot3 = Math.Abs(Vector3D.DotProduct(segmentVector, vertexVector));
                    if (dot1 > dot2 && dot1 > dot3)
                    {
                        linedVertex = 1;
                        linedVertexPos = face.V1.Position;
                    }
                    else if (dot2 > dot3 && dot2 > dot1)
                    {
                        linedVertex = 2;
                        linedVertexPos = face.V2.Position;
                    }
                    else
                    {
                        linedVertex = 3;
                        linedVertexPos = face.V3.Position;
                    }

                    // Now find which of the intersection endpoints is nearest to that vertex.
                    if ((linedVertexPos - startPos).Length > (linedVertexPos - endPos).Length)
                    {
                        BreakFaceInFive(facePos, startPos, endPos, linedVertex);
                    }
                    else
                    {
                        BreakFaceInFive(facePos, endPos, startPos, linedVertex);
                    }
                }
            }
        }
    }
}