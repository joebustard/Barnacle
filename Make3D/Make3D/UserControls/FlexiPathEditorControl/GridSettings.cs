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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml;

namespace Barnacle.UserControls
{
    public class GridSettings
    {
        public GridSettings()
        {
            ShowGrid = GridStyle.Rectangular;
        }
        public enum GridStyle
        {
            Hidden,
            Rectangular,
            Polar
        }
        public GridStyle ShowGrid { get; set; }
        private double rectangularGridSize = 10;

        public double RectangularGridSize
        {
            get { return rectangularGridSize; }
            set
            {
                if (value != rectangularGridSize)
                {
                    rectangularGridSize = value;
                }
            }
        }

        private double polarGridRadius = 10;

        public double PolarGridRadius
        {
            get { return polarGridRadius; }
            set
            {
                if (value != polarGridRadius)
                {
                    polarGridRadius = value;
                }
            }
        }

        private double polarGridAngle = 36;

        public double PolarGridAngle
        {
            get { return polarGridAngle; }
            set
            {
                if (value != polarGridAngle)
                {
                    polarGridAngle = value;
                }
            }
        }

        private Point centre;

        public Point Centre
        {
            get { return centre; }
            set { centre = value; }
        }

        internal void SetPolarCentre(double v1, double v2)
        {
            centre = new Point(v1, v2);
        }

        public Color LineColour { get; set; } = Colors.Black;
        public double LineOpacity { get; set; } = 0.5;
        public double LineThickness { get; set; } = 4;
        public bool Save()
        {
            string pth = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Barnacle\\GridSettings.xml";
            bool res = false;
            XmlDocument doc = new XmlDocument();
            doc.XmlResolver = null;

            XmlElement root = doc.CreateElement("GridSettings");
            root.SetAttribute("ShowGrid", ShowGrid.ToString());
            root.SetAttribute("LineThickness", LineThickness.ToString());
            root.SetAttribute("LineColour", LineColour.ToString());
            root.SetAttribute("LineOpacity", LineOpacity.ToString());
            root.SetAttribute("RectangularGridSize", RectangularGridSize.ToString());
            root.SetAttribute("PolarGridRadius", PolarGridRadius.ToString());
            root.SetAttribute("PolarGridAngle", PolarGridAngle.ToString());
            doc.AppendChild(root);
            doc.Save(pth);

            return res;
        }
        public bool Load()
        {
            bool res = false;
            string pth = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Barnacle\\GridSettings.xml";
            if (System.IO.File.Exists(pth))
            {
                XmlDocument doc = new XmlDocument();
                doc.XmlResolver = null;

                try
                {
                    doc.Load(pth);
                    XmlElement root = doc.SelectSingleNode("GridSettings") as XmlElement;
                    String s = root.GetAttribute("ShowGrid");
                    switch (s)
                    {
                        case "Hidden":
                            {
                                ShowGrid = GridStyle.Hidden;
                            }
                            break;

                        case "Rectangular":
                            {
                                ShowGrid = GridStyle.Rectangular;
                            }
                            break;
                        case "Polar":
                            {
                                ShowGrid = GridStyle.Polar;
                            }
                            break;
                    }
                    s = root.GetAttribute("LineThickness");
                    LineThickness = Convert.ToDouble(s);

                    s = root.GetAttribute("LineOpacity");
                    LineOpacity = Convert.ToDouble(s);

                    s = root.GetAttribute("RectangularGridSize");
                    RectangularGridSize = Convert.ToDouble(s);

                    s = root.GetAttribute("PolarGridRadius");
                    PolarGridRadius = Convert.ToDouble(s);

                    s = root.GetAttribute("PolarGridAngle");
                    PolarGridAngle = Convert.ToDouble(s);

                    s = root.GetAttribute("LineColour");
                    LineColour  = (Color)ColorConverter.ConvertFromString(s);

                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);
                }

            }
            return res;
        }
    }
}