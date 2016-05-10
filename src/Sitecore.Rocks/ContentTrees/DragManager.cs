// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;

namespace Sitecore.Rocks.ContentTrees
{
    public static class DragManager
    {
        public const string DragIdentifier = "Sitecore.Items";

        public static void DoDragDrop([CanBeNull] DependencyObject dragSource, [CanBeNull] object data, DragDropEffects effects)
        {
            try
            {
                DragDrop.DoDragDrop(dragSource, data, effects);
            }
            catch (COMException ex)
            {
                if (ex.Message.IndexOf("Unspecified error", StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    AppHost.MessageBox("Oh, operating system, come on! You threw an unspecified error.\n\nWell, my dear user, you just have to try again.", "Infoimation", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        public static void HandleMouseDown([NotNull] IInputElement source, [NotNull] MouseEventArgs e, out Point origin)
        {
            Assert.ArgumentNotNull(source, nameof(source));
            Assert.ArgumentNotNull(e, nameof(e));

            origin = e.GetPosition(source);
        }

        public static bool IsDragStart([NotNull] IInputElement source, [NotNull] MouseEventArgs e, ref Point origin)
        {
            Assert.ArgumentNotNull(source, nameof(source));
            Assert.ArgumentNotNull(e, nameof(e));

            if (origin.X == double.MinValue && origin.Y == double.MinValue)
            {
                return false;
            }

            if (e.LeftButton != MouseButtonState.Pressed)
            {
                origin.X = double.MinValue;
                origin.Y = double.MinValue;
                return false;
            }

            var delta = origin - e.GetPosition(source);
            if (Math.Abs(delta.X) <= SystemParameters.MinimumHorizontalDragDistance && Math.Abs(delta.Y) <= SystemParameters.MinimumVerticalDragDistance)
            {
                return false;
            }

            origin.X = double.MinValue;
            origin.Y = double.MinValue;

            var control = e.OriginalSource as FrameworkElement;
            if (control == null)
            {
                return true;
            }

            return control.GetAncestorOrSelf<ScrollBar>() == null;
        }

        public static void SetData([NotNull] DataObject dataObject, [NotNull] IEnumerable<object> items)
        {
            Assert.ArgumentNotNull(dataObject, nameof(dataObject));
            Assert.ArgumentNotNull(items, nameof(items));

            SetItemsData(dataObject, items);

            if (items.Count() == 1)
            {
                var dragSetData = items.OfType<IDragSetData>().FirstOrDefault();
                if (dragSetData != null)
                {
                    dragSetData.SetData(dataObject);
                }
            }
        }

        [NotNull]
        public static DataObject SetData([NotNull] IEnumerable<object> items)
        {
            Assert.ArgumentNotNull(items, nameof(items));

            var result = new DataObject();

            SetData(result, items);

            if (items.Count() == 1)
            {
                var item = items.OfType<IDragSetData>().FirstOrDefault();
                if (item != null)
                {
                    item.SetData(result);
                }
            }

            return result;
        }

        private static void SetDragDropText([NotNull] DataObject dataObject, [NotNull] IItem item)
        {
            Debug.ArgumentNotNull(dataObject, nameof(dataObject));
            Debug.ArgumentNotNull(item, nameof(item));

            var itemData = item as IItemData;
            if (itemData == null)
            {
                return;
            }

            var text = itemData.GetData("ex.dragdrop.text");
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            dataObject.SetData(DataFormats.Text, text);
        }

        private static void SetItemsData([NotNull] DataObject dataObject, [NotNull] IEnumerable<object> items)
        {
            Debug.ArgumentNotNull(dataObject, nameof(dataObject));
            Debug.ArgumentNotNull(items, nameof(items));

            var list = new List<IItem>();

            foreach (var o in items)
            {
                var item = o as IItem;
                if (item == null)
                {
                    return;
                }

                list.Add(item);
            }

            dataObject.SetData(DragIdentifier, list);

            if (list.Count == 1)
            {
                SetDragDropText(dataObject, list.First());
            }
        }
    }
}
