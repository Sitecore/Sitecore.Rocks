// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.InfoPanes
{
    public partial class InfoPaneListBox
    {
        public InfoPaneListBox()
        {
            InitializeComponent();
        }

        [NotNull]
        public string Header
        {
            get { return Label.Text ?? string.Empty; }
            set
            {
                Assert.ArgumentNotNull(value, nameof(value));
                Label.Text = value;
            }
        }

        [NotNull]
        public ItemCollection Items
        {
            get { return ListBox.Items; }
        }

        [CanBeNull]
        public object SelectedItem
        {
            get { return ListBox.SelectedItem; }
        }

        public event SelectionChangedEventHandler SelectionChanged;

        private void RaiseSelectionChanged([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectionChanged = SelectionChanged;
            if (selectionChanged == null)
            {
                return;
            }

            selectionChanged(this, e);
        }
    }
}
