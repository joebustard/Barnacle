// **************************************************************************
// *   Copyright (c) 2024 Joe Bustard <barnacle3d@gmailcom>                  *
// *                                                                         *
// *   This file is part of the Barnacle 3D application.                     *
// *                                                                         *
// *   This application is free software. You can redistribute it and/or     *
// *   modify it under the terms of the GNU Library General Public           *
// *   License as published by the Free Software Foundation. Either          *
// *   version 2 of the License, or (at your option) any later version.      *
// *                                                                         *
// *   This application is distributed in the hope that it will be useful,   *
// *   but WITHOUT ANY WARRANTY. Without even the implied warranty of        *
// *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
// *   GNU Library General Public License for more details.                  *
// *                                                                         *
// *************************************************************************

using Senilias.StepTypes;
using SeniliasLib.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace Senilias.TargetTypes
{
    public class TargetDependency : INotifyPropertyChanged
    {
        protected int level;

        private string actionString;

        private Visibility browserVisible;

        private string source;

        private string target;

        public TargetDependency()
        {
            Action = new StepType();
            Triggered = false;
            Predecessor = null;
            BrowserVisible = Visibility.Hidden;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public enum TypeOfDependency
        {
            Unknown,
            File,
            Folder,
            Collection
        };

        public StepType Action
        {
            get; set;
        }

        public string ActionString
        {
            get
            {
                if (Action != null)
                {
                    return Action.ToString();
                }
                else
                {
                    return "";
                }
            }
            set
            {
                if (value != actionString)
                {
                    actionString = value;
                    string l = actionString.ToLower().Trim();
                    if (l == "nop" || l == "")
                    {
                        Action = new NopStep();
                    }
                    else
                    if (l.StartsWith("run ") && l.Length > 4)
                    {
                        Action = new ScriptStep(actionString.Substring(4));
                    }
                    else
                    if (l.StartsWith("touch ") && l.Length > 6)
                    {
                        Action = new TouchStep(actionString.Substring(6));
                    }
                }
            }
        }

        public Visibility BrowserVisible
        {
            get
            {
                return browserVisible;
            }
            set
            {
                if (value != browserVisible)
                {
                    browserVisible = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Only used for indentation
        /// </summary>
        public int Level
        {
            get
            {
                return level;
            }
        }

        public TargetDependency? Predecessor
        {
            get;
            set;
        }

        public bool Selected
        {
            get;
            set;
        }

        public string Source
        {
            get
            {
                return source;
            }
            set
            {
                if (source != value)
                {
                    source = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string Target
        {
            get
            {
                return target;
            }
            set
            {
                if (value != target)
                {
                    target = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool Triggered
        {
            get; set;
        }

        public void AddSource(XmlDocument doc, XmlElement xmlElement)
        {
            XmlNode nd = doc.CreateElement("Source");
            nd.InnerText = Source;
            xmlElement.AppendChild(nd);
        }

        public void AddTarget(XmlDocument doc, XmlElement xmlElement)
        {
            XmlNode nd = doc.CreateElement("Target");
            nd.InnerText = Target;
            xmlElement.AppendChild(nd);
        }

        public virtual bool CheckTriggered()
        {
            Triggered = false;
            return Triggered;
        }

        public virtual void ForceTriggers()
        {
            Triggered = true;
            if (Predecessor != null)
            {
                Predecessor.ForceTriggers();
            }
        }

        public virtual void Load(XmlNode ele)
        {
        }

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public string RelativeToProject(string s)
        {
            string res = Utils.RelativeToProject(s);
            return res;
        }

        public virtual void Save(XmlDocument doc, XmlNode node)
        {
        }

        public virtual void SetLevel(int l)
        {
            level = l;
            if (Action != null)
            {
                Action.SetLevel(l);
            }
        }

        public virtual void ToDo(List<string> res)
        {
        }

        internal virtual TargetDependency Clone()
        {
            TargetDependency res = null;
            return res;
        }

        internal virtual void Dump()
        {
        }

        internal virtual bool IsPredecessor(TargetDependency dep2)
        {
            bool res = false;
            if (Source.ToLower() == dep2.Target.ToLower())
            {
                res = true;
                Predecessor = dep2;
            }
            else
            {
                if (Predecessor != null)
                {
                    res = Predecessor.IsPredecessor(dep2);
                }
            }
            return res;
        }

        internal void MakePathsAbsolue()
        {
            Source = RelativeToProject(Source);
            Target = RelativeToProject(Target);
            if (Action != null)
            {
                Action.MakePathsAbsolute();
            }
        }
    }
}