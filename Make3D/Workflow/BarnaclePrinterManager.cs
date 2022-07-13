using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Workflow
{
    internal class BarnaclePrinterManager
    {
        public BarnaclePrinterManager()
        {
            Printers = new List<BarnaclePrinter>();
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
            doc.Save(fileName);
        }
    }
}