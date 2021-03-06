// � 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.Publishing.Commands
{
    [Command]
    public class Next250 : CommandBase
    {
        public Next250()
        {
            Text = Resources.Next100_Next100_Next_250;
            Group = "Refresh";
            SortingValue = 9990;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as PublishingQueueContext;
            if (context == null)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as PublishingQueueContext;
            if (context == null)
            {
                return;
            }

            context.PublishingQueueViewer.NextPage();
        }
    }
}
