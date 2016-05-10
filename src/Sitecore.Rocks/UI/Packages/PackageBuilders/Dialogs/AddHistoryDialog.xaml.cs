// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI.Packages.PackageBuilders.Dialogs
{
    public partial class AddHistoryDialog
    {
        public AddHistoryDialog()
        {
            InitializeComponent();
            this.InitializeDialog();
        }

        [NotNull]
        public IEnumerable<ItemUri> SelectedItems
        {
            get { return ListView.SelectedItems; }
        }

        [NotNull]
        public Site Site { get; private set; }

        public void Initialize([NotNull] Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            Site = site;
            ListView.Site = site;
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.Close(true);
        }
    }
}
