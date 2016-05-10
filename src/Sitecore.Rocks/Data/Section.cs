// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Data
{
    public class Section
    {
        public Section()
        {
            Name = string.Empty;
            ItemUri = ItemUri.Empty;
            ExpandedByDefault = true;
            Icon = Icon.Empty;
        }

        public bool ExpandedByDefault { get; set; }

        [NotNull]
        public Icon Icon { get; set; }

        [NotNull]
        public ItemUri ItemUri { get; set; }

        [NotNull]
        public string Name { get; set; }

        public int SortOrder { get; set; }
    }
}
