// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Indexes.Commands
{
    [Command]
    public class OptimizeIndex : CommandBase
    {
        public OptimizeIndex()
        {
            Text = "Optimize Index";
            Group = "Indexes";
            SortingValue = 2000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IndexViewerContext;
            if (context == null)
            {
                return false;
            }

            if (context.ClickTarget != IndexViewerContext.IndexList)
            {
                return false;
            }

            var index = context.IndexViewer.IndexList.SelectedItem as IndexViewer.IndexDescriptor;
            if (index == null)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IndexViewerContext;
            if (context == null)
            {
                return;
            }

            var index = context.IndexViewer.GetSelectedIndex();
            if (index == null)
            {
                return;
            }

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                AppHost.MessageBox(string.Format("Optimizing \"{0}\"...", index.Name), "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            };

            context.IndexViewer.Context.Site.DataService.ExecuteAsync("Indexes.OptimizeIndex", completed, index.Name);
        }
    }
}
