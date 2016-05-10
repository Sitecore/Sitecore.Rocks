// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.UI.Management.ManagementItems.SiteViewers.Dialogs
{
    public partial class EditSiteDefinitionDialog
    {
        public EditSiteDefinitionDialog([NotNull] IEnumerable<SiteDefinition> sites, [NotNull] SiteDefinition siteDefinition)
        {
            Assert.ArgumentNotNull(sites, nameof(sites));
            Assert.ArgumentNotNull(siteDefinition, nameof(siteDefinition));

            InitializeComponent();
            this.InitializeDialog();

            SiteDefinition = siteDefinition;
            Properties.SelectedObject = siteDefinition;

            foreach (var site in sites)
            {
                var comboBoxItem = new ComboBoxItem
                {
                    Content = site.Name
                };

                Inherit.Items.Add(comboBoxItem);
            }
        }

        protected SiteDefinition SiteDefinition { get; set; }

        private void CancelClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            this.Close(false);
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            if (string.IsNullOrEmpty(SiteDefinition.Name))
            {
                AppHost.MessageBox("Site must have a name.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            this.Close(true);
        }

        private void SetInheritance([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedItem = Inherit.SelectedItem as ComboBoxItem;
            if (selectedItem == null)
            {
                SiteDefinition.Inherits = string.Empty;
                return;
            }

            SiteDefinition.Inherits = selectedItem.Content as string ?? string.Empty;
            Properties.Update();
        }
    }
}
