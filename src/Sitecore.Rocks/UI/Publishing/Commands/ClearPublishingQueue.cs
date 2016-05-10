// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.UI.Publishing.Commands
{
    [Command]
    public class ClearPublishingQueue : CommandBase
    {
        public ClearPublishingQueue()
        {
            Text = "Clear Publishing Queue";
            Group = "Publishing";
            SortingValue = 200;
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

            if (AppHost.MessageBox("Are you sure you want to clear the publishing queue?", "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
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

            databaseUri.Site.DataService.ExecuteAsync("Publishing.ClearPublishingQueue", c, databaseUri.DatabaseName.Name);
        }
    }
}
