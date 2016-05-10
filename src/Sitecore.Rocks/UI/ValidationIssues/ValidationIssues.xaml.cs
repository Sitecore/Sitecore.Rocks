// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ContextMenuExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.TreeViewItemExtensions;
using Sitecore.Rocks.Extensions.UserControlExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.UI.ValidationIssues
{
    public partial class ValidationIssues : IContextProvider
    {
        private readonly List<ValidationIssue> validationIssues = new List<ValidationIssue>();

        private string filterText;

        public ValidationIssues()
        {
            InitializeComponent();

            NextItemId = ItemUri.Empty;
            filterText = string.Empty;

            Notifications.RegisterItemEvents(this, deleted: ItemDeleted, renamed: ItemRenamed);
        }

        [NotNull]
        public ItemUri NextItemId { get; set; }

        [NotNull]
        protected ItemUri Source { get; set; }

        [NotNull]
        public object GetContext()
        {
            var context = new ValidationIssuesContext(this);

            var selectedItem = Issues.SelectedItem as TreeViewItem;
            if (selectedItem != null)
            {
                context.SelectedItem = selectedItem.Tag as ValidationIssue;
            }

            return context;
        }

        public void LoadIssues()
        {
            Loading.Visibility = Visibility.Visible;
            Issues.Visibility = Visibility.Collapsed;

            Issues.Items.Clear();
            validationIssues.Clear();

            LoadIssues(Source);
        }

        public void Next()
        {
            LoadIssues(NextItemId);
        }

        public void SetSource([NotNull] ItemUri item)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            Source = item;

            LoadIssues();
        }

        private void FilterNodesChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            filterText = Filter.Text;

            Issues.FilterTreeViewItems(filterText, GetHeaderText);
        }

        [NotNull]
        private string GetHeaderText([NotNull] TreeViewItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var leaf = item.Header as LeafHeader;
            if (leaf != null)
            {
                return leaf.Text;
            }

            var node = item.Header as NodeHeader;
            if (node != null)
            {
                return node.Text;
            }

            return string.Empty;
        }

        [NotNull]
        private TreeViewItem GetLeaf([NotNull] ValidationIssue issue, [NotNull] string text, [CanBeNull] Icon icon)
        {
            Debug.ArgumentNotNull(issue, nameof(issue));
            Debug.ArgumentNotNull(text, nameof(text));

            var header = new LeafHeader
            {
                Text = text
            };

            header.DoubleClick += delegate
            {
                var itemVersionUri = new ItemVersionUri(issue.ItemUri, LanguageManager.CurrentLanguage, Data.Version.Latest);

                AppHost.OpenContentEditor(itemVersionUri);
            };

            if (icon != null)
            {
                header.Icon = icon;
            }

            var result = new TreeViewItem
            {
                Header = header,
                Tag = issue
            };

            return result;
        }

        [NotNull]
        private TreeViewItem GetNode([NotNull] TreeViewPathItem pathItem)
        {
            Debug.ArgumentNotNull(pathItem, nameof(pathItem));

            var header = new NodeHeader
            {
                Text = pathItem.Text
            };

            if (pathItem.Icon != null)
            {
                header.Icon = pathItem.Icon;
            }

            var result = new TreeViewItem
            {
                Header = header,
                Tag = pathItem.Tag,
                Margin = new Thickness(0, 4, 0, 0)
            };

            return result;
        }

        [NotNull]
        private IEnumerable<TreeViewPathItem> GetPath([NotNull] string group, [NotNull] ValidationIssue issue)
        {
            Debug.ArgumentNotNull(group, nameof(group));
            Debug.ArgumentNotNull(issue, nameof(issue));

            var result = new List<TreeViewPathItem>();

            foreach (var c in group.ToCharArray())
            {
                switch (c)
                {
                    case 'c':
                        result.Add(new TreeViewPathItem
                        {
                            Tag = issue.CategoryName,
                            Text = issue.CategoryName,
                            Icon = issue.CategoryIcon
                        });
                        break;
                    case 'v':
                        result.Add(new TreeViewPathItem
                        {
                            Tag = issue.ValidatorId,
                            Text = issue.ValidatorName,
                            Icon = issue.ValidatorIcon
                        });
                        break;
                    case 'i':
                        result.Add(new TreeViewPathItem
                        {
                            Tag = issue.ItemUri.ToString(),
                            Text = issue.Name,
                            Icon = issue.ItemIcon
                        });
                        break;
                }
            }

            return result;
        }

        private void GroupByChanged([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Refresh();
        }

        private void ItemDeleted([NotNull] object sender, [NotNull] ItemUri itemuri)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(itemuri, nameof(itemuri));

            if (validationIssues == null)
            {
                return;
            }

            var changed = false;
            for (var index = validationIssues.Count - 1; index >= 0; index--)
            {
                var issue = validationIssues[index];
                if (issue.ItemUri != itemuri)
                {
                    continue;
                }

                validationIssues.Remove(issue);
                changed = true;
            }

            if (!changed)
            {
                return;
            }

            Issues.Items.Clear();
            RenderIssues(validationIssues);
            Issues.FilterTreeViewItems(filterText, GetHeaderText);
        }

        private void ItemRenamed([NotNull] object sender, [NotNull] ItemUri itemuri, [NotNull] string newname)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(itemuri, nameof(itemuri));
            Debug.ArgumentNotNull(newname, nameof(newname));

            if (validationIssues == null)
            {
                return;
            }

            var changed = false;
            for (var index = validationIssues.Count - 1; index >= 0; index--)
            {
                var issue = validationIssues[index];
                if (issue.ItemUri != itemuri)
                {
                    continue;
                }

                issue.Name = issue.Name.Left(issue.Name.LastIndexOf('/')) + '/' + newname;
                changed = true;
            }

            if (!changed)
            {
                return;
            }

            Issues.Items.Clear();
            RenderIssues(validationIssues);
            Issues.FilterTreeViewItems(filterText, GetHeaderText);
        }

        private void LoadIssues([NotNull] ItemUri itemUri)
        {
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));

            ExecuteCompleted callback = delegate(string response, ExecuteResult executeResult)
            {
                Loading.Visibility = Visibility.Collapsed;
                Issues.Visibility = Visibility.Visible;

                if (!DataService.HandleExecute(response, executeResult))
                {
                    return;
                }

                LoadIssues(response);
            };

            Source.Site.DataService.ExecuteAsync("Sitecore.Rocks.Server.Requests.Validation.GetValidationIssues", callback, itemUri.DatabaseName.ToString(), itemUri.ItemId.ToString(), Source.ItemId.ToString());
        }

        private void LoadIssues([NotNull] string response)
        {
            Debug.ArgumentNotNull(response, nameof(response));

            string nextItemId;

            var issues = Parse(Source.DatabaseUri, response, out nextItemId);

            RenderIssues(issues);

            Issues.FilterTreeViewItems(filterText, GetHeaderText);

            validationIssues.AddRange(issues);

            if (!string.IsNullOrEmpty(nextItemId))
            {
                NextItemId = new ItemUri(Source.DatabaseUri, new ItemId(new Guid(nextItemId)));
            }
            else
            {
                NextItemId = ItemUri.Empty;
            }
        }

        private void OpenContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ContextMenu = null;

            var context = GetContext();

            var commands = Rocks.Commands.CommandManager.GetCommands(context);
            if (!commands.Any())
            {
                e.Handled = true;
                return;
            }

            var contextMenu = new ContextMenu();

            contextMenu.Build(commands, context);

            ContextMenu = contextMenu;
        }

        [NotNull]
        private List<ValidationIssue> Parse([NotNull] DatabaseUri databaseUri, [NotNull] string response, [NotNull] out string nextItemId)
        {
            Debug.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Debug.ArgumentNotNull(response, nameof(response));

            nextItemId = string.Empty;

            var result = new List<ValidationIssue>();

            XDocument doc;
            try
            {
                doc = XDocument.Parse(response);
            }
            catch
            {
                return result;
            }

            var root = doc.Root;
            if (root == null)
            {
                return result;
            }

            foreach (var element in root.Elements(@"issue"))
            {
                result.Add(new ValidationIssue(new ItemUri(databaseUri, new ItemId(new Guid(element.GetAttributeValue("itemid")))), element.GetAttributeValue("path"), new Icon(databaseUri.Site, element.GetAttributeValue("icon")), new Icon(databaseUri.Site, element.GetAttributeValue("itemicon")), element.GetAttributeValue("categoryname"), new Icon(databaseUri.Site, element.GetAttributeValue("categoryicon")), element.Value, element.GetAttributeValue("validatorid"), element.GetAttributeValue("validatorname"), new Icon(databaseUri.Site, element.GetAttributeValue("validatoricon"))));
            }

            var next = root.Element(@"next");
            if (next != null)
            {
                nextItemId = next.Value;
            }

            return result;
        }

        private void PreselectItem([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var treeViewItem = TreeViewItemExtensions.VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
            if (treeViewItem != null)
            {
                treeViewItem.IsSelected = true;
            }
        }

        private void Refresh()
        {
            var treeView = Issues;
            if (treeView == null)
            {
                return;
            }

            var itemCollection = treeView.Items;
            if (itemCollection == null)
            {
                return;
            }

            itemCollection.Clear();
            RenderIssues(validationIssues);
        }

        private void RefreshClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Refresh();
        }

        private void RenderIssue([NotNull] string group, [NotNull] ValidationIssue issue)
        {
            Debug.ArgumentNotNull(group, nameof(group));
            Debug.ArgumentNotNull(issue, nameof(issue));

            var path = GetPath(group, issue);

            var collection = Issues.FindPath(path, GetNode);
            if (collection == null)
            {
                return;
            }

            var treeViewItem = GetLeaf(issue, issue.Text, issue.Icon);

            collection.Add(treeViewItem);
        }

        private void RenderIssues([NotNull] IEnumerable<ValidationIssue> issues)
        {
            Debug.ArgumentNotNull(issues, nameof(issues));

            var group = string.Empty;

            var selectedItem = GroupBy.SelectedItem as ComboBoxItem;
            if (selectedItem != null)
            {
                group = selectedItem.Tag as string ?? string.Empty;
            }

            if (string.IsNullOrEmpty(group))
            {
                group = @"vi";
            }

            foreach (var issue in issues)
            {
                RenderIssue(group, issue);
            }

            foreach (var item in Issues.Items)
            {
                var treeViewItem = item as TreeViewItem;
                if (treeViewItem == null)
                {
                    continue;
                }

                UpdateLeafCount(treeViewItem);
            }
        }

        private void ToolBarLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.InitializeToolBar(sender);
        }

        private int UpdateLeafCount([NotNull] TreeViewItem treeViewItem)
        {
            Debug.ArgumentNotNull(treeViewItem, nameof(treeViewItem));

            var count = 0;

            foreach (var child in treeViewItem.Items)
            {
                var item = child as TreeViewItem;
                if (item == null)
                {
                    continue;
                }

                count += UpdateLeafCount(item);
            }

            var header = treeViewItem.Header as NodeHeader;
            if (header != null)
            {
                header.LeafCount = count;
            }

            if (treeViewItem.Items.Count == 0)
            {
                return 1;
            }

            return count;
        }
    }
}
