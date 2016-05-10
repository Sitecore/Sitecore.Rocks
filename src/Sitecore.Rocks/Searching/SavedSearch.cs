// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Searching
{
    public class SavedSearch
    {
        public string Field { get; set; }

        public string Name { get; set; }

        public string QueryText { get; set; }

        public Site Site { get; set; }
    }
}
