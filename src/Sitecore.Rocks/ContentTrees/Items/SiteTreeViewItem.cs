// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items.SelectedObjects;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.ContentTrees.Items
{
    public class SiteTreeViewItem : BaseSiteTreeViewItem, ICanRefresh, ICanDrag, IScopeable, ISelectable
    {
        public const string DragIdentifier = "SitecoreSite";

        public SiteTreeViewItem([NotNull] Site site) : base(site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            ToolTip = string.Empty;
            ToolTipOpening += OpenToolTip;
            Icon = new Icon("Resources/16x16/server_delete.png");

            Notifications.RegisterSiteEvents(this, dataServiceStatusChanged: HandleDataServiceStatusChanged, changed: HandleSiteChanged, activeDatabaseChanged: HandleActiveDatabaseChanged);

            Text = GetSiteName();
            ItemHeader.IsActive = site == AppHost.Settings.ActiveDatabaseUri.Site;

            UpdateStatus();
        }

        [Obsolete]
        public bool IsProject { get; set; }

        public override bool GetChildren(GetChildrenDelegate callback, bool async)
        {
            Assert.ArgumentNotNull(callback, nameof(callback));

            if (!async)
            {
                var databases = Site.DataService.GetDatabases();
                LoadDatabases(databases, callback);
                return true;
            }

            var thread = new DataServiceThread(delegate
            {
                var databases = Site.DataService.GetDatabases().ToList();

                Dispatcher.Invoke(new Action(() => LoadDatabases(databases, callback)));
            });

            var result = thread.Start(Site);

            if (!result)
            {
                IsExpanded = false;
                MakeExpandable();
                UpdateStatus();
            }

            return result;
        }

        public void UpdateStatus()
        {
            Action action = delegate
            {
                switch (Site.DataService.Status)
                {
                    case DataServiceStatus.NotConnected:
                        Icon = new Icon("Resources/16x16/server_delete.png");
                        break;
                    case DataServiceStatus.Connected:
                        Icon = new Icon("Resources/16x16/server.png");
                        break;
                    case DataServiceStatus.Failed:
                        Icon = new Icon("Resources/16x16/server_warning.png");
                        break;
                }
            };

            Dispatcher.BeginInvoke(action);
        }

        protected override bool Renamed(string newName)
        {
            Debug.ArgumentNotNull(newName, nameof(newName));

            return false;
        }

        string ICanDrag.GetDragIdentifier()
        {
            return DragIdentifier;
        }

        BaseTreeViewItem IScopeable.GetScopedTreeViewItem()
        {
            var result = new SiteTreeViewItem(Site);

            result.MakeExpandable();

            return result;
        }

        object ISelectable.GetSelectedObject()
        {
            return new SiteSelectedObject(Site);
        }

        [NotNull]
        private string GetSiteName()
        {
            var comment = string.Empty;

            if (!string.IsNullOrEmpty(Site.WebRootPath))
            {
                if (!Directory.Exists(Site.WebRootPath))
                {
                    comment = "Web Root Path not found";
                }
                else if (!Directory.Exists(Path.Combine(Site.WebRootPath, "bin")))
                {
                    comment = "/bin folder not found";
                }
                else if (!Directory.Exists(Path.Combine(Site.WebRootPath, "sitecore")))
                {
                    comment = "/sitecore folder not found";
                }
                else
                {
                    comment = WebAdministration.GetWebSiteState(Site);
                }
            }

            var result = Site.Name;

            if (!string.IsNullOrEmpty(comment))
            {
                result += string.Format(" ({0})", comment);
            }

            return result;
        }

        private void HandleActiveDatabaseChanged([NotNull] object sender, [NotNull] DatabaseUri databaseUri)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(databaseUri, nameof(databaseUri));

            ItemHeader.IsActive = databaseUri.Site == Site;
        }

        private void HandleDataServiceStatusChanged([NotNull] DataService dataservice, DataServiceStatus newstatus, DataServiceStatus previousstatus)
        {
            Debug.ArgumentNotNull(dataservice, nameof(dataservice));

            if (Site.DataService != dataservice)
            {
                return;
            }

            UpdateStatus();
        }

        private void HandleSiteChanged([NotNull] object sender, [NotNull] Site site)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(site, nameof(site));

            if (Site != site)
            {
                return;
            }

            UpdateStatus();
        }

        private void LoadDatabases([NotNull] IEnumerable<DatabaseInfo> databases, [NotNull] GetChildrenDelegate callback)
        {
            Debug.ArgumentNotNull(databases, nameof(databases));
            Debug.ArgumentNotNull(callback, nameof(callback));

            var result = new List<BaseTreeViewItem>();

            foreach (var databaseInfo in databases)
            {
                if (databaseInfo.DatabaseName.ToString() == @"filesystem")
                {
                    continue;
                }

                var item = new DatabaseTreeViewItem(new DatabaseUri(Site, databaseInfo.DatabaseName))
                {
                    Text = databaseInfo.DatabaseName.ToString()
                };

                item.MakeExpandable();
                result.Add(item);
            }

            callback(result);
        }

        private void OpenToolTip([NotNull] object sender, [NotNull] ToolTipEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ToolTip = ToolTipBuilder.BuildToolTip(Site);
        }
    }
}
