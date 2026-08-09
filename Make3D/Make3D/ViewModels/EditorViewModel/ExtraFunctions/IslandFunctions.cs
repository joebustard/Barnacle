// **************************************************************************
// *   Copyright (c) 2024 Joe Bustard <barnacle3d@gmailcom>                  *
// *                                                                         *
// *   This file is part of the Barnacle 3D application.                     *
// *                                                                         *
// *   This application is free software. You can redistribute it and/or     *
// *   modify it under the terms of the GNU Library General Public           *
// *   License as published by the Free Software Foundation. Either          *
// *   version 2 of the License, or (at your option) any later version.      *
// *                                                                         *
// *   This application is distributed in the hope that it will be useful,   *
// *   but WITHOUT ANY WARRANTY. Without even the implied warranty of        *
// *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
// *   GNU Library General Public License for more details.                  *
// *                                                                         *
// *************************************************************************

using Barnacle.Models;
using Barnacle.Object3DLib;
using FileUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Barnacle.ViewModels
{
    internal partial class EditorViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private void MeshIslands(Object3D srcObj)
        {
            bool split = false;
            CheckPoint();
            // put the object into a half edge structure
            Point3DCollection vertices = srcObj.AbsoluteObjectVertices;

            HalfEdgeLib.Mesh hemesh = new HalfEdgeLib.Mesh(vertices, srcObj.TriangleIndices);

            // tag all the faces as not processed yet
            for (int i = 0; i < hemesh.Faces.Count; i++)
            {
                hemesh.Faces[i].Tag = false;
            }

            // make a face id queue
            List<int> faceQueue = new List<int>();

            // select a random face, may as well be the first unprocessed one.
            bool done = false;
            int islandCount = 0;
            while (!done)
            {
                done = true;
                for (int faceId = 0; faceId < hemesh.Faces.Count; faceId++)
                {
                    if (!(bool)hemesh.Faces[faceId].Tag)
                    {
                        faceQueue.Add(faceId);
                        done = false;
                        break;
                    }
                }
                if (!done)
                {
                    islandCount++;
                    Object3D island = new Object3D();
                    island.Name = srcObj.Name + "_" + islandCount.ToString();
                    island.PrimType = "Mesh";
                    while (faceQueue.Count > 0)
                    {
                        // pull the next face
                        int faceId = faceQueue[0];
                        faceQueue.RemoveAt(0);

                        // mark the face as processed
                        hemesh.Faces[faceId].Tag = true;

                        // get the edges
                        int he1 = hemesh.Faces[faceId].FirstEdge;
                        int he2 = hemesh.HalfEdges[he1].Next;
                        int he3 = hemesh.HalfEdges[he2].Next;

                        // get the vertices for each edge
                        HalfEdgeLib.Vertex sv1 = hemesh.Vertices[hemesh.HalfEdges[he1].StartVertex];
                        HalfEdgeLib.Vertex sv2 = hemesh.Vertices[hemesh.HalfEdges[he2].StartVertex];
                        HalfEdgeLib.Vertex sv3 = hemesh.Vertices[hemesh.HalfEdges[he3].StartVertex];

                        // add to the new object
                        // dont worry about duplicates at the moment
                        island.AbsoluteObjectVertices.Add(new Point3D(sv1.X, sv1.Y, sv1.Z));
                        island.TriangleIndices.Add(island.AbsoluteObjectVertices.Count - 1);

                        island.AbsoluteObjectVertices.Add(new Point3D(sv2.X, sv2.Y, sv2.Z));
                        island.TriangleIndices.Add(island.AbsoluteObjectVertices.Count - 1);

                        island.AbsoluteObjectVertices.Add(new Point3D(sv3.X, sv3.Y, sv3.Z));
                        island.TriangleIndices.Add(island.AbsoluteObjectVertices.Count - 1);

                        // now look at the faces next to this one
                        int twin1 = hemesh.HalfEdges[he1].Twin;
                        int twin2 = hemesh.HalfEdges[he2].Twin;
                        int twin3 = hemesh.HalfEdges[he3].Twin;

                        int twinface1 = hemesh.HalfEdges[twin1].Face;
                        int twinface2 = hemesh.HalfEdges[twin2].Face;
                        int twinface3 = hemesh.HalfEdges[twin3].Face;

                        if (twinface1 >= 0 && !((bool)hemesh.Faces[twinface1].Tag))
                        {
                            if (!faceQueue.Contains(twinface1))
                            {
                                faceQueue.Add(twinface1);
                            }
                        }

                        if (twinface2 >= 0 && !((bool)hemesh.Faces[twinface2].Tag))
                        {
                            if (!faceQueue.Contains(twinface2))
                            {
                                faceQueue.Add(twinface2);
                            }
                        }

                        if (twinface3 >= 0 && !((bool)hemesh.Faces[twinface3].Tag))
                        {
                            if (!faceQueue.Contains(twinface3))
                            {
                                faceQueue.Add(twinface3);
                            }
                        }
                    }
                    island.CalculateAbsoluteBounds();
                    island.Position = new Point3D(island.AbsoluteBounds.MidPoint().X,
                                                  island.AbsoluteBounds.MidPoint().Y,
                                                  island.AbsoluteBounds.MidPoint().Z);
                    island.AbsoluteToRelative();
                    island.Remesh();
                    island.Color = Document.ProjectSettings.DefaultObjectColour;
                    Document.Content.Add(island);
                    split = true;
                }
            }
            if (split)
            {
                Document.Content.Remove(srcObj);
            }
        }
    }
}