// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;

namespace Sitecore.Rocks.UI.Archives
{
    public class ArchiveEntry
    {
        public DateTime Archived { get; set; }

        public string ArchivedBy { get; set; }

        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }
    }
}
