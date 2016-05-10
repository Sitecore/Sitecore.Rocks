// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.UI.LogViewer
{
    public class LogItem
    {
        public string Category { get; set; }

        public string Description { get; set; }

        public DateTime PublishDate { get; set; }

        public string Title { get; set; }

        [NotNull]
        public override string ToString()
        {
            return string.Format(Resources.LogItem_ToString_Category___0___PublishDate___1_, Category, PublishDate);
        }
    }
}
