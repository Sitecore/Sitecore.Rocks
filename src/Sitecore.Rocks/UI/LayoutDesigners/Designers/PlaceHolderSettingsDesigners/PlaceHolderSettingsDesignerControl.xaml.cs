// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Items;
using TaskDialogInterop;

namespace Sitecore.Rocks.UI.LayoutDesigners.Designers.PlaceHolderSettingsDesigners
{
    public partial class PlaceHolderSettingsDesignerControl : IDesigner
    {
        public PlaceHolderSettingsDesignerControl([NotNull] PlaceHolderTreeViewItem placeHolderTreeViewItem)
        {
            Assert.ArgumentNotNull(placeHolderTreeViewItem, nameof(placeHolderTreeViewItem));

            InitializeComponent();

            PlaceHolderTreeViewItem = placeHolderTreeViewItem;
        }

        [NotNull]
        public PlaceHolderTreeViewItem PlaceHolderTreeViewItem { get; }

        public void Activate()
        {
        }

        public void Close()
        {
        }

        private void CreatePlaceHolderSettings([NotNull] string placeHolderName, [NotNull] string placeHolderPath, [NotNull] Action<ItemHeader> action)
        {
            Debug.ArgumentNotNull(placeHolderName, nameof(placeHolderName));
            Debug.ArgumentNotNull(placeHolderPath, nameof(placeHolderPath));
            Debug.ArgumentNotNull(action, nameof(action));

            var device = PlaceHolderTreeViewItem.DeviceTreeViewItem.Device;

            var options = new TaskDialogOptions
            {
                Owner = this.GetAncestorOrSelf<Window>(),
                Title = "Place Holder Settings",
                CommonButtons = TaskDialogCommonButtons.None,
                MainInstruction = "The place holder settings item was not found.",
                MainIcon = VistaTaskDialogIcon.Information,
                CommandButtons = new[]
                {
                    string.Format("Create place holder settings with name \"{0}\"", placeHolderName),
                    string.Format("Create place holder settings with qualified name \"{0}\"", placeHolderPath)
                },
                AllowDialogCancellation = true
            };

            var r = TaskDialog.Show(options).CommandButtonResult;
            if (r == null)
            {
                return;
            }

            var path = r == 0 ? placeHolderName : placeHolderPath;

            Site.RequestCompleted completed = delegate(string response)
            {
                if (string.IsNullOrEmpty(response))
                {
                    AppHost.MessageBox("Could not create the Place Holder Settings item.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    AppHost.MessageBox("Could not create the Place Holder Settings item.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var itemHeader = ItemHeader.Parse(device.DatabaseUri, root);

                action(itemHeader);
            };

            device.DatabaseUri.Site.Execute("Layouts.CreatePlaceHolderSettings", completed, device.DatabaseUri.DatabaseName.ToString(), path);
        }

        private void Edit([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            FindPlaceHolderSettingsItem(AppHost.OpenUsingDefaultAction);
        }

        private void FindPlaceHolderSettingsItem([NotNull] Action<ItemHeader> action)
        {
            Debug.ArgumentNotNull(action, nameof(action));
            var placeHolderName = PlaceHolderTreeViewItem.PlaceHolderName;
            var placeHolderPath = PlaceHolderTreeViewItem.GetPlaceHolderPath();

            var device = PlaceHolderTreeViewItem.DeviceTreeViewItem.Device;

            Site.RequestCompleted completed = delegate(string response)
            {
                if (string.IsNullOrEmpty(response))
                {
                    CreatePlaceHolderSettings(placeHolderName, placeHolderPath, action);
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    return;
                }

                var itemHeader = ItemHeader.Parse(device.DatabaseUri, root);
                action(itemHeader);
            };

            device.DatabaseUri.Site.Execute("LayoutBuilders.GetPlaceHolderSettings", completed, device.DatabaseUri.DatabaseName.ToString(), placeHolderName, placeHolderPath);
        }

        private void Locate([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            FindPlaceHolderSettingsItem(delegate(ItemHeader itemHeader)
            {
                var activeTree = ActiveContext.ActiveContentTree;
                if (activeTree != null)
                {
                    activeTree.Locate(itemHeader.ItemUri);
                }
            });
        }
    }
}
