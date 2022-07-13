using System;
using System.Xml;

namespace Workflow
{
    public class BarnaclePrinter
    {
        public BarnaclePrinter()
        {
            Name = "Ender 3 Pro";
            CuraPrinterFile = @"creality_ender3pro";
            CuraExtruderFile = @"creality_base_extruder_0";
            StartGCode = @"M117 $NAME \n; Ender 3 Custom Start G-code\nG92 E0 ; Reset Extruder\nG28 ; Home all axes\nG29 ; BLTouch\nG1 Z2.0 F3000 ; Move Z Axis up little to prevent scratching of Heat Bed\nG1 X0.1 Y20 Z0.3 F5000.0 ; Move to start position\nG1 X0.1 Y200.0 Z0.3 F1500.0 E15 ; Draw the first line\nG1 X0.4 Y200.0 Z0.3 F5000.0 ; Move to side a little\nG1 X0.4 Y20 Z0.3 F1500.0 E30 ; Draw the second line\nG92 E0 ; Reset Extruder\nG1 Z2.0 F3000 ; Move Z Axis up little to prevent scratching of Heat Bed\nG1 X5 Y20 Z0.3 F5000.0 ; Move over to prevent blob squish\n";
            EndGCode = @"G91 ;Relative positioning\nG1 E-2 F2700 ;Retract a bit\nG1 E-2 Z0.2 F2400 ;Retract and raise Z\nG1 X5 Y5 F3000 ;Wipe out\nG1 Z10 ;Raise Z more\nG90 ;Absolute positioning\n";
        }

        public string CuraExtruderFile { get; set; }
        public string CuraPrinterFile { get; set; }
        public String EndGCode { get; set; }
        public string Name { get; set; }
        public String StartGCode { get; set; }

        public XmlElement SaveAsXml(XmlDocument doc)
        {
            doc.XmlResolver = null;
            XmlElement docNode = doc.CreateElement("Printer");
            docNode.SetAttribute("PrinterName", Name);
            docNode.SetAttribute("CuraPrinter", CuraPrinterFile);

            docNode.SetAttribute("CuraExtruder", CuraExtruderFile);
            doc.AppendChild(docNode);

            XmlElement sgcode = doc.CreateElement("StartGCode");
            sgcode.InnerText = StartGCode;
            docNode.AppendChild(sgcode);

            XmlElement egcode = doc.CreateElement("EndGCode");
            egcode.InnerText = EndGCode;
            docNode.AppendChild(egcode);
            return docNode;
        }

        internal void LoadFromXml(XmlElement el)
        {
            Name = el.GetAttribute("PrinterName");
            CuraPrinterFile = el.GetAttribute("CuraPrinter");
            CuraExtruderFile = el.GetAttribute("CuraExtruder");
            XmlNode sg = el.SelectSingleNode("StartGCode");
            StartGCode = sg.InnerText;
            XmlNode eg = el.SelectSingleNode("EndGCode");
            EndGCode = eg.InnerText;
        }
    }
}