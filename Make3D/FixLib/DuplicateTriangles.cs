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
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace FixLib
{
    public class DuplicateTriangles
    {
        public static int RemoveDuplicateTriangles(int totalPoints, Int32Collection tris)
        {
            int maxSum = 3 * totalPoints;
            int numBuckets = 200;
            int bucketSize = (maxSum / (numBuckets - 1)) + 1;
            int duplicatesFound = 0;
            // initialise the storeage buckets
            List<faceRecord>[] buckets = new List<faceRecord>[numBuckets];
            for (int i = 0; i < numBuckets; i++)
            {
                buckets[i] = new List<faceRecord>();
            }

            // go through each original triangle
            for (int t = 0; t < tris.Count / 3; t++)
            {
                int findex = t * 3;
                if (findex + 2 < tris.Count)
                {
                    int sum = tris[findex] + tris[findex + 1] + tris[findex + 2];
                    int b = sum / bucketSize;
                    System.Diagnostics.Debug.WriteLine($"tri {t} =  {tris[findex]}, {tris[findex + 1]}, {tris[findex + 2]} sum {sum} bucket {b}");
                    // which bucket would it be in

                    if (b < 0 || b >= buckets.GetLength(0))
                    {
                        bool bBuggered = true;
                    }
                    else
                    {
                        if (buckets[b].Count == 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"first in bucket");
                            faceRecord fr = new faceRecord();
                            fr.Index = t;
                            fr.Sum = sum;
                            fr.V0 = tris[findex];
                            fr.V1 = tris[findex + 1];
                            fr.V2 = tris[findex + 2];
                            buckets[b].Add(fr);
                        }
                        else
                        {
                            bool found = false;
                            for (int j = 0; j < buckets[b].Count && found == false; j++)
                            {
                                // look for a rough match
                                if (buckets[b][j].Sum == sum)
                                {
                                    int matches = 0;
                                    if ((tris[findex] == buckets[b][j].V0) ||
                                        (tris[findex] == buckets[b][j].V1) ||
                                        (tris[findex] == buckets[b][j].V2))
                                    {
                                        matches++;
                                    }

                                    if ((tris[findex + 1] == buckets[b][j].V0) ||
                                        (tris[findex + 1] == buckets[b][j].V1) ||
                                        (tris[findex + 1] == buckets[b][j].V2))
                                    {
                                        matches++;
                                    }

                                    if ((tris[findex + 2] == buckets[b][j].V0) ||
                                        (tris[findex + 2] == buckets[b][j].V1) ||
                                        (tris[findex + 2] == buckets[b][j].V2))
                                    {
                                        matches++;
                                    }
                                    if (matches == 3)
                                    {
                                        // this is a duplicate
                                        found = true;
                                        duplicatesFound++;
                                    }
                                }
                            }
                            if (!found)
                            {
                                faceRecord fr = new faceRecord();
                                fr.Index = t;
                                fr.Sum = sum;
                                fr.V0 = tris[findex];
                                fr.V1 = tris[findex + 1];
                                fr.V2 = tris[findex + 2];
                                buckets[b].Add(fr);
                            }
                        }
                    }
                }
            }
            tris.Clear();
            for (int buck = 0; buck < buckets.Length; buck++)
            {
                for (int fi = 0; fi < buckets[buck].Count; fi++)
                {
                    tris.Add(buckets[buck][fi].V0);
                    tris.Add(buckets[buck][fi].V1);
                    tris.Add(buckets[buck][fi].V2);
                }
            }
            return duplicatesFound;
        }

        private struct faceRecord
        {
            public int Index;
            public int Sum;
            public int V0;
            public int V1;
            public int V2;
        }
    }
}