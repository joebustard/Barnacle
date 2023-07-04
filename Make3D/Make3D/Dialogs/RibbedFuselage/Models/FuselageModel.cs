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

        public struct NameRec
        {
            public string newName;
            public string originalName;
            public int ribIndex;
        }

        private void RenameRibsAfterInsertions(string nameStart)
        {
            List<NameRec> nameRecs = new List<NameRec>();

            int j = 0;
            for (int i = 0; i < Ribs.Count; i++)
            {
                if (Ribs[i].Name.StartsWith(nameStart))
                {
                    NameRec rec = new NameRec();
                    rec.ribIndex = i;
                    rec.originalName = Ribs[i].Name;
                    if (j == 0)
                    {
                        rec.newName = nameStart;
                    }
                    else
                    {
                        rec.newName = nameStart + j.ToString();
                    }
                    nameRecs.Add(rec);
                    j++;
                }
            }

            // now rename the ribs locally
            for (int i = 0; i < nameRecs.Count; i++)
            {
                Ribs[nameRecs[i].ribIndex].Name = nameRecs[i].newName;
            }
        }

        private const double defaultRibSpacing = 10;

        public RibImageDetailsModel CloneRib(int ribNumber)
        {
            RibImageDetailsModel rib = null;
            if (ribNumber != -1)
            {
                RibImageDetailsModel selectedRib = Ribs[ribNumber];
                rib = selectedRib.Clone();
                string nameStart = selectedRib.Name.Substring(0, 1);
                int subName = 0;
                foreach (RibImageDetailsModel rc in Ribs)
                {
                    if (rc.Name.StartsWith(nameStart))
                    {
                        subName++;
                    }
                }
                rib.Name = nameStart + subName.ToString();
                if (ribNumber < Ribs.Count - 1)
                {
                    rib.MarkerPosition = selectedRib.MarkerPosition + (Ribs[ribNumber + 1].MarkerPosition - selectedRib.MarkerPosition) / 2;
                }
                else
                {
                    rib.MarkerPosition += defaultRibSpacing;
                }
                Ribs.Insert(ribNumber + 1, rib);

                RenameRibsAfterInsertions(nameStart);
            }
            return rib;
        }

        public void RenameAllRibs()
        {
            if (Ribs.Count > 0)
            {
                List<NameRec> nameRecs = new List<NameRec>();

                int j = 1;
                for (int i = 0; i < Ribs.Count; i++)
                {
                    NameRec rec = new NameRec();
                    rec.ribIndex = i;
                    rec.originalName = Ribs[i].Name;
                    if (i <= 26)
                    {
                        rec.newName = ((char)('A' + i)).ToString();
                    }
                    else
                    {
                        rec.newName = "Z" + j.ToString();
                        j++;
                    }
                    nameRecs.Add(rec);
                }
                NextNameLetter = (char)('A' + Ribs.Count);
                NextNameNumber = 0;
                if (Ribs.Count > 26)
                {
                    NextNameLetter = 'Z';
                    NextNameNumber = Ribs.Count - 26;
                }

                // now rename the ribs locally
                for (int i = 0; i < nameRecs.Count; i++)
                {
                    Ribs[nameRecs[i].ribIndex].Name = nameRecs[i].newName;
                }
            }
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
                }
                if (sparNode.HasAttribute("NextLetter"))
                {
                    string v = sparNode.GetAttribute("NextNumber");
                    NextNameNumber = Convert.ToInt16(v);
                }

                if (sparNode.HasAttribute("NextMarkerPosition"))
                {
                    string v = sparNode.GetAttribute("NextMarkerPosition");
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
                foreach (XmlNode nd in ml)
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