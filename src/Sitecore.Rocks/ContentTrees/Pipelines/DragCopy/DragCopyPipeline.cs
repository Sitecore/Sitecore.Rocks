// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.ContentTrees.Pipelines.DragCopy
{
    public class DragCopyPipeline : Pipeline<DragCopyPipeline>
    {
        public DragCopyPipeline()
        {
            NewItems = new List<NewItem>();
            Confirm = true;
        }

        public ItemTreeViewItem Anchor { get; set; }

        public bool Confirm { get; set; }

        public IEnumerable<IItem> Items { get; set; }

        public DragDropKeyStates KeyStates { get; set; }

        public List<NewItem> NewItems { get; private set; }

        public ItemTreeViewItem Owner { get; set; }

        public ControlDragAdornerPosition Position { get; set; }

        public ItemTreeViewItem Target { get; set; }

        [NotNull]
        public DragCopyPipeline WithParameters([NotNull] ItemTreeViewItem target, [NotNull] IEnumerable<IItem> items, ControlDragAdornerPosition position, DragDropKeyStates keyStates)
        {
            Assert.ArgumentNotNull(target, nameof(target));
            Assert.ArgumentNotNull(items, nameof(items));

            KeyStates = keyStates;
            Target = target;
            Items = items;
            Position = position;

            Start();

            return this;
        }

        public class NewItem
        {
            public IItem Item { get; set; }

            public ItemUri NewItemUri { get; set; }

            public string NewName { get; set; }

            public int SortOrder { get; set; }
        }
    }
}
