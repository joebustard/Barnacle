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
using System.Windows.Media;
using System.Xml;

namespace Barnacle.ViewModel.BuildPlates
{
    internal class BuildPlate
    {
        public BuildPlate()
        {
            PrinterName = "Default";
            Width = 210;
            Height = 210;
            Length = 210;
            BorderThickness = 5;
            BorderColour = Colors.CadetBlue;
        }

        public Color BorderColour
        {
            get; set;
        }

        public double BorderThickness
        {
            get; set;
        }

        public double Height
        {
            get; set;
        }

        public double Length
        {
            get; set;
        }

        public String PrinterName
        {
            get; set;
        }

        public double Width
        {
            get; set;
        }

        internal virtual XmlElement Read(XmlDocument doc, XmlElement ele)
        {
            PrinterName = ele.GetAttribute("PrinterName");
            Length = GetDouble(ele, "Length");
            Width = GetDouble(ele, "Width");
            Height = GetDouble(ele, "Height");
            BorderColour = Colors.Blue;
            if (ele.HasAttribute("Colour"))
            {
                string s = ele.GetAttribute("Colour");
                BorderColour = (Color)ColorConverter.ConvertFromString(s);
            }
            BorderThickness = GetDouble(ele, "BorderThickness");
            return ele;
        }

        internal virtual XmlElement Write(XmlDocument doc, XmlElement docNode)
        {
            XmlElement ele = doc.CreateElement("BuildPlate");
            docNode.AppendChild(ele);
            ele.SetAttribute("PrinterName", PrinterName);
            ele.SetAttribute("Length", Length.ToString());
            ele.SetAttribute("Width", Width.ToString());
            ele.SetAttribute("Height", Height.ToString());
            ele.SetAttribute("Colour", BorderColour.ToString());
            ele.SetAttribute("BorderThickness", BorderThickness.ToString());
            return ele;
        }

        private double GetDouble(XmlElement ele, string nm)
        {
            double res = 0;
            if (ele.HasAttribute(nm))
            {
                try
                {
                    string s = ele.GetAttribute(nm);
                    res = Convert.ToDouble(s);
                }
                catch (Exception ex)
                {
                    LoggerLib.Logger.LogLine(ex.Message);
                }
            }
            return res;
        }
    }
}