// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.ContentTrees.Commands.Connections
{
    [Command(Submenu = ConnectionsSubmenu.Name), CommandId(CommandIds.SitecoreExplorer.Disconnect, typeof(ContentTreeContext), Text = "Disconnect")]
    public class DisconnectSite : CommandBase
    {
        public DisconnectSite()
        {
            Text = Resources.DisconnectSite_DisconnectSite_Delete;
            Group = "Connection";
            SortingValue = 2000;
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
            if (site == null)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return;
            }

            var site = GetSite(context.SelectedItems);
            if (site == null)
            {
                return;
            }

            var item = context.SelectedItems.FirstOrDefault();
            if (item == null)
            {
                return;
            }

            if (AppHost.MessageBox(string.Format(Resources.DisconnectSite_Execute_Are_you_sure_you_want_to_disconnect___0___, site.Name), Resources.DisconnectSite_Execute_Disconnect, MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            AppHost.Sites.Disconnect(site);
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
