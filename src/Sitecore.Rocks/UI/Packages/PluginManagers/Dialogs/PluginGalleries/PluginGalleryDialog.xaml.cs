// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.UI.Packages.PluginManagers.Dialogs.PluginGalleries
{
    public partial class PluginGalleryDialog
    {
        public PluginGalleryDialog()
        {
            InitializeComponent();
            this.InitializeDialog();
        }

        private void CloseDialog([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));
            Debug.ArgumentNotNull(sender, nameof(sender));

            if (!string.IsNullOrEmpty(ListBox.NameTextBox.Text) && !string.IsNullOrEmpty(ListBox.LocationTextBox.Text))
            {
                ListBox.AddLocation();
            }

            ListBox.Save();

            this.Close(true);
        }
    }
}
