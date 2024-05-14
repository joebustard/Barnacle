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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barnacle.Dialogs
{
    public class EdgeProcessor
    {
        public List<EdgeRecord> EdgeRecords { get; set; }

        public EdgeProcessor()
        {
            EdgeRecords = new List<EdgeRecord>();
        }

        public void Add(int s, int e)
        {
            EdgeRecords.Add(new EdgeRecord(s, e));
        }

        internal List<EdgeRecord> MakeLoop()
        {
            List<EdgeRecord> loop = new List<EdgeRecord>();
            int startId;
            int endId;
            if (EdgeRecords != null && EdgeRecords.Count > 2)
            {
                loop.Add(EdgeRecords[0]);
                EdgeRecords.RemoveAt(0);
                startId = loop[0].Start;
                endId = loop[0].End;
                bool keepOn = true;
                bool closed = false;
                while (!closed && keepOn)
                {
                    keepOn = false;
                    for (int i = 0; i < EdgeRecords.Count; i++)
                    {
                        if (EdgeRecords[i].Start == endId)
                        {
                            endId = EdgeRecords[i].End;
                            loop.Add(EdgeRecords[i]);
                            EdgeRecords.RemoveAt(i);

                            if (endId == startId)
                            {
                                closed = true;
                            }
                            else
                            {
                                keepOn = true;
                            }
                            break;
                        }
                        else
                        if (EdgeRecords[i].End == startId)
                        {
                            startId = EdgeRecords[i].Start;
                            loop.Insert(0, EdgeRecords[i]);
                            EdgeRecords.RemoveAt(i);
                            if (startId == endId)
                            {
                                closed = true;
                            }
                            else
                            {
                                keepOn = true;
                            }
                            break;
                        }
                    }
                }
            }

            return loop;
        }
    }
}