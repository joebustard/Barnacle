/**************************************************************************
*   Copyright (c) 2024 Joe Bustard <barnacle3d@gmailcom>                  *
*                                                                         *
*   This file is part of the Barnacle 3D application.                     *
*                                                                         *
*   This application is free software; you can redistribute it and/or     *
*   modify it under the terms of the GNU Library General Public           *
*   License as published by the Free Software Foundation; either          *
*   version 2 of the License, or (at your option) any later version.      *
*                                                                         *
*   This application is distributed in the hope that it will be useful,   *
*   but WITHOUT ANY WARRANTY; without even the implied warranty of        *
*   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
*   GNU Library General Public License for more details.                  *
*                                                                         *
**************************************************************************/

using Barnacle.Dialogs.RibbedFuselage.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;

namespace Barnacle.RibbedFuselage.Models
{
    internal class FuselageModel
    {
        private const double defaultMarkerSpacing = 0.1;
        private const double defaultRibSpacing = 0.1;
        private String dataFilePath;
        private string description;
        private int lastRibPosition;
        private ObservableCollection<MarkerModel> markers;
        private string name;
        private double nextNewMakerPosition;
        private ObservableCollection<RibImageDetailsModel> ribs;
        private ImageDetailsModel sideImageDetails;
        private ImageDetailsModel topImageDetails;

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
            get
            {
                return description;
            }

            set
            {
                if (description != value)
                {
                    description = value;
                }
            }
        }

