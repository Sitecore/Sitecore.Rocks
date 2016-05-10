// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.UI.LayoutDesigners.Dialogs.SelectRenderingsDialogs.Panes;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners.Dialogs.SelectRenderingsDialogs
{
    public partial class SelectRenderingsDialog
    {
        public SelectRenderingsDialog()
        {
            InitializeComponent();
            this.InitializeDialog();

            DatabaseUri = DatabaseUri.Empty;
            Panes = new List<ISelectRenderingsDialogPane>();

            AppHost.Extensibility.ComposeParts(this);
        }

        [CanBeNull]
        public ISelectRenderingsDialogPane ActivePane { get; set; }

        public bool AllowMultipleRenderings { get; set; }

        [NotNull]
        public DatabaseUri DatabaseUri { get; set; }

        [NotNull, ImportMany(typeof(ISelectRenderingsDialogPane))]
        public IEnumerable<ISelectRenderingsDialogPane> Panes { get; }

        [CanBeNull]
        public IRenderingContainer RenderingContainer { get; set; }

        [NotNull]
        public string SpeakCoreVersionId { get; set; } = string.Empty;

        public void GetSelectedRenderings([NotNull] Action<IEnumerable<RenderingItem>> completed)
        {
            Assert.ArgumentNotNull(completed, nameof(completed));

            if (ActivePane == null)
            {
                completed(Enumerable.Empty<RenderingItem>());
                return;
            }

            ActivePane.GetSelectedRenderings(completed);
        }

        public new bool ShowDialog()
        {
            LoadPanes();

            EnableButtons();

            return AppHost.Shell.ShowDialog(this) == true;
        }

        private void ChangeActiveTab([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            e.Handled = true;

            var tabItem = PanesTabControl.SelectedItem as TabItem;
            if (tabItem == null)
            {
                return;
            }

            ActivePane = tabItem.Content as ISelectRenderingsDialogPane;
            EnableButtons();
        }

        private void EnableButtons()
        {
            OkButton.IsEnabled = ActivePane?.AreButtonsEnabled() ?? false;
        }

        private void EnableButtons([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            EnableButtons();
        }

        private void LoadPanes()
        {
            Assert.IsNotNull(DatabaseUri, "DatabaseUri must be set");
            Assert.IsFalse(DatabaseUri == DatabaseUri.Empty, "DatabaseUri must be set");
            Assert.IsNotNull(RenderingContainer, "RenderingContainer must be set");

            foreach (var pane in Panes)
            {
                if (!AllowMultipleRenderings && !pane.AllowMultipleRenderings)
                {
                    continue;
                }

                var tabItem = new TabItem
                {
                    Header = pane.Header,
                    Content = pane
                };

                pane.DatabaseUri = DatabaseUri;
                pane.RenderingContainer = RenderingContainer;
                pane.SpeakCoreVersion = SpeakCoreVersionId;
                pane.DoubleClick += OkClick;
                pane.SelectionChanged += EnableButtons;

                PanesTabControl.Items.Add(tabItem);
            }

            PanesTabControl.SelectedIndex = 0;
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            foreach (var pane in Panes)
            {
                pane.Close();
            }

            this.Close(true);
        }
    }
}
