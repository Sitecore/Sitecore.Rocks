// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Data
{
    public class ItemPath
    {
        [NotNull]
        public ItemId ItemId => ItemUri.ItemId;

        [NotNull]
        public ItemUri ItemUri { get; set; }

        [NotNull]
        public string Name { get; set; }

        [NotNull]
        public override string ToString()
        {
            return Name;
        }
    }
}
