using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using TemplateLib;

namespace Make3D.Dialogs
{
    /// <summary>
    /// Interaction logic for NewProject.xaml
    /// </summary>
    public partial class NewProjectDlg : Window
    {
        private ProjectTemplator templator;
        private Dictionary<string, string> descriptions;
        private string projectRoot;
        private String projPath;
        private string projName;
        private string selectedTemplate;

        public NewProjectDlg()
        {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (templator.ProcessTemplate(projName, projPath, selectedTemplate))
            {
                DialogResult = true;
                Close();
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool ok = true;
            projName = (sender as TextBox).Text;
            string name = projName;
            if (!name.StartsWith("\\"))
            {
                name = "\\" + name;
            }
            projPath = projectRoot + name;
            char[] illegal = System.IO.Path.GetInvalidPathChars();

            if (projPath.IndexOfAny(illegal) > -1)
            {
                ok = false;
            }
            illegal = new char[]
            {
            '$',
            '@',
            '(',
            ')',
            '!',
            '£',
            '$',
            '%',
            '^',
            '&',
            '+',
            '=',
            '[',
            ']',
            '#',
            '~'
            };

            if (projPath.IndexOfAny(illegal) > -1)
            {
                ok = false;
            }

            // can't rcrrd max len
            if (projPath.Length > 260)
            {
                ok = false;
            }

            // can't reuse the project directory
            if (Directory.Exists(projPath))
            {
                ok = false;
            }

            OK_Button.IsEnabled = ok;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TemplateBox.SelectedIndex != -1)
            {
                string n = TemplateBox.SelectedItem.ToString();
                DescriptionBox.Text = descriptions[n];
                selectedTemplate = n;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            projectRoot = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            projectRoot += "\\Barnacle";
            descriptions = new Dictionary<string, string>();
            templator = new ProjectTemplator();
            templator.TemplateDefinitionPath = AppDomain.CurrentDomain.BaseDirectory + "templates";
            templator.ScanForTemplates();
            TemplateBox.Items.Clear();
            for (int i = 0; i < templator.NumberOfTemplates(); i++)
            {
                string n = string.Empty;
                string d = string.Empty;
                templator.GetTemplateDetails(i, ref n, ref d);
                TemplateBox.Items.Add(n);
                descriptions[n] = d;
            }
            if (TemplateBox.Items.Count > 0)
            {
                TemplateBox.SelectedIndex = 0;
            }
            OK_Button.IsEnabled = false;
        }
    }
}