using Barnacle.LineLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barnacle.Dialogs
{
    public class LinkPart
    {
        private FlexiPath flexipath;
        private List<System.Windows.Point> profile;

        public LinkPart()
        {
            PathText = "";
            profile = null;
        }

        public String Name { get; set; }
        public String PathText { get; set; }

        public List<System.Windows.Point> Profile
        {
            get
            {
                if (profile == null)
                {
                    if (PathText != "")
                    {
                        FlexiPath fp = new FlexiPath();
                        if (fp.FromTextPath(PathText))
                        {
                            profile = fp.DisplayPoints();
                        }
                    }
                }
                return profile;
            }
        }

        public double W { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }
}