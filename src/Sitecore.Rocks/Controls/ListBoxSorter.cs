// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Controls
{
    public class ListBoxSorter
    {
        private readonly List<ControlDragAdorner> _adorners = new List<ControlDragAdorner>();

        private Point _origin;

        public ListBoxSorter([NotNull] ListBox listBox)
        {
            Assert.ArgumentNotNull(listBox, nameof(listBox));

            ListBox = listBox;

            ListBox.PreviewMouseDown += PreviewMouseDown;
            ListBox.MouseMove += MouseMove;
            ListBox.DragOver += DragOver;
            ListBox.Drop += Drop;
        }

        public ListBox ListBox { get; set; }

        private void DragOver([NotNull] object sender, [NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            e.Effects = DragDropEffects.None;
            e.Handled = true;

            if (!e.Data.GetDataPresent(DragManager.DragIdentifier))
            {
                return;
            }

            e.Effects = DragDropEffects.Move;
        }

        private void Drop([NotNull] object sender, [NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            foreach (var adorner in _adorners)
            {
                adorner.Remove();
            }

            _adorners.Clear();

            var items = new List<object>(e.Data.GetData(DragManager.DragIdentifier) as IEnumerable<object>);

            foreach (var item in items)
            {
                ListBox.Items.Remove(item);
            }

            var index = ListBox.Items.IndexOf(sender);

            for (var i = items.Count - 1; i >= 0; i--)
            {
                var item = items[i];
                ListBox.Items.Insert(index, item);
            }
        }

        private void MouseMove([NotNull] object sender, [NotNull] MouseEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (!DragManager.IsDragStart(ListBox, e, ref _origin))
            {
                return;
            }

            var items = new List<object>();

            foreach (var selectedItem in ListBox.SelectedItems)
            {
                items.Add(selectedItem);
            }

            _adorners.Clear();
            /*
      foreach (var item in this.ListBox.Items)
      {
        if (this.ListBox.SelectedItems.Contains(item))
        {
          continue;
        }

        var control = item as Control;
        if (control == null)
        {
          continue;
        }

        var adorner = new ControlDragAdorner(control, ControlDragAdornerPosition.Bottom | ControlDragAdornerPosition.Top);

        this.adorners.Add(adorner);
      }
      */
            var dragData = DragManager.SetData(items);

            DragManager.DoDragDrop(ListBox, dragData, DragDropEffects.Move);

            e.Handled = true;
        }

        private void PreviewMouseDown([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            DragManager.HandleMouseDown(ListBox, e, out _origin);
        }
    }
}
