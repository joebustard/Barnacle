using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Barnacle.Dialogs.RibbedFuselage.Models
{
    internal class MarkerModel
    {
        public String Name
        {
            get;
            set;
        }

        public double Position
        {
            get;
            set;
        }

        public double LowValue
        {
            get;
            set;
        }

        public String HighValue
        {
            get;
            set;
        }

        public RibImageDetailsModel AssociatedRib
        {
            get;
            set;
        }

        public void Load(XmlElement ele)
        {
            if (ele.HasAttribute("Name"))
            {
                Name = ele.GetAttribute("Name");
            }
            if (ele.HasAttribute("Position"))
            {
                string v = ele.GetAttribute("Position");
                Position = Convert.ToDouble(v);
            }
        }

        public void Save(XmlElement ele, XmlDocument doc)
        {
            ele.SetAttribute("Name", Name);
            ele.SetAttribute("Position", Position.ToString());
        }
    }
}