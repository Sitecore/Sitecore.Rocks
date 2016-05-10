// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Applications.Storages;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Shell.Dialogs
{
    public partial class NewItemWizardDialog
    {
        public const string NewItemWizardRegistryKey = "NewItemWizard";

        public NewItemWizardDialog()
        {
            InitializeComponent();
            this.InitializeDialog();
        }

        [CanBeNull]
        public ItemUri ItemUri { get; set; }

        [CanBeNull]
        public string SelectedPath { get; set; }

        [NotNull]
        protected Site Site { get; private set; }

        [NotNull]
        protected string TemplateName { get; set; }

        public void Initialize([NotNull] Site site, [NotNull] string templateName)
        {
            Assert.ArgumentNotNull(site, nameof(site));
            Assert.ArgumentNotNull(templateName, nameof(templateName));

            Site = site;
            TemplateName = templateName;

            var siteTreeViewItem = new SiteTreeViewItem(site);
            TreeView.Items.Add(siteTreeViewItem);

            var key = GetStorageKey(site, templateName);
            var lastSelected = Storage.ReadString(NewItemWizardRegistryKey, key, string.Empty);

            if (!string.IsNullOrEmpty(lastSelected))
            {
                ItemUri itemUri;
                if (ItemUri.TryParse(lastSelected, out itemUri))
                {
                    if (TreeView.ExpandTo(itemUri) != null)
                    {
                        return;
                    }
                }
            }

            siteTreeViewItem.IsExpanded = true;
        }

        [NotNull]
        private string GetStorageKey([NotNull] Site site, [NotNull] string templateName)
        {
            Debug.ArgumentNotNull(site, nameof(site));
            Debug.ArgumentNotNull(templateName, nameof(templateName));

            return site.Name + templateName;
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var baseTreeViewItem = TreeView.SelectedItem as ItemTreeViewItem;

            if (baseTreeViewItem == null)
            {
                SelectedPath = string.Empty;
                ItemUri = ItemUri.Empty;
            }
            else
            {
                SelectedPath = baseTreeViewItem.GetPath();
                ItemUri = baseTreeViewItem.Item.ItemUri;

                var key = GetStorageKey(Site, TemplateName);
                Storage.Write(NewItemWizardRegistryKey, key, baseTreeViewItem.Item.ItemUri.ToString());
            }

            this.Close(true);
        }
    }
}
