// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentEditors.Panels;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.ContentEditors.Dialogs
{
    public partial class AppearanceDialog
    {
        public AppearanceDialog()
        {
            InitializeComponent();
            this.InitializeDialog();
        }

        public void Initialize([NotNull] ContentModel contentModel)
        {
            Assert.ArgumentNotNull(contentModel, nameof(contentModel));

            RenderDockPanels();
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            foreach (var child in DockPanelList.Children)
            {
                var checkbox = child as CheckBox;
                if (checkbox == null)
                {
                    continue;
                }

                var descriptor = checkbox.Tag as PanelManager.PanelDescriptor;
                if (descriptor == null)
                {
                    continue;
                }

                descriptor.IsVisible = checkbox.IsChecked == true;
            }

            this.Close(true);
        }

        private void RenderDockPanels()
        {
            var dockPanels = new List<PanelManager.PanelDescriptor>(PanelManager.Panels);

            dockPanels.Sort((d1, d2) => string.Compare(d1.Name, d2.Name, StringComparison.InvariantCultureIgnoreCase));

            foreach (var descriptor in dockPanels)
            {
                var checkBox = new CheckBox
                {
                    Content = descriptor.Name,
                    Margin = new Thickness(0, 2, 0, 2),
                    Tag = descriptor,
                    IsChecked = descriptor.IsVisible
                };

                DockPanelList.Children.Add(checkBox);
            }
        }
    }
}
