using FileUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace VisualSolutionExplorerLib.Dialogs
{
    /// <summary>
    /// Interaction logic for ConfirmName
    /// </summary>
    public partial class ConfirmName : Window, INotifyPropertyChanged
    {
        private string fileName;

        private Dictionary<string, string> fileTemplates;
        private bool nameValid;
        private Visibility okVisible;
        private String selectedTemplateName;
        private List<string> templateNames;

        private Visibility templatesVisible = Visibility.Hidden;

        public ConfirmName()
        {
            InitializeComponent();

            NameValid = false;

            DataContext = this;
            Extension = "";
            FolderPath = "";
            FileName = "";
            SelectedTemplate = "";
            SelectedTemplateName = "";
            fileTemplates = new Dictionary<string, string>();
            templateNames = new List<string>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Extension
        {
            get; internal set;
        }

        public String FileName
        {
            get
            {
                return fileName;
            }
            set
            {
                if (value != fileName)
                {
                    fileName = value;
                    NotifyPropertyChanged();
                    CheckNameValid();
                }
            }
        }

        public string FolderPath
        {
            get; internal set;
        }

        public bool NameValid
        {
            get
            {
                return nameValid;
            }
            set
            {
                if (value != nameValid)
                {
                    nameValid = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Visibility OKVisible
        {
            get
            {
                return okVisible;
            }
            set
            {
                if (value != okVisible)
                {
                    okVisible = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String SelectedTemplate
        {
            get; set;
        }

        public String SelectedTemplateName
        {
            get
            {
                return selectedTemplateName;
            }
            set
            {
                if (selectedTemplateName != value)
                {
                    selectedTemplateName = value;
                    NotifyPropertyChanged();

                    if (selectedTemplateName != null && selectedTemplateName != "")
                    {
                        SelectedTemplate = fileTemplates[selectedTemplateName];
                    }
                    else
                    {
                        SelectedTemplate = "";
                    }
                }
            }
        }

        public List<string> TemplateNames
        {
            get
            {
                return this.templateNames;
            }

            set
            {
                this.templateNames = value;
                NotifyPropertyChanged();
            }
        }

        public Visibility TemplatesVisible
        {
            get
            {
                return templatesVisible;
            }
            set
            {
                if (value != templatesVisible)
                {
                    templatesVisible = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public virtual void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        internal void SetFileTemplates(string fileTemplate, string supportedFileExtension)
        {
            string key = System.IO.Path.GetFileNameWithoutExtension(fileTemplate);
            fileTemplates[key] = fileTemplate;
            templateNames.Add(key);

            TemplatesVisible = Visibility.Visible;
            switch (supportedFileExtension)
            {
                case "lmp":
                    {
                        string dataPath = AppDomain.CurrentDomain.BaseDirectory + "data\\ScriptTemplates";
                        GetTemplatesFrom(dataPath, fileTemplates, templateNames);
                        dataPath = PathManager.UserScriptTemplatesFolder();
                        GetTemplatesFrom(dataPath, fileTemplates, templateNames);
                    }
                    break;

                default:
                    break;
            }
            SelectedTemplate = "";
            NotifyPropertyChanged("TemplateNames");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CheckNameValid()
        {
            bool valid = false;
            if (Extension != "" && FileName.ToLower().EndsWith(Extension.ToLower()))
            {
                valid = true;
            }
            if (valid && FolderPath != "")
            {
                String full = FolderPath + System.IO.Path.DirectorySeparatorChar + FileName;

                if (System.IO.File.Exists(full))
                {
                    valid = false;
                }
            }
            NameValid = valid;
            if (valid)
            {
                OKVisible = Visibility.Visible;
            }
            else
            {
                OKVisible = Visibility.Hidden;
            }
        }

        private void GetTemplatesFrom(string p, Dictionary<string, string> fileTemplates, List<string> templateNames)
        {
            if (System.IO.Directory.Exists(p))
            {
                String[] fileNames = System.IO.Directory.GetFiles(p);
                foreach (string name in fileNames)
                {
                    string fname = System.IO.Path.GetFileNameWithoutExtension(name);
                    fileTemplates.Add(fname, name);
                    TemplateNames.Add(fname);
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (templateNames.Count > 0)
            {
                TemplatesVisible = Visibility.Visible;
            }
            else
            {
                TemplatesVisible = Visibility.Hidden;
            }
            NotifyPropertyChanged("TemplateNames");
        }
    }
}