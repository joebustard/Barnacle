using Barnacle.RibbedFuselage.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Barnacle.RibbedFuselage
{
    internal class ImageDetailsModel : INotifyPropertyChanged
    {
        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ImageDetailsModel()
        {
            imageFilePath = "";
            name = "";
            flexiPathText = "";
            DisplayFileName = "None";
        }

        private string imageFilePath;

        /// <summary>
        /// The path to the image
        /// </summary>
        public String ImageFilePath
        {
            get { return imageFilePath; }
            set
            {
                if (value != imageFilePath)
                {
                    imageFilePath = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string flexiPathText;

        /// <summary>
        /// The text that defines the flexipath imposed over the image
        /// </summary>
        public String FlexiPathText
        {
            get { return flexiPathText; }
            set
            {
                if (value != flexiPathText)
                {
                    flexiPathText = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string name;

        /// <summary>
        /// The  name to be displayed.
        /// </summary>
        public String Name
        {
            get { return name; }
            set
            {
                if (value != name)
                {
                    name = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public EditorSettingsModel EditorSettings
        {
            get => default;
            set
            {
            }
        }

        private string displayFileName;

        public event PropertyChangedEventHandler PropertyChanged;

        public String DisplayFileName
        {
            get
            {
                return displayFileName;
            }
            set
            {
                if (value != displayFileName)
                {
                    displayFileName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public virtual void Load(XmlElement ele)
        {
            if (ele.HasAttribute("Name"))
            {
                Name = ele.GetAttribute("Name");
            }
            if (ele.HasAttribute("ImageFilePath"))
            {
                ImageFilePath = ele.GetAttribute("ImageFilePath");
            }

            if (ele.HasAttribute("FlexiPathText"))
            {
                FlexiPathText = ele.GetAttribute("FlexiPathText");
            }
        }

        public virtual void Save(XmlElement ele, XmlDocument doc)
        {
            ele.SetAttribute("Name", Name);
            ele.SetAttribute("ImageFilePath", ImageFilePath);
            ele.SetAttribute("FlexiPathText", FlexiPathText);
        }
    }
}