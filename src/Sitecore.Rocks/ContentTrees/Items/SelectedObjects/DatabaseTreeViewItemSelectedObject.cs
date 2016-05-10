// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentTrees.Items.SelectedObjects
{
    public class DatabaseTreeViewItemSelectedObject : SiteSelectedObject
    {
        public DatabaseTreeViewItemSelectedObject([NotNull] DatabaseTreeViewItem databaseTreeViewItem) : base(databaseTreeViewItem.DatabaseUri.Site)
        {
            Assert.ArgumentNotNull(databaseTreeViewItem, nameof(databaseTreeViewItem));

            DatabaseTreeViewItem = databaseTreeViewItem;
        }

        [Description("The connection string."), DisplayName("Connection String"), Category("Database")]
        public string ConnectionString
        {
            get { return DatabaseTreeViewItem.ConnectionString; }
        }

        [NotNull, Description("The name of the database."), DisplayName("Database Name"), Category("Database")]
        public string DatabaseName
        {
            get { return DatabaseTreeViewItem.DatabaseUri.DatabaseName.ToString(); }
        }

        [NotNull]
        protected internal DatabaseTreeViewItem DatabaseTreeViewItem { get; }
    }
}
