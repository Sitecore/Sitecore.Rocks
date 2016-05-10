// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.UI.Management.ManagementItems.CommandViewers;

namespace Sitecore.Rocks.UI.Management.CommandViewers.Commands
{
    [Command]
    public class SearchForUsages : CommandBase
    {
        public SearchForUsages()
        {
            Text = "Go to Definition";
            Group = "Search";
            SortingValue = 2000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as CommandViewerContext;
            if (context == null)
            {
                return false;
            }

            var selectedItem = context.CommandViewer.CommandsList.SelectedItem as CommandViewer.CommandDescriptor;
            if (selectedItem == null)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as CommandViewerContext;
            if (context == null)
            {
                return;
            }

            var selectedItem = context.CommandViewer.CommandsList.SelectedItem as CommandViewer.CommandDescriptor;
            if (selectedItem == null)
            {
                return;
            }

            var type = selectedItem.Type;
            var n = type.IndexOf(",", StringComparison.Ordinal);
            if (n >= 0)
            {
                type = type.Left(n);
            }

            SitecorePackage.Instance.Dte.ExecuteCommand("View.ObjectBrowserSearch", type);
        }
    }
}
