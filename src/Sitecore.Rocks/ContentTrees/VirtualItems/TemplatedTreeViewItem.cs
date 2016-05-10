// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentTrees.VirtualItems
{
    public class TemplatedTreeViewItem : BaseTreeViewItem, ITemplatedItem, ICanDrag, IItemData
    {
        [CanBeNull]
        private Dictionary<string, string> data;

        public TemplatedTreeViewItem()
        {
            ToolTip = string.Empty;
            ToolTipOpening += OpenToolTip;
        }

        [NotNull]
        public ItemUri ItemUri { get; set; }

        [NotNull]
        public ItemId TemplateId { get; set; }

        [NotNull]
        public string TemplateName { get; set; }

        [NotNull]
        string IItem.Name
        {
            get { return Text; }
        }

        public override bool GetChildren(GetChildrenDelegate callback, bool async)
        {
            Assert.ArgumentNotNull(callback, nameof(callback));

            callback(Enumerable.Empty<BaseTreeViewItem>());

            return true;
        }

        [CanBeNull]
        public string GetData(string key)
        {
            Assert.ArgumentNotNull(key, nameof(key));

            if (data == null)
            {
                return null;
            }

            string value;
            if (!data.TryGetValue(key, out value))
            {
                return null;
            }

            return value;
        }

        [NotNull]
        public string GetDragIdentifier()
        {
            return ItemTreeViewItem.BaseTreeViewItemDragIdentifier;
        }

        public void SetData([NotNull] string key, [NotNull] string value)
        {
            Debug.ArgumentNotNull(key, nameof(key));
            Debug.ArgumentNotNull(value, nameof(value));

            if (data == null)
            {
                data = new Dictionary<string, string>();
            }

            data[key] = value;
        }

        private void OpenToolTip([NotNull] object sender, [NotNull] ToolTipEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ToolTip = ToolTipBuilder.BuildToolTip(this);
        }
    }
}
