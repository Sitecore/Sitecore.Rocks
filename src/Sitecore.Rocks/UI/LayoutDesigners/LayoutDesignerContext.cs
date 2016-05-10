// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.Commandy;
using Sitecore.Rocks.UI.Commandy.Modes;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners
{
    public class LayoutDesignerContext : ICommandContext, IDefaultCommandyMode
    {
        public LayoutDesignerContext([NotNull] LayoutDesigner layoutDesigner, [CanBeNull] LayoutDesignerItem selectedItem, [NotNull] IEnumerable<LayoutDesignerItem> selectedItems)
        {
            Assert.ArgumentNotNull(layoutDesigner, nameof(layoutDesigner));
            Assert.ArgumentNotNull(selectedItems, nameof(selectedItems));

            LayoutDesigner = layoutDesigner;
            SelectedItem = selectedItem;
            SelectedItems = selectedItems;
        }

        public IEnumerable<IItem> Items => SelectedItems.OfType<RenderingItem>();

        [NotNull]
        public LayoutDesigner LayoutDesigner { get; private set; }

        [CanBeNull]
        public LayoutDesignerItem SelectedItem { get; set; }

        [NotNull]
        public IEnumerable<LayoutDesignerItem> SelectedItems { get; }

        IMode IDefaultCommandyMode.GetCommandyMode(Commandy.Commandy commandy)
        {
            Debug.ArgumentNotNull(commandy, nameof(commandy));

            return new InsertRenderingMode(commandy);
        }
    }
}
