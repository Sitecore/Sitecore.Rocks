// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Dialogs.SetValidators
{
    public partial class ValidatorList
    {
        public ValidatorList()
        {
            InitializeComponent();
        }

        [NotNull]
        public List<ItemId> SelectedItems { get; private set; }

        public void Initialize([NotNull] DatabaseUri databaseUri, [NotNull] List<ItemId> selectedItems)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(selectedItems, nameof(selectedItems));

            ValidatorPicker.Initialize(databaseUri, selectedItems);
            SelectedItems = selectedItems;
        }

        private void HandleKeyDown([NotNull] object sender, [NotNull] KeyEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.Key != Key.Delete)
            {
                return;
            }

            var selectedItems = Validators.SelectedItems.OfType<ListBoxItem>().Select(listBoxItem => listBoxItem.Tag).OfType<TemplateHeader>().Select(templateHeader => templateHeader.TemplateId).ToList();

            foreach (var selectedItem in selectedItems)
            {
                ValidatorPicker.Remove(selectedItem);
            }
        }

        private void Refresh()
        {
            Validators.Items.Clear();

            foreach (var selectedItem in SelectedItems)
            {
                var validatorHeader = ValidatorPicker.GetValidatorHeader(selectedItem);
                if (validatorHeader == null)
                {
                    continue;
                }

                var listBoxItem = new ListBoxItem
                {
                    Tag = validatorHeader,
                    Content = validatorHeader.Name
                };

                Validators.Items.Add(listBoxItem);
            }
        }

        private void SelectionChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            SelectedItems = ValidatorPicker.SelectedItems;
            Refresh();
        }

        private void ValidatorsLoaded([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Refresh();
        }
    }
}
