/**************************************************************************
*   Copyright (c) 2024 Joe Bustard <barnacle3d@gmailcom>                  *
*                                                                         *
*   This file is part of the Barnacle 3D application.                     *
*                                                                         *
*   This application is free software; you can redistribute it and/or     *
*   modify it under the terms of the GNU Library General Public           *
*   License as published by the Free Software Foundation; either          *
*   version 2 of the License, or (at your option) any later version.      *
*                                                                         *
*   This application is distributed in the hope that it will be useful,   *
*   but WITHOUT ANY WARRANTY; without even the implied warranty of        *
*   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
*   GNU Library General Public License for more details.                  *
*                                                                         *
**************************************************************************/

using System;

namespace asdflibrary
{
    internal class FieldNode
    {
        // indices of field points in the corners
        public int[] Corners;

        public FieldNode[] SubNodes;
        private int centre;

        public FieldNode()
        {
            Corners = new int[8];
            SubNodes = new FieldNode[8];
            for (int i = 0; i < 8; i++)
            {
                Corners[i] = -1;
                SubNodes[i] = null;
            }
        }

        public void SetCorner(int index, int fp)
        {
            if (index >= 0 && index < 8)
            {
                Corners[index] = fp;
            }
        }

        public int GetCorner(int index)
        {
            if (index >= 0 && index < 8)
            {
                return Corners[index];
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
                v = Corners[c];
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

                // if it doesn't belong to the current node maybe its in one of the subnodes

                int subnode = -1;
                // is it above or below the centre
                if (y < AdaptiveSignedDistanceField.allFieldPoints[centre].y)
                {
                    // below is it front or behind the centre
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
                            subnode = 4;
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
                            subnode = 5;
                        }
                    }
                }
                else
                {
                    // above is it front or behind the centre
                    if (z < AdaptiveSignedDistanceField.allFieldPoints[centre].z)
                    {
                        if (x < AdaptiveSignedDistanceField.allFieldPoints[centre].x)
                        {
                            // left
                            subnode = 2;
                        }
                        else
                        {
                            // right
                            subnode = 6;
                        }
                    }
                    else
                    {
                        // front
                        if (x < AdaptiveSignedDistanceField.allFieldPoints[centre].x)
                        {
                            // left
                            subnode = 3;
                        }
                        else
                        {
                            // right
                            subnode = 7;
                        }
                    }
                }
                if (SubNodes[subnode] != null)
                {
                    result = SubNodes[subnode].FindFieldPoint(x, y, z);
                }
            }
            return result;
        }

        private const int MAX_DEPTH = 10;

        public void Subdivide(int depth = 0)
        {
            // cant subdivide if its already subdivided
            for (int i = 0; i < 8; i++)
            {
                if (SubNodes[i] != null)
                {
                    return;
                }
            }

            int[,,] nps = new int[3, 3, 3];
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        nps[i, j, k] = -1;
                    }
                }
            }
            // record the existing lower corners
            nps[0, 0, 0] = Corners[0];
            nps[0, 0, 2] = Corners[1];
            nps[2, 0, 2] = Corners[2];
            nps[2, 0, 0] = Corners[3];

            // create the points bewteen them
            nps[0, 0, 1] = SplitMidpoint(Corners[0], Corners[1]);
            nps[1, 0, 2] = SplitMidpoint(Corners[1], Corners[2]);
            nps[2, 0, 1] = SplitMidpoint(Corners[2], Corners[3]);
            nps[1, 0, 0] = SplitMidpoint(Corners[3], Corners[0]);

            // and on the bottom in the middle
            nps[1, 0, 1] = SplitMidpoint(Corners[0], Corners[2]);

            // top ones
            nps[0, 2, 0] = Corners[4];
            nps[0, 2, 2] = Corners[5];
            nps[2, 2, 2] = Corners[6];
            nps[2, 2, 0] = Corners[7];

            nps[0, 2, 1] = SplitMidpoint(Corners[4], Corners[5]);
            nps[1, 2, 2] = SplitMidpoint(Corners[5], Corners[6]);
            nps[2, 2, 1] = SplitMidpoint(Corners[6], Corners[7]);
            nps[1, 2, 0] = SplitMidpoint(Corners[7], Corners[4]);

            nps[1, 2, 1] = SplitMidpoint(Corners[4], Corners[6]);

            // middle one
            nps[0, 1, 0] = SplitMidpoint(Corners[0], Corners[4]);
            nps[0, 1, 2] = SplitMidpoint(Corners[1], Corners[5]);
            nps[2, 1, 2] = SplitMidpoint(Corners[2], Corners[6]);
            nps[2, 1, 0] = SplitMidpoint(Corners[3], Corners[7]);

            nps[0, 1, 1] = SplitMidpoint(nps[0, 1, 0], nps[0, 1, 2]);
            nps[1, 1, 2] = SplitMidpoint(nps[0, 1, 2], nps[2, 1, 2]);
            nps[2, 1, 1] = SplitMidpoint(nps[2, 1, 2], nps[2, 1, 0]);
            nps[1, 1, 0] = SplitMidpoint(nps[2, 1, 0], nps[0, 1, 0]);

            nps[1, 1, 1] = centre;

            int sub = 0;
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        SubNodes[sub] = new FieldNode();
                        SubNodes[sub].SetCorners(nps[i, j, k], nps[i, j, k + 1], nps[i + 1, j, k + 1], nps[i + 1, j, k],
                                                nps[i, j + 1, k], nps[i, j + 1, k + 1], nps[i + 1, j + 1, k + 1], nps[i + 1, j + 1, k]);

                        sub++;
                    }
                }
            }
            if (depth < MAX_DEPTH)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (SubNodes[i] != null && !SubNodes[i].SignsSame())
                    {
                        // a node which isn't all in or all out
                        if (SubNodes[i].Differential() > 0.75)
                        {
                            SubNodes[i].Subdivide(depth + 1);
                        }
                    }
                }
            }
        }

        public float Differential()
        {
            float v = AdaptiveSignedDistanceField.allFieldPoints[Corners[0]].v;
            float v1 = AdaptiveSignedDistanceField.allFieldPoints[Corners[7]].v;
            return Math.Abs(v) + Math.Abs(v1);
        }

        public bool SignsSame()
        {
            bool res = true;
            int sg = Math.Sign(AdaptiveSignedDistanceField.allFieldPoints[centre].v);
            for (int i = 0; i < 8 && res; i++)
            {
                if (Math.Sign(AdaptiveSignedDistanceField.allFieldPoints[Corners[i]].v) != sg)
                {
                    res = false;
                }
            }
            return res;
        }

        private void SetCorners(int v0, int v1, int v2, int v3, int v4, int v5, int v6, int v7)
        {
            Corners[0] = v0;
            Corners[1] = v1;
            Corners[2] = v2;
            Corners[3] = v3;
            Corners[4] = v4;
            Corners[5] = v5;
            Corners[6] = v6;
            Corners[7] = v7;

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
            // the centre point should not exist already but a quick check wont hurt
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

            // the centre point should not exist already but a quick check wont hurt
            int ci = FindFieldPoint(cx, cy, cz);
            if (ci == -1)
            {
                ci = AdaptiveSignedDistanceField.AddFieldPoint(cx, cy, cz);
            }
            int p0 = l;

            int p1 = FindFieldPoint(lx, ly, lz + dz);
            if (p1 == -1)
            {
                p1 = AdaptiveSignedDistanceField.AddFieldPoint(lx, ly, lz + dz);
            }

            int p2 = FindFieldPoint(lx + dx, ly, lz + dz);
            if (p2 == -1)
            {
                p2 = AdaptiveSignedDistanceField.AddFieldPoint(lx + dx, ly, lz + dz);
            }

            int p3 = FindFieldPoint(lx + dx, ly, lz);
            if (p3 == -1)
            {
                p3 = AdaptiveSignedDistanceField.AddFieldPoint(lx + dx, ly, lz);
            }

            int p4 = FindFieldPoint(lx, ly + dy, lz);
            if (p4 == -1)
            {
                p4 = AdaptiveSignedDistanceField.AddFieldPoint(lx, ly + dy, lz);
            }

            int p5 = FindFieldPoint(lx, ly + dy, lz + dz);
            if (p5 == -1)
            {
                p5 = AdaptiveSignedDistanceField.AddFieldPoint(lx, ly + dy, lz + dz);
            }

            int p6 = h;
            int p7 = FindFieldPoint(lx + dx, ly + dy, lz);
            if (p7 == -1)
            {
                p7 = AdaptiveSignedDistanceField.AddFieldPoint(lx + dx, ly + dy, lz);
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

        public void Dump(int level = 0)
        {
            string insert = "";
            for (int ind = 0; ind < level; ind++)
            {
                insert += "    ";
            }
            for (int c = 0; c < 8; c++)
            {
                System.Diagnostics.Debug.Write($"{insert} c {c} ({Corners[c]}) ");
                AdaptiveSignedDistanceField.allFieldPoints[Corners[c]].Dump(insert);
            }
            for (int i = 0; i < SubNodes.GetLength(0); i++)
            {
                if (SubNodes[i] != null)
                {
                    System.Diagnostics.Debug.WriteLine($"{insert} subnode {i}");
                    SubNodes[i].Dump(level + 1);
                }
            }
        }
    }
}