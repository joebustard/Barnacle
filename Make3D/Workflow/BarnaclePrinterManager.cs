using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Workflow
{
    public class BarnaclePrinterManager
    {
        private string filePath;

        public BarnaclePrinterManager()
        {
            Printers = new List<BarnaclePrinter>();

            string folder = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            folder += "\\Barnacle\\PrinterProfiles\\";
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            else
            {
                filePath = folder + "printers.xml";
                LoadFromXml(filePath);
            }
        }

        public void Save()
        {
            if (filePath != null && filePath != "")
            {
                SaveAsXml(filePath);
            }
        }

        public List<BarnaclePrinter> Printers { get; set; }

        public void LoadFromXml(string fileName)
        {
            if (File.Exists(fileName))
            {
                if (Printers == null)
                {
                    Printers = new List<BarnaclePrinter>();
                }
                else
                {
                    Printers.Clear();
                }
                XmlDocument doc = new XmlDocument();
                doc.XmlResolver = null;
                doc.Load(fileName);
                XmlNode root = doc.SelectSingleNode("Printers");
                if (root != null)
                {
                    XmlNodeList prnts = root.SelectNodes("Printer");
                    foreach (XmlNode nd in prnts)
                    {
                        XmlElement el = nd as XmlElement;
                        BarnaclePrinter printer = new BarnaclePrinter();
                        printer.LoadFromXml(el);
                        Printers.Add(printer);
                    }
                }
            }
        }

        public void SaveAsXml(string fileName)
        {
            XmlDocument doc = new XmlDocument();
            doc.XmlResolver = null;
            XmlElement docNode = doc.CreateElement("Printers");
            if (Printers != null)
            {
                foreach (BarnaclePrinter prnt in Printers)
                {
                    docNode.AppendChild(prnt.SaveAsXml(doc));
                }
            }
            doc.AppendChild(docNode);
            doc.Save(fileName);
        }

        public List<string> GetPrinterNames()
        {
            List<String> res = new List<string>();
            foreach (BarnaclePrinter bp in Printers)
            {
                res.Add(bp.Name);
            }
            return res;
        }

        public BarnaclePrinter FindPrinter(String name)
        {
            BarnaclePrinter res = null;
            foreach (BarnaclePrinter bp in Printers)
            {
                if (bp.Name == name)
                {
                    res = bp;
                    break;
                }
            }
            return res;
        }

        public void AddPrinter(string printerName, string curaPrinter, string curaExtuder, string startGCode, string endGCode)
        {
            if (Printers != null)
            {
                BarnaclePrinter bp = new BarnaclePrinter();
                bp.Name = printerName;
                bp.CuraPrinterFile = curaPrinter;
                bp.CuraExtruderFile = curaExtuder;
                bp.StartGCode = startGCode;
                bp.EndGCode = endGCode;
                Printers.Add(bp);
            }
        }
    }
}