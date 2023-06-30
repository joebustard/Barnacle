using Barnacle.RibbedFuselage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Barnacle.Dialogs.RibbedFuselage.Models
{
    internal class RibImageDetailsModel : ImageDetailsModel
    {
        public double MarkerPosition
        {
            get; set;
        }

        public override void Load(XmlElement ele)
        {
            base.Load(ele);
            if (ele.HasAttribute("MarkerPosition"))
            {
                String v = ele.GetAttribute("MarkerPosition");
                MarkerPosition = Convert.ToDouble(v);
            }
        }

        public override void Save(XmlElement ele, XmlDocument doc)
        {
            base.Save(ele, doc);
            ele.SetAttribute("MarkerPosition", MarkerPosition.ToString());
        }

        internal RibImageDetailsModel Clone()
        {
            RibImageDetailsModel cln = new RibImageDetailsModel();
            cln.MarkerPosition = MarkerPosition;
            cln.ImageFilePath = ImageFilePath;
            cln.DisplayFileName = DisplayFileName;
            cln.Name = Name;
            cln.FlexiPathText = FlexiPathText;
            return cln;
        }
    }
}