using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml;

namespace VisualSolutionExplorer
{
    public class ProjectSettings
    {
        public ProjectSettings()
        {
            BaseScale = "1";
            ExportScale = "1";
            ExportRootName = "<FileName>";
            Description = "A 3D project created by Barnacle";
            ExportRotation = new Point3D(0, 0, 0);
            ExportAxisSwap = true;
            ImportStlAxisSwap = true;
            ImportObjAxisSwap = false;
            ImportOffAxisSwap = false;
            SetOriginToCentroid = true;
            FloorAllOnExport = true;
            VersionExport = false;
            ClearPreviousVersionsOnExport = true;
            ExportEmptyFiles = false;
            AutoSaveScript = true;
            DefaultObjectColour = Colors.DarkGray;
            SlicerPath = @"";
            PlaceNewAtMarker = true;
        }

        public int AutoSaveMinutes
        {
            get; set;
        }

        public bool AutoSaveOn
        {
            get; set;
        }

        public bool AutoSaveScript
        {
            get; set;
        }

        public string BaseScale
        {
            get; set;
        }

        public bool ClearPreviousVersionsOnExport
        {
            get; set;
        }

        public Color DefaultObjectColour
        {
            get; set;
        }

        public string Description
        {
            get; set;
        }

        public bool ExportAxisSwap
        {
            get; set;
        }

        public bool ExportEmptyFiles
        {
            get; set;
        }

        public string ExportRootName
        {
            get; set;
        }

        public Point3D ExportRotation
        {
            get; set;
        }

        public string ExportScale
        {
            get; set;
        }

        public bool FloorAllOnExport
        {
            get; set;
        }

        public bool ImportObjAxisSwap
        {
            get; set;
        }

        public bool ImportOffAxisSwap
        {
            get; set;
        }

        public bool ImportStlAxisSwap
        {
            get; set;
        }

        public bool PlaceNewAtMarker
        {
            get; set;
        }

        public bool RepeatHoleFix
        {
            get; set;
        }

        public string SDCardName
        {
            get; set;
        }

        public bool SetOriginToCentroid
        {
            get; set;
        }

        public string SlicerPath
        {
            get; set;
        }

        public bool VersionExport
        {
            get; set;
        }

        internal void Read(XmlNode nd)
        {
            XmlElement ele = nd as XmlElement;
            if (ele.HasAttribute("AutoSaveScript"))
            {
                string s = ele.GetAttribute("AutoSaveScript");
                AutoSaveScript = Convert.ToBoolean(s);
            }
            if (ele.HasAttribute("ExportRootName"))
            {
                ExportRootName = ele.GetAttribute("ExportRootName");
            }
            if (ele.HasAttribute("BaseScale"))
            {
                BaseScale = ele.GetAttribute("BaseScale");
            }
            if (ele.HasAttribute("ExportScale"))
            {
                ExportScale = ele.GetAttribute("ExportScale");
            }

            ExportAxisSwap = GetBool(ele, "ExportAxisSwap", ExportAxisSwap);
            SetOriginToCentroid = GetBool(ele, "SetOriginToCentroid", SetOriginToCentroid);
            ImportStlAxisSwap = GetBool(ele, "ImportStlAxisSwap", ImportStlAxisSwap);
            ImportObjAxisSwap = GetBool(ele, "ImportObjAxisSwap", ImportObjAxisSwap);
            ImportOffAxisSwap = GetBool(ele, "ImportOffAxisSwap", ImportOffAxisSwap);
            FloorAllOnExport = GetBool(ele, "FloorAllOnExport", FloorAllOnExport);
            if (ele.HasAttribute("SlicerPath"))
            {
                SlicerPath = ele.GetAttribute("SlicerPath");
            }

            if (ele.HasAttribute("PlaceNewAtMarker"))
            {
                string s = ele.GetAttribute("PlaceNewAtMarker");
                PlaceNewAtMarker = Convert.ToBoolean(s);
            }

            if (ele.HasAttribute("VersionExport"))
            {
                string s = ele.GetAttribute("VersionExport");
                VersionExport = Convert.ToBoolean(s);
            }
            if (ele.HasAttribute("ClearPreviousVersionsOnExport"))
            {
                string s = ele.GetAttribute("ClearPreviousVersionsOnExport");
                ClearPreviousVersionsOnExport = Convert.ToBoolean(s);
            }

            if (ele.HasAttribute("DefaultObjectColour"))
            {
                string s = ele.GetAttribute("DefaultObjectColour");
                DefaultObjectColour = (Color)ColorConverter.ConvertFromString(s);
            }
            XmlNode rt = nd.SelectSingleNode("Rotation");
            if (rt != null)
            {
                Point3D r = new Point3D();
                r.X = GetDouble(rt, "X");
                r.Y = GetDouble(rt, "Y");
                r.Z = GetDouble(rt, "Z");
                ExportRotation = r;
            }
            XmlNode des = nd.SelectSingleNode("Description");
            if (des != null)
            {
                Description = des.InnerText;
            }
        }

        internal void Write(XmlDocument doc, XmlElement docNode)
        {
            XmlElement ele = doc.CreateElement("Settings");
            docNode.AppendChild(ele);
            ele.SetAttribute("ExportRootName", ExportRootName);
            ele.SetAttribute("BaseScale", BaseScale);
            ele.SetAttribute("ExportScale", ExportScale);
            ele.SetAttribute("ExportSwapAxis", ExportAxisSwap.ToString());
            ele.SetAttribute("ImportStlSwapAxis", ImportStlAxisSwap.ToString());
            ele.SetAttribute("ImportObjSwapAxis", ImportObjAxisSwap.ToString());
            ele.SetAttribute("ImportOffSwapAxis", ImportOffAxisSwap.ToString());
            ele.SetAttribute("SetOriginToCentroid", SetOriginToCentroid.ToString());
            ele.SetAttribute("FloorAllOnExport", FloorAllOnExport.ToString());
            ele.SetAttribute("VersionExport", VersionExport.ToString());
            ele.SetAttribute("ClearPreviousVersionsOnExport", ClearPreviousVersionsOnExport.ToString());

            ele.SetAttribute("AutoSaveScript", AutoSaveScript.ToString());
            ele.SetAttribute("DefaultObjectColour", DefaultObjectColour.ToString());
            ele.SetAttribute("PlaceNewAtMarker", PlaceNewAtMarker.ToString());

            XmlElement rot = doc.CreateElement("Rotation");
            rot.SetAttribute("X", ExportRotation.X.ToString());
            rot.SetAttribute("Y", ExportRotation.Y.ToString());
            rot.SetAttribute("Z", ExportRotation.Z.ToString());
            ele.AppendChild(rot);
            XmlElement des = doc.CreateElement("Description");
            des.InnerText = Description;
        }

        protected double GetDouble(XmlNode pn, string v)
        {
            XmlElement el = pn as XmlElement;
            string val = el.GetAttribute(v);
            return Convert.ToDouble(val);
        }

        private bool GetBool(XmlElement ele, string key, bool defvalue)
        {
            if (ele.HasAttribute(key))
            {
                try
                {
                    string s = ele.GetAttribute(key);
                    bool b = Convert.ToBoolean(s);
                    return b;
                }
                catch (Exception ex)
                {
                    LoggerLib.Logger.Log(ex.Message);
                    return defvalue;
                }
            }
            else
            {
                return defvalue;
            }
        }
    }
}