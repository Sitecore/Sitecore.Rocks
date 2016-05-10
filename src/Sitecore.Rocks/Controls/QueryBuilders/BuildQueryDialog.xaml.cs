// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Controls.QueryBuilders
{
    public partial class BuildQueryDialog
    {
        public BuildQueryDialog([NotNull] string query, CustomValidationType type)
        {
            Assert.ArgumentNotNull(query, nameof(query));

            InitializeComponent();
            this.InitializeDialog();

            Loaded += ControlLoaded;

            Text = query;
            Type = type;

            if (type == CustomValidationType.ExpandedWebConfig || type == CustomValidationType.WebConfig)
            {
                DatabasePanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                SitePanel.Visibility = Visibility.Collapsed;
            }

            DatabaseUri = DatabaseSelector.DatabaseUri;
            DatabaseSelector.SelectionChanged += SetDatabaseUri;
            SiteSelector.SelectionChanged += SetSite;
        }

        [CanBeNull]
        public DatabaseUri DatabaseUri
        {
            get { return QueryBuilder.DatabaseUri; }

            set
            {
                DatabaseSelector.DatabaseUri = value;
                SiteSelector.Site = value == null ? null : value.Site;
                QueryBuilder.DatabaseUri = value;
            }
        }

        [CanBeNull]
        public Site Site
        {
            get { return QueryBuilder.Site; }

            set
            {
                QueryBuilder.Site = value;
                SiteSelector.Site = value;
            }
        }

        [NotNull]
        public string Text
        {
            get { return QueryBuilder.Text; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));
                QueryBuilder.Text = value;
            }
        }

        public CustomValidationType Type
        {
            get { return QueryBuilder.Type; }

            set { QueryBuilder.Type = value; }
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            if (Type == CustomValidationType.ExpandedWebConfig || Type == CustomValidationType.WebConfig)
            {
                var text = AppHost.Settings.GetString("Management\\Validation\\Builder", "Site", string.Empty);

                var site = SiteManager.GetSite(text);
                if (site != null)
                {
                    SiteSelector.Site = site;
                }
            }
            else
            {
                var text = AppHost.Settings.GetString("Management\\Validation\\Builder", "Database", string.Empty);

                DatabaseUri databaseUri;
                if (DatabaseUri.TryParse(text, out databaseUri))
                {
                    DatabaseUri = databaseUri;
                }
            }
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (Type == CustomValidationType.ExpandedWebConfig || Type == CustomValidationType.WebConfig)
            {
                var site = SiteSelector.Site;
                if (site != null && site != Site.Empty)
                {
                    AppHost.Settings.Set("Management\\Validation\\Builder", "Site", site.Name);
                }
            }
            else
            {
                var databaseUri = DatabaseSelector.DatabaseUri;
                if (databaseUri != null && databaseUri != DatabaseUri.Empty)
                {
                    AppHost.Settings.Set("Management\\Validation\\Builder", "Database", databaseUri.ToString());
                }
            }

            this.Close(true);
        }

        private void SetDatabaseUri([NotNull] object sender, [NotNull] DatabaseUri databaseUri)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(databaseUri, nameof(databaseUri));

            DatabaseUri = databaseUri;
        }

        private void SetSite([NotNull] object sender, [NotNull] Site site)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(site, nameof(site));

            Site = site;
        }
    }
}
