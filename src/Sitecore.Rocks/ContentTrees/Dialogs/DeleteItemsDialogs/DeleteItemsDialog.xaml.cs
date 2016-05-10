// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.ContentTrees.Dialogs.DeleteItemsDialogs
{
    public partial class DeleteItemsDialog
    {
        public enum MessageLevel
        {
            None,

            Info,

            Warning,

            Error
        }

        private readonly List<Message> messages = new List<Message>();

        public DeleteItemsDialog([NotNull] IEnumerable<IItem> selectedItems)
        {
            Assert.ArgumentNotNull(selectedItems, nameof(selectedItems));

            InitializeComponent();
            this.InitializeDialog();

            SelectedItems = selectedItems;

            Group = AppHost.Settings.GetString("ContentTree\\Delete", "TreeViewGroup", "cs");
            switch (Group)
            {
                case "csp":
                    Groupcsp.IsSelected = true;
                    break;
                case "p":
                    Groupp.IsSelected = true;
                    break;
                case "c":
                    Groupc.IsSelected = true;
                    break;
                case "s":
                    Groups.IsSelected = true;
                    break;
                default:
                    Groupcs.IsSelected = true;
                    break;
            }

            Loaded += ControlLoaded;
        }

        [NotNull]
        public string Group { get; set; }

        [NotNull]
        protected IEnumerable<IItem> SelectedItems { get; set; }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            var item = SelectedItems.First();
            var items = string.Join("|", SelectedItems.Select(i => i.ItemUri.ItemId.ToString()));

            Site.RequestCompleted completed = delegate(string response)
            {
                PreparingPanel.Visibility = Visibility.Collapsed;

                var root = response.ToXElement();
                if (root == null)
                {
                    NoMessagesPanel.Visibility = Visibility.Visible;
                    NoMessagesText.Text = "The server returned an unexpected reply.";
                    return;
                }

                OK.IsEnabled = true;

                var count = int.Parse(root.GetAttributeValue("count"));
                if (count == 1)
                {
                    ConfirmText.Text = "Are you sure you want to delete this item?";
                    NoMessagesText.Text = "It is safe to delete this item.";
                }
                else
                {
                    ConfirmText.Text = string.Format("Are you sure you want to delete these {0} items?", count);
                    NoMessagesText.Text = string.Format("It is safe to delete these {0} items.", count);
                }

                if (!root.Elements().Any())
                {
                    NoMessagesPanel.Visibility = Visibility.Visible;
                    return;
                }

                HasMessagesPanel.Visibility = Visibility.Visible;

                ParseMessages(root);
                RenderMessages(messages);
            };

            item.ItemUri.Site.Execute("Items.DeleteItems", completed, item.ItemUri.DatabaseName.ToString(), items, true, false);
        }

        private int CountLeaves([NotNull] TreeViewItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            if (item.Items.Count == 0)
            {
                return 1;
            }

            var count = 0;

            foreach (var subitem in item.Items)
            {
                var treeViewItem = subitem as TreeViewItem;
                if (treeViewItem == null)
                {
                    continue;
                }

                var leaves = CountLeaves(treeViewItem);

                count += leaves;
            }

            if (count > 1)
            {
                var header = item.Header as TreeViewHeader;
                if (header != null)
                {
                    header.Count = count;
                }
            }

            return count;
        }

        [NotNull]
        private ItemCollection GetTreeViewItem([NotNull] ItemCollection itemsCollection, [NotNull] string header)
        {
            Debug.ArgumentNotNull(itemsCollection, nameof(itemsCollection));
            Debug.ArgumentNotNull(header, nameof(header));

            foreach (var item in itemsCollection)
            {
                var treeViewItem = item as TreeViewItem;
                if (treeViewItem == null)
                {
                    continue;
                }

                var h = treeViewItem.Header as TreeViewHeader;
                if (h == null)
                {
                    continue;
                }

                if (h.Text == header)
                {
                    return treeViewItem.Items;
                }
            }

            var treeViewHeader = new TreeViewHeader(MessageLevel.None, header);

            var result = new TreeViewItem
            {
                Header = treeViewHeader,
                IsExpanded = true
            };

            itemsCollection.Add(result);

            return result.Items;
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var item = SelectedItems.First();
            var items = string.Join("|", SelectedItems.Select(i => i.ItemUri.ItemId.ToString()));

            Site.RequestCompleted completed = delegate
            {
                foreach (var selectedItem in SelectedItems)
                {
                    Notifications.RaiseItemDeleted(this, selectedItem.ItemUri);
                }

                this.Close(true);
            };

            OK.IsEnabled = false;
            Cancel.IsEnabled = false;

            item.ItemUri.Site.Execute("Items.DeleteItems", completed, item.ItemUri.DatabaseName.ToString(), items, false, false);
        }

        private void ParseMessages([NotNull] XElement root)
        {
            Debug.ArgumentNotNull(root, nameof(root));

            messages.Clear();

            foreach (var element in root.Elements())
            {
                var text = element.Value;
                var section = element.GetAttributeValue("section");
                var path = element.GetAttributeValue("path");
                var level = MessageLevel.Info;

                switch (element.GetAttributeValue("level"))
                {
                    case "warning":
                        level = MessageLevel.Warning;
                        break;
                    case "error":
                        level = MessageLevel.Error;
                        break;
                }

                messages.Add(new Message(text, section, path, level));
            }
        }

        private void RenderMessages([NotNull] IEnumerable<Message> validations)
        {
            Debug.ArgumentNotNull(validations, nameof(validations));

            Messages.Items.Clear();

            var group = Group.ToCharArray();

            foreach (var item in validations)
            {
                var itemsCollection = Messages.Items;

                foreach (var g in group)
                {
                    switch (g)
                    {
                        case 'c':
                            itemsCollection = GetTreeViewItem(itemsCollection, item.Section);
                            break;

                        case 's':
                            itemsCollection = GetTreeViewItem(itemsCollection, item.Level.ToString());
                            break;

                        case 'p':
                            var path = item.Path;

                            if (!string.IsNullOrEmpty(path))
                            {
                                foreach (var s in path.Split('/', StringSplitOptions.RemoveEmptyEntries))
                                {
                                    itemsCollection = GetTreeViewItem(itemsCollection, s);
                                }
                            }

                            break;
                    }
                }

                var treeViewItem = new TreeViewItem
                {
                    Header = new TreeViewHeader(item.Level, item.Text),
                    Tag = item
                };

                itemsCollection.Add(treeViewItem);
            }

            foreach (var item in Messages.Items)
            {
                var treeViewItem = item as TreeViewItem;
                if (treeViewItem == null)
                {
                    continue;
                }

                CountLeaves(treeViewItem);
            }
        }

        private void SetGroup([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var comboBoxItem = Grouping.SelectedItem as ComboBoxItem;
            if (comboBoxItem == null)
            {
                return;
            }

            Group = (string)comboBoxItem.Tag;
            RenderMessages(messages);

            AppHost.Settings.Set("ContentTree\\Delete", "TreeViewGroup", Group);
        }

        public class Message
        {
            public Message([NotNull] string text, [NotNull] string section, [NotNull] string path, MessageLevel level)
            {
                Assert.ArgumentNotNull(text, nameof(text));
                Assert.ArgumentNotNull(section, nameof(section));
                Assert.ArgumentNotNull(path, nameof(path));

                Text = text;
                Section = section;
                Path = path;
                Level = level;
            }

            public MessageLevel Level { get; }

            [NotNull]
            public string Path { get; }

            [NotNull]
            public string Section { get; }

            [NotNull]
            public string Text { get; }
        }
    }
}
