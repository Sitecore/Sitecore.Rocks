// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Extensions.ListBoxExtensions
{
    public static class ListBoxExtensions
    {
        [CanBeNull]
        public static object RemoveSelectedItem([NotNull] this Selector listBox)
        {
            Assert.ArgumentNotNull(listBox, nameof(listBox));

            var selectedItem = listBox.SelectedItem;
            if (selectedItem == null)
            {
                return null;
            }

            var index = listBox.SelectedIndex;

            listBox.Items.Remove(selectedItem);
            if (index >= listBox.Items.Count)
            {
                index = listBox.Items.Count - 1;
            }

            if (index >= 0)
            {
                listBox.SelectedIndex = index;
            }

            return selectedItem;
        }

        [NotNull]
        public static IList RemoveSelectedItems([NotNull] this ListBox listBox)
        {
            Assert.ArgumentNotNull(listBox, nameof(listBox));
            var selectedItems = listBox.SelectedItems;
            if (selectedItems.Count == 0)
            {
                return selectedItems;
            }

            var first = int.MaxValue;

            var list = new ArrayList(selectedItems);
            foreach (var item in list)
            {
                var index = listBox.Items.IndexOf(item);
                if (index < first && index >= 0)
                {
                    first = index;
                }

                listBox.Items.Remove(item);
            }

            if (first >= listBox.Items.Count)
            {
                first = listBox.Items.Count - 1;
            }

            if (first >= 0)
            {
                listBox.SelectedIndex = first;
            }

            return selectedItems;
        }
    }
}
