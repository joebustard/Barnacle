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

        public List<Object3D> Content;

        public string Caption
        {
            get { return caption; }
            set { caption = value; }
        }

        internal void Clear()
        {
            FilePath = "";
            FileFilter = "Text Files (*.txt)|*.txt";
            FileName = "Untitled";
            Extension = ".txt";
            caption = "untitled";
            Content = new List<Object3D>();

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
            caption = FileName;
        }

        public virtual void Read(string file)
        {
            try
            {
                Content.Clear();
                XmlDocument doc = new XmlDocument();
                doc.Load(file);
                XmlNode docNode = doc.SelectSingleNode("Document");

                XmlNodeList nodes = docNode.SelectNodes("obj");
                foreach (XmlNode nd in nodes)
                {
                    Object3D obj = new Object3D();
                    obj.Read(nd);
                    obj.SetMesh();
                    Content.Add(obj);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void Save(string fileName)
        {
            Write(fileName);
            FilePath = fileName;
            FileName = System.IO.Path.GetFileName(fileName);
            caption = FileName;
            Dirty = false;
        }

        public virtual void Write(string file)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement docNode = doc.CreateElement("Document");
            foreach (Object3D ob in Content)
            {
                ob.Write(doc, docNode);
            }
            doc.AppendChild(docNode);
            doc.Save(file);
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
        }
    }
}