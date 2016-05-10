// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.UI.TemplateHierarchies.Items
{
    public class TemplateHierarchyTreeViewItem : BaseTreeViewItem, ITemplatedItem
    {
        private static readonly ItemId templateId = new ItemId(new Guid("{AB86861A-6030-46C5-B394-E8F99E8B87DB}"));

        public TemplateHierarchyTreeViewItem([NotNull] XElement element, [NotNull] DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(element, nameof(element));
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            Element = element;
            DatabaseUri = databaseUri;
            Text = element.GetAttributeValue("name");
            Icon = new Icon(databaseUri.Site, element.GetAttributeValue("icon"));
            ItemUri = new ItemUri(DatabaseUri, new ItemId(new Guid(Element.GetAttributeValue("id"))));

            if (element.Elements().Any())
            {
                Add(DummyTreeViewItem.Instance);
            }

            Notifications.RegisterItemEvents(this, renamed: ItemRenamed, deleted: ItemDeleted);
        }

        public ItemUri ItemUri { get; }

        public ItemId TemplateId
        {
            get { return templateId; }
        }

        public string TemplateName
        {
            get { return "Template"; }
        }

        protected DatabaseUri DatabaseUri { get; set; }

        protected XElement Element { get; set; }

        public override bool GetChildren(GetChildrenDelegate callback, bool async)
        {
            Assert.ArgumentNotNull(callback, nameof(callback));

            var result = Element.Elements().Select(element => new TemplateHierarchyTreeViewItem(element, DatabaseUri)).ToList();

            callback(result);

            return true;
        }

        private void ItemDeleted([NotNull] object sender, [NotNull] ItemUri deletedItemUri)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(deletedItemUri, nameof(deletedItemUri));

            if (ItemUri == deletedItemUri)
            {
                Remove();
            }
        }

        private void ItemRenamed([NotNull] object sender, [NotNull] ItemUri renamedItemUri, [NotNull] string newName)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(renamedItemUri, nameof(renamedItemUri));
            Debug.ArgumentNotNull(newName, nameof(newName));

            if (ItemUri == renamedItemUri)
            {
                ItemHeader.Text = newName;
            }
        }
    }
}