        public ObservableCollection<MarkerModel> Markers
        {
            get
            {
                return markers;
            }

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
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        public char NextNameLetter
        {
            get; set;
        }

        public int NextNameNumber
        {
            get; set;
        }

        public ObservableCollection<RibImageDetailsModel> Ribs
        {
            get
            {
                return ribs;
            }

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
            get
            {
                return sideImageDetails;
            }

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
            get
            {
                return topImageDetails;
            }

            set
            {
                if (topImageDetails != value)
                {
                    topImageDetails = value;
                }
            }
        }

        internal String SidePath
        {
            get
            {
                return SideImageDetails.FlexiPathText;
            }
        }

        internal String TopPath
        {
            get
            {
                return TopImageDetails.FlexiPathText;
            }
        }

        public RibImageDetailsModel AddRib()
        {
            RibImageDetailsModel ribmod = new RibImageDetailsModel();
            ribmod.Name = GetNewRibName();
            ribs.Add(ribmod);
            return ribmod;
        }

        public RibImageDetailsModel CloneRib(int ribNumber)
        {
            RibImageDetailsModel rib = null;
            if (ribNumber != -1)
            {
                RibImageDetailsModel selectedRib = Ribs[ribNumber];
                double selectPos = FindMarkerPos(selectedRib.Name);
                int selectedMarkerIndex = FindMarkerIndex(selectedRib.Name);
                double clonePos = selectPos + defaultRibSpacing;
                RibImageDetailsModel nextRib = null;
                if (ribNumber < Ribs.Count - 1)
                {
                    nextRib = Ribs[ribNumber + 1];
                    double nextRibPosition = FindMarkerPos(nextRib.Name);
                    clonePos = selectPos + (nextRibPosition - selectPos) / 2.0;
                }

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

                Ribs.Insert(ribNumber + 1, rib);

                MarkerModel marker = new MarkerModel();
                marker.Name = rib.Name;
                marker.AssociatedRib = rib;
                marker.Position = clonePos;
                if (selectedMarkerIndex != -1)
                {
                    markers.Insert(selectedMarkerIndex + 1, marker);
                }
                else
                {
                    markers.Add(marker);
                }

                RenameRibsAfterInsertions(nameStart);
                RenameMarkers();
            }
            return rib;
        }

        public bool DeleteRib(RibImageDetailsModel selectedRib)
        {
            bool deleted = false;
            if (selectedRib != null)
            {
                if (Ribs.Contains(selectedRib))
                {
                    string nameStart = selectedRib.Name[0].ToString();
                    Ribs.Remove(selectedRib);
                    deleted = true;
                    for (int i = 0; i < Markers.Count; i++)
                    {
                        if (Markers[i].AssociatedRib == selectedRib)
                        {
                            Markers.Remove(Markers[i]);
                            break;
                        }
                    }
                    RenameRibsAfterInsertions(nameStart);
                    RenameMarkers();
                }
            }

            return deleted;
        }

        public RibImageDetailsModel InsertRib(int ribNumber)
        {
            RibImageDetailsModel rib = null;
            if (ribNumber != -1)
            {
                RibImageDetailsModel selectedRib = Ribs[ribNumber];
                double selectPos = FindMarkerPos(selectedRib.Name);
                int selectedMarkerIndex = FindMarkerIndex(selectedRib.Name);
                double clonePos = selectPos + defaultRibSpacing;
                RibImageDetailsModel nextRib = null;
                if (ribNumber < Ribs.Count - 1)
                {
                    nextRib = Ribs[ribNumber + 1];
                    double nextRibPosition = FindMarkerPos(nextRib.Name);
                    clonePos = selectPos + (nextRibPosition - selectPos) / 2.0;
                }

                rib = new RibImageDetailsModel();
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

                Ribs.Insert(ribNumber + 1, rib);

                MarkerModel marker = new MarkerModel();
                marker.Name = rib.Name;
                marker.AssociatedRib = rib;
                marker.Position = clonePos;
                if (selectedMarkerIndex != -1)
                {
                    markers.Insert(selectedMarkerIndex + 1, marker);
                }
                else
                {
                    markers.Add(marker);
                }

                RenameRibsAfterInsertions(nameStart);
                RenameMarkers();
            }
            return rib;
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
                    foreach (RibImageDetailsModel rib in ribs)
                    {
                        if (rib.Name == mk.Name)
                        {
                            mk.AssociatedRib = rib;
                        }
                    }
                    markers.Add(mk);
                }
            }
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
                RenameMarkers();
            }
        }

        public void RenameRib()
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

        public void SortRibsByPosition()
        {
            throw new System.NotImplementedException();
        }

        internal void AddMarker(RibImageDetailsModel rib)
        {
            MarkerModel marker = new MarkerModel();
            marker.Name = rib.Name;
            marker.Position = nextNewMakerPosition;
            if (marker.Position > 1.0)
            {
                marker.Position = 1;
            }
            marker.AssociatedRib = rib;
            markers.Add(marker);
            nextNewMakerPosition += defaultMarkerSpacing;
            if (nextNewMakerPosition > 1.0)
            {
                nextNewMakerPosition = 1;
            }
        }

        internal void ResetMarkerPositions()
        {
            int mc = markers.Count - 1;
            if (mc > 0)
            {
                double md = 1.0 / mc;

                for (int i = 0; i < markers.Count; i++)
                {
                    markers[i].Position = (double)i * md;
                }
            }
        }

        internal void SetSideImage(string imagePath)
        {
            SideImageDetails.ImageFilePath = imagePath;
        }

        internal void SetSidePath(string pathText)
        {
            LoggerLib.Logger.LogLine($"SetSidePath(string pathText) {pathText}");

            SideImageDetails.FlexiPathText = pathText;
        }

        internal void SetTopImage(string imagePath)
        {
            TopImageDetails.ImageFilePath = imagePath;
        }

        internal void SetTopPath(string pathText)
        {
            LoggerLib.Logger.LogLine($"SetTopPath(string pathText) {pathText}");
            TopImageDetails.FlexiPathText = pathText;
        }

        private int FindMarkerIndex(string name)
        {
            int res = 0;
            foreach (MarkerModel mk in markers)
            {
                if (mk.Name == name)
                {
                    return res;
                }
                res++;
            }
            return -1;
        }

        private double FindMarkerPos(string name)
        {
            foreach (MarkerModel mk in markers)
            {
                if (mk.Name == name)
                {
                    return mk.Position;
                }
            }
            return 0.0;
        }

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

        private void RenameMarkers()
        {
            foreach (MarkerModel mk in Markers)
            {
                if (mk.AssociatedRib != null)
                {
                    mk.Name = mk.AssociatedRib.Name;
                }
            }
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

        public struct NameRec
        {
            public string newName;
            public string originalName;
            public int ribIndex;
        }
    }
}