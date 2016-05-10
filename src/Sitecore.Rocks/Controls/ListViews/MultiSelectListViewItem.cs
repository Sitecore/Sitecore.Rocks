// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Controls.ListViews
{
    public class MultiSelectListViewItem : ListViewItem
    {
        protected override void OnMouseEnter([CanBeNull] MouseEventArgs e)
        {
        }

        protected override void OnMouseLeftButtonDown([CanBeNull] MouseButtonEventArgs e)
        {
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(e, nameof(e));

            base.OnMouseLeftButtonDown(e);
        }
    }
}
