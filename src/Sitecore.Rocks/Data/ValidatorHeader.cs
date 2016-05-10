// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Data
{
    public class ValidatorHeader
    {
        public string Icon { get; set; }

        [NotNull]
        public ItemId ItemId
        {
            get { return ItemUri.ItemId; }
        }

        public ItemUri ItemUri { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public string Section { get; set; }
    }
}
