// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices.Dialogs;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.Sites.Connections;
using Sitecore.Rocks.UI.UpdateServerComponents;

namespace Sitecore.Rocks.ContentTrees.Commands.Connections
{
    [Command(Submenu = ConnectionsSubmenu.Name), CommandId(CommandIds.SitecoreExplorer.ModifyConnection, typeof(ContentTreeContext))]
    public class ModifyConnection : CommandBase
    {
        public ModifyConnection()
        {
            Text = Resources.EditSite_EditSite_Modify_Connection___;
            Group = "Connection";
            SortingValue = 1000;
            Icon = new Icon("Resources/16x16/server.png");
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

            var oldHostName = site.HostName;
            var oldUserName = site.UserName;

            var siteEditor = new SiteEditor();
            siteEditor.Load(site);
            if (AppHost.Shell.ShowDialog(siteEditor) != true)
            {
                return;
            }

            ConnectionManager.Save();

            item.UpdateStatus();
            item.Text = site.Name;
            item.Refresh();

            Notifications.RaiseSiteChanged(this, site);
            Notifications.RaiseSiteModified(this, site, oldHostName, oldUserName);

            Update(siteEditor, site);
        }

        [CanBeNull]
        protected virtual Site GetSite([NotNull] IEnumerable<BaseTreeViewItem> selectedItems)
        {
            Debug.ArgumentNotNull(selectedItems, nameof(selectedItems));

            var item = selectedItems.FirstOrDefault() as SiteTreeViewItem;

            return item == null ? null : item.Site;
        }

        protected virtual void Update([NotNull] SiteEditor siteEditor, [NotNull] Site site)
        {
            Debug.ArgumentNotNull(site, nameof(site));
            Debug.ArgumentNotNull(siteEditor, nameof(siteEditor));

            var editor = siteEditor.DataServiceEditor as WebServiceSiteEditor;
            if (editor == null)
            {
                return;
            }

            if (editor.AutomaticUpdate.IsChecked == true)
            {
                UpdateServerComponentsDialog.AutomaticUpdate(site.DataService, site.Name, site.WebRootPath, site);
            }
        }
    }
}
