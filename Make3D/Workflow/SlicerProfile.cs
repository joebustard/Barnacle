using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using static Workflow.CuraDefinition;

namespace Workflow
{
    public class SlicerProfile
    {
        
      
        public SlicerProfile(string fileName)
        {
            LoadOverrides(fileName);
        }

        public SlicerProfile()
        {
        }

        public void LoadOverrides(string fileName)
        {
            
        }

        public void SaveOverrides(String fName)
        {
            try
            {
                if (File.Exists(fName))
                {
                    File.Delete(fName);
                }
 
            }
            catch (Exception ex)
            {
                LoggerLib.Logger.LogException(ex);
            }
        }

        internal void Prepare()
        {
           // CuraFile.ProcessSettings();
        }

        public void Save(string fName)
        {
            try
            {
                if (File.Exists(fName))
                {
                    File.Delete(fName);
                }
               
            }
            catch (Exception ex)
            {
                LoggerLib.Logger.LogException(ex);
            }
        }
    }
}