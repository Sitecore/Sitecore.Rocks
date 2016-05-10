// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ContextMenuExtensions;

namespace Sitecore.Rocks.Shell.Environment
{
    public class ContextMenuHost
    {
        [CanBeNull]
        public ContextMenu Build([NotNull] object context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            return ContextMenuExtensions.GetContextMenu(context);
        }

        [CanBeNull]
        public ContextMenu Build([NotNull] object context, [NotNull] ContextMenuEventArgs e)
        {
            Assert.ArgumentNotNull(context, nameof(context));
            Assert.ArgumentNotNull(e, nameof(e));

            return ContextMenuExtensions.GetContextMenu(context, e);
        }
    }
}
