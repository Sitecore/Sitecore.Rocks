// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Extensions.ListViewExtensions
{
    public static class ListViewExtensions
    {
        public static void ResizeColumn([NotNull] this ListView listView, [NotNull] GridViewColumn column)
        {
            Assert.ArgumentNotNull(column, nameof(column));
            Assert.ArgumentNotNull(listView, nameof(listView));

            if (double.IsNaN(column.Width))
            {
                column.Width = column.ActualWidth;
            }

            column.Width = double.NaN;
        }
    }
}
