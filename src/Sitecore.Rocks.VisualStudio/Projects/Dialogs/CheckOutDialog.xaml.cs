// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Projects.Dialogs
{
    public partial class CheckOutDialog
    {
        public CheckOutDialog([NotNull] Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            InitializeComponent();
            this.InitializeDialog();

            Site = site;

            Loaded += ControlLoaded;
        }

        [NotNull]
        public Site Site { get; }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            var fileUri = new FileUri(Site, @"\", FileUriBaseFolder.Web, true);

            var web = new RootFileTreeViewItem(fileUri)
            {
                Text = Rocks.Resources.CheckOutDialog_ControlLoaded_Web_Site
            };

            web.Items.Add(DummyTreeViewItem.Instance);
            Files.Items.Add(web);

            web.IsExpanded = true;
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.Close(true);
        }
    }
}
