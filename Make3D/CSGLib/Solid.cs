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
using System.Windows.Media;
using System.Windows.Media.Media3D;

//using OpenToolkit.Mathematics;

namespace CSGLib
{
    /// <summary>
    /// Class representing a 3D solid.
    /// </summary>
    public class Solid
    {
        /** array of indices for the vertices from the 'vertices' attribute */
        protected int[] Indices;
        /** array of points defining the solid's vertices */
        protected Vector3D[] Vertices;

        //--------------------------------CONSTRUCTORS----------------------------------//

        /** Constructs an empty solid. */

        private Vertex max;

        private Vertex min;

        public Solid()
        {
            SetInitialFeatures();
        }

        public Solid(Point3DCollection vertices, Int32Collection indices, bool switchWindingOrder) : this()
        {
            Vertices = new Vector3D[vertices.Count];
            Indices = new int[indices.Count];
            double minx = double.MaxValue;
            double miny = double.MaxValue;
            double minz = double.MaxValue;

            double maxx = double.MinValue;
            double maxy = double.MinValue;
            double maxz = double.MinValue;

            if (indices.Count != 0)
            {
                for (int i = 0; i < vertices.Count; i++)
                {
                    Vertices[i] = new Vector3D(vertices[i].X, vertices[i].Y, vertices[i].Z);

                    if (Vertices[i].X < minx)
                    {
                        minx = Vertices[i].X;
                    }
                    if (Vertices[i].X > maxx)
                    {
                        maxx = Vertices[i].X;
                    }

                    if (Vertices[i].Y < miny)
                    {
                        miny = Vertices[i].Y;
                    }
                    if (Vertices[i].Y > maxy)
                    {
                        maxy = Vertices[i].Y;
                    }

                    if (Vertices[i].Z < minz)
                    {
                        minz = Vertices[i].Z;
                    }
                    if (Vertices[i].Z > maxz)
                    {
                        maxz = Vertices[i].Z;
                    }
                }
                min = new Vertex(minx, miny, minz);
                max = new Vertex(maxx, maxy, maxz);
                int face = 0;
                for (int i = 0; i < indices.Count; i += 3)
                {
                    if (i + 2 < indices.Count)
                    {
                        Indices[i] = indices[i];
                        if (switchWindingOrder)
                        {
                            Indices[i + 1] = indices[i + 2];
                            Indices[i + 2] = indices[i + 1];
                        }
                        else
                        {
                            Indices[i + 1] = indices[i + 1];
                            Indices[i + 2] = indices[i + 2];
                        }
                        face++;
                    }
                }
            }
        }

        public Solid(Vector3D[] vertices, int[] indices) : this()
        {
            SetData(vertices, indices);
        }

        public bool IsEmpty => Indices.Length == 0;

        public Vertex Maximum
        {
            get { return max; }
        }

        public Vertex Minimum
        {
            get { return min; }
        }

        public void DumpResult()
        {
            Logger.Log("Results\r\n======\r\n");
            Logger.Log($"Number Of vertice {Vertices.Length}\r\n");
            Logger.Log($"Number Of indices {Indices.Length}\r\n");

            for (int i = 0; i < Vertices.Length; i++)
            {
                Logger.Log($"Vertex {i}, {Vertices[i].X:F3},{Vertices[i].Y:F3},{Vertices[i].Z:F3}\r\n");
            }
            int face = 0;
            for (int i = 0; i < Indices.Length; i += 3)
            {
                Logger.Log($"Face {face}\r\n");
                Logger.Log($" {Vertices[Indices[i]].X},{Vertices[Indices[i]].Y},{Vertices[Indices[i]].Z}\r\n");
                Logger.Log($" {Vertices[Indices[i + 1]].X},{Vertices[Indices[i + 1]].Y},{Vertices[Indices[i + 1]].Z}\r\n");
                Logger.Log($" {Vertices[Indices[i + 2]].X},{Vertices[Indices[i + 2]].Y},{Vertices[Indices[i + 2]].Z}\r\n\r\n");
                face++;
            }
        }

        /**
        * Construct a solid based on data arrays. An exception may occur in the case of
        * abnormal arrays (indices making references to inexistent vertices, there are less
        * colors than vertices...)
        *
        * @param vertices array of points defining the solid vertices
        * @param indices array of indices for a array of vertices
        * @param colors array of colors defining the vertices colors
        */
        /** Sets the initial features common to all constructors */

        public int[] GetIndices()
        {
            int[] newIndices = new int[Indices.Length];
            Array.Copy(Indices, 0, newIndices, 0, Indices.Length);
            return newIndices;
        }

        public Vector3D[] GetVertices()
        {
            Vector3D[] newVertices = new Vector3D[Vertices.Length];
            for (int i = 0; i < newVertices.Length; i++)
            {
                newVertices[i] = Vertices[i];
            }
            return newVertices;
        }

        public void SetData(Vector3D[] vertices, int[] indices)
        {
            Vertices = new Vector3D[vertices.Length];
            Indices = new int[indices.Length];
            if (indices.Length != 0)
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    Vertices[i] = vertices[i];
                }
                Array.Copy(indices, 0, Indices, 0, indices.Length);
            }
        }

        protected Vector3D GetMean()
        {
            Vector3D mean = new Vector3D();
            for (int i = 0; i < Vertices.Length; i++)
            {
                mean.X += Vertices[i].X;
                mean.Y += Vertices[i].Y;
                mean.Z += Vertices[i].Z;
            }
            mean.X /= Vertices.Length;
            mean.Y /= Vertices.Length;
            mean.Z /= Vertices.Length;

            return mean;
        }

        protected void SetInitialFeatures()
        {
            Vertices = new Vector3D[0];
            Indices = new int[0];
        }

        //---------------------------------------GETS-----------------------------------//

        /**
        * Gets the solid vertices
        *
        * @return solid vertices
        */
        /** Gets the solid indices for its vertices
        *
        * @return solid indices for its vertices
        */
        /**
        * Gets if the solid is empty (without any vertex)
        *
        * @return true if the solid is empty, false otherwise
        */
        //---------------------------------------SETS-----------------------------------//

        /**
        * Sets the solid data. Each vertex may have a different color. An exception may
        * occur in the case of abnormal arrays (e.g., indices making references to
        * inexistent vertices, there are less colors than vertices...)
        *
        * @param vertices array of points defining the solid vertices
        * @param indices array of indices for a array of vertices
        * @param colors array of colors defining the vertices colors
        */
        //-------------------------GEOMETRICAL_TRANSFORMATIONS-------------------------//

        /**
        * Applies a rotation into a solid
        *
        * @param dx rotation on the x axis
        * @param dy rotation on the y axis
        */

        //-----------------------------------PRIVATES--------------------------------//

        /**
        * Gets the solid mean
        *
        * @return point representing the mean
        */
    }
}