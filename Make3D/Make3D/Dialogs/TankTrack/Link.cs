﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barnacle.Dialogs
{
    public class Link
    {
        public Link()
        {
            Name = "";
            Parts = new List<LinkPart>();
        }

        public String Name { get; set; }
        public List<LinkPart> Parts { get; set; }

        public void Add(LinkPart s)
        {
            Parts.Add(s);
        }
    }
}