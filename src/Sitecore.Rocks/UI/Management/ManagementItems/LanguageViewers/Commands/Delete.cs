// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Pipelines.DeleteItem;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.UI.Management.ManagementItems.LanguageViewers.Commands
{
    [Command]
    public class Delete : CommandBase
    {
        public Delete()
        {
            Text = Resources.Delete_Delete_Delete;
            Group = "Edit";
            SortingValue = 3000;
            Icon = new Icon("Resources/16x16/delete.png");
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as LanguageViewerContext;
            if (context == null)
            {
                return false;
            }

            if (context.LanguageViewer.LanguageList.SelectedItems.Count != 1)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as LanguageViewerContext;
            if (context == null)
            {
                return;
            }

            var selectedItem = context.LanguageViewer.LanguageList.SelectedItem as LanguageViewer.LanguageDescriptor;
            if (selectedItem == null)
            {
                return;
            }

            if (AppHost.MessageBox(string.Format(Resources.Delete_Execute_Are_you_sure_you_want_to_delete___0___, selectedItem.DisplayName), Resources.Confirmation, MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) != MessageBoxResult.OK)
            {
                return;
            }

            var pipeline = PipelineManager.GetPipeline<DeleteItemPipeline>();

            pipeline.ItemUri = selectedItem.ItemUri;
            pipeline.DeleteFiles = false;

            pipeline.Start();

            context.LanguageViewer.LoadLanguages();
        }
    }
}
