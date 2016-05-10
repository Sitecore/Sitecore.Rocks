// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Packages.PackageBuilders
{
    public class PackageItem : ITemplatedItem
    {
        public PackageItem([NotNull] ItemHeader itemHeader)
        {
            Assert.ArgumentNotNull(itemHeader, nameof(itemHeader));

            ItemHeader = itemHeader;
        }

        [NotNull]
        public string DatabaseName
        {
            get { return ItemHeader.ItemUri.DatabaseName.ToString(); }
        }

        public Icon Icon
        {
            get { return ItemHeader.Icon; }
        }

        [NotNull]
        public ItemHeader ItemHeader { get; }

        public ItemUri ItemUri
        {
            get { return ItemHeader.ItemUri; }
        }

        public string Name
        {
            get { return ItemHeader.Name; }
        }

        [NotNull]
        public string Path
        {
            get { return ItemHeader.Path; }
        }

        [NotNull]
        public ItemId TemplateId
        {
            get { return ItemHeader.TemplateId; }
        }

        [NotNull]
        public string TemplateName
        {
            get { return ItemHeader.TemplateName; }
        }

        public override string ToString()
        {
            return ItemHeader.Path;
        }
    }
}
