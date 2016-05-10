// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items.SelectedObjects;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentTrees.Items
{
    public class DatabaseTreeViewItem : BaseSiteTreeViewItem, ICanRefresh, IScopeable, ISelectable
    {
        public static readonly Guid RootItemGuid = new Guid(@"{11111111-1111-1111-1111-111111111111}");

        public DatabaseTreeViewItem([NotNull] DatabaseUri databaseUri) : base(databaseUri.Site)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            DatabaseUri = databaseUri;
            ItemHeader.IsEditable = false;
            ToolTip = databaseUri.DatabaseName.Name;
            ConnectionString = string.Empty;
            Icon = new Icon("Resources/16x16/database.png");

            Notifications.RegisterSiteEvents(this, activeDatabaseChanged: HandleActiveDatabaseChanged);

            ItemHeader.IsActive = databaseUri == AppHost.Settings.ActiveDatabaseUri;
        }

        [NotNull]
        public string ConnectionString { get; set; }

        [NotNull]
        public DatabaseUri DatabaseUri { get; }

        public override bool GetChildren(GetChildrenDelegate callback, bool async)
        {
            Assert.ArgumentNotNull(callback, nameof(callback));

            var result = new List<BaseSiteTreeViewItem>();

            var children = Site.DataService.GetRootItems(DatabaseUri).ToList();
            if (!children.Any())
            {
                return false;
            }

            foreach (var child in children)
            {
                var item = new ItemTreeViewItem(child);

                if (child.HasChildren)
                {
                    item.MakeExpandable();
                }

                result.Add(item);
            }

            callback(result);

            return true;
        }

        BaseTreeViewItem IScopeable.GetScopedTreeViewItem()
        {
            var result = new DatabaseTreeViewItem(DatabaseUri)
            {
                Text = Text
            };

            result.MakeExpandable();

            return result;
        }

        object ISelectable.GetSelectedObject()
        {
            return new DatabaseTreeViewItemSelectedObject(this);
        }

        private void HandleActiveDatabaseChanged([NotNull] object sender, [NotNull] DatabaseUri databaseUri)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(databaseUri, nameof(databaseUri));

            ItemHeader.IsActive = databaseUri == DatabaseUri;
        }
    }
}
