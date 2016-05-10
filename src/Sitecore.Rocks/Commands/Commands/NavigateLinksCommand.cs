// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.Commands.Commands
{
    public class NavigateLinksCommand : CommandBase
    {
        public override bool CanExecute(object parameter)
        {
            var selection = parameter as IItemSelectionContext;
            if (selection == null)
            {
                return false;
            }

            if (selection.Items.Count() != 1)
            {
                return false;
            }

            var item = selection.Items.FirstOrDefault();
            if (item == null)
            {
                return false;
            }

            if (!item.ItemUri.Site.DataService.CanExecuteAsync("Links.GetLinks"))
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var selection = parameter as IItemSelectionContext;
            if (selection == null)
            {
                return;
            }

            var item = selection.Items.FirstOrDefault();
            if (item == null)
            {
                return;
            }

            var linkViewer = AppHost.Windows.OpenLinkViewer();
            if (linkViewer == null)
            {
                return;
            }

            var linkTab = linkViewer.CreateTab(item.ItemUri);

            linkTab.Initialize(item.ItemUri);
        }
    }
}
