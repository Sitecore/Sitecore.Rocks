// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.Sites.Connections;
using Sitecore.Rocks.Sites.Dialogs;

namespace Sitecore.Rocks.ContentTrees.Commands.Connections
{
    [Command(Submenu = ConnectionsSubmenu.Name)]
    public class EditConnectionProperties : CommandBase
    {
        public EditConnectionProperties()
        {
            Text = "Edit Connection Properties...";
            Group = "Properties";
            SortingValue = 5000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return false;
            }

            if (context.SelectedItems.Count() != 1)
            {
                return false;
            }

            var site = GetSite(context.SelectedItems);

            return site != null;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return;
            }

            var item = context.SelectedItems.FirstOrDefault() as SiteTreeViewItem;
            if (item == null)
            {
                return;
            }

            var site = GetSite(context.SelectedItems);
            if (site == null)
            {
                return;
            }

            var dialog = new EditConnectionDialog(site.Connection);
            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return;
            }

            ConnectionManager.Save();

            item.UpdateStatus();

            Notifications.RaiseSiteChanged(this, site);
        }

        [CanBeNull]
        protected virtual Site GetSite([NotNull] IEnumerable<BaseTreeViewItem> selectedItems)
        {
            Debug.ArgumentNotNull(selectedItems, nameof(selectedItems));

            var item = selectedItems.FirstOrDefault() as SiteTreeViewItem;

            return item == null ? null : item.Site;
        }
    }
}
