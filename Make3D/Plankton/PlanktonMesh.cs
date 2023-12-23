using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Plankton
{
    /// <summary>
    /// This is the main class that describes a plankton mesh.
    /// </summary>
    public class PlanktonMesh
    {
        private PlanktonVertexList _vertices;
        private PlanktonHalfEdgeList _halfedges;
        private PlanktonFaceList _faces;

        /// <summary>
        /// Initializes a new (empty) instance of the <see cref="PlanktonMesh"/> class.
        /// </summary>
        public PlanktonMesh()
        {
        }

        /// <summary>
        /// Initializes a new (duplicate) instance of the <see cref="PlanktonMesh"/> class.
        /// </summary>
        public PlanktonMesh(PlanktonMesh source)
        {
            foreach (var v in source.Vertices)
            {
                this.Vertices.Add(new PlanktonVertex()
                {
                    OutgoingHalfedge = v.OutgoingHalfedge,
                    X = v.X,
                    Y = v.Y,
                    Z = v.Z
                });
            }
            foreach (var f in source.Faces)
            {
                this.Faces.Add(new PlanktonFace() { FirstHalfedge = f.FirstHalfedge });
            }
            foreach (var h in source.Halfedges)
            {
                this.Halfedges.Add(new PlanktonHalfedge()
                {
                    StartVertex = h.StartVertex,
                    Twin = h.Twin,
                    NextHalfedge = h.NextHalfedge,
                    PrevHalfedge = h.PrevHalfedge,
                });
            }
        }

        public PlanktonMesh(Point3DCollection verts, Int32Collection sourceFaces)
        {
            // create the right number of vertices and set their coordinates
            // We don't know their half edges yet
            foreach (var v in verts)
            {
                this.Vertices.Add(new PlanktonVertex()
                {
                    OutgoingHalfedge = -1,
                    X = (float)v.X,
                    Y = (float)v.Y,
                    Z = (float)v.Z
                });
            }
            // create the right number of faces.
            // We don't know anything else about them yet
            for (int i = 0; i < sourceFaces.Count / 3; i++)
            {
                int j = i * 3;
                PlanktonHalfedge he0 = new PlanktonHalfedge();
                he0.StartVertex = sourceFaces[j];
                he0.Face = this.Faces.Count;
                int heInd0 = Halfedges.Count;
                this.Halfedges.Add(he0);
                this.Vertices[he0.StartVertex].OutgoingHalfedge = heInd0;

                PlanktonHalfedge he1 = new PlanktonHalfedge();
                he1.StartVertex = sourceFaces[j + 1];
                he1.Face = this.Faces.Count;
                int heInd1 = Halfedges.Count;
                this.Halfedges.Add(he1);
                this.Vertices[he1.StartVertex].OutgoingHalfedge = heInd1;

                PlanktonHalfedge he2 = new PlanktonHalfedge();
                he2.StartVertex = sourceFaces[j + 2];
                he2.Face = this.Faces.Count;
                int heInd2 = Halfedges.Count;
                this.Halfedges.Add(he2);
                this.Vertices[he2.StartVertex].OutgoingHalfedge = heInd2;

                he0.NextHalfedge = heInd1;
                he1.NextHalfedge = heInd2;
                he2.NextHalfedge = heInd0;

                he0.PrevHalfedge = heInd2;
                he1.PrevHalfedge = heInd0;
                he2.PrevHalfedge = heInd1;

                PlanktonFace f = new PlanktonFace();
                f.FirstHalfedge = heInd0;

                this.Faces.Add(f);
            }

            // crude halfedge twinner
            for (int i = 0; i < this.Halfedges.Count - 1; i++)
            {
                if (this.Halfedges[i].Twin == -1)
                {
                    int s0 = this.Halfedges[i].StartVertex;
                    int e0 = this.Halfedges[this.Halfedges[i].NextHalfedge].StartVertex;
                    bool more = true;
                    for (int j = i + 1; j < this.Halfedges.Count && more; j++)
                    {
                        if (this.Halfedges[j].Twin == -1)
                        {
                            int s1 = this.Halfedges[j].StartVertex;
                            int e1 = this.Halfedges[this.Halfedges[j].NextHalfedge].StartVertex;
                            if (s0 == e1 && e0 == s1)
                            {
                                this.Halfedges[i].Twin = j;
                                this.Halfedges[j].Twin = i;
                                more = false;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets access to the <see cref="PlanktonVertexList"/> collection in this mesh.
        /// </summary>
        public PlanktonVertexList Vertices
        {
            get { return _vertices ?? (_vertices = new PlanktonVertexList(this)); }
        }

        /// <summary>
        /// Gets access to the <see cref="PlanktonHalfedgeList"/> collection in this mesh.
        /// </summary>
        public PlanktonHalfEdgeList Halfedges
        {
            get { return _halfedges ?? (_halfedges = new PlanktonHalfEdgeList(this)); }
        }

        /// <summary>
        /// Gets access to the <see cref="PlanktonFaceList"/> collection in this mesh.
        /// </summary>
        public PlanktonFaceList Faces
        {
            get { return _faces ?? (_faces = new PlanktonFaceList(this)); }
        }

        public PlanktonXYZ GetVertexNormal(int index)
        {
            return this.Vertices.GetNormal(index);
        }

        /// <summary>
        /// Calculate the volume of the mesh
        /// </summary>
        public double Volume()
        {
            double VolumeSum = 0;
            for (int i = 0; i < this.Faces.Count; i++)
            {
                int[] FaceVerts = this.Faces.GetFaceVertices(i);
                int EdgeCount = FaceVerts.Length;
                if (EdgeCount == 3)
                {
                    PlanktonXYZ P = this.Vertices[FaceVerts[0]].ToXYZ();
                    PlanktonXYZ Q = this.Vertices[FaceVerts[1]].ToXYZ();
                    PlanktonXYZ R = this.Vertices[FaceVerts[2]].ToXYZ();
                    //get the signed volume of the tetrahedron formed by the triangle and the origin
                    VolumeSum += (1 / 6d) * (
                           P.X * Q.Y * R.Z +
                           P.Y * Q.Z * R.X +
                           P.Z * Q.X * R.Y -
                           P.X * Q.Z * R.Y -
                           P.Y * Q.X * R.Z -
                           P.Z * Q.Y * R.X);
                }
                else
                {
                    PlanktonXYZ P = this._faces.GetFaceCenter(i);
                    for (int j = 0; j < EdgeCount; j++)
                    {
                        PlanktonXYZ Q = this.Vertices[FaceVerts[j]].ToXYZ();
                        PlanktonXYZ R = this.Vertices[FaceVerts[(j + 1) % EdgeCount]].ToXYZ();
                        VolumeSum += (1 / 6d) * (
                            P.X * Q.Y * R.Z +
                            P.Y * Q.Z * R.X +
                            P.Z * Q.X * R.Y -
                            P.X * Q.Z * R.Y -
                            P.Y * Q.X * R.Z -
                            P.Z * Q.Y * R.X);
                    }
                }
            }
            return VolumeSum;
        }

        public PlanktonMesh Dual()
        {
            // hack for open meshes
            // TODO: improve this ugly method
            if (this.IsClosed() == false)
            {
                var dual = new PlanktonMesh();

                // create vertices from face centers
                for (int i = 0; i < this.Faces.Count; i++)
                {
                    dual.Vertices.Add(this.Faces.GetFaceCenter(i));
                }

                // create faces from the adjacent face indices of non-boundary vertices
                for (int i = 0; i < this.Vertices.Count; i++)
                {
                    if (this.Vertices.IsBoundary(i))
                    {
                        continue;
                    }
                    dual.Faces.AddFace(this.Vertices.GetVertexFaces(i));
                }

                return dual;
            }

            // can later add options for other ways of defining face centres (barycenter/circumcenter etc)
            // won't work yet with naked boundaries

            PlanktonMesh P = this;
            PlanktonMesh D = new PlanktonMesh();

            //for every primal face, add the barycenter to the dual's vertex list
            //dual vertex outgoing HE is primal face's start HE
            //for every vertex of the primal, add a face to the dual
            //dual face's startHE is primal vertex's outgoing's pair

            for (int i = 0; i < P.Faces.Count; i++)
            {
                var fc = P.Faces.GetFaceCenter(i);
                D.Vertices.Add(new PlanktonVertex(fc.X, fc.Y, fc.Z));
                int[] FaceHalfedges = P.Faces.GetHalfedges(i);
                for (int j = 0; j < FaceHalfedges.Length; j++)
                {
                    if (P.Halfedges[P.Halfedges.GetPairHalfedge(FaceHalfedges[j])].Twin != -1)
                    {
                        // D.Vertices[i].OutgoingHalfedge = FaceHalfedges[j];
                        D.Vertices[D.Vertices.Count - 1].OutgoingHalfedge = P.Halfedges.GetPairHalfedge(FaceHalfedges[j]);
                        break;
                    }
                }
            }

            for (int i = 0; i < P.Vertices.Count; i++)
            {
                if (P.Vertices.NakedEdgeCount(i) == 0)
                {
                    int df = D.Faces.Add(PlanktonFace.Unset);
                    // D.Faces[i].FirstHalfedge = P.PairHalfedge(P.Vertices[i].OutgoingHalfedge);
                    D.Faces[df].FirstHalfedge = P.Vertices[i].OutgoingHalfedge;
                }
            }

            // dual halfedge start V is primal AdjacentFace
            // dual halfedge AdjacentFace is primal end V
            // dual nextHE is primal's pair's prev
            // dual prevHE is primal's next's pair

            // halfedge pairs stay the same

            for (int i = 0; i < P.Halfedges.Count; i++)
            {
                if ((P.Halfedges[i].Twin != -1) & (P.Halfedges[P.Halfedges.GetPairHalfedge(i)].Twin != -1))
                {
                    PlanktonHalfedge DualHE = PlanktonHalfedge.Unset;
                    PlanktonHalfedge PrimalHE = P.Halfedges[i];
                    //DualHE.StartVertex = PrimalHE.AdjacentFace;
                    DualHE.StartVertex = P.Halfedges[P.Halfedges.GetPairHalfedge(i)].Twin;

                    if (P.Vertices.NakedEdgeCount(PrimalHE.StartVertex) == 0)
                    {
                        //DualHE.AdjacentFace = P.Halfedges[P.PairHalfedge(i)].StartVertex;
                        DualHE.Twin = PrimalHE.StartVertex;
                    }
                    else { DualHE.Twin = -1; }

                    //This will currently fail with open meshes...
                    //one option could be to build the dual with all halfedges, but mark some as dead
                    //if they connect to vertex -1
                    //mark the 'external' faces all as -1 (the ones that are dual to boundary verts)
                    //then go through and if any next or prevs are dead hes then replace them with the next one around
                    //this needs to be done repeatedly until no further change

                    //DualHE.NextHalfedge = P.Halfedges[P.PairHalfedge(i)].PrevHalfedge;
                    DualHE.NextHalfedge = P.Halfedges.GetPairHalfedge(PrimalHE.PrevHalfedge);

                    //DualHE.PrevHalfedge = P.PairHalfedge(PrimalHE.NextHalfedge);
                    DualHE.PrevHalfedge = P.Halfedges[P.Halfedges.GetPairHalfedge(i)].NextHalfedge;

                    D.Halfedges.Add(DualHE);
                }
            }
            return D;
        }

        public bool IsClosed()
        {
            for (int i = 0; i < this.Halfedges.Count; i++)
            {
                if (this.Halfedges[i].Twin < 0)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Truncates the vertices of a mesh.
        /// </summary>
        /// <param name="t">Optional parameter for the normalised distance along each edge to control the amount of truncation.</param>
        /// <returns>A new mesh, the result of the truncation.</returns>
        public PlanktonMesh TruncateVertices(float t = 1f / 3)
        {
            // TODO: handle special cases (t = 0.0, t = 0.5, t > 0.5)
            var tMesh = new PlanktonMesh(this);

            var vxyz = tMesh.Vertices.Select(v => v.ToXYZ()).ToArray();
            PlanktonXYZ v0, v1, v2;
            int[] oh;
            for (int i = 0; i < this.Vertices.Count; i++)
            {
                oh = this.Vertices.GetHalfedges(i);
                tMesh.Vertices.TruncateVertex(i);
                foreach (var h in oh)
                {
                    v0 = vxyz[this.Halfedges[h].StartVertex];
                    v1 = vxyz[this.Halfedges.EndVertex(h)];
                    v2 = v0 + (v1 - v0) * t;
                    tMesh.Vertices.SetVertex(tMesh.Halfedges[h].StartVertex, v2.X, v2.Y, v2.Z);
                }
            }

            return tMesh;
        }

        /* Hide for the time being to avoid confusion...
        public void RefreshVertexNormals()
        {
        }
        public void RefreshFaceNormals()
        {
        }
        public void RefreshEdgeNormals()
        {
        }
        */

        /// <summary>
        /// Removes any unreferenced objects from arrays, reindexes as needed and shrinks arrays to minimum required size.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if halfedge count is odd after compaction.
        /// Most likely caused by only marking one of the halfedges in a pair for deletion.</exception>
        public void Compact()
        {
            // Compact vertices, faces and halfedges
            this.Vertices.CompactHelper();
            this.Faces.CompactHelper();
            this.Halfedges.CompactHelper();
        }

        public void ToSoup(Point3DCollection vertices, Int32Collection faces)
        {
            foreach (PlanktonVertex pv in this.Vertices)
            {
                vertices.Add(new Point3D((double)pv.X, (double)pv.Y, (double)pv.Z));
            }

            foreach (PlanktonFace pf in this.Faces)
            {
                int edge = pf.FirstHalfedge;
                int startEdge = edge;
                do
                {
                    faces.Add(this.Halfedges[edge].StartVertex);
                    edge = this.Halfedges[edge].NextHalfedge;
                } while (edge != startEdge);
            }
        }

        //dihedral angle for an edgepf in this.
        //

        //skeletonize - build a new mesh with 4 faces for each original edge

        public int TriangleSplit(int faceId)
        {
            return this.Halfedges.TriangleSplitEdge(this.Faces[faceId].FirstHalfedge);
        }
    }
}