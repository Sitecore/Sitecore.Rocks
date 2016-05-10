// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;
using Sitecore.Rocks.Extensions.GuidExtensions;

namespace Sitecore.Rocks.UI.Dialogs.SelectItemDialogs.Panes.ContentTreePanes
{
    [Export(typeof(ISelectDialogPane), CreationPolicy = CreationPolicy.NonShared, Priority = 1000)]
    public partial class ContentTreePane : ISelectDialogPane
    {
        private bool loaded;

        public ContentTreePane()
        {
            InitializeComponent();

            Header = "Content Tree";

            AppHost.Extensibility.ComposeParts(this);
        }

        public string Header { get; }

        [NotNull]
        protected SelectItemDialog SelectItemDialog { get; set; }

        [NotNull, ImportMany(typeof(IShortcut))]
        protected IEnumerable<IShortcut> Shortcuts { get; set; }

        public void Close()
        {
            foreach (var shortcut in Shortcuts)
            {
                shortcut.Close(SelectItemDialog);
            }
        }

        public void Initialize(SelectItemDialog selectItemDialog)
        {
            Assert.ArgumentNotNull(selectItemDialog, nameof(selectItemDialog));

            SelectItemDialog = selectItemDialog;

            RenderShortcuts();

            ContentTreeView.GetContext += GetContext;
        }

        public void SetActive()
        {
            var databaseUri = SelectItemDialog.DatabaseUri;
            if (databaseUri == DatabaseUri.Empty)
            {
                return;
            }

            if (!loaded)
            {
                loaded = true;

                BaseTreeViewItem treeViewItem;
                if (SelectItemDialog.ShowAllDatabases)
                {
                    treeViewItem = new SiteTreeViewItem(databaseUri.Site)
                    {
                        Text = databaseUri.Site.Name
                    };
                }
                else
                {
                    treeViewItem = new DatabaseTreeViewItem(databaseUri)
                    {
                        Text = databaseUri.DatabaseName.ToString()
                    };
                }

                treeViewItem.MakeExpandable();

                ContentTreeView.TreeView.Items.Add(treeViewItem);

                treeViewItem.ExpandAndWait();
            }

            if (SelectItemDialog.SelectedItemUri != ItemUri.Empty)
            {
                ContentTreeView.ExpandTo(SelectItemDialog.SelectedItemUri);
            }
            else if (!string.IsNullOrEmpty(SelectItemDialog.InitialItemPath))
            {
                ContentTreeView.ExpandTo(databaseUri, SelectItemDialog.InitialItemPath);
            }
            else if (!string.IsNullOrEmpty(SelectItemDialog.SettingsKey))
            {
                var id = AppHost.Settings.GetString("SelectItemDialog\\" + SelectItemDialog.SettingsKey, databaseUri.ToString(), string.Empty);
                if (id.IsGuid())
                {
                    Guid guid;
                    if (Guid.TryParse(id, out guid))
                    {
                        var itemUri = new ItemUri(databaseUri, new ItemId(guid));
                        ContentTreeView.ExpandTo(itemUri);
                    }
                }
            }

            ContentTreeView.SetFocus();
        }

        private void ExpandToShortcut([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var hyperlink = sender as Hyperlink;
            if (hyperlink == null)
            {
                return;
            }

            var item = hyperlink.Tag as IItem;
            if (item == null)
            {
                return;
            }

            var baseSiteTreeViewItem = ContentTreeView.ExpandTo(item.ItemUri);
            if (baseSiteTreeViewItem == null)
            {
                return;
            }

            baseSiteTreeViewItem.BringIntoView();
            baseSiteTreeViewItem.Focus();
            Keyboard.Focus(baseSiteTreeViewItem);
        }

        private void FilterChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ContentTreeView.Filter(Filter.Text);
        }

        private void FindKeyDown([NotNull] object sender, [NotNull] KeyEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.Key != Key.Enter)
            {
                return;
            }

            ContentTreeView.FindItem(Find.Text);
            e.Handled = true;
        }

        private object GetContext(object source)
        {
            var control = ContentTreeView.TreeView.GetBaseTreeViewItem(source);
            if (control == null)
            {
                return null;
            }

            return new SelectItemDialogContext(SelectItemDialog, ContentTreeView, ContentTreeView.GetSelectedItems(control));
        }

        private void GoClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ContentTreeView.FindItem(Find.Text);
        }

        private void RenderShortcuts()
        {
            ShortcutsStackPanel.Children.Clear();

            var databaseUri = SelectItemDialog.DatabaseUri;
            if (databaseUri == DatabaseUri.Empty)
            {
                return;
            }

            foreach (var shortcut in Shortcuts)
            {
                var header = shortcut.Header;
                var items = shortcut.GetItems(databaseUri).ToList();
                if (!items.Any())
                {
                    continue;
                }

                var headerTextBlock = new TextBlock
                {
                    Text = header
                };

                ShortcutsStackPanel.Children.Add(headerTextBlock);

                headerTextBlock.Style = TryFindResource("ShortcutHeader") as Style;

                foreach (var item in items)
                {
                    var hyperlink = new Hyperlink
                    {
                        Tag = item
                    };

                    hyperlink.Inlines.Add(new TextBlock
                    {
                        Text = item.Name
                    });

                    hyperlink.Click += ExpandToShortcut;

                    var hyperlinkTextBlock = new TextBlock();
                    hyperlinkTextBlock.Inlines.Add(hyperlink);
                    hyperlinkTextBlock.Style = TryFindResource("ShortcutLinkBlock") as Style;

                    ShortcutsStackPanel.Children.Add(hyperlinkTextBlock);

                    hyperlink.Style = TryFindResource("ShortcutLink") as Style;
                }
            }
        }

        private void SetSelectedItems([NotNull] object sender, [NotNull] RoutedPropertyChangedEventArgs<object> e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            SelectItemDialog.SetSelectedItems(ContentTreeView.SelectedItems.OfType<ItemTreeViewItem>());
        }

        private void TreeViewDoubleClick([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            SelectItemDialog.Ok();
        }
    }
}
