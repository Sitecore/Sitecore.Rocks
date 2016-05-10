// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.ContentEditors.Dialogs.SetValidators
{
    [UsedImplicitly]
    public partial class ValidatorsPicker
    {
        private IEnumerable<ValidatorHeader> validatorHeaders;

        public ValidatorsPicker()
        {
            InitializeComponent();

            Loaded += ControlLoaded;
        }

        [NotNull]
        public DatabaseUri DatabaseUri { get; set; }

        [NotNull]
        public List<ItemId> SelectedItems { get; set; }

        [CanBeNull]
        public ValidatorHeader GetValidatorHeader([NotNull] ItemId itemId)
        {
            Assert.ArgumentNotNull(itemId, nameof(itemId));

            if (validatorHeaders == null)
            {
                return null;
            }

            return validatorHeaders.FirstOrDefault(header => header.ItemId == itemId);
        }

        public void Initialize([NotNull] DatabaseUri databaseUri, [NotNull] IEnumerable<ItemId> selectedItems)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(selectedItems, nameof(selectedItems));

            DatabaseUri = databaseUri;
            SelectedItems = new List<ItemId>(selectedItems);
        }

        public void Remove([NotNull] ItemId itemId)
        {
            Assert.ArgumentNotNull(itemId, nameof(itemId));

            foreach (var child in Validators.Children)
            {
                var checkBox = child as CheckBox;
                if (checkBox == null)
                {
                    continue;
                }

                var validatorHeader = checkBox.Tag as ValidatorHeader;
                if (validatorHeader == null || validatorHeader.ItemId != itemId)
                {
                    continue;
                }

                checkBox.IsChecked = false;
            }
        }

        public event EventHandler SelectionChanged;

        public event EventHandler ValidatorsLoaded;

        private void AddSection([NotNull] string name)
        {
            Debug.ArgumentNotNull(name, nameof(name));

            var sectionItem = new ListBoxItem
            {
                Content = name,
                IsEnabled = false,
                Foreground = SystemColors.HighlightBrush,
                Margin = new Thickness(4, 8, 4, 0)
            };

            Validators.Children.Add(sectionItem);
        }

        private void AddValidator([NotNull] ValidatorHeader validatorHeader)
        {
            Debug.ArgumentNotNull(validatorHeader, nameof(validatorHeader));

            var item = new CheckBox
            {
                Tag = validatorHeader,
                Content = validatorHeader.Name,
                Margin = new Thickness(16, 1, 0, 1),
                IsChecked = SelectedItems.Contains(validatorHeader.ItemId)
            };

            item.Checked += ValidatorCheck;
            item.Unchecked += ValidatorUncheck;

            Validators.Children.Add(item);
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            ExecuteCompleted completed = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    return;
                }

                var result = new List<ValidatorHeader>();

                var doc = XDocument.Parse(response);

                var root = doc.Root;
                if (root == null)
                {
                    LoadValidators(result);
                    return;
                }

                foreach (var element in root.Elements())
                {
                    var header = new ValidatorHeader
                    {
                        Icon = element.GetAttributeValue("icon"),
                        Name = element.Value,
                        ItemUri = new ItemUri(DatabaseUri, new ItemId(new Guid(element.GetAttributeValue("id")))),
                        Path = element.GetAttributeValue("path"),
                        Section = element.GetAttributeValue("section")
                    };

                    result.Add(header);
                }

                LoadValidators(result);
            };

            DatabaseUri.Site.DataService.ExecuteAsync("Validation.GetValidators", completed, DatabaseUri.DatabaseName.ToString());
        }

        private void FilterTextChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            foreach (Control item in Validators.Children)
            {
                if (!item.IsEnabled)
                {
                    continue;
                }

                var validatorHeader = item.Tag as ValidatorHeader;
                if (validatorHeader == null)
                {
                    continue;
                }

                item.Visibility = validatorHeader.Name.IsFilterMatch(ValidatorSelectorFilter.Text) ? Visibility.Visible : Visibility.Collapsed;
            }

            var hasItems = false;

            for (var n = Validators.Children.Count - 1; n >= 0; n--)
            {
                var item = Validators.Children[n] as Control;
                if (item == null)
                {
                    continue;
                }

                if (!item.IsEnabled)
                {
                    item.Visibility = hasItems ? Visibility.Visible : Visibility.Collapsed;
                    hasItems = false;
                    continue;
                }

                if (item.Visibility == Visibility.Visible)
                {
                    hasItems = true;
                }
            }
        }

        private void LoadValidators([NotNull] IEnumerable<ValidatorHeader> validators)
        {
            Debug.ArgumentNotNull(validators, nameof(validators));

            Validators.Children.Clear();

            validatorHeaders = validators;

            string section = null;

            foreach (var validatorHeader in validators)
            {
                if (validatorHeader.Section != section)
                {
                    AddSection(validatorHeader.Section);

                    section = validatorHeader.Section;
                }

                AddValidator(validatorHeader);
            }

            Loading.Visibility = Visibility.Collapsed;
            Stack.Visibility = Visibility.Visible;

            var loaded = ValidatorsLoaded;
            if (loaded != null)
            {
                loaded(this, EventArgs.Empty);
            }
        }

        private void RaiseSelectionChanged()
        {
            var changed = SelectionChanged;
            if (changed != null)
            {
                changed(this, EventArgs.Empty);
            }
        }

        private void ValidatorCheck([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var checkBox = sender as CheckBox;
            if (checkBox == null)
            {
                return;
            }

            var validatorHeader = checkBox.Tag as ValidatorHeader;
            if (validatorHeader == null)
            {
                return;
            }

            SelectedItems.Add(validatorHeader.ItemId);

            RaiseSelectionChanged();
        }

        private void ValidatorUncheck([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var checkBox = sender as CheckBox;
            if (checkBox == null)
            {
                return;
            }

            var validatorHeader = checkBox.Tag as ValidatorHeader;
            if (validatorHeader == null)
            {
                return;
            }

            SelectedItems.Remove(validatorHeader.ItemId);

            RaiseSelectionChanged();
        }
    }
}
