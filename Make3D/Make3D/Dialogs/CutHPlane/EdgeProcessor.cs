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
