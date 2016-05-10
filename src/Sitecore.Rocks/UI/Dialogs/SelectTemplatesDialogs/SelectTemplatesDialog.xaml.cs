// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.UI.Dialogs.SelectTemplatesDialogs
{
    public partial class SelectTemplatesDialog
    {
        private ListBoxSorter listBoxSorter;

        public SelectTemplatesDialog()
        {
            InitializeComponent();
            this.InitializeDialog();

            listBoxSorter = new ListBoxSorter(SelectedTemplatesListBox);
        }

        [NotNull]
        public string HelpText
        {
            get { return DialogHelpText.Text; }

            set { DialogHelpText.Text = value; }
        }

        [NotNull]
        public string Label
        {
            get { return LabelTextBlock.Text; }

            set { LabelTextBlock.Text = value; }
        }

        [NotNull]
        public List<ItemId> SelectedTemplates
        {
            get { return SelectedTemplatesListBox.Items.OfType<ListBoxItem>().Select(listBoxItem => listBoxItem.Tag).OfType<TemplateHeader>().Select(templateHeader => templateHeader.TemplateId).ToList(); }
        }

        [NotNull]
        private List<ItemId> SelectedItems { get; set; }

        public void Initialize([NotNull] string title, [NotNull] DatabaseUri databaseUri, [NotNull] IEnumerable<ItemId> selectedTemplates, bool includeBranches = false)
        {
            Assert.ArgumentNotNull(title, nameof(title));
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(selectedTemplates, nameof(selectedTemplates));

            Title = title;
            SelectedItems = new List<ItemId>(selectedTemplates);

            TemplatePicker.Initialize(databaseUri, selectedTemplates, includeBranches);
        }

        private void HandleKeyDown([NotNull] object sender, [NotNull] KeyEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.Key == Key.Delete)
            {
                RemoveSelected();
            }
        }

        private void MoveDown(object sender, RoutedEventArgs e)
        {
            for (var n = SelectedTemplatesListBox.Items.Count - 2; n >= 0; n--)
            {
                var item = SelectedTemplatesListBox.Items[n] as ListBoxItem;
                if (item == null)
                {
                    continue;
                }

                if (!item.IsSelected)
                {
                    continue;
                }

                SelectedTemplatesListBox.Items.RemoveAt(n);

                SelectedTemplatesListBox.Items.Insert(n + 1, item);

                item.IsSelected = true;
            }
        }

        private void MoveUp(object sender, RoutedEventArgs e)
        {
            for (var n = 1; n < SelectedTemplatesListBox.Items.Count; n++)
            {
                var item = SelectedTemplatesListBox.Items[n] as ListBoxItem;
                if (item == null)
                {
                    continue;
                }

                if (!item.IsSelected)
                {
                    continue;
                }

                SelectedTemplatesListBox.Items.RemoveAt(n);

                SelectedTemplatesListBox.Items.Insert(n - 1, item);

                item.IsSelected = true;
            }
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.Close(true);
        }

        private void OpenContextMenu(object sender, ContextMenuEventArgs e)
        {
            var selectedItem = SelectedTemplatesListBox.SelectedItem as ListBoxItem;
            if (selectedItem == null)
            {
                return;
            }

            var templateHeader = selectedItem.Tag as TemplateHeader;
            if (templateHeader == null)
            {
                return;
            }

            var context = new ItemSelectionContext(new TemplatedItemDescriptor(templateHeader.TemplateUri, string.Empty, templateHeader.TemplateId, templateHeader.Name));

            SelectedTemplatesBorder.ContextMenu = AppHost.ContextMenus.Build(context, e);
        }

        private void Refresh()
        {
            SelectedTemplatesListBox.Items.Clear();

            foreach (var selectedItem in SelectedItems)
            {
                var templateHeader = TemplatePicker.GetTemplateHeader(selectedItem);
                if (templateHeader == null)
                {
                    continue;
                }

                var listBoxItem = new ListBoxItem
                {
                    Tag = templateHeader,
                    Content = templateHeader.Name
                };

                SelectedTemplatesListBox.Items.Add(listBoxItem);
            }
        }

        private void Remove([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            RemoveSelected();
        }

        private void RemoveSelected()
        {
            var index = SelectedTemplatesListBox.SelectedIndex;

            var selectedItems = SelectedTemplatesListBox.SelectedItems.OfType<ListBoxItem>().Select(listBoxItem => listBoxItem.Tag).OfType<TemplateHeader>().Select(templateHeader => templateHeader.TemplateId).ToList();

            foreach (var selectedItem in selectedItems)
            {
                TemplatePicker.Remove(selectedItem);
            }

            // workaround for WPF quirk when checkbox is not visible, the Checked event does not fire
            SelectedItems = TemplatePicker.SelectedItems;
            Refresh();

            if (index >= SelectedTemplatesListBox.Items.Count)
            {
                index = SelectedTemplatesListBox.Items.Count - 1;
            }

            if (index >= 0)
            {
                SelectedTemplatesListBox.SelectedIndex = index;
            }
        }

        private void SelectionChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            SelectedItems = TemplatePicker.SelectedItems;
            Refresh();
        }

        private void SetSelection([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedItem = SelectedTemplatesListBox.SelectedItems.OfType<ListBoxItem>().Select(listBoxItem => listBoxItem.Tag).OfType<TemplateHeader>().Select(templateHeader => templateHeader.TemplateId).FirstOrDefault();
            if (selectedItem == null)
            {
                return;
            }

            TemplatePicker.FocusItem(selectedItem);
        }

        private void TemplatesLoaded([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Refresh();
        }
    }
}
