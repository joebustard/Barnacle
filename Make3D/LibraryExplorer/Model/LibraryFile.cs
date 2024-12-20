﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LibraryExplorer
{
    internal class LibraryFile : IComparable<LibraryFile>
    {
        public LibraryFile(string fileName)
        {
            this.FileName = fileName;
        }

        public LibraryFile()
        {
            FileName = String.Empty;
            Source = String.Empty;
            Backup = false;
            Export = false;
            EditFile = false;
            RunFile = false;
            OutOfDate = false;
        }

        // should this file be added to the backup when a backup command is issued
        public bool Backup { get; set; }

        // should the menu for this file show "Edit"
        public bool EditFile { get; set; }

        // should this file be exported when an export all command is issued
        public bool Export { get; set; }

        public string FileName { get; set; }

        public string FilePath { get; internal set; }

        public int IconNumber
        {
            get
            {
                if (FileName == null || FileName == "")
                {
                    return 0;
                }
                else
                {
                    int res = 0;
                    string ext = System.IO.Path.GetExtension(FileName);
                    ext = ext.ToLower();
                    switch (ext)
                    {
                        case ".png":
                        case ".jpg":
                        case ".bmp":
                            {
                                res = 1;
                            }
                            break;
                    }
                    return res;
                }
            }
        }

        public string OldName { get; set; }

        public bool OutOfDate { get; set; }

        // should the menu for this file show "Run"
        public bool RunFile { get; set; }

        public String Source { get; set; }

        public int CompareTo(LibraryFile comparePart)
        {
            // A null value means that this object is greater.
            if (comparePart == null)
                return 1;
            else
                return this.FileName.CompareTo(comparePart.FileName);
        }

        public void Load(XmlDocument doc, XmlNode nd)
        {
            XmlElement ele = nd as XmlElement;
            if (ele != null)
            {
                if (ele.HasAttribute("Name"))
                {
                    FileName = ele.GetAttribute("Name");
                }

                if (ele.HasAttribute("Backup"))
                {
                    Backup = Convert.ToBoolean(ele.GetAttribute("Backup"));
                }
                if (ele.HasAttribute("Export"))
                {
                    Export = Convert.ToBoolean(ele.GetAttribute("Export"));
                }
                if (ele.HasAttribute("Edit"))
                {
                    EditFile = Convert.ToBoolean(ele.GetAttribute("Edit"));
                }
                if (ele.HasAttribute("Run"))
                {
                    RunFile = Convert.ToBoolean(ele.GetAttribute("Run"));
                }
            }
        }

        public void Save(XmlDocument solutionDoc, XmlElement root)
        {
            XmlElement fe = solutionDoc.CreateElement("File");
            fe.SetAttribute("Name", FileName);

            fe.SetAttribute("Backup", Backup.ToString());
            fe.SetAttribute("Export", Export.ToString());
            if (EditFile)
            {
                fe.SetAttribute("Edit", "True");
            }
            if (RunFile)
            {
                fe.SetAttribute("Run", "True");
            }
            root.AppendChild(fe);
        }

        internal void RecordOldName()
        {
            OldName = FileName;
        }

        internal void UpdatePath()
        {
            String pth = System.IO.Path.GetDirectoryName(FilePath);
            pth += "\\" + FileName;
            FilePath = pth;
        }
    }
}