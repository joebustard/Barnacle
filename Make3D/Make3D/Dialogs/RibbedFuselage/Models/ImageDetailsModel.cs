using Barnacle.RibbedFuselage.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Barnacle.RibbedFuselage
{
    internal class ImageDetailsModel
    {
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