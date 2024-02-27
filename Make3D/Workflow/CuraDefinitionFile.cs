using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Text.Json;

namespace Workflow
{
    internal class CuraDefinitionFile
    {
        private String FilePath { get; set; }

        public CuraDefinition definition;

        public CuraDefinitionFile BaseFile { get; set; }

        public bool Load(String fileName)
        {
            bool res = false;
            FilePath = fileName;
            BaseFile = null;
            if (File.Exists(fileName))
            {
                try
                {
                    string jsonString = File.ReadAllText(fileName);
                    definition = JsonSerializer.Deserialize<CuraDefinition>(jsonString);
                    if (definition.inherits != null && definition.inherits != "")
                    {
                        String pth = Path.GetDirectoryName(fileName);
                        String baseName = Path.Combine(pth, definition.inherits);
                        BaseFile = new CuraDefinitionFile();
                        BaseFile.Load(baseName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            return res;
        }
    }
}