// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentTrees.Items.SelectedObjects
{
    [DisplayName(@"Item"), DefaultProperty("Name"), Description("Sitecore Item")]
    public class ItemTreeViewItemSelectedObject : DatabaseUriSelectedObject, IItem
    {
        public ItemTreeViewItemSelectedObject([NotNull] ItemTreeViewItem item) : base(item.Item.ItemUri.DatabaseUri)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            Item = item;
        }

        [NotNull]
        public Icon Icon
        {
            get { return Item.Icon; }
        }

        [NotNull, Description("The Guid of the item."), DisplayName("Item ID"), Category("Item")]
        public string ID
        {
            get
            {
                var i = Item as IItemUri;
                if (i == null)
                {
                    return string.Empty;
                }

                return i.ItemUri.ItemId.ToString();
            }
        }

        [NotNull]
        public ItemUri ItemUri
        {
            get { return Item.ItemUri; }
        }

        [NotNull, Description("Lock."), DisplayName("Lock"), Category("Security")]
        public string Locked
        {
            get
            {
                var i = Item;
                return i.Item.Locked;
            }
        }

        [NotNull, Description("The name of the item."), DisplayName("Item Name"), Category("Item")]
        public string Name
        {
            get
            {
                var i = Item as IItem;
                if (i == null)
                {
                    return string.Empty;
                }

                return i.Name;
            }
        }

        [NotNull, Description("Ownership."), DisplayName("Ownership"), Category("Security")]
        public string Ownership
        {
            get
            {
                var i = Item;
                if (i == null)
                {
                    return string.Empty;
                }

                return i.Item.Ownership;
            }
        }

        [NotNull, Description("The item path."), DisplayName("Item Path"), Category("Item")]
        public string Path
        {
            get
            {
                var i = Item;
                if (i == null)
                {
                    return string.Empty;
                }

                return i.GetPath();
            }
        }

        [NotNull, Description("The ID of the items template."), DisplayName("Template ID"), Category("Template")]
        public string TemplateId
        {
            get
            {
                var i = Item as ITemplatedItem;
                if (i == null)
                {
                    return string.Empty;
                }

                return i.TemplateId.ToString();
            }
        }

        [NotNull, Description("The items template"), DisplayName("Template Name"), Category("Template")]
        public string TemplateName
        {
            get
            {
                var i = Item as ITemplatedItem;
                if (i == null)
                {
                    return string.Empty;
                }

                return i.TemplateName;
            }
        }

        [CanBeNull, Description("Updated."), DisplayName("Updated"), Category("Statistics")]
        public DateTime? Updated
        {
            get
            {
                var i = Item;
                if (i == null)
                {
                    return null;
                }

                return i.Item.Updated;
            }
        }

        [NotNull, Description("Updated By."), DisplayName("Updated By"), Category("Statistics")]
        public string UpdatedBy
        {
            get
            {
                var i = Item;
                if (i == null)
                {
                    return string.Empty;
                }

                return i.Item.UpdatedBy;
            }
        }

        internal ItemTreeViewItem Item { get; set; }
    }
}
