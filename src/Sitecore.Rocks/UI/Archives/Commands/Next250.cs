// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.Archives.Commands
{
    [Command]
    public class Next250 : CommandBase
    {
        public Next250()
        {
            Text = Resources.Next100_Next100_Next_250;
            Group = "Refresh";
            SortingValue = 8000;
        }

        public override bool CanExecute(object parameter)
        {
            return parameter is ArchiveContext;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ArchiveContext;
            if (context == null)
            {
                return;
            }

            context.ArchiveViewer.NextPage();
        }
    }
}
