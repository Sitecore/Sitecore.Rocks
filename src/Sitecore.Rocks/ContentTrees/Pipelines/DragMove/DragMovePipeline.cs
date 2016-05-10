// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.ContentTrees.Pipelines.DragMove
{
    public class DragMovePipeline : Pipeline<DragMovePipeline>
    {
        public DragMovePipeline()
        {
            MovedItems = new List<IItem>();
            Confirm = true;
        }

        public ItemTreeViewItem Anchor { get; set; }

        public bool Confirm { get; set; }

        public IEnumerable<IItem> Items { get; set; }

        public DragDropKeyStates KeyStates { get; set; }

        public List<IItem> MovedItems { get; private set; }

        public ItemTreeViewItem Owner { get; set; }

        public ControlDragAdornerPosition Position { get; set; }

        public ItemTreeViewItem Target { get; set; }

        [NotNull]
        public DragMovePipeline WithParameters([NotNull] ItemTreeViewItem target, [NotNull] IEnumerable<IItem> items, ControlDragAdornerPosition position, DragDropKeyStates keyStates)
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
    }
}
