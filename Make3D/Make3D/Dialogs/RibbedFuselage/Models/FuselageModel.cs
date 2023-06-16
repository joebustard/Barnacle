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
        }

        public String Description
        {
            get => default;
            set
            {
            }
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

        public void AddRib()
        {
            RibImageDetailsModel ribmod = new RibImageDetailsModel();
            ribmod.Name = GetNewRibName();
            ribs.Add(ribmod);
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

        public void Load(XmlElement ele)
        {
        }

        public void RenameRib()
        {
            throw new System.NotImplementedException();
        }

        public void Save(XmlElement ele, XmlDocument doc)
        {
        }

        public void SortRibsByPosition()
        {
            throw new System.NotImplementedException();
        }
    }
}