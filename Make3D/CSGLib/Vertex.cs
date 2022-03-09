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
using System.Windows.Media.Media3D;

//using OpenToolkit.Mathematics;

namespace CSGLib
{
    /// <summary>
    /// Represents of a 3d face vertex.
    /// </summary>
    public class Vertex
    {
        public Vector3D _Position;
        /** references to vertices conected to it by an edge  */
        private List<Vertex> AdjacentVertices;
        /** vertex status relative to other object */
        private Status _Status;

        /** tolerance value to test equalities */
        private static readonly double EqualityTolerance = 1e-8f;
        //private static readonly double EqualityTolerance = 1e-8f;
        //----------------------------------CONSTRUCTORS--------------------------------//

        /**
     * Constructs a vertex with unknown status
     *
     * @param position vertex position
     */

        public Vertex(Vector3D position)
        {
            _Position = position;

            AdjacentVertices = new List<Vertex>();
            _Status = Status.UNKNOWN;
        }

        /**
        * Constructs a vertex with unknown status
        *
        * @param x coordinate on the x axis
        * @param y coordinate on the y axis
        * @param z coordinate on the z axis
        */

        public Vertex(double x, double y, double z)
        {
            _Position.X = x;
            _Position.Y = y;
            _Position.Z = z;

            AdjacentVertices = new List<Vertex>();
            _Status = Status.UNKNOWN;
        }

        /// <summary>
        /// Constructs a vertex with definite status
        /// </summary>
        /// <param name="position">vertex position</param>
        /// <param name="status">vertex status - UNKNOWN, BOUNDARY, INSIDE or OUTSIDE</param>
        public Vertex(Vector3D position, Status status)
        {
            _Position.X = position.X;
            _Position.Y = position.Y;
            _Position.Z = position.Z;

            AdjacentVertices = new List<Vertex>();
            _Status = status;
        }

        /// <summary>
        /// Constructs a vertex with a definite status
        /// </summary>
        /// <param name="x">coordinate on the x axis</param>
        /// <param name="y">coordinate on the y axis</param>
        /// <param name="z">coordinate on the z axis</param>
        /// <param name="status">vertex status - UNKNOWN, BOUNDARY, INSIDE or OUTSIDE</param>
        public Vertex(double x, double y, double z, Status status)
        {
            _Position = new Vector3D(x, y, z);

            AdjacentVertices = new List<Vertex>();
            _Status = status;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        private Vertex()
        {
        }

        /// <summary>
        /// Clones the vertex object
        /// </summary>
        /// <returns>cloned vertex object</returns>
        public Vertex Clone()
        {
            Vertex clone = new Vertex();
            clone._Position = _Position;
            clone._Status = _Status;
            clone.AdjacentVertices = new List<Vertex>();
            for (int i = 0; i < AdjacentVertices.Count; i++)
            {
                clone.AdjacentVertices.Add(AdjacentVertices[i].Clone());
            }

            return clone;
        }

        /**
        * Makes a string definition for the Vertex object
        *
        * @return the string definition
        */

        public override string ToString()
        {
            return $"({_Position.X:F3},{_Position.Y:F3}, {_Position.Z:F3})";
        }

        /**
        * Checks if an vertex is equal to another. To be equal, they have to have the same
        * coordinates(with some tolerance)
        *
        * @param anObject the other vertex to be tested
        * @return true if they are equal, false otherwise.
        */

        public bool Equals(Vertex vertex)
        {
            bool eq = false;
            if ((Math.Abs(_Position.X - vertex._Position.X) <= EqualityTolerance) &&
                (Math.Abs(_Position.Y - vertex._Position.Y) <= EqualityTolerance) &&
                (Math.Abs(_Position.Z - vertex._Position.Z) <= EqualityTolerance))
            {
                eq = true;
            }

            return eq;
        }

        //--------------------------------------SETS------------------------------------//

        /**
        * Sets the vertex status
        *
        * @param status vertex status - UNKNOWN, BOUNDARY, INSIDE or OUTSIDE
        */

        public void SetStatus(Status status)
        {
            if (status >= Status.UNKNOWN && status <= Status.BOUNDARY)
            {
                _Status = status;
            }
        }

        //--------------------------------------GETS------------------------------------//

        /**
        * Gets the vertex position
        *
        * @return vertex position
        */
        public Vector3D Position => _Position;

        /**
        * Gets an array with the adjacent vertices
        *
        * @return array of the adjacent vertices
        */

        public List<Vertex> GetAdjacentVertices(bool unknownOnly = true)
        {
            List<Vertex> vertices = new List<Vertex>();
            for (int i = 0; i < AdjacentVertices.Count; i++)
            {
                if (unknownOnly == true)
                {
                    if (AdjacentVertices[i].Status == Status.UNKNOWN)
                    {
                        vertices.Add(AdjacentVertices[i]);
                    }
                }
                else
                {
                    vertices.Add(AdjacentVertices[i]);
                }
            }
            return vertices;
        }

        /**
        * Gets the vertex status
        *
        * @return vertex status - UNKNOWN, BOUNDARY, INSIDE or OUTSIDE
        */

        public Status Status
        {
            get { return _Status; }
            set { _Status = value; }
        }

        //----------------------------------OTHERS--------------------------------------//

        /**
        * Sets a vertex as being adjacent to it
        *
        * @param adjacentVertex an adjacent vertex
        */

        public void AddAdjacentVertex(Vertex adjacentVertex)
        {
            if (!AdjacentVertices.Contains(adjacentVertex))
            {
                AdjacentVertices.Add(adjacentVertex);
            }
        }

        /**
        * Sets the vertex status, setting equally the adjacent ones
        *
        * @param status new status to be set
        */
        const int depthLimit =10;
        public void Mark(Status status, int depth = 0)
        {
            //mark vertex
            _Status = status;

            //mark adjacent vertices
            List<Vertex> adjacentVerts = GetAdjacentVertices();
            foreach (Vertex v in adjacentVerts)
            {
                v.Status = status;
            }
            if (depth < depthLimit)
            {
                foreach (Vertex v in adjacentVerts)
                {
                    v.Mark(status, depth + 1);
                }
            }
        }
    }
}