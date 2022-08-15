using System;
using System.Collections.Generic;
using System.Text;

namespace asdflib
{
    class FieldNode
    {
        // indices of field points in the corners
        int[] corners;
        FieldNode[] subNodes;
        int centre;
        public FieldNode()
        {
            corners = new int[8];
            subNodes = new FieldNode[8];
            for (int i = 0; i < 8; i++)
            {
                corners[i] = -1;
                subNodes[i] = null;
            }
        }

        public void SetCorner(int index, int fp)
        {
            if (index >= 0 && index < 8)
            {
                corners[index] = fp;
            }
        }

        public int GetCorner(int index)
        {
            if (index >= 0 && index < 8)
            {
                return corners[index];
            }
            return -1;
        }

        internal void SetCentre(int v)
        {
            centre = v;
        }

        public int FindFieldPoint(float x, float y, float z)
        {
            int result = -1;
            int c = 0;
            int v;
            while (c < 8 && result == -1)
            {
                v = corners[c];
                if (v != -1)
                {
                    if (AdaptiveSignedDistanceField.allFieldPoints[v].At(x, y, z))
                    {
                        result = v;
                    }
                }
                c++;
            }

            if (result == -1)
            {
                // is it the centre point

                if (AdaptiveSignedDistanceField.allFieldPoints[centre].At(x, y, z))
                {
                    result = centre;
                }


                // if it doesn't belong to the current node maybe its in  one of the subnodes

                int subnode = -1;
                // is it above or below the centre
                if (y < AdaptiveSignedDistanceField.allFieldPoints[centre].y)
                {
                    // below
                    // is it front or behind the centre
                    if (z < AdaptiveSignedDistanceField.allFieldPoints[centre].z)
                    {

                        if (x < AdaptiveSignedDistanceField.allFieldPoints[centre].x)
                        {
                            // left
                            subnode = 0;
                        }
                        else
                        {
                            // right
                            subnode = 3;
                        }
                    }
                    else
                    {
                        // front
                        if (x < AdaptiveSignedDistanceField.allFieldPoints[centre].x)
                        {
                            // left
                            subnode = 1;
                        }
                        else
                        {
                            // right
                            subnode = 2;
                        }

                    }
                }
                else
                {
                    // above
                    // is it front or behind the centre
                    if (z < AdaptiveSignedDistanceField.allFieldPoints[centre].z)
                    {

                        if (x < AdaptiveSignedDistanceField.allFieldPoints[centre].x)
                        {
                            // left
                            subnode = 4;
                        }
                        else
                        {
                            // right
                            subnode = 7;
                        }
                    }
                    else
                    {
                        // front
                        if (x < AdaptiveSignedDistanceField.allFieldPoints[centre].x)
                        {
                            // left
                            subnode = 5;
                        }
                        else
                        {
                            // right
                            subnode = 6;
                        }

                    }
                }
                if (subNodes[subnode] != null)
                {
                    result = subNodes[subnode].FindFieldPoint(x, y, z);
                }
            }
            return result;
        }

