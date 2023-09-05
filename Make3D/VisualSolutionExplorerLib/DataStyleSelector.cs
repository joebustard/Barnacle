using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace VisualSolutionExplorer
{
    class DataStyleSelector : DataTemplateSelector
    {
        public override DataTemplate
            SelectTemplate(object item, DependencyObject container)
        {
            DataTemplate result = null;
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item != null)
            {
                TreeViewItemViewModel tvi = item as TreeViewItemViewModel;
                switch (tvi.GetType().Name)
                {
                    case "ProjectFileViewModel":
                        {
                            ProjectFileViewModel fileModel = tvi as ProjectFileViewModel;
                            if (fileModel != null)
                            {
                                if (fileModel.IsEditing)
                                {
                                    result = element.FindResource("editingProjectFileView") as DataTemplate;
                                }
                                else
                                {
                                    if (fileModel.IsSelected)
                                    {
                                        result = element.FindResource("selectedProjectFileView") as DataTemplate;
                                    }
                                    else
                                    {
                                        result = element.FindResource("normalProjectFileView") as DataTemplate;
                                    }
                                }
                            }
                        }
                        break;

                    case "ProjectFolderViewModel":
                        {
                            ProjectFolderViewModel folderModel = tvi as ProjectFolderViewModel;
                            if (folderModel != null)
                            {
                                if (folderModel.IsSelected)
                                {
                                    result = element.FindResource("selectedProjectFolderView") as DataTemplate;
                                }
                                else
                                {
                                    result = element.FindResource("normalProjectFolderView") as DataTemplate;
                                }
                            }
                        }
                        break;
                }
            }

            return result;
        }
    }
}
