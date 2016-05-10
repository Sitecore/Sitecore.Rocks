// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentTrees.Items.SelectedObjects
{
    public class DatabaseUriSelectedObject : SiteSelectedObject
    {
        public DatabaseUriSelectedObject([NotNull] DatabaseUri databaseUri) : base(databaseUri.Site)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            DatabaseUri = databaseUri;
        }

        [NotNull, Description("The name of the database."), DisplayName("Database Name"), Category("Database")]
        public string DatabaseName
        {
            get { return DatabaseUri.DatabaseName.ToString(); }
        }

        [NotNull]
        protected internal DatabaseUri DatabaseUri { get; }
    }
}
