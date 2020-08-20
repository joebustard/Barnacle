using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Xml;

namespace Make3D.Models
{
    internal class Document : INotifyPropertyChanged
    {
        private string caption;
        private static int nextId;
        public static String NextName
        {
            get
            {
                nextId++;
                return "Object_" + nextId.ToString();
            }
        }
        public List<Object3D> Content;

        public string Caption
        {
            get { return caption; }
            set
            {
                if (caption != value)
                {
                    caption = value;
                    NotifyPropertyChanged();
                }
            }
        }

        internal void Clear()
        {
            FilePath = "";
            FileFilter = "Text Files (*.txt)|*.txt";
            FileName = "Untitled";
            Extension = ".txt";
            caption = "untitled";
            Content = new List<Object3D>();
            nextId = 0;
            Dirty = false;
        }

        private bool dirty;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool Dirty
        {
            get
            {
                return dirty;
            }
            set
            {
                if (dirty != value)
                {
                    dirty = value;
                    caption = FileName;
                    if (dirty)
                    {
                        caption += "*";
                    }
                }
            }
        }

        public string Extension { get; set; }
        public string FileFilter { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }

        public void Load(string fileName)
        {
            Read(fileName);
            FilePath = fileName;
            FileName = System.IO.Path.GetFileName(fileName);

            Dirty = false;
            Caption = FileName;
        }

        internal void InsertFile(string fileName)
        {
            Read(fileName, false);


            Dirty = false;
            
        }

        public virtual void Read(string file, bool clearFirst = true)
        {
            try
            {
                if (clearFirst)
                {
                    Content.Clear();
                }
                XmlDocument doc = new XmlDocument();
                doc.Load(file);
                XmlNode docNode = doc.SelectSingleNode("Document");
                if (clearFirst)
                {
                    XmlElement docele = docNode as XmlElement;
                    if (docele != null)
                    {
                        GetInt(docele, "NextId", ref nextId, 0);
                    }
                }
                XmlNodeList nodes = docNode.ChildNodes;
                foreach (XmlNode nd in nodes)
                {
                    if (nd.Name == "obj")
                    {
                        Object3D obj = new Object3D();
                        obj.Read(nd);

                        obj.SetMesh();
                        Content.Add(obj);
                    }
                    if (nd.Name == "groupobj")
                    {
                        Group3D obj = new Group3D();
                        obj.Read(nd);

                        obj.SetMesh();
                        Content.Add(obj);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        internal void Export(string v)
        {
            if (v=="STL")
            {
                STLExporter exp = new STLExporter();
                if (FileName == "")
                {
                    exp.Export("C:\\tmp\\test.stl", Content);
                }
                else
                {
                    string expName = System.IO.Path.ChangeExtension(FilePath, "stl");
                    exp.Export(expName, Content);
                    MessageBox.Show("Model exported to " + expName);
                }
            }
        }

        internal void DeleteObject(Object3D o)
        {
            Content.Remove(o);
            Dirty = true;
        }

        private void GetInt(XmlElement docele, string nm, ref int res, int v)
        {
            res = v;
            if ( docele.HasAttribute(nm))
            {
                string s = docele.GetAttribute(nm);
                if (s != "")
                {
                    res = Convert.ToInt32(s);
                }
            }
        }

        public void Save(string fileName)
        {
            Write(fileName);
            FilePath = fileName;
            FileName = System.IO.Path.GetFileName(fileName);
            Caption = FileName;
            Dirty = false;
        }

        public virtual void Write(string file)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement docNode = doc.CreateElement("Document");
            docNode.SetAttribute("NextId", nextId.ToString());
            foreach (Object3D ob in Content)
            {
                ob.Write(doc, docNode);
            }
            doc.AppendChild(docNode);
            doc.Save(file);
        }

        internal void SplitGroup(Group3D grp)
        {
            Content.Remove(grp);
            Content.Add(grp.LeftObject);
            Content.Add(grp.RightObject);
            Dirty = true;
        }

        internal void Add(Object3D leftObject)
        {
            
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        internal void ReplaceObjectsByGroup(Group3D grp)
        {
            Content.Remove(grp.LeftObject);
            Content.Remove(grp.RightObject);
            Content.Add(grp);
            Dirty = true;
        }

        public Document()
        {
            Clear();
            NotificationManager.Subscribe("DocDirty", OnDocDirty);
        }

        private void OnDocDirty(object param)
        {
            Dirty = true;
        }
    }
}