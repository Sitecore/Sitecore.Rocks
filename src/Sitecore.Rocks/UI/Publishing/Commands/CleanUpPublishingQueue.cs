// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.UI.Publishing.Commands
{
    public class CleanUpPublishingQueue : CommandBase
    {
        public CleanUpPublishingQueue()
        {
            Text = "Clean Up Publishing Queue";
            Group = "Publishing";
            SortingValue = 100;
        }

        public override bool CanExecute(object parameter)
        {
            return parameter is PublishingQueueContext;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as PublishingQueueContext;
            if (context == null)
            {
                return;
            }

            if (AppHost.MessageBox("Are you sure you want to clean up the publishing queue?", "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            var databaseUri = context.PublishingQueueViewer.DatabaseUri;

            ExecuteCompleted c = (response, result) =>
            {
                DataService.HandleExecute(response, result);

                context.PublishingQueueViewer.Clear();
                context.PublishingQueueViewer.GetPublishingCandidates(0);
            };

            databaseUri.Site.DataService.ExecuteAsync("Publishing.CleanUpPublishingQueue", c, databaseUri.DatabaseName.Name);
        }
    }
}
