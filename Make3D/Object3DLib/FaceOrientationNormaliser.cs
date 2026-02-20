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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Object3DLib
{
    internal class FaceOrientationNormaliser
    {
        public static int StandardiseFaceOrientations(int totalPoints, Int32Collection tris)
        {
            int maxSum = 3 * totalPoints;
            int numBuckets = 200;
            // int bucketSize = (maxSum / (numBuckets - 1)) + 1;
            int wrongOns = 0;
            // initialise the storeage buckets
            List<int>[] buckets = new List<int>[numBuckets];
            for (int i = 0; i < numBuckets; i++)
            {
                buckets[i] = new List<int>();
            }

            List<faceRecord> allrecords = new List<faceRecord>();
            // go through each original triangle
            for (int t = 0; t < tris.Count / 3; t++)
            {
                int findex = t * 3;
                if (findex + 2 < tris.Count)
                {
                    faceRecord fr = new faceRecord();
                    fr.Index = t;

                    fr.swapped = false;
                    fr.V0 = tris[findex];
                    fr.V1 = tris[findex + 1];
                    fr.V2 = tris[findex + 2];
                    fr.EdgeSum1 = fr.V0 + fr.V1;
                    fr.EdgeSum2 = fr.V1 + fr.V2;
                    fr.EdgeSum3 = fr.V2 + fr.V0;
                    fr.added = false;
                    allrecords.Add(fr);

                    int b1 = fr.EdgeSum1 % numBuckets;
                    buckets[b1].Add(t);
                    int b2 = fr.EdgeSum2 % numBuckets;
                    if (b2 != b1)
                    {
                        buckets[b2].Add(t);
                    }
                    int b3 = fr.EdgeSum3 % numBuckets;
                    if (b3 != b2 && b3 != b1)
                    {
                        buckets[b3].Add(t);
                    }
                }
            }

            for (int buck = 0; buck < buckets.Length; buck++)
            {
                for (int fi = 0; fi < buckets[buck].Count - 1; fi++)
                {
                    int ia = buckets[buck][fi];
                    faceRecord fa = allrecords[ia];
                    for (int fj = fi + 1; fj < buckets[buck].Count; fj++)
                    {
                        int ib = buckets[buck][fj];
                        faceRecord fb = allrecords[ib];
                        if (fa.EdgeSum1 == fb.EdgeSum1 || fa.EdgeSum2 == fb.EdgeSum2 || fa.EdgeSum3 == fb.EdgeSum3)
                        {
                            if ((fa.V0 == fb.V0 && fa.V1 == fb.V1) ||
                                 (fa.V1 == fb.V1 && fa.V2 == fb.V2) ||
                                 (fa.V2 == fb.V2 && fa.V0 == fb.V0))
                            {
                                if (!fb.swapped)
                                {
                                    int tmp = allrecords[ib].V2;
                                    allrecords[ib].V2 = allrecords[ib].V1;
                                    allrecords[ib].V1 = tmp;
                                    allrecords[ib].swapped = true;
                                    wrongOns++;
                                }
                            }
                        }
                    }
                }
            }

            tris.Clear();
            foreach (faceRecord face in allrecords)
            {
                tris.Add(face.V0);
                tris.Add(face.V1);
                tris.Add(face.V2);
            }

            return wrongOns;
        }

        private static void DumpTris(string v, Int32Collection tris)
        {
            System.Diagnostics.Debug.WriteLine(v);
            for (int i = 0; i < tris.Count; i += 3)
            {
                System.Diagnostics.Debug.WriteLine($"{tris[i]}, {tris[i + 1]}, {tris[i + 2]}");
            }
        }

        private class faceRecord
        {
            public bool added;
            public int EdgeSum1;
            public int EdgeSum2;
            public int EdgeSum3;
            public int Index;
            public bool swapped;
            public int V0;
            public int V1;
            public int V2;

            internal void SetAdded()
            {
                added = true;
            }
        }
    }
}