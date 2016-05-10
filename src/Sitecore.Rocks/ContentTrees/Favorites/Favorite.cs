// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.ContentTrees.Favorites
{
    public class Favorite
    {
        public string FullPath { get; set; }

        public Icon Icon { get; set; }

        public ItemVersionUri ItemVersionUri { get; set; }

        public string Name { get; set; }
    }
}
