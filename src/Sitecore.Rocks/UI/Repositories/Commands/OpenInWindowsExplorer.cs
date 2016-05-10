// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.IO;

namespace Sitecore.Rocks.UI.Repositories.Commands
{
    [Command]
    public class OpenInWindowsExplorer : CommandBase
    {
        public OpenInWindowsExplorer()
        {
            Text = "Open in Windows Explorer";
            Group = "File";
            SortingValue = 1000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IRepositorySelectionContext;
            if (context == null)
            {
                return false;
            }

            if (context.FileNames.Count() != 1)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IRepositorySelectionContext;
            if (context == null)
            {
                return;
            }

            var fileName = context.FileNames.First();
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            File.OpenInWindowsExplorer(fileName);
        }
    }
}
