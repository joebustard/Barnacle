using Barnacle.Dialogs.RibbedFuselage.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;

namespace Barnacle.RibbedFuselage.Models
{
    internal class FuselageModel
    {
        private String dataFilePath;
        private int lastRibPosition;
        private ObservableCollection<MarkerModel> markers;
        private ObservableCollection<RibImageDetailsModel> ribs;
        private ImageDetailsModel sideImageDetails;
        private ImageDetailsModel topImageDetails;
        private const double defaultMarkerSpacing = 10;

        public FuselageModel()
        {
            Ribs = new ObservableCollection<RibImageDetailsModel>();
            markers = new ObservableCollection<MarkerModel>();
            lastRibPosition = 0;
            dataFilePath = "";
            Name = "";
            Description = "";
            NextNameLetter = 'A';
            NextNameNumber = 0;
            nextNewMakerPosition = 0;
            SideImageDetails = new ImageDetailsModel();
            TopImageDetails = new ImageDetailsModel();
        }

        public String Description
        {
            get => default;
            set
            {
            }
        }

        internal void SetTopImage(string imagePath)
        {
            TopImageDetails.ImageFilePath = imagePath;
        }

        internal void SetTopPath(string pathText)
        {
            TopImageDetails.FlexiPathText = pathText;
        }

        internal void SetSidePath(string pathText)
        {
            SideImageDetails.FlexiPathText = pathText;
        }

        internal void SetSideImage(string imagePath)
        {
            SideImageDetails.ImageFilePath = imagePath;
        }

        public ObservableCollection<MarkerModel> Markers
        {
            get { return markers; }

            set
            {
                if (markers != value)
                {
                    markers = value;
                }
            }
        }

        public String Name
        {
            get => default;
            set
            {
            }
        }

        private double nextNewMakerPosition;

        internal void AddMarker(string name)
        {
            MarkerModel marker = new MarkerModel();
            marker.Name = name;
            marker.Position = nextNewMakerPosition;
            markers.Add(marker);
            nextNewMakerPosition += defaultMarkerSpacing;
        }

        public ObservableCollection<RibImageDetailsModel> Ribs
        {
            get { return ribs; }
            set
            {
                if (ribs != value)
                {
                    ribs = value;
                }
            }
        }

        public ImageDetailsModel SideImageDetails
        {
            get { return sideImageDetails; }
            set
            {
                if (sideImageDetails != value)
                {
                    sideImageDetails = value;
                }
            }
        }

        public ImageDetailsModel TopImageDetails
        {
            get { return topImageDetails; }
            set
            {
                if (topImageDetails != value)
                {
                    topImageDetails = value;
                }
            }
        }

        public RibImageDetailsModel AddRib()
        {
            RibImageDetailsModel ribmod = new RibImageDetailsModel();
            ribmod.Name = GetNewRibName();
            ribs.Add(ribmod);
            return ribmod;
        }

        public char NextNameLetter { get; set; }
        public int NextNameNumber { get; set; }

        private string GetNewRibName()
        {
            string name = "" + NextNameLetter;
            if (NextNameNumber > 0)
            {
                name += NextNameNumber.ToString();
            }
            NextNameLetter++;
            if (NextNameLetter == 'Z' + 1)
            {
                NextNameLetter = 'A';
                NextNameNumber++;
            }

            return name;
        }

        public void CopyRib()
        {
            throw new System.NotImplementedException();
        }

        public bool DeleteRib(RibImageDetailsModel selectedRib)
        {
            bool deleted = false;
            if (selectedRib != null)
            {
                if (Ribs.Contains(selectedRib))
                {
                    Ribs.Remove(selectedRib);
                    deleted = true;
                }
            }

            return deleted;
        }

        public void Load(string f)
        {
            ribs.Clear();
            markers.Clear();
            XmlDocument doc = new XmlDocument();
            doc.XmlResolver = null;
            doc.Load(f);
            XmlElement sparNode = doc.SelectSingleNode("Spars") as XmlElement;
            if (sparNode != null)
            {
                if (sparNode.HasAttribute("NextLetter"))
                {
                    string v = sparNode.GetAttribute("NextLetter");
                    NextNameLetter = v[0];
                    v = sparNode.GetAttribute("NextNumber");
                    NextNameNumber = Convert.ToInt16(v);
                    v = sparNode.GetAttribute("NextMarkerPosition");
                    nextNewMakerPosition = Convert.ToDouble(v);
                }
                XmlElement topEle = sparNode.SelectSingleNode("TopView") as XmlElement;
                topImageDetails.Load(topEle);
                XmlElement sideEle = sparNode.SelectSingleNode("SideView") as XmlElement;
                sideImageDetails.Load(sideEle);

                XmlNodeList rl = sparNode.SelectNodes("Rib");
                foreach (XmlNode nd in rl)
                {
                    XmlElement el = nd as XmlElement;
                    RibImageDetailsModel rib = new RibImageDetailsModel();
                    rib.Load(el);
                    ribs.Add(rib);
                }

                XmlNodeList ml = sparNode.SelectNodes("Marker");
                foreach (XmlNode nd in rl)
                {
                    XmlElement el = nd as XmlElement;
                    MarkerModel mk = new MarkerModel();
                    mk.Load(el);
                    markers.Add(mk);
                }
            }
        }

        public void RenameRib()
        {
            throw new System.NotImplementedException();
        }

        public void SortRibsByPosition()
        {
            throw new System.NotImplementedException();
        }

        public void Save(string f)
        {
            XmlDocument doc = new XmlDocument();
            doc.XmlResolver = null;
            XmlElement docNode = doc.CreateElement("Spars");
            docNode.SetAttribute("NextLetter", NextNameLetter.ToString());
            docNode.SetAttribute("NextNumber", NextNameNumber.ToString());
            docNode.SetAttribute("NextMarkerPosition", nextNewMakerPosition.ToString());
            XmlElement topNode = doc.CreateElement("TopView");
            topImageDetails.Save(topNode, doc);
            docNode.AppendChild(topNode);
            XmlElement sideNode = doc.CreateElement("SideView");
            sideImageDetails.Save(sideNode, doc);
            docNode.AppendChild(sideNode);

            foreach (RibImageDetailsModel ob in ribs)
            {
                XmlElement ribNode = doc.CreateElement("Rib");
                ob.Save(ribNode, doc);
                docNode.AppendChild(ribNode);
            }
            foreach (MarkerModel m in markers)
            {
                XmlElement markerNode = doc.CreateElement("Marker");
                m.Save(markerNode, doc);
                docNode.AppendChild(markerNode);
            }
            doc.AppendChild(docNode);
            doc.Save(f);
        }
    }
}