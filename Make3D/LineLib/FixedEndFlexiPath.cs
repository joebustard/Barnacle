﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Barnacle.LineLib
{
    public class FixedEndFlexiPath : FlexiPath
    {
        public FlexiPoint fixedEndPoint;
        public FlexiPoint fixedMidPoint;
        public FlexiPoint fixedStartPoint;

        public FixedEndFlexiPath()
        {
            ClipAgainstBounds = true;
        }

        public FlexiPoint FixedEndPoint
        {
            get
            {
                return fixedEndPoint;
            }

            set
            {
                if (fixedEndPoint != value)
                {
                    fixedEndPoint = value;
                }
            }
        }

        public FlexiPoint FixedMidPoint
        {
            get
            {
                return fixedMidPoint;
            }

            set
            {
                if (fixedMidPoint != value)
                {
                    fixedMidPoint = value;
                }
            }
        }

        public FlexiPoint FixedStartPoint
        {
            get
            {
                return fixedStartPoint;
            }

            set
            {
                if (fixedStartPoint != value)
                {
                    fixedStartPoint = value;
                }
            }
        }

        public override void Clear()
        {
            segs.Clear();
            flexiPoints.Clear();
            if (fixedStartPoint != null)
            {
                Start = fixedStartPoint;
            }
            if (fixedMidPoint != null)
            {
                AddLineToFlexipoint(fixedMidPoint);
            }
            if (fixedEndPoint != null)
            {
                AddLineToFlexipoint(fixedEndPoint);
            }
        }

        public override Point MoveTo(Point position)
        {
            return new Point(0, 0);
        }

        public void SetBaseSegment(Point st, Point md, Point ed)
        {
            fixedStartPoint = new FlexiPoint(st);
            fixedMidPoint = new FlexiPoint(md);
            fixedEndPoint = new FlexiPoint(ed);
            Clear();
        }

        // This is similar to the base version but
        // we cant split the segment from the last point back to the first
        public override bool SplitSelectedLineSegment(System.Windows.Point position)
        {
            bool found = false;
            for (int i = 0; i < segs.Count; i++)
            {
                if (segs[i].Selected)
                {
                    if (segs[i] is LineSegment)
                    {
                        int start = segs[i].Start();
                        int end = segs[i].End();
                        // closing segment which goes back to 0 is a special case
                        if (end != 0)
                        {
                            InsertLineSegment(start, position);
                            found = true;
                        }
                    }
                    break;
                }
            }
            return found;
        }

        private void AddLineToFlexipoint(FlexiPoint fp)
        {
            AddLine(new System.Windows.Point(fp.X, fp.Y));
        }
    }
}