        public void Subdivide()
        {

            // cant subdivide if its already subdivided
            for (int i = 0; i < 8; i++)
            {

                if (subNodes[i] != null)
                {
                    return;
                }
            }

            int[,,] nps = new int[3, 3, 3];

            // record the existing lower corners
            nps[0, 0, 0] = corners[0];
            nps[0, 0, 2] = corners[1];
            nps[2, 0, 2] = corners[2];
            nps[2, 0, 0] = corners[3];

            // create the points bewteen them
            nps[0,0,1] = SplitMidpoint(corners[0], corners[1]);
            nps[1,0,2] = SplitMidpoint(corners[1], corners[2]);
            nps[2,0,1] = SplitMidpoint(corners[2], corners[3]);
            nps[1,0,0] = SplitMidpoint(corners[3], corners[0]);
            
            // and on the bottom in the middle
            nps[1,0,1] = SplitMidpoint(corners[0], corners[2]);


            // top ones
            nps[0, 2, 0] = corners[4];
            nps[0, 2, 2] = corners[5];
            nps[2, 2, 2] = corners[6];
            nps[2, 2, 0] = corners[7];

            nps[0,2,1] = SplitMidpoint(corners[4], corners[5]);
            nps[1,2,2] = SplitMidpoint(corners[5], corners[6]);
            nps[2,2,1] = SplitMidpoint(corners[6], corners[7]);
            nps[1,2,0] = SplitMidpoint(corners[7], corners[4]);

            nps[1,2,1] = SplitMidpoint(corners[4], corners[6]);


            // middle one
            nps[0, 1, 0] = SplitMidpoint(corners[0], corners[4]);
            nps[0, 1, 2] = SplitMidpoint(corners[1], corners[5]);
            nps[2, 1, 2] = SplitMidpoint(corners[2], corners[6]);
            nps[2, 1, 0] = SplitMidpoint(corners[3], corners[7]);

            nps[1, 1, 1] = centre;

            int sub = 0;
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        subNodes[sub] = new FieldNode();
                        subNodes[sub].SetCorners(nps[i, j, k], nps[i, j, k+1], nps[i+1, j, k+1], nps[i+1, j, k],
                                                nps[i, j + 1, k], nps[i, j + 1, k + 1], nps[i+1, j+1, k + 1], nps[i+1, j+1, k]);
                    }
                }
            }
        }

        private void SetCorners(int v0, int v1, int v2, int v3, int v4, int v5, int v6, int v7)
        {
            corners[0] = v0;
            corners[1] = v1;
            corners[2] = v2;
            corners[3] = v3;
            corners[4] = v4;
            corners[5] = v5;
            corners[6] = v6;
            corners[7] = v7;

            centre = SplitMidpoint(v0, v6);
        }

        private int SplitMidpoint(int l, int h)
        {
            float lx = AdaptiveSignedDistanceField.allFieldPoints[l].x;
            float ly = AdaptiveSignedDistanceField.allFieldPoints[l].y;
            float lz = AdaptiveSignedDistanceField.allFieldPoints[l].z;

            float hx = AdaptiveSignedDistanceField.allFieldPoints[h].x;
            float hy = AdaptiveSignedDistanceField.allFieldPoints[h].y;
            float hz = AdaptiveSignedDistanceField.allFieldPoints[h].z;

            float cx = lx + (hx - lx) / 2.0F;
            float cy = ly + (hy - ly) / 2.0F;
            float cz = lz + (hz - lz) / 2.0F;
            // the centre point should not exist already
            // but a quick check wont hurt
            int ci = FindFieldPoint(cx, cy, cz);
            if (ci == -1)
            {
                ci = AdaptiveSignedDistanceField.AddFieldPoint(cx, cy, cz);
            }
            return ci;

        }

        private FieldNode CreateSubNode(int l, int h)
        {
            FieldNode nd = new FieldNode();
            float lx = AdaptiveSignedDistanceField.allFieldPoints[l].x;
            float ly = AdaptiveSignedDistanceField.allFieldPoints[l].y;
            float lz = AdaptiveSignedDistanceField.allFieldPoints[l].z;

            float hx = AdaptiveSignedDistanceField.allFieldPoints[h].x;
            float hy = AdaptiveSignedDistanceField.allFieldPoints[h].y;
            float hz = AdaptiveSignedDistanceField.allFieldPoints[h].z;

            float dx = hx - lx;
            float dy = hy - ly;
            float dz = hz - lz;

            float cx = lx + (hx - lx) / 2.0F;
            float cy = ly + (hy - ly) / 2.0F;
            float cz = lz + (hz - lz) / 2.0F;

            // the centre point should not exist already
            // but a quick check wont hurt
            int ci = FindFieldPoint(cx, cy, cz);
            if(ci==-1)
            {
                ci = AdaptiveSignedDistanceField.AddFieldPoint(cx, cy, cz);
            }
            int p0 = l;

            int p1 = FindFieldPoint(lx, ly, lz + dz);
            if (p1 == -1)
            {
                p1 = AdaptiveSignedDistanceField.AddFieldPoint(lx, ly, lz + dz);
            }

            int p2 = FindFieldPoint(lx+dx, ly, lz + dz);
            if (p2 == -1)
            {
                p2 = AdaptiveSignedDistanceField.AddFieldPoint(lx+dx, ly, lz + dz);
            }

            int p3 = FindFieldPoint(lx + dx, ly, lz);
            if (p3 == -1)
            {
                p3 = AdaptiveSignedDistanceField.AddFieldPoint(lx + dx, ly, lz);
            }

            int p4 = FindFieldPoint(lx, ly+dy, lz);
            if (p4 == -1)
            {
                p4 = AdaptiveSignedDistanceField.AddFieldPoint(lx, ly+dy, lz);
            }

            int p5 = FindFieldPoint(lx, ly+dy, lz+dz);
            if (p5 == -1)
            {
                p5 = AdaptiveSignedDistanceField.AddFieldPoint(lx, ly+dy, lz+dz);
            }

            int p6 = h;
            int p7 = FindFieldPoint(lx+dx, ly + dy, lz);
            if (p7 == -1)
            {
                p7 = AdaptiveSignedDistanceField.AddFieldPoint(lx+dx, ly + dy, lz );
            }
            nd.SetCorner(0, p0);
            nd.SetCorner(1, p1);
            nd.SetCorner(2, p2);
            nd.SetCorner(3, p3);
            nd.SetCorner(4, p4);
            nd.SetCorner(5, p5);
            nd.SetCorner(6, p6);
            nd.SetCorner(7, p7);

            nd.SetCentre(ci);
            return nd;
        }
    }
}
