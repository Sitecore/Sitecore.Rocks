// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;

namespace Sitecore.Rocks.UI.Dialogs.SelectItemDialogs.Panes.SearchPanes
{
    [Export(typeof(ISelectDialogPane), CreationPolicy = CreationPolicy.NonShared, Priority = 2000)]
    public partial class SearchPane : ISelectDialogPane
    {
        public SearchPane()
        {
            InitializeComponent();

            Header = "Search";

            SearchControl.SettingsKey = "SelectItemDialog";
            SearchControl.SelectionChanged += SetSelectedItems;
        }

        public string Header { get; }

        [NotNull]
        protected SelectItemDialog SelectItemDialog { get; set; }

        public void Close()
        {
        }

        public void Initialize(SelectItemDialog selectItemDialog)
        {
            Assert.ArgumentNotNull(selectItemDialog, nameof(selectItemDialog));

            SelectItemDialog = selectItemDialog;

            SearchControl.DatabaseUri = SelectItemDialog.DatabaseUri;
            SearchControl.SelectionMode = SelectItemDialog.AllowMultipleSelection ? SelectionMode.Extended : SelectionMode.Single;
        }

        public void SetActive()
        {
            SelectItemDialog.SetSelectedItems(SearchControl.SelectedItems);

            SearchControl.Code.Focus();
            Keyboard.Focus(SearchControl.Code);
        }

        private void SetSelectedItems([NotNull] object sender, [NotNull] IEnumerable<ItemHeader> items)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(items, nameof(items));

            SelectItemDialog.SetSelectedItems(items);
        }
    }
}
