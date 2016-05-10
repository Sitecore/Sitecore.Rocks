// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Data
{
    public class Warning
    {
        [NotNull]
        public string Icon { get; set; }

        [NotNull]
        public string Text { get; set; }

        [NotNull]
        public string Title { get; set; }
    }
}
